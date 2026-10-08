// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.DebugServices.Implementation;
using SOS.Extensions.Clrma;
using SOS.Hosting;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop;
using Xunit;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe partial class ScopedComTests
    {
        private const int S_OK = 0;
        private const int E_NOINTERFACE = unchecked((int)0x80004002);
        private const int E_INVALIDARG = unchecked((int)0x80070057);

        [Theory]
        [InlineData("WINDOWS", 1)]
        [InlineData("LINUX", 2)]
        [InlineData("OSX", 3)]
        [InlineData("UNKNOWN", 0)]
        public void HostAndTargetPreserveAbi(string operatingSystem, int nativeOperatingSystem)
        {
            TestServices services = new() { OperatingSystem = OSPlatform.Create(operatingSystem) };
            TargetWrapper target = new(services, services, new SymbolService(services), services);
            services.Container.AddService(target);
            HostServicesComTests.TestHostServices hostServices = new();
            HostWrapper host = new(services, hostServices);
            using NativeInterface<IHostServicesGenerated> nativeHostServices = new(hostServices);
            using NativeInterface<IHostGenerated> nativeHost = new(host);
            using NativeInterface<ITargetGenerated> nativeTarget = new(target);
            IntPtr hostPointer = nativeHost.Pointer;
            IntPtr targetInterface = nativeTarget.Pointer;
            AssertInterface(hostPointer, typeof(IHostGenerated).GUID, 6);
            AssertInterface(targetInterface, typeof(ITargetGenerated).GUID, 7);

            IntPtr* hostVtable = *(IntPtr**)hostPointer;
            delegate* unmanaged<IntPtr, HostType> getHostType = (delegate* unmanaged<IntPtr, HostType>)hostVtable[3];
            Assert.Equal(HostType.DotnetDump, getHostType(hostPointer));

            delegate* unmanaged<IntPtr, Guid*, IntPtr*, int> getService = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)hostVtable[4];
            Guid serviceId = typeof(IHostGenerated).GUID;
            IntPtr service = IntPtr.Zero;
            Assert.Equal(E_NOINTERFACE, getService(hostPointer, &serviceId, &service));
            Assert.Equal(IntPtr.Zero, service);
            serviceId = typeof(IHostServicesGenerated).GUID;
            Assert.Equal(S_OK, getService(hostPointer, &serviceId, &service));
            try
            {
                Assert.Equal(nativeHostServices.Pointer, service);
            }
            finally
            {
                Release(service);
            }

            delegate* unmanaged<IntPtr, IntPtr*, int> getTarget = (delegate* unmanaged<IntPtr, IntPtr*, int>)hostVtable[5];
            IntPtr targetPointer = IntPtr.Zero;
            Assert.Equal(S_OK, getTarget(hostPointer, &targetPointer));
            try
            {
                Assert.Equal(targetInterface, targetPointer);
            }
            finally
            {
                Release(targetPointer);
            }
            services.Container.RemoveService(typeof(ITarget));
            Assert.Equal(E_NOINTERFACE, getTarget(hostPointer, &targetPointer));
            Assert.Equal(IntPtr.Zero, targetPointer);

            IntPtr* targetVtable = *(IntPtr**)targetInterface;
            delegate* unmanaged<IntPtr, int> getOperatingSystem = (delegate* unmanaged<IntPtr, int>)targetVtable[3];
            Assert.Equal(nativeOperatingSystem, getOperatingSystem(targetInterface));

            delegate* unmanaged<IntPtr, Guid*, IntPtr*, int> getTargetService = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)targetVtable[4];
            Assert.Equal(E_NOINTERFACE, getTargetService(targetInterface, &serviceId, &service));
            Assert.Equal(IntPtr.Zero, service);

            delegate* unmanaged<IntPtr, void> flush = (delegate* unmanaged<IntPtr, void>)targetVtable[6];
            flush(targetInterface);
            Assert.Equal(1, services.FlushCount);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TargetExposesOnlySymbolAndClrmaServices(bool customSymbolFactory)
        {
            TestServices services = new();
            HostWrapper host = new(services);
            using NativeInterface<IHostGenerated> nativeHost = new(host);
            Guid hostServicesId = typeof(IHostServicesGenerated).GUID;
            IntPtr pointer = IntPtr.Zero;
            IntPtr* hostVtable = *(IntPtr**)nativeHost.Pointer;
            delegate* unmanaged<IntPtr, Guid*, IntPtr*, int> getHostService =
                (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)hostVtable[4];
            Assert.Equal(E_NOINTERFACE, getHostService(nativeHost.Pointer, &hostServicesId, &pointer));
            Assert.Equal(IntPtr.Zero, pointer);

            SymbolService symbolService = new(services);
            ClrmaComTests.CrashData crashData = new(services);
            services.Container.AddService<ICrashInfoService>(crashData);
            TargetWrapper target = new(services, services, symbolService, services);
            Assert.NotNull(target.SymbolServiceFactory);
            int symbolFactoryCalls = 0;
            if (customSymbolFactory)
            {
                target.SymbolServiceFactory = () =>
                {
                    symbolFactoryCalls++;
                    return new SymbolServiceWrapper(symbolService, services);
                };
            }
            Assert.Equal(0, services.PointerSizeReads);
            Assert.Equal(0, symbolFactoryCalls);
            using NativeInterface<ITargetGenerated> nativeTarget = new(target);
            IntPtr targetPointer = nativeTarget.Pointer;
            IntPtr* vtable = *(IntPtr**)targetPointer;
            delegate* unmanaged<IntPtr, Guid*, IntPtr*, int> getService =
                (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[4];

            Guid symbolId = typeof(ISymbolServiceGenerated).GUID;
            Guid clrmaId = typeof(IClrmaServiceGenerated).GUID;
            Guid unknownId = new("00000000-0000-0000-C000-000000000046");
            Guid unsupportedId = new("DEADBEEF-DEAD-BEEF-DEAD-BEEFDEADBEEF");
            foreach (Guid id in new[] { typeof(IHostGenerated).GUID, typeof(ITargetGenerated).GUID, typeof(IRuntimeGenerated).GUID, hostServicesId, unknownId, unsupportedId })
            {
                Guid serviceId = id;
                pointer = new IntPtr(1);
                Assert.Equal(E_NOINTERFACE, getService(targetPointer, &serviceId, &pointer));
                Assert.Equal(IntPtr.Zero, pointer);
            }

            Assert.Equal(E_NOINTERFACE, getService(targetPointer, &clrmaId, &pointer));
            Assert.Equal(IntPtr.Zero, pointer);
            int clrmaFactoryCalls = 0;
            target.ClrmaServiceFactory = () =>
            {
                clrmaFactoryCalls++;
                return new ClrmaServiceWrapper(services.Container);
            };

            IntPtr symbol = IntPtr.Zero;
            Assert.Equal(S_OK, getService(targetPointer, &symbolId, &symbol));
            try
            {
                AssertInterface(symbol, symbolId, 12);
                Assert.Equal(S_OK, getService(targetPointer, &symbolId, &pointer));
                Assert.Equal(symbol, pointer);
                Release(pointer);
                ISymbolServiceGenerated managedSymbol = target.GetSymbolService();
                Assert.Same(managedSymbol, target.GetSymbolService());
                Assert.Equal(0xDEADBEEFUL, managedSymbol.GetExpressionValue("0xdeadbeef"));
                Assert.Equal(customSymbolFactory ? 1 : 0, symbolFactoryCalls);
                Assert.Equal(1, services.PointerSizeReads);
            }
            finally
            {
                Release(symbol);
            }

            target.SymbolServiceFactory = null;
            Assert.Equal(E_NOINTERFACE, getService(targetPointer, &symbolId, &pointer));
            Assert.Equal(IntPtr.Zero, pointer);

            IntPtr clrmaPointer = IntPtr.Zero;
            Assert.Equal(S_OK, getService(targetPointer, &clrmaId, &clrmaPointer));
            try
            {
                AssertInterface(clrmaPointer, clrmaId, 8);
                Assert.Equal(S_OK, getService(targetPointer, &clrmaId, &pointer));
                Assert.Equal(clrmaPointer, pointer);
                Release(pointer);
                Assert.Equal(1, clrmaFactoryCalls);
                IntPtr* clrmaVtable = *(IntPtr**)clrmaPointer;
                delegate* unmanaged<IntPtr, IntPtr, int> associateClient =
                    (delegate* unmanaged<IntPtr, IntPtr, int>)clrmaVtable[3];
                Assert.Equal(S_OK, associateClient(clrmaPointer, IntPtr.Zero));
            }
            finally
            {
                Release(clrmaPointer);
            }
        }

        [Theory]
        [InlineData(RuntimeType.Desktop, "WINDOWS", 0)]
        [InlineData(RuntimeType.NetCore, "WINDOWS", 1)]
        [InlineData(RuntimeType.NetCore, "LINUX", 2)]
        [InlineData(RuntimeType.SingleFile, "OSX", 2)]
        [InlineData(RuntimeType.NativeAOT, "WINDOWS", 4)]
        public void RuntimePreservesAbi(RuntimeType runtimeType, string operatingSystem, int configuration)
        {
            TestServices services = new() { RuntimeType = runtimeType, OperatingSystem = OSPlatform.Create(operatingSystem) };
            using RuntimeWrapper runtime = new(services.Container, services);
            services.Container.AddService(runtime);
            TargetWrapper target = new(services, services, new SymbolService(services), services);
            using NativeInterface<IRuntimeGenerated> nativeRuntime = new(runtime);
            using NativeInterface<ITargetGenerated> nativeTarget = new(target);
            IntPtr runtimeInterface = nativeRuntime.Pointer;
            IntPtr targetInterface = nativeTarget.Pointer;
            AssertInterface(runtimeInterface, typeof(IRuntimeGenerated).GUID, 12);
            Assert.Equal(runtimeInterface, runtime.IRuntime);
            uint referenceCount = AddRef(runtimeInterface);
            Release(runtimeInterface);

            IntPtr* targetVtable = *(IntPtr**)targetInterface;
            delegate* unmanaged<IntPtr, IntPtr*, int> getRuntime = (delegate* unmanaged<IntPtr, IntPtr*, int>)targetVtable[5];
            Assert.Equal(E_INVALIDARG, getRuntime(targetInterface, null));
            IntPtr runtimePointer = IntPtr.Zero;
            Assert.Equal(S_OK, getRuntime(targetInterface, &runtimePointer));
            Assert.Equal(runtimeInterface, runtimePointer);
            // GetRuntime returns a borrowed pointer; repeated calls must not AddRef.
            Assert.Equal(referenceCount, AddRef(runtimePointer));
            Release(runtimePointer);
            Assert.Equal(S_OK, getRuntime(targetInterface, &runtimePointer));
            Assert.Equal(referenceCount, AddRef(runtimePointer));
            Release(runtimePointer);
            services.Container.RemoveService(typeof(IRuntime));
            Assert.Equal(E_NOINTERFACE, getRuntime(targetInterface, &runtimePointer));
            Assert.Equal(IntPtr.Zero, runtimePointer);

            IntPtr* vtable = *(IntPtr**)runtimeInterface;
            delegate* unmanaged<IntPtr, int> getConfiguration = (delegate* unmanaged<IntPtr, int>)vtable[3];
            Assert.Equal(configuration, getConfiguration(runtimeInterface));
            delegate* unmanaged<IntPtr, ulong> getAddress = (delegate* unmanaged<IntPtr, ulong>)vtable[4];
            delegate* unmanaged<IntPtr, ulong> getSize = (delegate* unmanaged<IntPtr, ulong>)vtable[5];
            Assert.Equal(services.ImageBase, getAddress(runtimeInterface));
            Assert.Equal(services.ImageSize, getSize(runtimeInterface));

            delegate* unmanaged<IntPtr, byte*, void> setDirectory = (delegate* unmanaged<IntPtr, byte*, void>)vtable[6];
            delegate* unmanaged<IntPtr, IntPtr> getDirectory = (delegate* unmanaged<IntPtr, IntPtr>)vtable[7];
            string ReadDirectory()
            {
                IntPtr directory = getDirectory(runtimeInterface);
                try
                {
                    return Marshal.PtrToStringAnsi(directory);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(directory);
                }
            }
            Assert.Equal("runtime", ReadDirectory());
            Assert.Equal("runtime", ReadDirectory());
            byte[] directoryBytes = System.Text.Encoding.ASCII.GetBytes("new-runtime\0");
            fixed (byte* buffer = directoryBytes)
            {
                setDirectory(runtimeInterface, buffer);
            }
            Assert.Equal("new-runtime", services.RuntimeModuleDirectory);
            Assert.Equal("new-runtime", ReadDirectory());
            setDirectory(runtimeInterface, null);
            Assert.Null(services.RuntimeModuleDirectory);
            Assert.Equal(System.IO.Path.GetDirectoryName(services.FileName), ReadDirectory());

            delegate* unmanaged<IntPtr, CDacLoadPolicy, IntPtr*, int> getDataProcess =
                (delegate* unmanaged<IntPtr, CDacLoadPolicy, IntPtr*, int>)vtable[8];
            Assert.Equal(E_INVALIDARG, getDataProcess(runtimeInterface, CDacLoadPolicy.OnlyUseCDac, null));
            IntPtr process = IntPtr.Zero;
            Assert.Equal(S_OK, getDataProcess(runtimeInterface, CDacLoadPolicy.OnlyUseCDac, &process));
            Assert.Equal(TestServices.DataProcess, process);
            services.CDacAvailable = false;
            Assert.Equal(E_NOINTERFACE, getDataProcess(runtimeInterface, CDacLoadPolicy.OnlyUseCDac, &process));
            Assert.Equal(IntPtr.Zero, process);

            delegate* unmanaged<IntPtr, IntPtr*, int> getCorDebug = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[9];
            Assert.Equal(E_INVALIDARG, getCorDebug(runtimeInterface, null));

            delegate* unmanaged<IntPtr, VS_FIXEDFILEINFO*, byte*, int, int> getVersion =
                (delegate* unmanaged<IntPtr, VS_FIXEDFILEINFO*, byte*, int, int>)vtable[10];
            Assert.Equal(E_INVALIDARG, getVersion(runtimeInterface, null, null, 0));
            VS_FIXEDFILEINFO version = default;
            byte* versionBuffer = stackalloc byte[64];
            Assert.Equal(S_OK, getVersion(runtimeInterface, &version, versionBuffer, 64));
            Assert.Equal(0x000A0002u, version.dwFileVersionMS);
            Assert.Equal(0x00030004u, version.dwFileVersionLS);
            Assert.Equal("10.2.3.4-test", Marshal.PtrToStringAnsi((IntPtr)versionBuffer));

            delegate* unmanaged<IntPtr, CDacLoadPolicy> getPolicy = (delegate* unmanaged<IntPtr, CDacLoadPolicy>)vtable[11];
            Assert.Equal(CDacLoadPolicy.PreferCDac, getPolicy(runtimeInterface));

            runtime.Dispose();
            runtime.Dispose();
            Assert.Equal(referenceCount - 1, AddRef(runtimeInterface));
            Release(runtimeInterface);
            Assert.True(runtime.IsDisposed);
            Assert.Equal(IntPtr.Zero, runtime.IRuntime);
        }

        internal static void AssertInterface(IntPtr pointer, Guid interfaceId, int slots)
        {
            Assert.NotEqual(IntPtr.Zero, pointer);
            IntPtr* vtable = *(IntPtr**)pointer;
            for (int slot = 0; slot < slots; slot++)
            {
                Assert.NotEqual(IntPtr.Zero, vtable[slot]);
            }
            delegate* unmanaged<IntPtr, Guid*, IntPtr*, int> queryInterface = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
            IntPtr queried = IntPtr.Zero;
            Assert.Equal(S_OK, queryInterface(pointer, &interfaceId, &queried));
            try
            {
                Assert.Equal(pointer, queried);
            }
            finally
            {
                Release(queried);
            }

            Guid unknownId = new("00000000-0000-0000-C000-000000000046");
            Assert.Equal(S_OK, queryInterface(pointer, &unknownId, &queried));
            try
            {
                IntPtr identity = IntPtr.Zero;
                IntPtr* unknownVtable = *(IntPtr**)queried;
                delegate* unmanaged<IntPtr, Guid*, IntPtr*, int> queryUnknown = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)unknownVtable[0];
                Assert.Equal(S_OK, queryUnknown(queried, &unknownId, &identity));
                try
                {
                    Assert.Equal(queried, identity);
                }
                finally
                {
                    Release(identity);
                }
            }
            finally
            {
                Release(queried);
            }

            Guid unsupported = new("DEADBEEF-DEAD-BEEF-DEAD-BEEFDEADBEEF");
            queried = new IntPtr(1);
            Assert.Equal(E_NOINTERFACE, queryInterface(pointer, &unsupported, &queried));
            Assert.Equal(IntPtr.Zero, queried);
        }

        private static uint AddRef(IntPtr pointer)
        {
            IntPtr* vtable = *(IntPtr**)pointer;
            return ((delegate* unmanaged<IntPtr, uint>)vtable[1])(pointer);
        }

        internal static void Release(IntPtr pointer)
        {
            IntPtr* vtable = *(IntPtr**)pointer;
            ((delegate* unmanaged<IntPtr, uint>)vtable[2])(pointer);
        }

        internal sealed class NativeInterface<T> : IDisposable where T : class
        {
            internal IntPtr Pointer { get; }

            internal NativeInterface(T instance)
            {
                Pointer = (IntPtr)ComInterfaceMarshaller<T>.ConvertToUnmanaged(instance);
            }

            public void Dispose() => ComInterfaceMarshaller<T>.Free((void*)Pointer);
        }

        internal class TestServices : IHost, ITarget, IRuntime, IContextService, IModule, IMemoryService
        {
            internal static readonly IntPtr DataProcess = new(0x1234);
            internal ServiceContainer Container { get; } = new(null);
            internal int FlushCount { get; private set; }
            internal bool CDacAvailable { get; set; } = true;
            internal int PointerSizeReads { get; private set; }

            internal TestServices()
            {
                Container.AddService<IContextService>(this);
                Container.AddService<ITarget>(this);
                Container.AddService<IRuntime>(this);
            }

            public IServiceProvider Services => Container;
            public HostType HostType => HostType.DotnetDump;
            public IServiceEvent OnShutdownEvent { get; } = new ServiceEvent();
            public IServiceEvent<ITarget> OnTargetCreate { get; } = new ServiceEvent<ITarget>();
            public IEnumerable<ITarget> EnumerateTargets() => new ITarget[] { this };
            public int AddTarget(ITarget target) => throw new NotSupportedException();
            public string GetTempDirectory() => "sos-temp";

            public IHost Host => this;
            public int Id => 0;
            public OSPlatform OperatingSystem { get; set; } = OSPlatform.Windows;
            public Architecture Architecture { get; set; } = Architecture.X64;
            public bool IsDump => true;
            public uint? ProcessId => 42;
            public IServiceEvent OnFlushEvent { get; } = new ServiceEvent();
            public IServiceEvent OnDestroyEvent { get; } = new ServiceEvent();
            public void Flush() => FlushCount++;
            public void Destroy() => throw new NotSupportedException();

            public ITarget Target => this;
            public RuntimeType RuntimeType { get; set; } = RuntimeType.NetCore;
            public Version RuntimeVersion => new(10, 2, 3, 4);
            public IModule RuntimeModule => this;
            public string RuntimeModuleDirectory { get; set; } = "runtime";
            public string GetDacFilePath(out bool verifySignature) => throw new NotSupportedException();
            public string GetDbiFilePath() => throw new NotSupportedException();
            public int GetClrDataProcessFromCDac(out IntPtr clrDataProcess)
            {
                clrDataProcess = CDacAvailable ? DataProcess : IntPtr.Zero;
                return CDacAvailable ? S_OK : E_NOINTERFACE;
            }

            public IServiceEvent OnContextChange { get; } = new ServiceEvent();
            public void SetCurrentTarget(int targetId) => throw new NotSupportedException();
            public void ClearCurrentTarget() => throw new NotSupportedException();
            public void SetCurrentThread(uint threadId) => throw new NotSupportedException();
            public void ClearCurrentThread() => throw new NotSupportedException();
            public void SetCurrentRuntime(int runtimeId) => throw new NotSupportedException();
            public void ClearCurrentRuntime() => throw new NotSupportedException();

            public int ModuleIndex => 0;
            public string FileName => System.IO.Path.Combine("default-runtime", "coreclr.dll");
            public ulong ImageBase => 0xDEADBEEF12345678;
            public ulong ImageSize => 0x123456789;
            public uint? IndexFileSize => null;
            public uint? IndexTimeStamp => null;
            public ImmutableArray<byte> BuildId => ImmutableArray<byte>.Empty;
            public bool IsPEImage => true;
            public bool IsManaged => false;
            public bool? IsFileLayout => false;
            public IEnumerable<PdbFileInfo> GetPdbFileInfos() => throw new NotSupportedException();
            public string GetSymbolFileName() => throw new NotSupportedException();
            public Version GetVersionData() => RuntimeVersion;
            public string GetVersionString() => "10.2.3.4-test";
            public string LoadSymbols() => throw new NotSupportedException();
            public ImmutableArray<byte> GetMetadata() => throw new NotSupportedException();

            public int PointerSize
            {
                get
                {
                    PointerSizeReads++;
                    return 8;
                }
            }
            public bool ReadMemory(ulong address, Span<byte> buffer, out int bytesRead) => throw new NotSupportedException();
            public bool WriteMemory(ulong address, Span<byte> buffer, out int bytesWritten) => throw new NotSupportedException();
        }
    }
}
