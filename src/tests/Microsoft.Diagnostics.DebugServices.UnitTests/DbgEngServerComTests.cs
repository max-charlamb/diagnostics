// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop.DbgEng;
using Xunit;
using static Microsoft.Diagnostics.DebugServices.UnitTests.DataTargetComTests;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe class DbgEngServerComTests
    {
        [Fact]
        public void NativeStructuresHaveTheSdkBufferSizes()
        {
            Assert.Equal(128, sizeof(DEBUG_STACK_FRAME));
            Assert.Equal(64, sizeof(DEBUG_MODULE_PARAMETERS));
            Assert.Equal(32, sizeof(DEBUG_VALUE));
            Assert.Equal(48, sizeof(MEMORY_BASIC_INFORMATION64));
        }

        [Fact]
        public void AllNativeInterfacesShareIdentityAndPreserveTheirFinalSlots()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            using TestHost host = new();
            Assert.Equal(2u, AddRef(host.Pointer));
            Assert.Equal(1u, ReleaseReference(host.Pointer));
            IntPtr identity = QueryInterface(host.Pointer, new Guid("00000000-0000-0000-C000-000000000046"));
            try
            {
                (Guid Iid, int Slots)[] interfaces =
                [
                    (typeof(IDebugClientGenerated).GUID, 48),
                    (typeof(IDebugAdvancedGenerated).GUID, 5),
                    (typeof(IDebugControlGenerated).GUID, 95),
                    (typeof(IDebugControl2Generated).GUID, 103),
                    (typeof(IDebugDataSpacesGenerated).GUID, 23),
                    (typeof(IDebugDataSpaces2Generated).GUID, 29),
                    (typeof(IDebugRegistersGenerated).GUID, 14),
                    (typeof(IDebugSymbolsGenerated).GUID, 52),
                    (typeof(IDebugSymbols2Generated).GUID, 60),
                    (typeof(IDebugSymbols3Generated).GUID, 126),
                    (typeof(IDebugSystemObjectsGenerated).GUID, 32)
                ];
                foreach ((Guid iid, int slots) in interfaces)
                {
                    IntPtr pointer = QueryInterface(host.Pointer, iid);
                    try
                    {
                        ScopedComTests.AssertInterface(pointer, iid, slots);
                        IntPtr queriedIdentity = QueryInterface(pointer, new Guid("00000000-0000-0000-C000-000000000046"));
                        try
                        {
                            Assert.Equal(identity, queriedIdentity);
                        }
                        finally
                        {
                            ReleaseReference(queriedIdentity);
                        }
                    }
                    finally
                    {
                        ReleaseReference(pointer);
                    }
                }
            }
            finally
            {
                ReleaseReference(identity);
            }
            Assert.Equal(2u, AddRef(host.Pointer));
            Assert.Equal(1u, ReleaseReference(host.Pointer));
        }

        [Fact]
        public void RawSlotsForwardContextMemoryControlAndFinalRegisterMethods()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            using TestHost host = new();
            IntPtr advanced = QueryInterface(host.Pointer, typeof(IDebugAdvancedGenerated).GUID);
            IntPtr data = QueryInterface(host.Pointer, typeof(IDebugDataSpaces2Generated).GUID);
            IntPtr control = QueryInterface(host.Pointer, typeof(IDebugControl2Generated).GUID);
            IntPtr registers = QueryInterface(host.Pointer, typeof(IDebugRegistersGenerated).GUID);
            IntPtr system = QueryInterface(host.Pointer, typeof(IDebugSystemObjectsGenerated).GUID);
            try
            {
                byte* bytes = stackalloc byte[8];
                new Span<byte>(bytes, 8).Fill(0xCC);
                delegate* unmanaged<IntPtr, IntPtr, uint, int> getContext =
                    (delegate* unmanaged<IntPtr, IntPtr, uint, int>)VTable(advanced)[3];
                Assert.Equal(HResult.S_OK, getContext(advanced, (IntPtr)bytes, 4));
                Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xCC, 0xCC, 0xCC, 0xCC }, new ReadOnlySpan<byte>(bytes, 8).ToArray());

                delegate* unmanaged<IntPtr, ulong, IntPtr, uint, uint*, int> read =
                    (delegate* unmanaged<IntPtr, ulong, IntPtr, uint, uint*, int>)VTable(data)[3];
                delegate* unmanaged<IntPtr, ulong, byte*, uint, uint*, int> write =
                    (delegate* unmanaged<IntPtr, ulong, byte*, uint, uint*, int>)VTable(data)[4];
                uint transferred = uint.MaxValue;
                Assert.Equal(HResult.S_OK, read(data, 0xFEDCBA9800001234, (IntPtr)bytes, 8, &transferred));
                Assert.Equal(4u, transferred);
                Assert.Equal(0xFEDCBA9800001234ul, host.Services.LastAddress);
                Assert.Equal(0xCC, bytes[4]);
                Assert.Equal(HResult.S_OK, write(data, 0xFEDCBA9800005678, bytes, 4, &transferred));
                Assert.Equal(4u, transferred);
                Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, host.Services.WrittenBytes);
                host.Services.MemoryAvailable = false;
                transferred = uint.MaxValue;
                Assert.Equal(HResult.E_FAIL, read(data, 0, (IntPtr)bytes, 4, &transferred));
                Assert.Equal(uint.MaxValue, transferred);

                delegate* unmanaged<IntPtr, uint, byte*, uint, uint*, int> readDebuggerData =
                    (delegate* unmanaged<IntPtr, uint, byte*, uint, uint*, int>)VTable(data)[21];
                transferred = uint.MaxValue;
                Assert.Equal(HResult.E_NOTIMPL, readDebuggerData(data, 0, bytes, 8, &transferred));
                Assert.Equal(uint.MaxValue, transferred);

                delegate* unmanaged<IntPtr, int> getInterrupt = (delegate* unmanaged<IntPtr, int>)VTable(control)[3];
                Assert.Equal(HResult.E_FAIL, getInterrupt(control));
                using CancellationTokenSource cancellation = new();
                cancellation.Cancel();
                host.Console.CancellationToken = cancellation.Token;
                Assert.Equal(HResult.S_OK, getInterrupt(control));
                uint pageSize = 0;
                Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, uint*, int>)VTable(control)[41])(control, &pageSize));
                Assert.Equal(4096u, pageSize);
                DEBUG_FORMAT format = (DEBUG_FORMAT)uint.MaxValue;
                Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, DEBUG_FORMAT*, int>)VTable(control)[97])(control, &format));
                Assert.Equal(DEBUG_FORMAT.DEFAULT, format);

                byte[] registerName = System.Text.Encoding.ASCII.GetBytes("fp\0");
                uint registerIndex = uint.MaxValue;
                fixed (byte* name = registerName)
                {
                    Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, byte*, uint*, int>)VTable(registers)[5])(registers, name, &registerIndex));
                }
                Assert.Equal(2u, registerIndex);
                DEBUG_VALUE value = default;
                Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, uint, DEBUG_VALUE*, int>)VTable(registers)[6])(registers, registerIndex, &value));
                Assert.Equal(DEBUG_VALUE_TYPE.INT64, value.Type);
                Assert.Equal(TestThreads.FrameOffset, value.I64);
                ulong frameOffset = 0;
                Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, ulong*, int>)VTable(registers)[13])(registers, &frameOffset));
                Assert.Equal(TestThreads.FrameOffset, frameOffset);
                uint processId = 0;
                Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, uint*, int>)VTable(system)[27])(system, &processId));
                Assert.Equal(42u, processId);
                Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, ulong*, int>)VTable(system)[15])(system, null));
            }
            finally
            {
                ReleaseReference(system);
                ReleaseReference(registers);
                ReleaseReference(control);
                ReleaseReference(data);
                ReleaseReference(advanced);
            }
        }

        [Fact]
        public void NativeSymbolBuffersPreserveBoundsRequiredSizesAndWideInput()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            using TestHost host = new();
            IntPtr symbols = QueryInterface(host.Pointer, typeof(IDebugSymbols3Generated).GUID);
            try
            {
                uint index = uint.MaxValue;
                ulong moduleBase = 0;
                fixed (char* name = "coreclr")
                {
                    Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, char*, uint, uint*, ulong*, int>)VTable(symbols)[65])(
                        symbols, name, 0, &index, &moduleBase));
                }
                Assert.Equal(0u, index);
                Assert.Equal(host.Services.ImageBase, moduleBase);

                byte* image = stackalloc byte[6];
                byte* module = stackalloc byte[2];
                byte* loaded = stackalloc byte[2];
                new Span<byte>(image, 6).Fill(0xCC);
                new Span<byte>(module, 2).Fill(0xCC);
                new Span<byte>(loaded, 2).Fill(0xCC);
                uint imageSize = 0;
                uint moduleSize = 0;
                uint loadedSize = uint.MaxValue;
                delegate* unmanaged<IntPtr, uint, ulong, byte*, uint, uint*, byte*, uint, uint*, byte*, uint, uint*, int> getNames =
                    (delegate* unmanaged<IntPtr, uint, ulong, byte*, uint, uint*, byte*, uint, uint*, byte*, uint, uint*, int>)VTable(symbols)[16];
                Assert.Equal(HResult.S_OK, getNames(symbols, index, moduleBase, image, 5, &imageSize, module, 1, &moduleSize, loaded, 1, &loadedSize));
                Assert.Equal(host.Module.FileName.Substring(0, 4), Marshal.PtrToStringAnsi((IntPtr)image));
                Assert.Equal((uint)host.Module.FileName.Length + 1, imageSize);
                Assert.Equal(8u, moduleSize);
                Assert.Equal(0u, loadedSize);
                Assert.Equal(0, module[0]);
                Assert.Equal(0, loaded[0]);
                Assert.Equal(0xCC, image[5]);
                Assert.Equal(0xCC, module[1]);
                Assert.Equal(0xCC, loaded[1]);
                Assert.Equal(HResult.S_OK, getNames(symbols, index, moduleBase, null, 0, &imageSize, null, 0, &moduleSize, null, 0, &loadedSize));
                Assert.Equal((uint)host.Module.FileName.Length + 1, imageSize);
                Assert.Equal(8u, moduleSize);

                byte* fullImage = stackalloc byte[64];
                IntPtr expectedImage = Marshal.StringToCoTaskMemAnsi(host.Module.FileName);
                try
                {
                    Assert.Equal(HResult.S_OK, getNames(symbols, index, moduleBase, fullImage, 64, &imageSize, null, 0, &moduleSize, null, 0, &loadedSize));
                    Assert.Equal(Marshal.PtrToStringAnsi(expectedImage), Marshal.PtrToStringAnsi((IntPtr)fullImage));
                }
                finally
                {
                    Marshal.FreeCoTaskMem(expectedImage);
                }

                char* wide = stackalloc char[3];
                wide[0] = 'x';
                wide[1] = 'y';
                wide[2] = 'z';
                uint needed = uint.MaxValue;
                ulong displacement = ulong.MaxValue;
                Assert.Equal(HResult.E_NOTIMPL, ((delegate* unmanaged<IntPtr, ulong, char*, uint, uint*, ulong*, int>)VTable(symbols)[60])(
                    symbols, ulong.MaxValue, wide, 2, &needed, &displacement));
                Assert.Equal('\0', wide[0]);
                Assert.Equal('y', wide[1]);
                Assert.Equal('z', wide[2]);
                Assert.Equal(0u, needed);
                Assert.Equal(0ul, displacement);
                needed = uint.MaxValue;
                Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, char*, uint, uint*, int>)VTable(symbols)[76])(symbols, null, 0, &needed));
                Assert.Equal(0u, needed);
            }
            finally
            {
                ReleaseReference(symbols);
            }
        }

        [Fact]
        public void MemoryTransfersAcceptNullCounts()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            using TestHost host = new();
            IntPtr data = QueryInterface(host.Pointer, typeof(IDebugDataSpacesGenerated).GUID);
            try
            {
                byte* buffer = stackalloc byte[4];
                Assert.Equal(HResult.S_OK,
                    ((delegate* unmanaged<IntPtr, ulong, IntPtr, uint, uint*, int>)VTable(data)[3])(
                        data, 0xDEADBEEF, (IntPtr)buffer, 4, null));
                Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, new ReadOnlySpan<byte>(buffer, 4).ToArray());
                Assert.Equal(HResult.S_OK,
                    ((delegate* unmanaged<IntPtr, ulong, byte*, uint, uint*, int>)VTable(data)[4])(
                        data, 0xDEADBEEF, buffer, 4, null));
                Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, host.Services.WrittenBytes);
                Assert.Equal(HResult.E_NOTIMPL,
                    ((delegate* unmanaged<IntPtr, uint, byte*, uint, uint*, int>)VTable(data)[21])(
                        data, 0, null, 0, null));
            }
            finally
            {
                ReleaseReference(data);
            }
        }

        [Fact]
        public void ModuleAndSymbolLookupsAcceptNullOutputs()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            using TestHost host = new();
            IntPtr symbols = QueryInterface(host.Pointer, typeof(IDebugSymbols3Generated).GUID);
            try
            {
                IntPtr* table = VTable(symbols);
                ulong moduleBase = 0;
                byte* name = stackalloc byte[] { (byte)'c', (byte)'o', (byte)'r', (byte)'e', (byte)'c', (byte)'l', (byte)'r', 0 };
                delegate* unmanaged<IntPtr, byte*, uint, uint*, ulong*, int> byName =
                    (delegate* unmanaged<IntPtr, byte*, uint, uint*, ulong*, int>)table[14];
                Assert.Equal(HResult.S_OK, byName(symbols, name, 0, null, &moduleBase));
                Assert.Equal(host.Module.ImageBase, moduleBase);
                Assert.Equal(HResult.S_OK, byName(symbols, name, 0, null, null));
                Assert.Equal(HResult.S_OK,
                    ((delegate* unmanaged<IntPtr, ulong, uint, uint*, ulong*, int>)table[15])(
                        symbols, moduleBase, 0, null, null));
                fixed (char* wideName = "coreclr")
                {
                    Assert.Equal(HResult.S_OK,
                        ((delegate* unmanaged<IntPtr, char*, uint, uint*, ulong*, int>)table[65])(
                            symbols, wideName, 0, null, null));
                }
                byte* module = stackalloc byte[16];
                Assert.Equal(HResult.S_OK,
                    ((delegate* unmanaged<IntPtr, uint, ulong, byte*, uint, uint*, byte*, uint, uint*, byte*, uint, uint*, int>)table[16])(
                        symbols, 0, moduleBase, null, 0, null, module, 16, null, null, 0, null));
                Assert.Equal("coreclr", Marshal.PtrToStringAnsi((IntPtr)module));
                Assert.Equal(HResult.E_NOTIMPL,
                    ((delegate* unmanaged<IntPtr, ulong, byte*, uint, uint*, ulong*, int>)table[7])(
                        symbols, moduleBase, null, 0, null, null));
                Assert.Equal(HResult.E_NOTIMPL,
                    ((delegate* unmanaged<IntPtr, ulong, char*, uint, uint*, ulong*, int>)table[60])(
                        symbols, moduleBase, null, 0, null, null));
                Assert.Equal(HResult.S_OK,
                    ((delegate* unmanaged<IntPtr, byte*, uint, uint*, int>)table[40])(symbols, null, 0, null));
                Assert.Equal(HResult.S_OK,
                    ((delegate* unmanaged<IntPtr, char*, uint, uint*, int>)table[76])(symbols, null, 0, null));
            }
            finally
            {
                ReleaseReference(symbols);
            }
        }

        [Fact]
        public void VersionInformationAcceptsNullSizeAndPreservesUnwrittenOutputs()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            using TestHost host = new();
            IntPtr symbols = QueryInterface(host.Pointer, typeof(IDebugSymbols2Generated).GUID);
            try
            {
                delegate* unmanaged<IntPtr, uint, ulong, byte*, byte*, uint, uint*, int> getVersion =
                    (delegate* unmanaged<IntPtr, uint, ulong, byte*, byte*, uint, uint*, int>)VTable(symbols)[52];
                byte* buffer = stackalloc byte[128];
                byte* fixedInfo = stackalloc byte[] { (byte)'\\', 0 };
                Assert.Equal(HResult.S_OK, getVersion(symbols, 0, 0, fixedInfo, buffer, 128, null));
                uint size = 0xDEADBEEF;
                Assert.Equal(HResult.E_INVALIDARG, getVersion(symbols, 0, 0, null, buffer, 128, &size));
                Assert.Equal(0xDEADBEEFu, size);
                IntPtr item = Marshal.StringToCoTaskMemAnsi("\\StringFileInfo\\040904B0\\FileVersion");
                try
                {
                    Assert.Equal(HResult.S_OK, getVersion(symbols, 0, 0, (byte*)item, buffer, 128, &size));
                    Assert.Equal(host.Module.GetVersionString(), Marshal.PtrToStringAnsi((IntPtr)buffer));
                    Assert.Equal(0xDEADBEEFu, size);
                    Assert.Equal(HResult.S_OK, getVersion(symbols, 0, 0, (byte*)item, buffer, 128, null));
                }
                finally
                {
                    Marshal.FreeCoTaskMem(item);
                }
            }
            finally
            {
                ReleaseReference(symbols);
            }
        }

        [Fact]
        public void HostReleasesExactlyTheOwnedClientReference()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            using TestHost host = new();
            IntPtr retained = QueryInterface(host.Pointer, typeof(IDebugRegistersGenerated).GUID);
            try
            {
                Assert.Equal(3u, AddRef(retained));
                Assert.Equal(2u, ReleaseReference(retained));
                host.Dispose();
                host.Dispose();
                Assert.Throws<ObjectDisposedException>(() => host.Host.ExecuteCommand("clrstack", string.Empty));
                Assert.Throws<ObjectDisposedException>(() => host.Host.GetHelpText("clrstack"));
                Assert.Equal(2u, AddRef(retained));
                Assert.Equal(1u, ReleaseReference(retained));
                ulong frameOffset = 0;
                Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, ulong*, int>)VTable(retained)[13])(retained, &frameOffset));
                Assert.Equal(TestThreads.FrameOffset, frameOffset);
            }
            finally
            {
                Assert.Equal(0u, ReleaseReference(retained));
            }
        }

        private sealed class TestHost : IDisposable
        {
            private readonly SOSHost _host;

            internal TestDataServices Services { get; } = new();
            internal IModule Module { get; } = new TestModule();
            internal TestConsole Console { get; } = new();
            internal IntPtr Pointer { get; }
            internal SOSHost Host => _host;

            internal TestHost()
            {
                TestThreads threads = new(Services);
                Services.Container.RemoveService(typeof(IThread));
                Services.Container.AddService<IThread>(threads);
                _host = new SOSHost(Services, Services, null);
                ContextService(_host) = Services;
                ModuleService(_host) = new TestModules(Module);
                ThreadService(_host) = threads;
                ConsoleService(_host) = Console;
                Pointer = NativeClient(_host);
            }

            public void Dispose()
            {
                ((IDisposable)_host).Dispose();
            }
        }

        // Inject only the services used by this fixture, without loading a native SOS library.
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_client")]
        private static extern ref IntPtr NativeClient(SOSHost host);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "ContextService")]
        private static extern ref IContextService ContextService(SOSHost host);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "ModuleService")]
        private static extern ref IModuleService ModuleService(SOSHost host);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "ThreadService")]
        private static extern ref IThreadService ThreadService(SOSHost host);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "ConsoleService")]
        private static extern ref IConsoleService ConsoleService(SOSHost host);

        private sealed class TestConsole : IConsoleService
        {
            public bool SupportsDml => false;
            public int WindowWidth => 80;
            public CancellationToken CancellationToken { get; set; }
            public void WriteString(OutputType type, OutputLevel level, string text) => throw new NotSupportedException();
        }

        private sealed class TestModule : ScopedComTests.TestServices, IModule
        {
            public new string FileName => "native-\u00E9\\coreclr.dll";
        }

        private sealed class TestModules(IModule module) : IModuleService
        {
            public IModule EntryPointModule => module;
            public IEnumerable<IModule> EnumerateModules() => [module];
            public IModule GetModuleFromIndex(int moduleIndex) => moduleIndex == 0 ? module : throw new DiagnosticsException("Unknown module");
            public IModule GetModuleFromBaseAddress(ulong baseAddress) => baseAddress == module.ImageBase ? module : throw new DiagnosticsException("Unknown module");
            public IModule GetModuleFromAddress(ulong address) => GetModuleFromBaseAddress(address);
            public IEnumerable<IModule> GetModuleFromModuleName(string moduleName) => moduleName == "coreclr.dll" ? [module] : [];
            public IModule CreateModule(int moduleIndex, ulong imageBase, ulong imageSize, string imageName) => throw new NotSupportedException();
        }

        private sealed class TestThreads(TestDataServices services) : IThreadService, IThread
        {
            internal const ulong FrameOffset = 0xFEDCBA9876543210;
            public IEnumerable<RegisterInfo> Registers => [];
            public int InstructionPointerIndex => 0;
            public int StackPointerIndex => 1;
            public int FramePointerIndex => 2;
            public bool TryGetRegisterIndexByName(string name, out int registerIndex)
            {
                registerIndex = name == "fp" ? FramePointerIndex : -1;
                return registerIndex >= 0;
            }
            public bool TryGetRegisterInfo(int registerIndex, out RegisterInfo info)
            {
                info = default;
                return false;
            }
            public IEnumerable<IThread> EnumerateThreads() => [this];
            public IThread GetThreadFromIndex(int threadIndex) => threadIndex == 0 ? this : throw new DiagnosticsException("Unknown thread");
            public IThread GetThreadFromId(uint threadId) => threadId == ThreadId ? this : throw new DiagnosticsException("Unknown thread");
            public IServiceProvider Services => services.Container;
            public ITarget Target => services;
            public int ThreadIndex => 0;
            public uint ThreadId => 42;
            public bool TryGetRegisterValue(int registerIndex, out ulong value)
            {
                value = FrameOffset;
                return true;
            }
            public ReadOnlySpan<byte> GetThreadContext() => services.GetThreadContext();
            public ulong GetThreadTeb() => 0;
        }
    }
}
