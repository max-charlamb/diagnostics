// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop;
using SOS.Hosting.Interop.DbgEng;
using Xunit;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe partial class GeneratedContractTests
    {
        [Theory]
        [InlineData(typeof(SOS.Hosting.DbgEng.DebugClient))]
        [InlineData(typeof(LLDBServices))]
        public void DebuggerServersDoNotStoreNativePointers(Type server)
        {
            Assert.DoesNotContain(server.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                field => field.FieldType == typeof(IntPtr));
        }

        [Theory]
        [InlineData(typeof(ICLRDataTargetGenerated), "3E11CCEE-D08B-43E5-AF01-32717A64DA03", 11)]
        [InlineData(typeof(ICLRDataTarget2Generated), "6D05FAE3-189C-4630-A6DC-1C251E1C01AB", 13)]
        [InlineData(typeof(ICLRMetadataLocatorGenerated), "AA8FA804-BC05-4642-B2C5-C353ED22FC63", 1)]
        [InlineData(typeof(ICLRRuntimeLocatorGenerated), "B760BF44-9377-4597-8BE7-58083BDC5146", 1)]
        [InlineData(typeof(ICLRContractLocatorGenerated), "17D5B8C6-34A9-407F-AF4F-A930201D4E02", 1)]
        [InlineData(typeof(ICLRSymbolProviderGenerated), "C4F8B7E2-9D3A-4F6C-B1E5-8A2D7C3F9B1E", 3)]
        [InlineData(typeof(ICorDebugDataTargetGenerated), "FE06DC28-49FB-4636-A4A3-E80DB4AE116C", 3)]
        [InlineData(typeof(ICorDebugDataTarget4Generated), "E799DC06-E099-4713-BDD9-906D3CC02CF2", 1)]
        [InlineData(typeof(ICorDebugMutableDataTargetGenerated), "A1B8A756-3CB6-4CCB-979F-3DF999673A59", 6)]
        [InlineData(typeof(ICorDebugMetaDataLocatorGenerated), "7CEF8BA9-2EF7-42BF-973F-4171474F87D9", 1)]
        [InlineData(typeof(ICLRDebuggingLibraryProvider2Generated), "E04E2FF1-DCFD-45D5-BCD1-16FFF2FAF7BA", 1)]
        [InlineData(typeof(IRemoteMemoryServiceGenerated), "CD6A0F22-8BCF-4297-9366-F440C2D1C781", 2)]
        [InlineData(typeof(IDebuggerThreadStackTraceServiceGenerated), "3F0DEFDA-A8A3-43B2-9209-935147C89B58", 1)]
        [InlineData(typeof(ICLRDebugging), "D28F3C5A-9634-4206-A509-477552EEFB10", 2)]
        [InlineData(typeof(ICLRDebuggingPolicy), "2D3B4F6A-1C7E-4B2A-9E5D-7F1A6C0B8D34", 2)]
        public void NativeContractsPreserveIdentityAndSlots(Type contract, string iid, int methodCount)
        {
            Assert.Equal(new Guid(iid), contract.GUID);
            Assert.NotNull(contract.GetCustomAttribute<GeneratedComInterfaceAttribute>());
            MethodInfo[] methods = GetContractMethods(contract).ToArray();
            Assert.Equal(methodCount, methods.Length);
            Assert.Equal(methodCount, methods.Select(method => method.Name).Distinct().Count());
            Assert.All(methods, method =>
            {
                Assert.Equal(typeof(int), method.ReturnType);
                Assert.NotNull(method.GetCustomAttribute<PreserveSigAttribute>());
            });
        }

        [Theory]
        [InlineData(typeof(IDebugClientGenerated), typeof(IDebugClient), 45)]
        [InlineData(typeof(IDebugClient2Generated), typeof(IDebugClient2), 53)]
        [InlineData(typeof(IDebugClient3Generated), typeof(IDebugClient3), 57)]
        [InlineData(typeof(IDebugClient4Generated), typeof(IDebugClient4), 63)]
        [InlineData(typeof(IDebugClient5Generated), typeof(IDebugClient5), 92)]
        [InlineData(typeof(IDebugClient6Generated), typeof(IDebugClient6), 93)]
        [InlineData(typeof(IDebugSymbolsGenerated), typeof(IDebugSymbols), 49)]
        [InlineData(typeof(IDebugSymbols2Generated), typeof(IDebugSymbols2), 57)]
        [InlineData(typeof(IDebugSymbols3Generated), typeof(IDebugSymbols3), 123)]
        [InlineData(typeof(IDebugSymbols4Generated), typeof(IDebugSymbols4), 130)]
        [InlineData(typeof(IDebugSymbols5Generated), typeof(IDebugSymbols5), 132)]
        public void DbgEngContractsPreserveIdentityAndSlotOrder(Type contract, Type legacyContract, int methodCount)
        {
            Assert.Equal(legacyContract.GUID, contract.GUID);
            Assert.NotNull(contract.GetCustomAttribute<GeneratedComInterfaceAttribute>());
            MethodInfo[] methods = GetContractMethods(contract).ToArray();
            MethodInfo[] legacyMethods = legacyContract.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .OrderBy(method => method.MetadataToken).ToArray();
            Assert.Equal(methodCount, methods.Length);
            Assert.Equal(legacyMethods.Select(method => method.Name == "OutputServer" ? "OutputServers" : method.Name),
                methods.Select(method => method.Name));
            Assert.All(methods, method =>
            {
                Assert.NotNull(method.GetCustomAttribute<PreserveSigAttribute>());
                Assert.Equal(typeof(int), method.ReturnType);
                Assert.Equal(legacyMethods.Single(legacyMethod =>
                    (legacyMethod.Name == "OutputServer" ? "OutputServers" : legacyMethod.Name) == method.Name).GetParameters().Length,
                    method.GetParameters().Length);
            });
        }

        [Fact]
        public void ClrDebuggingContractsPreserveSlotsAndVersionInputOutput()
        {
            Assert.Equal(new[] { "OpenVirtualProcess", "CanUnloadNow" },
                GetContractMethods(typeof(ICLRDebugging)).Select(method => method.Name));
            Assert.Equal(new[] { "SetCDacLoadPolicy", "GetCDacLoadPolicy" },
                GetContractMethods(typeof(ICLRDebuggingPolicy)).Select(method => method.Name));
            ParameterInfo version = typeof(ICLRDebugging).GetMethod("OpenVirtualProcess").GetParameters()[6];
            Assert.Equal(typeof(ClrDebuggingVersion).MakeByRefType(), version.ParameterType);
            Assert.False(version.IsOut);
            Assert.Equal(10, sizeof(ClrDebuggingVersion));
            Assert.Equal(typeof(uint), Enum.GetUnderlyingType(typeof(DbgShimCDacLoadPolicy)));
        }

        [Fact]
        public void LldbContractsPreserveNativeIdentityOrderAndReturnTypes()
        {
            Type contract = typeof(ILLDBServicesGenerated);
            Assert.Equal(new Guid("2E6C569A-9E14-4DA4-9DFC-CDB73A532566"), contract.GUID);
            Assert.NotNull(contract.GetCustomAttribute<GeneratedComInterfaceAttribute>());
            string[] names =
            [
                "GetCoreClrDirectory", "GetExpression", "VirtualUnwind", "SetExceptionCallback", "ClearExceptionCallback",
                "GetInterrupt", "OutputVaList", "GetDebuggeeType", "GetPageSize", "GetProcessorType", "Execute",
                "GetLastEventInformation", "Disassemble", "GetContextStackTrace", "ReadVirtual", "WriteVirtual",
                "GetSymbolOptions", "GetNameByOffset", "GetNumberModules", "GetModuleByIndex", "GetModuleByModuleName",
                "GetModuleByOffset", "GetModuleNames", "GetLineByOffset", "GetSourceFileLineOffsets", "FindSourceFile",
                "GetCurrentProcessSystemId", "GetCurrentThreadId", "SetCurrentThreadId", "GetCurrentThreadSystemId",
                "GetThreadIdBySystemId", "GetThreadContextBySystemId", "GetValueByName", "GetInstructionOffset",
                "GetStackOffset", "GetFrameOffset",
            ];
            MethodInfo[] methods = GetContractMethods(contract).ToArray();
            Assert.Equal(names, methods.Select(method => method.Name));
            Assert.All(methods, method =>
            {
                Assert.NotNull(method.GetCustomAttribute<PreserveSigAttribute>());
                Assert.Equal(method.Name switch
                {
                    "GetCoreClrDirectory" => typeof(string),
                    "GetExpression" => typeof(ulong),
                    _ => typeof(int),
                }, method.ReturnType);
            });
            Assert.Empty(contract.GetInterfaces());
            Type second = typeof(ILLDBServices2Generated);
            Assert.Equal(new Guid("012F32F0-33BA-4E8E-BC01-037D382D8A5E"), second.GUID);
            Assert.NotNull(second.GetCustomAttribute<GeneratedComInterfaceAttribute>());
            Assert.Empty(second.GetInterfaces());
            Assert.Equal(new[] { "LoadNativeSymbols", "AddModuleSymbol", "GetModuleInfo", "GetModuleVersionInformation", "SetRuntimeLoadedCallback" },
                GetContractMethods(second).Select(method => method.Name));
            Assert.All(GetContractMethods(second), method =>
            {
                Assert.Equal(typeof(int), method.ReturnType);
                Assert.NotNull(method.GetCustomAttribute<PreserveSigAttribute>());
            });
            ParameterInfo runtimeOnly = second.GetMethod("LoadNativeSymbols").GetParameters()[0];
            Assert.Equal(UnmanagedType.I1, runtimeOnly.GetCustomAttribute<MarshalAsAttribute>().Value);
        }

        [Fact]
        public void OptionalDebuggerContractsPreserveNativeLayoutAndSlots()
        {
            Assert.Equal(new[] { "AllocVirtual", "FreeVirtual" },
                GetContractMethods(typeof(IRemoteMemoryServiceGenerated)).Select(method => method.Name));
            MethodInfo stackTrace = Assert.Single(GetContractMethods(typeof(IDebuggerThreadStackTraceServiceGenerated)));
            Assert.Equal("GetDebuggerStackTrace", stackTrace.Name);
            Assert.Equal(new[] { typeof(uint), typeof(DebuggerStackFrame*), typeof(uint), typeof(uint).MakeByRefType() },
                stackTrace.GetParameters().Select(parameter => parameter.ParameterType));
            Assert.Equal(16, sizeof(DebuggerStackFrame));
            Assert.Equal(IntPtr.Zero, Marshal.OffsetOf<DebuggerStackFrame>(nameof(DebuggerStackFrame.InstructionPointer)));
            Assert.Equal((IntPtr)8, Marshal.OffsetOf<DebuggerStackFrame>(nameof(DebuggerStackFrame.StackPointer)));
        }

        [Fact]
        public void DebuggerServicesContractPreservesIdentityAndSlotOrder()
        {
            Type contract = typeof(IDebuggerServicesGenerated);
            Assert.Equal(new Guid("B4640016-6CA0-468E-BA2C-1FFF28DE7B72"), contract.GUID);
            Assert.NotNull(contract.GetCustomAttribute<GeneratedComInterfaceAttribute>());
            string[] names =
            [
                "GetOperatingSystem", "GetDebuggeeType", "GetProcessorType", "AddCommand", "OutputString",
                "ReadVirtual", "WriteVirtual", "GetNumberModules", "GetModuleByIndex", "GetModuleNames",
                "GetModuleInfo", "GetModuleVersionInformation", "GetModuleByModuleName", "GetNumberThreads",
                "GetThreadIdsByIndex", "GetThreadContextBySystemId", "GetCurrentProcessSystemId", "GetCurrentThreadSystemId",
                "SetCurrentThreadSystemId", "GetThreadTeb", "VirtualUnwind", "GetSymbolPath", "GetSymbolByOffset",
                "GetOffsetBySymbol", "GetTypeId", "GetFieldOffset", "GetOutputWidth", "SupportsDml", "OutputDmlString",
                "AddModuleSymbol", "GetLastEventInformation", "FlushCheck", "ExecuteHostCommand", "GetDacSignatureVerificationSettings",
            ];
            MethodInfo[] methods = GetContractMethods(contract).ToArray();
            Assert.Equal(names, methods.Select(method => method.Name));
            Assert.All(methods, method =>
            {
                Assert.NotNull(method.GetCustomAttribute<PreserveSigAttribute>());
                Type returnType = method.Name switch
                {
                    "OutputString" or "OutputDmlString" or "FlushCheck" => typeof(void),
                    "GetOutputWidth" => typeof(uint),
                    _ => typeof(int),
                };
                Assert.Equal(returnType, method.ReturnType);
            });
        }

        [Fact]
        public void RuntimeContractsUseNativeBufferAndInheritanceShapes()
        {
            Assert.Contains(typeof(ICLRDataTargetGenerated), typeof(ICLRDataTarget2Generated).GetInterfaces());
            Assert.Contains(typeof(ICorDebugDataTargetGenerated), typeof(ICorDebugMutableDataTargetGenerated).GetInterfaces());
            Assert.Empty(typeof(ICorDebugDataTarget4Generated).GetInterfaces());
            ParameterInfo[] request = typeof(ICLRDataTargetGenerated).GetMethod("Request").GetParameters();
            Assert.Equal(typeof(uint), request[3].ParameterType);
            Assert.Equal(typeof(byte*), request[4].ParameterType);
            ParameterInfo[] continuation = typeof(ICorDebugMutableDataTargetGenerated).GetMethod("ContinueStatusChanged").GetParameters();
            Assert.Equal(new[] { typeof(uint), typeof(uint) }, continuation.Select(parameter => parameter.ParameterType));
            ParameterInfo[] metadata = typeof(ICLRMetadataLocatorGenerated).GetMethod("GetMetadata").GetParameters();
            Assert.Equal(typeof(Guid*), metadata[3].ParameterType);
            Assert.Equal(typeof(uint*), metadata[8].ParameterType);
            ParameterInfo modulePath = typeof(ICLRDebuggingLibraryProvider2Generated).GetMethod("ProvideLibrary2").GetParameters()[3];
            Assert.True(modulePath.IsOut);
            Assert.Equal(UnmanagedType.LPWStr, modulePath.GetCustomAttribute<MarshalAsAttribute>().Value);
        }

        [Fact]
        public void RuntimeCallbacksDispatchInheritedAndFinalSlots()
        {
            TestRuntimeCallbacks callbacks = new();
            using ScopedComTests.NativeInterface<ICLRDataTarget2Generated> dataTarget = new(callbacks);
            using ScopedComTests.NativeInterface<ICorDebugMutableDataTargetGenerated> mutableTarget = new(callbacks);
            using ScopedComTests.NativeInterface<ICLRDebuggingLibraryProvider2Generated> libraryProvider = new(callbacks);
            ScopedComTests.AssertInterface(dataTarget.Pointer, typeof(ICLRDataTarget2Generated).GUID, 16);
            ScopedComTests.AssertInterface(mutableTarget.Pointer, typeof(ICorDebugMutableDataTargetGenerated).GUID, 9);
            ScopedComTests.AssertInterface(libraryProvider.Pointer, typeof(ICLRDebuggingLibraryProvider2Generated).GUID, 4);

            IntPtr* dataVtable = *(IntPtr**)dataTarget.Pointer;
            uint pointerSize = 0;
            Assert.Equal(0, ((delegate* unmanaged<IntPtr, uint*, int>)dataVtable[4])(dataTarget.Pointer, &pointerSize));
            Assert.Equal(8u, pointerSize);
            byte* input = stackalloc byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            byte* output = stackalloc byte[4];
            delegate* unmanaged<IntPtr, uint, uint, byte*, uint, byte*, int> request =
                (delegate* unmanaged<IntPtr, uint, uint, byte*, uint, byte*, int>)dataVtable[13];
            Assert.Equal(0, request(dataTarget.Pointer, 42, 4, input, 4, output));
            Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, new ReadOnlySpan<byte>(output, 4).ToArray());
            Assert.Equal(42u, callbacks.Value);
            ulong address = 0;
            delegate* unmanaged<IntPtr, ulong, uint, uint, uint, ulong*, int> allocate =
                (delegate* unmanaged<IntPtr, ulong, uint, uint, uint, ulong*, int>)dataVtable[14];
            Assert.Equal(0, allocate(dataTarget.Pointer, 0xDEADBEEF, 4, 0, 0, &address));
            Assert.Equal(0xDEADBEEFUL, address);
            Assert.Equal(0, ((delegate* unmanaged<IntPtr, ulong, uint, uint, int>)dataVtable[15])(dataTarget.Pointer, address, 4, 0));

            IntPtr* mutableVtable = *(IntPtr**)mutableTarget.Pointer;
            CorDebugPlatform platform = default;
            Assert.Equal(0, ((delegate* unmanaged<IntPtr, CorDebugPlatform*, int>)mutableVtable[3])(mutableTarget.Pointer, &platform));
            Assert.Equal(CorDebugPlatform.CORDB_PLATFORM_WINDOWS_AMD64, platform);
            Assert.Equal(0, ((delegate* unmanaged<IntPtr, uint, uint, int>)mutableVtable[8])(mutableTarget.Pointer, 123, 456));
            Assert.Equal(123u, callbacks.ThreadId);
            Assert.Equal(456u, callbacks.Value);

            IntPtr* providerVtable = *(IntPtr**)libraryProvider.Pointer;
            char* fileName = stackalloc char[] { 'd', 'a', 'c', '\0' };
            IntPtr path = IntPtr.Zero;
            delegate* unmanaged<IntPtr, char*, uint, uint, IntPtr*, int> provideLibrary =
                (delegate* unmanaged<IntPtr, char*, uint, uint, IntPtr*, int>)providerVtable[3];
            Assert.Equal(0, provideLibrary(libraryProvider.Pointer, fileName, 0, 0, &path));
            try
            {
                Assert.Equal("runtime\\dac.dll", Marshal.PtrToStringUni(path));
            }
            finally
            {
                Marshal.FreeCoTaskMem(path);
            }
        }

        [Fact]
        public void DbgEngSymbolsDispatchInheritedAndFinalSlots()
        {
            using NativeStub native = new(typeof(IDebugSymbols5Generated), 135);
            native.SetSlot(4, (IntPtr)(delegate* unmanaged[MemberFunction]<NativeStub.Instance*, SYMOPT, int>)&AddSymbolOptions);
            native.SetSlot(7, (IntPtr)(delegate* unmanaged[MemberFunction]<NativeStub.Instance*, ulong, byte*, uint, uint*, ulong*, int>)&GetNameByOffset);
            native.SetSlot(60, (IntPtr)(delegate* unmanaged[MemberFunction]<NativeStub.Instance*, ulong, char*, uint, uint*, ulong*, int>)&GetNameByOffsetWide);
            native.SetSlot(133, (IntPtr)(delegate* unmanaged[MemberFunction]<NativeStub.Instance*, DEBUG_FRAME, uint*, int>)&GetScopeIndex);
            native.SetSlot(134, (IntPtr)(delegate* unmanaged[MemberFunction]<NativeStub.Instance*, DEBUG_FRAME, uint, int>)&SetScopeIndex);
            IDebugSymbols5Generated symbols = UniqueComInterfaceMarshaller<IDebugSymbols5Generated>.ConvertToManaged(native.Pointer);
            try
            {
                Assert.Equal(0, symbols.AddSymbolOptions(SYMOPT.PUBLICS_ONLY));
                Assert.Equal((uint)SYMOPT.PUBLICS_ONLY, native.Value);
                byte* name = stackalloc byte[4];
                uint nameSize = 0;
                ulong displacement = 0;
                Assert.Equal(0, symbols.GetNameByOffset(0xDEADBEEF, name, 4, &nameSize, &displacement));
                Assert.Equal("sos", Marshal.PtrToStringAnsi((IntPtr)name));
                Assert.Equal(4u, nameSize);
                Assert.Equal(0xDEADBEEFUL, displacement);
                char* wideName = stackalloc char[4];
                Assert.Equal(0, symbols.GetNameByOffsetWide(0xDEADBEEF, wideName, 4, &nameSize, &displacement));
                Assert.Equal("sos", new string(wideName));
                Assert.Equal(4u, nameSize);
                Assert.Equal(0xDEADBEEFUL, displacement);
                Assert.Equal(0, symbols.SetScopeFrameByIndexEx(default, 42));
                Assert.Equal(0, symbols.GetCurrentScopeFrameIndexEx(default, out uint index));
                Assert.Equal(42u, index);
            }
            finally
            {
                Assert.IsType<ComObject>(symbols).FinalRelease();
            }
            Assert.Equal(1, native.ReferenceCount);
        }

        [Fact]
        public void DbgEngClientDispatchInheritedAndFinalSlots()
        {
            using NativeStub native = new(typeof(IDebugClient6Generated), 96);
            native.SetSlot(35, (IntPtr)(delegate* unmanaged[MemberFunction]<NativeStub.Instance*, uint*, int>)&GetOutputMask);
            native.SetSlot(36, (IntPtr)(delegate* unmanaged[MemberFunction]<NativeStub.Instance*, uint, int>)&SetOutputMask);
            native.SetSlot(95, (IntPtr)(delegate* unmanaged[MemberFunction]<NativeStub.Instance*, IntPtr, int>)&SetEventCallbacks);
            IDebugClient6Generated client = UniqueComInterfaceMarshaller<IDebugClient6Generated>.ConvertToManaged(native.Pointer);
            try
            {
                Assert.Equal(0, client.SetOutputMask(DEBUG_OUTPUT.ERROR));
                DEBUG_OUTPUT mask = default;
                Assert.Equal(0, client.GetOutputMask(&mask));
                Assert.Equal(DEBUG_OUTPUT.ERROR, mask);
                Assert.Equal(0, client.SetEventContextCallbacks(IntPtr.Zero));
                Assert.Equal(0u, native.Value);
            }
            finally
            {
                Assert.IsType<ComObject>(client).FinalRelease();
            }
            Assert.Equal(1, native.ReferenceCount);
        }

        private static IEnumerable<MethodInfo> GetContractMethods(Type contract)
        {
            Type parent = contract.GetInterfaces().OrderByDescending(type => type.GetInterfaces().Length).FirstOrDefault();
            if (parent != null)
            {
                foreach (MethodInfo method in GetContractMethods(parent))
                {
                    yield return method;
                }
            }
            foreach (MethodInfo method in contract.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttribute<GeneratedCodeAttribute>() == null)
                .OrderBy(method => method.MetadataToken))
            {
                yield return method;
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
        private static int AddSymbolOptions(NativeStub.Instance* self, SYMOPT options)
        {
            self->Value = (uint)options;
            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
        private static int GetNameByOffset(NativeStub.Instance* self, ulong offset, byte* buffer, uint bufferSize, uint* nameSize, ulong* displacement)
        {
            if (bufferSize != 4)
                return unchecked((int)0x80070057);
            buffer[0] = (byte)'s';
            buffer[1] = (byte)'o';
            buffer[2] = (byte)'s';
            buffer[3] = 0;
            *nameSize = 4;
            *displacement = offset;
            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
        private static int GetNameByOffsetWide(NativeStub.Instance* self, ulong offset, char* buffer, uint bufferSize, uint* nameSize, ulong* displacement)
        {
            if (bufferSize != 4)
                return unchecked((int)0x80070057);
            buffer[0] = 's';
            buffer[1] = 'o';
            buffer[2] = 's';
            buffer[3] = '\0';
            *nameSize = 4;
            *displacement = offset;
            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
        private static int GetScopeIndex(NativeStub.Instance* self, DEBUG_FRAME flags, uint* index)
        {
            *index = self->Value;
            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
        private static int SetScopeIndex(NativeStub.Instance* self, DEBUG_FRAME flags, uint index)
        {
            self->Value = index;
            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
        private static int GetOutputMask(NativeStub.Instance* self, uint* mask)
        {
            *mask = self->Value;
            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
        private static int SetOutputMask(NativeStub.Instance* self, uint mask)
        {
            self->Value = mask;
            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
        private static int SetEventCallbacks(NativeStub.Instance* self, IntPtr callbacks)
        {
            self->Value = callbacks == IntPtr.Zero ? 0u : 1u;
            return 0;
        }

        [GeneratedComClass]
        private sealed partial class TestRuntimeCallbacks : ICLRDataTarget2Generated, ICorDebugMutableDataTargetGenerated,
            ICLRDebuggingLibraryProvider2Generated
        {
            internal uint Value { get; private set; }
            internal uint ThreadId { get; private set; }

            public int GetMachineType(out IMAGE_FILE_MACHINE machineType)
            {
                machineType = IMAGE_FILE_MACHINE.AMD64;
                return 0;
            }

            public int GetPointerSize(out uint pointerSize)
            {
                pointerSize = 8;
                return 0;
            }

            public int GetImageBase(string imagePath, out ulong baseAddress)
            {
                baseAddress = 0xDEADBEEF;
                return 0;
            }

            public int ReadVirtual(ulong address, byte* buffer, uint bytesRequested, uint* bytesRead) => unchecked((int)0x80004001);
            public int WriteVirtual(ulong address, byte* buffer, uint bytesRequested, uint* bytesWritten) => unchecked((int)0x80004001);
            public int GetTLSValue(uint threadId, uint index, ulong* value) => unchecked((int)0x80004001);
            public int SetTLSValue(uint threadId, uint index, ulong value) => unchecked((int)0x80004001);

            public int GetCurrentThreadID(out uint threadId)
            {
                threadId = 123;
                return 0;
            }

            public int GetThreadContext(uint threadId, uint contextFlags, uint contextSize, byte* context) => unchecked((int)0x80004001);
            public int SetThreadContext(uint threadId, uint contextSize, byte* context) => unchecked((int)0x80004001);

            public int Request(uint requestCode, uint inputBufferSize, byte* inputBuffer, uint outputBufferSize, byte* outputBuffer)
            {
                if (inputBufferSize != outputBufferSize)
                    return unchecked((int)0x80070057);
                new ReadOnlySpan<byte>(inputBuffer, checked((int)inputBufferSize)).CopyTo(new Span<byte>(outputBuffer, checked((int)outputBufferSize)));
                Value = requestCode;
                return 0;
            }

            public int AllocVirtual(ulong address, uint size, uint typeFlags, uint protectFlags, ulong* buffer)
            {
                *buffer = address;
                return 0;
            }

            public int FreeVirtual(ulong address, uint size, uint typeFlags) => 0;

            public int GetPlatform(out CorDebugPlatform platform)
            {
                platform = CorDebugPlatform.CORDB_PLATFORM_WINDOWS_AMD64;
                return 0;
            }

            public int WriteVirtual(ulong address, byte* buffer, uint bytesRequested) => unchecked((int)0x80004001);

            public int ContinueStatusChanged(uint threadId, uint continueStatus)
            {
                ThreadId = threadId;
                Value = continueStatus;
                return 0;
            }

            public int ProvideLibrary2(string fileName, uint timeStamp, uint sizeOfImage, out string modulePath)
            {
                modulePath = "runtime\\" + fileName + ".dll";
                return 0;
            }
        }

        private sealed class NativeStub : IDisposable
        {
            internal struct Instance
            {
                internal IntPtr* Vtable;
                internal int ReferenceCount;
                internal uint Value;
                internal Guid InterfaceId;
            }

            private readonly Instance* _instance;
            internal void* Pointer => _instance;
            internal uint Value => _instance->Value;
            internal int ReferenceCount => _instance->ReferenceCount;

            internal NativeStub(Type contract, int slots)
            {
                _instance = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
                _instance->Vtable = (IntPtr*)NativeMemory.AllocZeroed((nuint)(slots * sizeof(IntPtr)));
                _instance->ReferenceCount = 1;
                _instance->InterfaceId = contract.GUID;
                SetSlot(0, (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, Guid*, void**, int>)&QueryInterface);
                SetSlot(1, (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint>)&AddRef);
                SetSlot(2, (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, uint>)&Release);
            }

            internal void SetSlot(int slot, IntPtr method) => _instance->Vtable[slot] = method;

            public void Dispose()
            {
                NativeMemory.Free(_instance->Vtable);
                NativeMemory.Free(_instance);
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int QueryInterface(Instance* self, Guid* iid, void** result)
            {
                bool supported = *iid == new Guid("00000000-0000-0000-C000-000000000046") || *iid == self->InterfaceId;
                if (self->InterfaceId == typeof(IDebugClient6Generated).GUID)
                {
                    supported |= *iid == typeof(IDebugClientGenerated).GUID || *iid == typeof(IDebugClient2Generated).GUID
                        || *iid == typeof(IDebugClient3Generated).GUID || *iid == typeof(IDebugClient4Generated).GUID
                        || *iid == typeof(IDebugClient5Generated).GUID;
                }
                if (self->InterfaceId == typeof(IDebugSymbols5Generated).GUID)
                {
                    supported |= *iid == typeof(IDebugSymbolsGenerated).GUID || *iid == typeof(IDebugSymbols2Generated).GUID
                        || *iid == typeof(IDebugSymbols3Generated).GUID || *iid == typeof(IDebugSymbols4Generated).GUID;
                }
                *result = supported ? self : null;
                if (!supported)
                    return unchecked((int)0x80004002);
                self->ReferenceCount++;
                return 0;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static uint AddRef(Instance* self) => (uint)++self->ReferenceCount;

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static uint Release(Instance* self) => (uint)--self->ReferenceCount;
        }
    }
}
