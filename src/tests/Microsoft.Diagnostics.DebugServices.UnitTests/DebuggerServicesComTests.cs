// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.DebugServices.Implementation;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Extensions;
using SOS.Hosting;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop;
using SOS.Hosting.Interop.DbgEng;
using Xunit;
using static Microsoft.Diagnostics.DebugServices.UnitTests.DataTargetComTests;
using static Microsoft.Diagnostics.DebugServices.UnitTests.ScopedComTests;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe class DebuggerServicesComTests
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void OptionalServicesShareTheMainProxyWhenAvailable(bool memoryAvailable, bool stackAvailable)
        {
            using NativeDebugger native = new() { MemoryAvailable = memoryAvailable, StackAvailable = stackAvailable };
            native.WithServices(debugger =>
            {
                Assert.Equal(memoryAvailable, debugger.RemoteMemoryService != null);
                Assert.Equal(stackAvailable, debugger.ThreadStackTraceService != null);
                if (memoryAvailable)
                    Assert.Same(debugger.DebugClient, debugger.RemoteMemoryService);
                if (stackAvailable)
                    Assert.Same(debugger.DebugClient, debugger.ThreadStackTraceService);
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(HResult.S_OK, true)]
        [InlineData(HResult.S_FALSE, false)]
        [InlineData(HResult.E_FAIL, false)]
        public void RemoteMemoryPreservesArgumentsAndExactSuccessResult(int result, bool expected)
        {
            using NativeDebugger native = new() { MemoryResult = result };
            native.WithServices(debugger =>
            {
                RemoteMemoryService memory = new(debugger.RemoteMemoryService);
                Assert.Equal(expected, memory.AllocateMemory(0x1234567800000000, 0xF0000001, 0x80001000, 0x80000004, out ulong address));
                Assert.Equal(0xFEDCBA9800002000ul, address);
                Assert.Equal(0x1234567800000000ul, native.MemoryAddress);
                Assert.Equal(0xF0000001u, native.MemorySize);
                Assert.Equal(0x80001000u, native.MemoryTypeFlags);
                Assert.Equal(0x80000004u, native.MemoryProtectionFlags);
                Assert.Equal(expected, memory.FreeMemory(address, 0xF0000002, 0x80008000));
                Assert.Equal(address, native.MemoryAddress);
                Assert.Equal(0xF0000002u, native.MemorySize);
                Assert.Equal(0x80008000u, native.MemoryTypeFlags);
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(2u)]
        public void DebuggerStackTracePreservesNativeFramesAndBounds(uint framesFilled)
        {
            using NativeDebugger native = new() { FramesFilled = framesFilled };
            native.WithServices(debugger =>
            {
                ThreadStackTraceService service = new(debugger.ThreadStackTraceService);
                IStack stack = service.GetDebuggerStackTrace(0x87654321, 2);
                Assert.Equal((int)framesFilled, stack.FrameCount);
                Assert.Equal(0x87654321u, native.StackThreadId);
                Assert.Equal(2u, native.StackCapacity);
                Assert.Equal(1, native.StackCalls);
                for (int index = 0; index < stack.FrameCount; index++)
                {
                    IStackFrame frame = stack.GetStackFrame(index);
                    Assert.Equal(0xFEDCBA9800000000ul + (uint)index, frame.InstructionPointer);
                    Assert.Equal(index == 0 ? 0x1234567800000000ul : 0ul, frame.StackPointer);
                    Assert.Throws<NotImplementedException>(() => frame.ModuleBase);
                    Assert.Throws<NotImplementedException>(() => frame.GetMethodName(out _, out _, out _));
                }
                Assert.Throws<ArgumentOutOfRangeException>(() => stack.GetStackFrame(-1));
                Assert.Throws<ArgumentOutOfRangeException>(() => stack.GetStackFrame(stack.FrameCount));
            });
            native.AssertBaselineReferences();
        }

        [Fact]
        public void EmptyAndNegativeStacksDoNotInvokeNativeCode()
        {
            using NativeDebugger native = new();
            native.WithServices(debugger =>
            {
                ThreadStackTraceService service = new(debugger.ThreadStackTraceService);
                Assert.Equal(0, service.GetDebuggerStackTrace(42, 0).FrameCount);
                Assert.Throws<ArgumentOutOfRangeException>(() => service.GetDebuggerStackTrace(42, -1));
                Assert.Equal(0, native.StackCalls);
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(HResult.E_FAIL, 2u)]
        [InlineData(HResult.S_OK, 3u)]
        public void DebuggerStackTraceReportsFailuresAndOversizedResults(int result, uint framesFilled)
        {
            using NativeDebugger native = new() { StackResult = result, FramesFilled = framesFilled };
            native.WithServices(debugger =>
            {
                ThreadStackTraceService service = new(debugger.ThreadStackTraceService);
                DiagnosticsException exception = Assert.Throws<DiagnosticsException>(() => service.GetDebuggerStackTrace(42, 2));
                Assert.Contains("0000002a", exception.Message);
                Assert.Equal(1, native.StackCalls);
            });
            native.AssertBaselineReferences();
        }

        [Fact]
        public void OptionalWrappersRetainProxyAfterDebuggerCollectionAndAreNotDisposable()
        {
            using NativeDebugger native = new();
            native.WithOptionalServices((memory, stack, debugger) =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Assert.False(debugger.IsAlive);
                Assert.False((object)memory is IDisposable);
                Assert.False((object)stack is IDisposable);
                TestServices services = new();
                services.Container.AddService<IRemoteMemoryService>(memory);
                services.Container.AddService<IThreadStackTraceService>(stack);
                services.Container.DisposeServices();
                Assert.True(memory.AllocateMemory(0, 4096, 0x1000, 4, out ulong address));
                Assert.True(memory.FreeMemory(address, 0, 0x8000));
                Assert.Equal(2, stack.GetDebuggerStackTrace(42, 2).FrameCount);
            });
            native.AssertBaselineReferences();
        }

        [Fact]
        public void MainProxyDispatchesNativeBuffersAndPreservesHResults()
        {
            using NativeDebugger native = new();
            native.WithServices(debugger =>
            {
                Assert.Equal(HResult.S_OK, (int)debugger.GetOperatingSystem(out IDebuggerServicesGenerated.OperatingSystem operatingSystem));
                Assert.Equal(IDebuggerServicesGenerated.OperatingSystem.Linux, operatingSystem);
                byte[] buffer = new byte[4];
                Assert.Equal(HResult.S_FALSE, (int)debugger.ReadVirtual(NativeDebugger.ModuleBase, buffer, out int bytesRead));
                Assert.Equal(4, bytesRead);
                Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, buffer);
                Assert.Equal(HResult.S_OK, (int)debugger.WriteVirtual(NativeDebugger.ModuleBase, buffer, out int bytesWritten));
                Assert.Equal(4, bytesWritten);
                Assert.Equal(buffer, native.WrittenMemory);
                Assert.Equal(HResult.S_OK, (int)debugger.GetThreadContext(42, 0x1234, buffer));
                Assert.Equal(new byte[] { 1, 2, 3, 4 }, buffer);
                Assert.Equal(HResult.S_OK, (int)debugger.VirtualUnwind(42, buffer));
                Assert.Equal(new byte[] { 4, 3, 2, 1 }, buffer);
                Assert.Equal(HResult.S_OK, (int)debugger.GetSymbolPath(out string symbolPath));
                Assert.Equal("C:\\symbols", symbolPath);
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(HResult.S_OK, 0x123456789ABCDEF0ul)]
        [InlineData(HResult.E_FAIL, 0ul)]
        public void ThreadTebPreservesZeroWhenNativeFailureDoesNotWrite(int result, ulong expected)
        {
            using NativeDebugger native = new() { TebResult = result };
            native.WithServices(debugger =>
            {
                Assert.Equal(result, (int)debugger.GetThreadTeb(42, out ulong teb));
                Assert.Equal(expected, teb);
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(HResult.S_OK, true)]
        [InlineData(HResult.E_FAIL, false)]
        public void MainProxyDispatchesVoidAndFinalSlots(int verificationResult, bool expectedEnabled)
        {
            using NativeDebugger native = new() { VerificationResult = verificationResult };
            native.WithServices(debugger =>
            {
                debugger.OutputString(DEBUG_OUTPUT.ERROR, "native output");
                Assert.Equal(DEBUG_OUTPUT.ERROR, native.OutputMask);
                Assert.Equal("native output", native.OutputText);
                Assert.Equal(132, debugger.GetOutputWidth());
                debugger.FlushCheck();
                Assert.Equal(1, native.FlushCalls);
                Assert.Equal(verificationResult, (int)debugger.GetDacSignatureVerificationSettings(out bool enabled));
                Assert.Equal(expectedEnabled, enabled);
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(HostType.DbgEng)]
        [InlineData(HostType.Lldb)]
        public void NativeClientHandoffReturnsCallerOwnedReference(HostType hostType)
        {
            using NativeDebugger native = new();
            native.WithServices(debugger =>
            {
                int references = native.Client->References;
                IntPtr client = debugger.GetNativeClient();
                Assert.Equal((IntPtr)native.Client, client);
                Assert.Equal(references + 1, native.Client->References);
                Release(client);
                Assert.Equal(references, native.Client->References);
            }, hostType);
            native.AssertBaselineReferences();
        }

        [Fact]
        public void MissingNativeClientReturnsZeroAndInvalidHostThrows()
        {
            using NativeDebugger native = new() { NativeClientAvailable = false };
            native.WithServices(debugger =>
            {
                Assert.NotNull(debugger.DebugClient);
                Assert.NotNull(debugger.DebugSymbols);
                Assert.Equal(0, native.NativeClientQueries);
                Assert.Equal(IntPtr.Zero, debugger.GetNativeClient());
            });
            native.WithServices(debugger => Assert.Throws<InvalidOperationException>(() => debugger.GetNativeClient()), HostType.DotnetDump);
            native.AssertBaselineReferences();
        }

        [Fact]
        public void MissingMainInterfaceReleasesIncomingReference()
        {
            using NativeDebugger native = new() { ServicesAvailable = false };
            Assert.Throws<InvalidCastException>(() => native.CreateServices());
            native.AssertBaselineReferences();
        }

        [Fact]
        public void GeneratedImportsApplySymbolOptionsAndReleaseAfterCollection()
        {
            using NativeDebugger native = new();
            native.WithServices(debugger =>
            {
                Assert.IsType<ComObject>(debugger.DebugClient);
                Assert.IsType<ComObject>(debugger.DebugSymbols);
                Assert.Same(debugger.DebugClient, debugger.DebugSymbols);
                Assert.Equal(0, native.NativeClientQueries);
                Assert.Equal(SYMOPT.PUBLICS_ONLY, native.AddedOptions);
                Assert.Equal(1, native.OptionsCalls);
                DEBUG_OUTPUT mask = default;
                Assert.Equal(HResult.S_OK, debugger.DebugClient.GetOutputMask(&mask));
                Assert.Equal(DEBUG_OUTPUT.ERROR, mask);
                IntPtr client = debugger.GetNativeClient();
                try
                {
                    Assert.Equal((IntPtr)native.Client, client);
                }
                finally
                {
                    Release(client);
                }
                TestServices services = new();
                services.Container.AddService(debugger);
                services.Container.DisposeServices();
                Assert.False((object)debugger is IDisposable);
                Assert.NotNull(debugger.DebugClient);
                Assert.NotNull(debugger.DebugSymbols);
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void MissingOptionalInterfacesPreserveUnknownSymbolStatus(bool clientAvailable, bool symbolsAvailable)
        {
            using NativeDebugger native = new() { ClientAvailable = clientAvailable, SymbolsAvailable = symbolsAvailable };
            native.WithServices(debugger =>
            {
                Assert.Equal(clientAvailable, debugger.DebugClient != null);
                Assert.Null(debugger.DebugSymbols);
                Assert.Equal(0, native.OptionsCalls);
                using ModuleServiceFromDebuggerServices modules = CreateModuleService(debugger);
                IModule module = ((IModuleService)modules).GetModuleFromIndex(0);
                Assert.Equal(SymbolStatus.Unknown, module.Services.GetService<IModuleSymbols>().GetSymbolStatus());
                Assert.Equal(0, native.ParametersCalls);
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(HostType.Lldb)]
        [InlineData(HostType.DotnetDump)]
        public void OtherHostsDoNotImportDbgEng(HostType hostType)
        {
            using NativeDebugger native = new();
            native.WithServices(debugger =>
            {
                Assert.Null(debugger.DebugClient);
                Assert.Null(debugger.DebugSymbols);
                Assert.Equal(0, native.OptionsCalls);
            }, hostType);
            native.AssertBaselineReferences();
        }

        [Fact]
        public void SymbolOptionsFailurePropagatesToSymbolLookup()
        {
            using NativeDebugger native = new() { OptionsResult = HResult.E_FAIL };
            native.WithServices(debugger =>
            {
                Assert.Equal(HResult.E_FAIL, (int)debugger.GetSymbolByOffset(0, NativeDebugger.ModuleBase, out string symbol, out ulong displacement));
                Assert.Null(symbol);
                Assert.Equal(0ul, displacement);
                Assert.Equal(1, native.OptionsCalls);
            });
            native.AssertBaselineReferences();
        }

        [Fact]
        public void InterfaceCastsDoNotReimportClientIdentity()
        {
            using NativeDebugger native = new() { UnknownQueryResult = HResult.E_FAIL };
            native.WithServices(debugger =>
            {
                Assert.NotNull(debugger.DebugClient);
                Assert.NotNull(debugger.DebugSymbols);
                Assert.Equal(0, native.NativeClientQueries);
                DEBUG_OUTPUT mask = default;
                Assert.Equal(HResult.S_OK, debugger.DebugClient.GetOutputMask(&mask));
                Assert.Equal(DEBUG_OUTPUT.ERROR, mask);
            });
            native.AssertBaselineReferences();
        }

        [Fact]
        public void SharedProxyRemainsUsableAfterOneServiceIsCollected()
        {
            using NativeDebugger native = new();
            native.WithServices(second =>
            {
                WeakReference firstReference = null;
                native.WithServices(first =>
                {
                    Assert.Same(first.DebugClient, second.DebugClient);
                    Assert.Same(first.DebugSymbols, second.DebugSymbols);
                    firstReference = new WeakReference(first);
                });
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Assert.False(firstReference.IsAlive);
                Assert.Equal(HResult.S_OK, (int)second.GetOperatingSystem(out IDebuggerServicesGenerated.OperatingSystem operatingSystem));
                Assert.Equal(IDebuggerServicesGenerated.OperatingSystem.Linux, operatingSystem);
                DEBUG_OUTPUT mask = default;
                Assert.Equal(HResult.S_OK, second.DebugClient.GetOutputMask(&mask));
                Assert.Equal(DEBUG_OUTPUT.ERROR, mask);
                Assert.NotNull(second.DebugSymbols);
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(DEBUG_SYMTYPE.PDB, DEBUG_SYMTYPE.PDB, true, SymbolStatus.Loaded, 0, 0, 1)]
        [InlineData(DEBUG_SYMTYPE.EXPORT, DEBUG_SYMTYPE.EXPORT, true, SymbolStatus.ExportOnly, 0, 0, 1)]
        [InlineData(DEBUG_SYMTYPE.NONE, DEBUG_SYMTYPE.PDB, true, SymbolStatus.Loaded, 1, 0, 2)]
        [InlineData(DEBUG_SYMTYPE.DEFERRED, DEBUG_SYMTYPE.NONE, true, SymbolStatus.NotLoaded, 1, 0, 2)]
        [InlineData(DEBUG_SYMTYPE.DEFERRED, DEBUG_SYMTYPE.PDB, false, SymbolStatus.Loaded, 1, 1, 2)]
        [InlineData(DEBUG_SYMTYPE.NONE, DEBUG_SYMTYPE.DEFERRED, false, SymbolStatus.NotLoaded, 1, 1, 2)]
        public void ModuleStatusPreservesReloadFallbackAndCaching(
            DEBUG_SYMTYPE initial, DEBUG_SYMTYPE loaded, bool reloadSucceeds, SymbolStatus expected,
            int reloadCalls, int nameCalls, int parametersCalls)
        {
            using NativeDebugger native = new()
            {
                InitialSymbolType = initial,
                LoadedSymbolType = loaded,
                ReloadResult = reloadSucceeds ? HResult.S_OK : HResult.E_FAIL,
            };
            native.WithServices(debugger =>
            {
                using ModuleServiceFromDebuggerServices modules = CreateModuleService(debugger);
                IModule module = ((IModuleService)modules).GetModuleFromIndex(0);
                IModuleSymbols symbols = module.Services.GetService<IModuleSymbols>();
                Assert.Equal(expected, symbols.GetSymbolStatus());
                Assert.Equal(expected, symbols.GetSymbolStatus());
                Assert.Equal(reloadCalls, native.ReloadCalls);
                Assert.Equal(nameCalls, native.NameCalls);
                Assert.Equal(parametersCalls, native.ParametersCalls);
                if (reloadCalls != 0)
                {
                    Assert.Equal("module_name.dll", native.ReloadName);
                }
            });
            native.AssertBaselineReferences();
        }

        [Fact]
        public void MissingModuleNameDoesNotForceReload()
        {
            using NativeDebugger native = new() { ModuleFileName = string.Empty, InitialSymbolType = DEBUG_SYMTYPE.NONE };
            native.WithServices(debugger =>
            {
                using ModuleServiceFromDebuggerServices modules = CreateModuleService(debugger);
                IModule module = ((IModuleService)modules).GetModuleFromIndex(0);
                Assert.Equal(SymbolStatus.NotLoaded, module.Services.GetService<IModuleSymbols>().GetSymbolStatus());
                Assert.Equal(0, native.ReloadCalls);
                Assert.Equal(0, native.NameCalls);
                Assert.Equal(2, native.ParametersCalls);
            });
            native.AssertBaselineReferences();
        }

        [Fact]
        public void ModuleParameterFailuresPreserveNotLoadedStatus()
        {
            using NativeDebugger native = new() { ParametersResult = HResult.E_FAIL };
            native.WithServices(debugger =>
            {
                using ModuleServiceFromDebuggerServices modules = CreateModuleService(debugger);
                IModule module = ((IModuleService)modules).GetModuleFromIndex(0);
                Assert.Equal(SymbolStatus.NotLoaded, module.Services.GetService<IModuleSymbols>().GetSymbolStatus());
                Assert.Equal(1, native.ReloadCalls);
                Assert.Equal(2, native.ParametersCalls);
            });
            native.AssertBaselineReferences();
        }

        private static ModuleServiceFromDebuggerServices CreateModuleService(DebuggerServices debugger)
        {
            TestServices services = new();
            ServiceManager manager = new();
            manager.FinalizeServices();
            services.Container.AddService<IServiceManager>(manager);
            return new ModuleServiceFromDebuggerServices(services.Container, debugger);
        }

        private sealed class NativeDebugger : IDisposable
        {
            internal const ulong ModuleBase = 0x12340000;
            private static readonly Guid s_unknownId = new("00000000-0000-0000-C000-000000000046");

            internal struct Instance
            {
                internal IntPtr* Vtable;
                internal int References;
                internal IntPtr Context;
                internal bool IsServices;
            }

            private readonly Instance* _services;
            internal readonly Instance* Client;
            private readonly Instance* _symbols;
            private readonly Instance* _memory;
            private readonly Instance* _stack;
            private readonly GCHandle _handle;
            private int _liveInstances = 5;
            internal bool ClientAvailable { get; set; } = true;
            internal bool SymbolsAvailable { get; set; } = true;
            internal bool ServicesAvailable { get; set; } = true;
            internal bool NativeClientAvailable { get; set; } = true;
            internal int ClientQueryResult { get; set; } = HResult.S_OK;
            internal int SymbolsQueryResult { get; set; } = HResult.S_OK;
            internal int UnknownQueryResult { get; set; } = HResult.S_OK;
            internal int OptionsResult { get; set; } = HResult.S_OK;
            internal int ReloadResult { get; set; } = HResult.S_OK;
            internal int ParametersResult { get; set; } = HResult.S_OK;
            internal DEBUG_SYMTYPE InitialSymbolType { get; set; } = DEBUG_SYMTYPE.PDB;
            internal DEBUG_SYMTYPE LoadedSymbolType { get; set; } = DEBUG_SYMTYPE.PDB;
            internal string ModuleFileName { get; set; } = "C:\\runtime\\module+name.dll";
            internal SYMOPT AddedOptions { get; private set; }
            internal int OptionsCalls { get; private set; }
            internal int ReloadCalls { get; private set; }
            internal int NameCalls { get; private set; }
            internal int ParametersCalls { get; private set; }
            internal string ReloadName { get; private set; }
            internal int TebResult { get; set; } = HResult.S_OK;
            internal int VerificationResult { get; set; } = HResult.S_OK;
            internal byte[] WrittenMemory { get; private set; }
            internal DEBUG_OUTPUT OutputMask { get; private set; }
            internal string OutputText { get; private set; }
            internal int FlushCalls { get; private set; }
            internal int NativeClientQueries { get; private set; }
            internal bool MemoryAvailable { get; set; } = true;
            internal bool StackAvailable { get; set; } = true;
            internal int MemoryResult { get; set; } = HResult.S_OK;
            internal int StackResult { get; set; } = HResult.S_OK;
            internal uint FramesFilled { get; set; } = 2;
            internal ulong MemoryAddress { get; private set; }
            internal uint MemorySize { get; private set; }
            internal uint MemoryTypeFlags { get; private set; }
            internal uint MemoryProtectionFlags { get; private set; }
            internal uint StackThreadId { get; private set; }
            internal uint StackCapacity { get; private set; }
            internal int StackCalls { get; private set; }

            internal NativeDebugger()
            {
                _handle = GCHandle.Alloc(this);
                _services = Allocate(37);
                _services->IsServices = true;
                Client = Allocate(95);
                _symbols = Allocate(135);
                _memory = Allocate(5);
                _memory->IsServices = true;
                _stack = Allocate(4);
                _stack->IsServices = true;
                _memory->Vtable[3] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, ulong, uint, uint, uint, ulong*, int>)&AllocVirtual;
                _memory->Vtable[4] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, ulong, uint, uint, int>)&FreeVirtual;
                _stack->Vtable[3] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint, DebuggerStackFrame*, uint, uint*, int>)&GetDebuggerStackTrace;
                _services->Vtable[3] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, int*, int>)&GetOperatingSystem;
                _services->Vtable[7] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, DEBUG_OUTPUT, byte*, void>)&OutputString;
                _services->Vtable[8] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, ulong, byte*, uint, int*, int>)&ReadVirtual;
                _services->Vtable[9] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, ulong, byte*, uint, int*, int>)&WriteVirtual;
                _services->Vtable[10] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint*, uint*, int>)&GetNumberModules;
                _services->Vtable[12] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint, ulong, byte*, uint, uint*, byte*, uint, uint*, byte*, uint, uint*, int>)&GetModuleNames;
                _services->Vtable[13] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint, ulong*, ulong*, uint*, uint*, int>)&GetModuleInfo;
                _services->Vtable[18] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint, uint, uint, byte*, int>)&GetThreadContext;
                _services->Vtable[22] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint, ulong*, int>)&GetThreadTeb;
                _services->Vtable[23] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint, uint, byte*, int>)&VirtualUnwind;
                _services->Vtable[24] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, byte*, uint, uint*, int>)&GetSymbolPath;
                _services->Vtable[29] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint>)&GetOutputWidth;
                _services->Vtable[34] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, void>)&FlushCheck;
                _services->Vtable[36] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, int*, int>)&GetDacSignatureVerificationSettings;
                Client->Vtable[35] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, DEBUG_OUTPUT*, int>)&GetOutputMask;
                _symbols->Vtable[4] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, SYMOPT, int>)&AddSymbolOptions;
                _symbols->Vtable[7] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, ulong, byte*, uint, uint*, ulong*, int>)&GetNameByOffset;
                _symbols->Vtable[17] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint, ulong*, uint, DEBUG_MODULE_PARAMETERS*, int>)&GetModuleParameters;
                _symbols->Vtable[39] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, byte*, int>)&Reload;
            }

            private Instance* Allocate(int slots)
            {
                Instance* instance = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
                instance->Vtable = (IntPtr*)NativeMemory.AllocZeroed((nuint)(slots * sizeof(IntPtr)));
                instance->References = 1;
                instance->Context = GCHandle.ToIntPtr(_handle);
                instance->Vtable[0] = (IntPtr)(delegate* unmanaged[Stdcall]<Instance*, Guid*, void**, int>)&Query;
                instance->Vtable[1] = (IntPtr)(delegate* unmanaged[Stdcall]<Instance*, uint>)&Retain;
                instance->Vtable[2] = (IntPtr)(delegate* unmanaged[Stdcall]<Instance*, uint>)&Drop;
                return instance;
            }

            internal DebuggerServices CreateServices(HostType hostType = HostType.DbgEng)
            {
                AddRef((IntPtr)_services);
                return new DebuggerServices((IntPtr)_services, hostType);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            internal void WithServices(Action<DebuggerServices> action, HostType hostType = HostType.DbgEng)
            {
                DebuggerServices debugger = CreateServices(hostType);
                action(debugger);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private (RemoteMemoryService Memory, ThreadStackTraceService Stack, WeakReference Debugger) CreateOptionalServices()
            {
                DebuggerServices debugger = CreateServices();
                return (new RemoteMemoryService(debugger.RemoteMemoryService),
                    new ThreadStackTraceService(debugger.ThreadStackTraceService), new WeakReference(debugger));
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            internal void WithOptionalServices(Action<RemoteMemoryService, ThreadStackTraceService, WeakReference> action)
            {
                (RemoteMemoryService memory, ThreadStackTraceService stack, WeakReference debugger) = CreateOptionalServices();
                action(memory, stack, debugger);
            }

            internal void AssertBaselineReferences()
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Assert.Equal(1, _services->References);
                Assert.Equal(1, Client->References);
                Assert.Equal(1, _symbols->References);
                Assert.Equal(1, _memory->References);
                Assert.Equal(1, _stack->References);
            }

            public void Dispose()
            {
                Release((IntPtr)_services);
                Release((IntPtr)Client);
                Release((IntPtr)_symbols);
                Release((IntPtr)_memory);
                Release((IntPtr)_stack);
            }

            private static NativeDebugger Context(Instance* self) => (NativeDebugger)GCHandle.FromIntPtr(self->Context).Target;

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
            private static int Query(Instance* self, Guid* iid, void** result)
            {
                NativeDebugger native = Context(self);
                Instance* instance = null;
                *result = null;
                if (*iid == s_unknownId)
                {
                    if (!self->IsServices && native.UnknownQueryResult != HResult.S_OK)
                        return native.UnknownQueryResult;
                    instance = self->IsServices ? native._services : native.Client;
                }
                else if (*iid == typeof(IDebuggerServicesGenerated).GUID && self->IsServices && native.ServicesAvailable)
                {
                    instance = native._services;
                }
                else if (*iid == typeof(IRemoteMemoryServiceGenerated).GUID && self->IsServices && native.MemoryAvailable)
                {
                    instance = native._memory;
                }
                else if (*iid == typeof(IDebuggerThreadStackTraceServiceGenerated).GUID && self->IsServices && native.StackAvailable)
                {
                    instance = native._stack;
                }
                else if (*iid == typeof(IDebugClientGenerated).GUID || *iid == LLDBServices.IID_ILLDBServices)
                {
                    native.NativeClientQueries++;
                    if (native.NativeClientAvailable)
                        instance = native.Client;
                }
                else if (*iid == typeof(IDebugClient2Generated).GUID || *iid == typeof(IDebugClient3Generated).GUID
                    || *iid == typeof(IDebugClient4Generated).GUID || *iid == typeof(IDebugClient5Generated).GUID)
                {
                    if (native.ClientQueryResult != HResult.S_OK)
                        return native.ClientQueryResult;
                    if (native.ClientAvailable)
                        instance = native.Client;
                }
                else if (*iid == typeof(IDebugSymbolsGenerated).GUID || *iid == typeof(IDebugSymbols2Generated).GUID
                    || *iid == typeof(IDebugSymbols3Generated).GUID || *iid == typeof(IDebugSymbols4Generated).GUID
                    || *iid == typeof(IDebugSymbols5Generated).GUID)
                {
                    if (native.SymbolsQueryResult != HResult.S_OK)
                        return native.SymbolsQueryResult;
                    if (native.SymbolsAvailable)
                        instance = native._symbols;
                }
                if (instance == null)
                    return HResult.E_NOINTERFACE;
                instance->References++;
                *result = instance;
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
            private static uint Retain(Instance* self) => (uint)++self->References;

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
            private static uint Drop(Instance* self)
            {
                uint references = (uint)--self->References;
                if (references == 0)
                {
                    NativeDebugger native = Context(self);
                    NativeMemory.Free(self->Vtable);
                    NativeMemory.Free(self);
                    if (System.Threading.Interlocked.Decrement(ref native._liveInstances) == 0)
                    {
                        native._handle.Free();
                    }
                }
                return references;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int AllocVirtual(Instance* self, ulong address, uint size, uint typeFlags, uint protectionFlags, ulong* remoteAddress)
            {
                NativeDebugger native = Context(self);
                native.MemoryAddress = address;
                native.MemorySize = size;
                native.MemoryTypeFlags = typeFlags;
                native.MemoryProtectionFlags = protectionFlags;
                *remoteAddress = 0xFEDCBA9800002000;
                return native.MemoryResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int FreeVirtual(Instance* self, ulong address, uint size, uint typeFlags)
            {
                NativeDebugger native = Context(self);
                native.MemoryAddress = address;
                native.MemorySize = size;
                native.MemoryTypeFlags = typeFlags;
                return native.MemoryResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetDebuggerStackTrace(Instance* self, uint threadId, DebuggerStackFrame* frames, uint capacity, uint* framesFilled)
            {
                NativeDebugger native = Context(self);
                native.StackCalls++;
                native.StackThreadId = threadId;
                native.StackCapacity = capacity;
                *framesFilled = native.FramesFilled;
                ulong* values = (ulong*)frames;
                for (uint index = 0; index < Math.Min(capacity, native.FramesFilled); index++)
                {
                    values[index * 2] = 0xFEDCBA9800000000 + index;
                    values[index * 2 + 1] = index == 0 ? 0x1234567800000000ul : 0;
                }
                return native.StackResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetOperatingSystem(Instance* self, int* operatingSystem)
            {
                *operatingSystem = (int)IDebuggerServicesGenerated.OperatingSystem.Linux;
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static void OutputString(Instance* self, DEBUG_OUTPUT mask, byte* message)
            {
                NativeDebugger native = Context(self);
                native.OutputMask = mask;
                native.OutputText = Marshal.PtrToStringAnsi((IntPtr)message);
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int ReadVirtual(Instance* self, ulong offset, byte* buffer, uint size, int* bytesRead)
            {
                *bytesRead = 0;
                if (offset != ModuleBase || size != 4)
                    return HResult.E_INVALIDARG;
                new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }.CopyTo(new Span<byte>(buffer, (int)size));
                *bytesRead = 4;
                return HResult.S_FALSE;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int WriteVirtual(Instance* self, ulong offset, byte* buffer, uint size, int* bytesWritten)
            {
                *bytesWritten = 0;
                if (offset != ModuleBase || size != 4)
                    return HResult.E_INVALIDARG;
                Context(self).WrittenMemory = new Span<byte>(buffer, (int)size).ToArray();
                *bytesWritten = 4;
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetThreadContext(Instance* self, uint threadId, uint flags, uint size, byte* context)
            {
                if (threadId != 42 || flags != 0x1234 || size != 4)
                    return HResult.E_INVALIDARG;
                new byte[] { 1, 2, 3, 4 }.CopyTo(new Span<byte>(context, (int)size));
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetThreadTeb(Instance* self, uint threadId, ulong* teb)
            {
                if (threadId != 42)
                    return HResult.E_INVALIDARG;
                NativeDebugger native = Context(self);
                if (native.TebResult >= 0)
                    *teb = 0x123456789ABCDEF0;
                return native.TebResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int VirtualUnwind(Instance* self, uint threadId, uint size, byte* context)
            {
                if (threadId != 42 || size != 4)
                    return HResult.E_INVALIDARG;
                new Span<byte>(context, (int)size).Reverse();
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetSymbolPath(Instance* self, byte* buffer, uint size, uint* pathSize)
            {
                byte[] path = Encoding.ASCII.GetBytes("C:\\symbols\0");
                *pathSize = (uint)path.Length;
                if (buffer != null)
                {
                    if (size < path.Length)
                        return HResult.E_INVALIDARG;
                    path.CopyTo(new Span<byte>(buffer, (int)size));
                }
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static uint GetOutputWidth(Instance* self) => 132;

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static void FlushCheck(Instance* self) => Context(self).FlushCalls++;

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetDacSignatureVerificationSettings(Instance* self, int* enabled)
            {
                *enabled = 1;
                return Context(self).VerificationResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetOutputMask(Instance* self, DEBUG_OUTPUT* mask)
            {
                *mask = DEBUG_OUTPUT.ERROR;
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int AddSymbolOptions(Instance* self, SYMOPT options)
            {
                NativeDebugger native = Context(self);
                native.OptionsCalls++;
                native.AddedOptions = options;
                return native.OptionsResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetNumberModules(Instance* self, uint* loaded, uint* unloaded)
            {
                *loaded = 1;
                *unloaded = 0;
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetModuleNames(Instance* self, uint index, ulong imageBase, byte* imageName, uint imageNameSize,
                uint* imageNameActual, byte* moduleName, uint moduleNameSize, uint* moduleNameActual,
                byte* loadedName, uint loadedNameSize, uint* loadedNameActual)
            {
                byte[] name = Encoding.ASCII.GetBytes(Context(self).ModuleFileName + "\0");
                *imageNameActual = (uint)name.Length;
                if (imageNameSize < name.Length)
                    return HResult.E_INVALIDARG;
                name.CopyTo(new Span<byte>(imageName, (int)imageNameSize));
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetModuleInfo(Instance* self, uint index, ulong* imageBase, ulong* imageSize, uint* timestamp, uint* checksum)
            {
                *imageBase = ModuleBase;
                *imageSize = 0x10000;
                *timestamp = 123;
                *checksum = 0;
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetModuleParameters(Instance* self, uint count, ulong* bases, uint start, DEBUG_MODULE_PARAMETERS* parameters)
            {
                NativeDebugger native = Context(self);
                native.ParametersCalls++;
                if (count != 1 || bases == null || *bases != ModuleBase || start != 0 || parameters == null)
                    return HResult.E_INVALIDARG;
                *parameters = default;
                bool loaded = native.ReloadResult >= 0 ? native.ReloadCalls != 0 : native.NameCalls != 0;
                parameters->SymbolType = loaded ? native.LoadedSymbolType : native.InitialSymbolType;
                return native.ParametersResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int Reload(Instance* self, byte* module)
            {
                NativeDebugger native = Context(self);
                native.ReloadCalls++;
                native.ReloadName = Marshal.PtrToStringAnsi((IntPtr)module);
                return native.ReloadResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetNameByOffset(Instance* self, ulong offset, byte* buffer, uint bufferSize, uint* nameSize, ulong* displacement)
            {
                NativeDebugger native = Context(self);
                native.NameCalls++;
                if (offset != ModuleBase || buffer != null || bufferSize != 0)
                    return HResult.E_INVALIDARG;
                *nameSize = 0;
                *displacement = 0;
                return HResult.S_OK;
            }
        }
    }
}
