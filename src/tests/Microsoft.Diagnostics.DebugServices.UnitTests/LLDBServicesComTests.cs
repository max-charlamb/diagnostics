// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting;
using SOS.Hosting.Interop;
using Xunit;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe class LLDBServicesComTests
    {
        [ThreadStatic]
        private static ModuleCallbackResult s_callbackResult;

        [Fact]
        public void InterfacesShareIdentityAndOneCallerOwnedReference()
        {
            using TestHost fixture = new();
            IntPtr pointer = fixture.Pointer;
            Assert.Equal(2, Marshal.AddRef(pointer));
            Assert.Equal(1, Marshal.Release(pointer));
            IntPtr second = QueryInterface(pointer, typeof(ILLDBServices2Generated).GUID);
            IntPtr firstIdentity = QueryInterface(pointer, new Guid("00000000-0000-0000-C000-000000000046"));
            IntPtr secondIdentity = QueryInterface(second, new Guid("00000000-0000-0000-C000-000000000046"));
            try
            {
                Assert.Equal(firstIdentity, secondIdentity);
                Guid unsupported = new("DEADBEEF-DEAD-BEEF-DEAD-BEEFDEADBEEF");
                Assert.Equal(HResult.E_NOINTERFACE, Marshal.QueryInterface(pointer, in unsupported, out IntPtr result));
                Assert.Equal(IntPtr.Zero, result);
            }
            finally
            {
                Marshal.Release(secondIdentity);
                Marshal.Release(firstIdentity);
                Marshal.Release(second);
            }
        }

        [Fact]
        public void FirstMiddleAndFinalSlotsForwardToHostServices()
        {
            using TestHost fixture = new();
            IntPtr pointer = fixture.Pointer;
            IntPtr* table = *(IntPtr**)pointer;
            IntPtr directory = ((delegate* unmanaged<IntPtr, IntPtr>)table[3])(pointer);
            try
            {
                Assert.Equal(Path.GetDirectoryName(fixture.Services.FileName), Marshal.PtrToStringAnsi(directory));
            }
            finally
            {
                Marshal.FreeCoTaskMem(directory);
            }
            fixture.Services.Container.AddService<TargetWrapper>(
                new TargetWrapper(fixture.Services, fixture.Services, fixture.Services, fixture.Services));
            IntPtr expression = Marshal.StringToCoTaskMemAnsi("0xDEADBEEFDEADBEEF");
            try
            {
                Assert.Equal(0xDEADBEEFDEADBEEFul,
                    ((delegate* unmanaged<IntPtr, IntPtr, ulong>)table[4])(pointer, expression));
            }
            finally
            {
                Marshal.FreeCoTaskMem(expression);
            }
            uint pageSize = 0;
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, uint*, int>)table[11])(pointer, &pageSize));
            Assert.Equal(4096u, pageSize);
            byte* buffer = stackalloc byte[4];
            uint transferred = 0;
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, ulong, IntPtr, uint, uint*, int>)table[17])(
                pointer, 0xDEADBEEF, (IntPtr)buffer, 4, &transferred));
            Assert.Equal(4u, transferred);
            Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, new ReadOnlySpan<byte>(buffer, 4).ToArray());
            Assert.Equal(0xDEADBEEFul, fixture.Services.LastAddress);
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, ulong, IntPtr, uint, uint*, int>)table[18])(
                pointer, 0xDEADBEEF, (IntPtr)buffer, 4, &transferred));
            Assert.Equal(4u, transferred);
            Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, fixture.Services.WrittenBytes);
            uint processId = 0;
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, uint*, int>)table[29])(pointer, &processId));
            Assert.Equal(42u, processId);
            ulong frameOffset = 0;
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, ulong*, int>)table[38])(pointer, &frameOffset));
            Assert.Equal(RegisterThread.RegisterValue, frameOffset);
        }

        [Fact]
        public void MissingRuntimeReturnsNullDirectoryWithoutAnHResult()
        {
            using TestHost fixture = new();
            fixture.Services.Container.RemoveService(typeof(IRuntime));
            IntPtr* table = *(IntPtr**)fixture.Pointer;
            Assert.Equal(IntPtr.Zero, ((delegate* unmanaged<IntPtr, IntPtr>)table[3])(fixture.Pointer));
        }

        [Fact]
        public void NativeStackStubAndInvalidContextSizeDoNotTouchBuffers()
        {
            using TestHost fixture = new();
            IntPtr pointer = fixture.Pointer;
            IntPtr* table = *(IntPtr**)pointer;
            byte* buffer = stackalloc byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            uint filled = uint.MaxValue;
            Assert.Equal(HResult.S_OK,
                ((delegate* unmanaged<IntPtr, IntPtr, uint, IntPtr, uint, IntPtr, uint, uint, uint*, int>)table[16])(
                    pointer, (IntPtr)buffer, 4, (IntPtr)buffer, 1, (IntPtr)buffer, 4, 4, &filled));
            Assert.Equal(0u, filled);
            Assert.Equal(HResult.E_INVALIDARG,
                ((delegate* unmanaged<IntPtr, uint, uint, uint, byte*, int>)table[34])(pointer, 42, 0, uint.MaxValue, buffer));
            Assert.Equal(HResult.E_NOTIMPL,
                ((delegate* unmanaged<IntPtr, uint, uint, byte*, int>)table[5])(pointer, 42, 4, buffer));
            Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, new ReadOnlySpan<byte>(buffer, 4).ToArray());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(128)]
        public void ModuleNamesRespectNativeBufferCapacity(int capacity)
        {
            using TestHost fixture = new();
            IntPtr pointer = fixture.Pointer;
            IntPtr* table = *(IntPtr**)pointer;
            byte* buffer = stackalloc byte[130];
            new Span<byte>(buffer, 130).Fill(0xEF);
            uint imageSize = 0;
            uint moduleSize = 0;
            uint loadedSize = uint.MaxValue;
            Assert.Equal(HResult.S_OK,
                ((delegate* unmanaged<IntPtr, uint, ulong, byte*, uint, uint*, byte*, uint, uint*, byte*, uint, uint*, int>)table[25])(
                    pointer, 0, 0, buffer, (uint)capacity, &imageSize, null, 0, &moduleSize, null, 0, &loadedSize));
            Assert.Equal((uint)fixture.Services.FileName.Length + 1, imageSize);
            Assert.Equal((uint)Path.GetFileNameWithoutExtension(fixture.Services.FileName).Length + 1, moduleSize);
            Assert.Equal(0u, loadedSize);
            Assert.Equal(0xEF, buffer[capacity]);
            if (capacity != 0)
            {
                string expected = fixture.Services.FileName[..Math.Min(capacity - 1, fixture.Services.FileName.Length)];
                Assert.Equal(expected, Marshal.PtrToStringAnsi((IntPtr)buffer));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SecondInterfaceUsesNativeBoolAndCdeclModuleCallback(bool runtimeOnly)
        {
            using TestHost fixture = new();
            IntPtr second = QueryInterface(fixture.Pointer, typeof(ILLDBServices2Generated).GUID);
            try
            {
                IntPtr* table = *(IntPtr**)second;
                s_callbackResult = new();
                IntPtr callback = (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, byte*, ulong, int, void>)&OnModuleLoad;
                Assert.Equal(HResult.S_OK,
                    ((delegate* unmanaged<IntPtr, byte, IntPtr, int>)table[3])(second, runtimeOnly ? (byte)1 : (byte)0, callback));
                Assert.Equal(1, s_callbackResult.Count);
                Assert.Equal(IntPtr.Zero, s_callbackResult.Parameter);
                Assert.Equal(fixture.Services.FileName, s_callbackResult.FileName);
                Assert.Equal(fixture.Services.ImageBase, s_callbackResult.Address);
                Assert.Equal(unchecked((int)fixture.Services.ImageSize), s_callbackResult.Size);
                Assert.Equal(HResult.E_INVALIDARG,
                    ((delegate* unmanaged<IntPtr, byte, IntPtr, int>)table[3])(second, 0, IntPtr.Zero));
                Assert.Equal(HResult.E_NOTIMPL,
                    ((delegate* unmanaged<IntPtr, IntPtr, int>)table[7])(second, callback));
            }
            finally
            {
                s_callbackResult = null;
                Marshal.Release(second);
            }
        }

        [Fact]
        public void SecondInterfacePreservesModuleInformationAndFailure()
        {
            using TestHost fixture = new();
            IntPtr second = QueryInterface(fixture.Pointer, typeof(ILLDBServices2Generated).GUID);
            try
            {
                IntPtr* table = *(IntPtr**)second;
                ulong address = 0;
                ulong size = 0;
                uint timestamp = 0;
                uint checksum = 0;
                delegate* unmanaged<IntPtr, uint, ulong*, ulong*, uint*, uint*, int> getInfo =
                    (delegate* unmanaged<IntPtr, uint, ulong*, ulong*, uint*, uint*, int>)table[5];
                Assert.Equal(HResult.S_OK, getInfo(second, 0, &address, &size, &timestamp, &checksum));
                Assert.Equal(fixture.Services.ImageBase, address);
                Assert.Equal(fixture.Services.ImageSize, size);
                Assert.Equal(SOSHost.InvalidTimeStamp, timestamp);
                Assert.Equal(SOSHost.InvalidChecksum, checksum);
                Assert.Equal(HResult.E_FAIL, getInfo(second, 1, null, null, null, null));
                byte* version = stackalloc byte[52];
                uint versionSize = 0;
                IntPtr item = Marshal.StringToCoTaskMemAnsi("\\");
                try
                {
                    Assert.Equal(HResult.S_OK,
                        ((delegate* unmanaged<IntPtr, uint, ulong, IntPtr, byte*, uint, uint*, int>)table[6])(
                            second, 0, 0, item, version, 52, &versionSize));
                    Assert.Equal(52u, versionSize);
                    Assert.Equal(10u << 16 | 2u, ((uint*)version)[2]);
                    Assert.Equal(3u << 16 | 4u, ((uint*)version)[3]);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(item);
                }
            }
            finally
            {
                Marshal.Release(second);
            }
        }

        private static IntPtr QueryInterface(IntPtr pointer, Guid iid)
        {
            Assert.Equal(HResult.S_OK, Marshal.QueryInterface(pointer, in iid, out IntPtr result));
            Assert.NotEqual(IntPtr.Zero, result);
            return result;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static void OnModuleLoad(IntPtr parameter, byte* fileName, ulong address, int size)
        {
            s_callbackResult.Count++;
            s_callbackResult.Parameter = parameter;
            s_callbackResult.FileName = Marshal.PtrToStringAnsi((IntPtr)fileName);
            s_callbackResult.Address = address;
            s_callbackResult.Size = size;
        }

        private sealed class ModuleCallbackResult
        {
            internal int Count;
            internal IntPtr Parameter;
            internal string FileName;
            internal ulong Address;
            internal int Size;
        }

        private sealed class TestHost : IDisposable
        {
            private readonly SOSHost _host;
            internal DataTargetComTests.TestDataServices Services { get; } = new();
            internal IntPtr Pointer { get; }

            internal TestHost()
            {
                Services.Container.RemoveService(typeof(IThread));
                Services.Container.AddService<IThread>(new RegisterThread(Services));
                _host = new SOSHost(Services, Services, null);
                // Only import services used by these callbacks; no native SOS library is loaded.
                typeof(SOSHost).GetField(nameof(SOSHost.ContextService), BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(_host, Services);
                typeof(SOSHost).GetField(nameof(SOSHost.ThreadService), BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(_host, Services);
                typeof(SOSHost).GetField(nameof(SOSHost.ModuleService), BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(_host, new ModuleService(Services));
                Pointer = (IntPtr)ComInterfaceMarshaller<ILLDBServicesGenerated>.ConvertToUnmanaged(new LLDBServices(_host));
            }

            public void Dispose()
            {
                Assert.Equal(0, Marshal.Release(Pointer));
                ((IDisposable)_host).Dispose();
                Services.Container.DisposeServices();
            }
        }

        private sealed class RegisterThread(ScopedComTests.TestServices services) : IThread
        {
            internal const ulong RegisterValue = 0xDEADBEEFDEADBEEF;
            public int ThreadIndex => 0;
            public uint ThreadId => 42;
            public ITarget Target => services;
            public IServiceProvider Services => services.Services;
            public ReadOnlySpan<byte> GetThreadContext() => [0xDE, 0xAD, 0xBE, 0xEF];
            public ulong GetThreadTeb() => 0;
            public bool TryGetRegisterValue(int registerIndex, out ulong value)
            {
                value = RegisterValue;
                return registerIndex == 0;
            }
        }

        private sealed class ModuleService(ScopedComTests.TestServices module) : IModuleService
        {
            public IModule EntryPointModule => module;
            public IEnumerable<IModule> EnumerateModules() => [module];
            public IModule GetModuleFromIndex(int moduleIndex) =>
                moduleIndex == module.ModuleIndex ? module : throw new DiagnosticsException("Unknown module");
            public IModule GetModuleFromBaseAddress(ulong baseAddress) =>
                baseAddress == module.ImageBase ? module : throw new DiagnosticsException("Unknown module");
            public IModule GetModuleFromAddress(ulong address) => GetModuleFromBaseAddress(address);
            public IEnumerable<IModule> GetModuleFromModuleName(string moduleName) =>
                moduleName == module.Target.GetPlatformModuleName("coreclr") ? [module] : [];
            public IModule CreateModule(int moduleIndex, ulong imageBase, ulong imageSize, string imageName) =>
                throw new NotSupportedException();
        }
    }
}
