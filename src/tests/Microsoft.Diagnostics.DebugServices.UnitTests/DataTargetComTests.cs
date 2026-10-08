// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.DebugServices.Implementation;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop;
using Xunit;
using static Microsoft.Diagnostics.DebugServices.UnitTests.ScopedComTests;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe class DataTargetComTests
    {
        [Fact]
        public void InterfacesShareIdentityAndRetainNativeReferences()
        {
            TestDataServices services = new();
            using DataTargetWrapper wrapper = new(services.Container, services);
            IntPtr pointer = wrapper.IDataTarget;
            Assert.Equal(2u, AddRef(pointer));
            Assert.Equal(1u, ReleaseReference(pointer));
            Guid[] interfaces =
            [
                typeof(ICLRDataTargetGenerated).GUID,
                typeof(ICLRDataTarget2Generated).GUID,
                typeof(ICorDebugDataTarget4Generated).GUID,
                typeof(ICLRMetadataLocatorGenerated).GUID,
                typeof(ICLRRuntimeLocatorGenerated).GUID,
                typeof(ICLRContractLocatorGenerated).GUID,
                typeof(ICLRSymbolProviderGenerated).GUID,
            ];
            Guid unknown = new("00000000-0000-0000-C000-000000000046");
            IntPtr identity = QueryInterface(pointer, unknown);
            try
            {
                foreach (Guid iid in interfaces)
                {
                    IntPtr queried = QueryInterface(pointer, iid);
                    IntPtr queriedIdentity = QueryInterface(queried, unknown);
                    try
                    {
                        Assert.Equal(identity, queriedIdentity);
                    }
                    finally
                    {
                        Release(queriedIdentity);
                        Release(queried);
                    }
                }
                Guid unsupported = new("DEADBEEF-DEAD-BEEF-DEAD-BEEFDEADBEEF");
                IntPtr result = new(1);
                Assert.Equal(HResult.E_NOINTERFACE, ((delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)VTable(pointer)[0])(pointer, &unsupported, &result));
                Assert.Equal(IntPtr.Zero, result);
            }
            finally
            {
                Release(identity);
            }

            IntPtr retained = QueryInterface(pointer, typeof(ICLRDataTarget2Generated).GUID);
            try
            {
                wrapper.Dispose();
                wrapper.Dispose();
                Assert.Equal(IntPtr.Zero, wrapper.IDataTarget);
                uint pointerSize = 0;
                Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, uint*, int>)VTable(retained)[4])(retained, &pointerSize));
                Assert.Equal(8u, pointerSize);
                Assert.Equal(2u, AddRef(retained));
                Assert.Equal(1u, ReleaseReference(retained));
            }
            finally
            {
                Assert.Equal(0u, ReleaseReference(retained));
            }
        }

        [Theory]
        [InlineData(4)]
        [InlineData(8)]
        public void MemoryCallbacksPreserveSizesAndAddressMasking(int pointerSize)
        {
            TestDataServices services = new() { TargetPointerSize = pointerSize };
            using DataTargetWrapper wrapper = new(services.Container, services);
            IntPtr pointer = wrapper.IDataTarget;
            IntPtr* table = VTable(pointer);
            IMAGE_FILE_MACHINE machine = default;
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, IMAGE_FILE_MACHINE*, int>)table[3])(pointer, &machine));
            Assert.Equal(IMAGE_FILE_MACHINE.AMD64, machine);
            uint size = 0;
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, uint*, int>)table[4])(pointer, &size));
            Assert.Equal((uint)pointerSize, size);
            byte* buffer = stackalloc byte[4];
            uint count = uint.MaxValue;
            delegate* unmanaged<IntPtr, ulong, byte*, uint, uint*, int> read = (delegate* unmanaged<IntPtr, ulong, byte*, uint, uint*, int>)table[6];
            delegate* unmanaged<IntPtr, ulong, byte*, uint, uint*, int> write = (delegate* unmanaged<IntPtr, ulong, byte*, uint, uint*, int>)table[7];
            Assert.Equal(HResult.S_OK, read(pointer, ulong.MaxValue, buffer, 4, &count));
            Assert.Equal(4u, count);
            Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, new ReadOnlySpan<byte>(buffer, 4).ToArray());
            Assert.Equal(pointerSize == 4 ? uint.MaxValue : ulong.MaxValue, services.LastAddress);
            Assert.Equal(HResult.S_OK, write(pointer, ulong.MaxValue, buffer, 4, &count));
            Assert.Equal(4u, count);
            Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, services.WrittenBytes);
            Assert.Equal(pointerSize == 4 ? uint.MaxValue : ulong.MaxValue, services.LastAddress);
            Assert.Equal(HResult.S_OK, read(pointer, 0, null, 0, &count));
            Assert.Equal(0u, count);
            Assert.Equal(HResult.S_OK, read(pointer, 0, buffer, 4, null));
            Assert.Equal(HResult.S_OK, write(pointer, 0, buffer, 4, null));
            services.MemoryAvailable = false;
            Assert.Equal(HResult.E_FAIL, read(pointer, 0, buffer, 4, &count));
            Assert.Equal(0u, count);
            Assert.Equal(HResult.E_FAIL, write(pointer, 0, buffer, 4, &count));
            Assert.Equal(0u, count);
        }

        [Fact]
        public void BaseCallbacksDispatchStringsThreadsAndUnsupportedSlots()
        {
            TestDataServices services = new();
            using DataTargetWrapper wrapper = new(services.Container, services);
            IntPtr pointer = wrapper.IDataTarget;
            IntPtr* table = VTable(pointer);
            delegate* unmanaged<IntPtr, char*, ulong*, int> imageBase = (delegate* unmanaged<IntPtr, char*, ulong*, int>)table[5];
            ulong address = 0;
            fixed (char* path = "coreclr.dll")
            {
                Assert.Equal(HResult.S_OK, imageBase(pointer, path, &address));
                Assert.Equal(services.ImageBase, address);
            }
            fixed (char* path = "missing.dll")
            {
                Assert.Equal(HResult.E_FAIL, imageBase(pointer, path, &address));
                Assert.Equal(0ul, address);
            }
            uint threadId = 0;
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, uint*, int>)table[10])(pointer, &threadId));
            Assert.Equal(42u, threadId);
            services.Container.RemoveService(typeof(IThread));
            Assert.Equal(HResult.E_FAIL, ((delegate* unmanaged<IntPtr, uint*, int>)table[10])(pointer, &threadId));
            Assert.Equal(0u, threadId);
            byte* context = stackalloc byte[4];
            delegate* unmanaged<IntPtr, uint, uint, uint, byte*, int> getContext = (delegate* unmanaged<IntPtr, uint, uint, uint, byte*, int>)table[11];
            Assert.Equal(HResult.S_OK, getContext(pointer, 42, 0, 4, context));
            Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, new ReadOnlySpan<byte>(context, 4).ToArray());
            Assert.Equal(HResult.E_INVALIDARG, getContext(pointer, 13, 0, 4, context));
            Assert.Equal(HResult.E_INVALIDARG, getContext(pointer, 42, 0, uint.MaxValue, context));
            Assert.Equal(HResult.E_NOTIMPL, ((delegate* unmanaged<IntPtr, uint, uint, ulong*, int>)table[8])(pointer, 42, 0, &address));
            Assert.Equal(HResult.E_NOTIMPL, ((delegate* unmanaged<IntPtr, uint, uint, ulong, int>)table[9])(pointer, 42, 0, 0));
            Assert.Equal(HResult.E_NOTIMPL, ((delegate* unmanaged<IntPtr, uint, uint, byte*, int>)table[12])(pointer, 42, 4, context));
            Assert.Equal(HResult.E_NOTIMPL, ((delegate* unmanaged<IntPtr, uint, uint, byte*, uint, byte*, int>)table[13])(pointer, 0, 4, context, 4, context));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OptionalAllocationAndUnwindServices(bool enabled)
        {
            TestDataServices services = new(enabled);
            using DataTargetWrapper wrapper = new(services.Container, services);
            using NativeInterface<ICLRDataTarget2Generated> target = new(wrapper);
            IntPtr pointer = target.Pointer;
            IntPtr* table = VTable(pointer);
            ulong address = 0;
            Assert.Equal(enabled ? HResult.S_OK : HResult.E_NOTIMPL,
                ((delegate* unmanaged<IntPtr, ulong, uint, uint, uint, ulong*, int>)table[14])(pointer, 0x1234, 16, 2, 4, &address));
            Assert.Equal(enabled ? 0x1234ul : 0ul, address);
            Assert.Equal(enabled ? HResult.S_OK : HResult.E_NOTIMPL,
                ((delegate* unmanaged<IntPtr, ulong, uint, uint, int>)table[15])(pointer, 0x1234, 16, 2));
            services.MemoryAvailable = false;
            Assert.Equal(enabled ? HResult.E_FAIL : HResult.E_NOTIMPL,
                ((delegate* unmanaged<IntPtr, ulong, uint, uint, uint, ulong*, int>)table[14])(pointer, 0x1234, 16, 2, 4, &address));
            Assert.Equal(enabled ? HResult.E_FAIL : HResult.E_NOTIMPL,
                ((delegate* unmanaged<IntPtr, ulong, uint, uint, int>)table[15])(pointer, 0x1234, 16, 2));

            using NativeInterface<ICorDebugDataTarget4Generated> unwind = new(wrapper);
            delegate* unmanaged<IntPtr, uint, uint, byte*, int> virtualUnwind = (delegate* unmanaged<IntPtr, uint, uint, byte*, int>)VTable(unwind.Pointer)[3];
            byte* context = stackalloc byte[4];
            new Span<byte>(context, 4).Clear();
            Assert.Equal(enabled ? HResult.S_OK : HResult.E_NOTIMPL, virtualUnwind(unwind.Pointer, 42, 4, context));
            Assert.Equal(enabled ? (byte)0xEF : (byte)0, context[3]);
            Assert.Equal(HResult.E_INVALIDARG, virtualUnwind(unwind.Pointer, 42, uint.MaxValue, context));
        }

        [Fact]
        public void RuntimeAndContractLocatorsDispatch()
        {
            TestDataServices services = new();
            using DataTargetWrapper wrapper = new(services.Container, services);
            using NativeInterface<ICLRRuntimeLocatorGenerated> runtime = new(wrapper);
            using NativeInterface<ICLRContractLocatorGenerated> contract = new(wrapper);
            ulong address = 0;
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, ulong*, int>)VTable(runtime.Pointer)[3])(runtime.Pointer, &address));
            Assert.Equal(services.ImageBase, address);
            Assert.Equal(HResult.E_FAIL, ((delegate* unmanaged<IntPtr, ulong*, int>)VTable(contract.Pointer)[3])(contract.Pointer, &address));
            Assert.Equal(0ul, address);
        }

        [Fact]
        public void MetadataLocatorSupportsOptionalGuidAndSizePointers()
        {
            TestDataServices services = new();
            using DataTargetWrapper wrapper = new(services.Container, services);
            using NativeInterface<ICLRMetadataLocatorGenerated> metadata = new(wrapper);
            delegate* unmanaged<IntPtr, char*, uint, uint, Guid*, uint, uint, uint, byte*, uint*, int> getMetadata =
                (delegate* unmanaged<IntPtr, char*, uint, uint, Guid*, uint, uint, uint, byte*, uint*, int>)VTable(metadata.Pointer)[3];
            Guid mvid = new("DEADBEEF-DEAD-BEEF-DEAD-BEEFDEADBEEF");
            using FileStream stream = File.OpenRead(typeof(DataTargetWrapper).Assembly.Location);
            using PEReader reader = new(stream);
            byte[] expected = reader.GetMetadata().GetContent().ToArray();
            byte* buffer = stackalloc byte[32];
            uint size = 0;
            fixed (char* path = "module-\u03A9.dll")
            {
                Assert.Equal(HResult.S_OK, getMetadata(metadata.Pointer, path, 1, 1, &mvid, 0, 0, 32, buffer, &size));
                Assert.Equal((uint)expected.Length, size);
                Assert.Equal(expected.AsSpan(0, 32).ToArray(), new ReadOnlySpan<byte>(buffer, 32).ToArray());
                Assert.Equal(HResult.S_OK, getMetadata(metadata.Pointer, path, 1, 1, null, 0, 0, 32, buffer, null));
                Assert.Equal(HResult.E_INVALIDARG, getMetadata(metadata.Pointer, path, 1, 1, null, 0, 0, 32, null, &size));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SymbolProviderMarshalsNamesAndOptionalOutputs(bool enabled)
        {
            TestDataServices services = new(enabled) { TargetPointerSize = 4 };
            using DataTargetWrapper wrapper = new(services.Container, services);
            using NativeInterface<ICLRSymbolProviderGenerated> symbols = new(wrapper);
            IntPtr pointer = symbols.Pointer;
            IntPtr* table = VTable(pointer);
            delegate* unmanaged<IntPtr, ulong, uint, char*, uint*, ulong*, int> getName =
                (delegate* unmanaged<IntPtr, ulong, uint, char*, uint*, ulong*, int>)table[3];
            char* buffer = stackalloc char[32];
            uint needed = 0;
            ulong displacement = 0;
            Assert.Equal(enabled ? HResult.S_OK : HResult.E_NOTIMPL, getName(pointer, ulong.MaxValue, 32, buffer, &needed, &displacement));
            if (enabled)
            {
                Assert.Equal("symbol-\u03A9", new string(buffer));
                Assert.Equal(9u, needed);
                Assert.Equal(7ul, displacement);
                Assert.Equal((ulong)uint.MaxValue, services.LastAddress);
                Assert.Equal(HResult.S_FALSE, getName(pointer, 0, 3, buffer, null, null));
                Assert.Equal("sy", new string(buffer));
                Assert.Equal(HResult.S_OK, getName(pointer, 0, 0, null, &needed, null));
            }
            Assert.Equal(HResult.E_INVALIDARG, getName(pointer, 0, uint.MaxValue, buffer, null, null));
            ulong address = 0;
            uint offset = 0;
            delegate* unmanaged<IntPtr, ulong, char*, ulong*, int> getAddress = (delegate* unmanaged<IntPtr, ulong, char*, ulong*, int>)table[4];
            delegate* unmanaged<IntPtr, ulong, char*, char*, uint*, int> getOffset = (delegate* unmanaged<IntPtr, ulong, char*, char*, uint*, int>)table[5];
            fixed (char* name = "symbol-\u03A9")
            {
                Assert.Equal(enabled ? HResult.S_OK : HResult.E_NOTIMPL, getAddress(pointer, ulong.MaxValue, name, &address));
                Assert.Equal(enabled ? 0x1234ul : 0ul, address);
                Assert.Equal(enabled ? HResult.S_OK : HResult.E_NOTIMPL, getOffset(pointer, ulong.MaxValue, name, name, &offset));
                Assert.Equal(enabled ? 16u : 0u, offset);
                Assert.Equal(HResult.E_INVALIDARG, getAddress(pointer, 0, name, null));
                Assert.Equal(HResult.E_INVALIDARG, getOffset(pointer, 0, name, name, null));
            }
            if (enabled)
            {
                Assert.Equal(HResult.E_INVALIDARG, getAddress(pointer, 0, null, &address));
                Assert.Equal(HResult.E_INVALIDARG, getOffset(pointer, 0, null, null, &offset));
                services.SymbolsAvailable = false;
                Assert.Equal(HResult.E_FAIL, getName(pointer, 0, 32, buffer, &needed, &displacement));
            }
        }

        internal static IntPtr* VTable(IntPtr pointer) => *(IntPtr**)pointer;
        internal static uint AddRef(IntPtr pointer) => ((delegate* unmanaged<IntPtr, uint>)VTable(pointer)[1])(pointer);
        internal static uint ReleaseReference(IntPtr pointer) => ((delegate* unmanaged<IntPtr, uint>)VTable(pointer)[2])(pointer);

        internal static IntPtr QueryInterface(IntPtr pointer, Guid iid)
        {
            IntPtr result = IntPtr.Zero;
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)VTable(pointer)[0])(pointer, &iid, &result));
            Assert.NotEqual(IntPtr.Zero, result);
            return result;
        }

        internal sealed class TestDataServices : TestServices, IMemoryService, IModuleService, IThreadService, IThread,
            IThreadUnwindService, IRemoteMemoryService, IClrSymbolProvider, ISymbolService
        {
            internal int TargetPointerSize { get; set; } = 8;
            internal bool MemoryAvailable { get; set; } = true;
            internal bool SymbolsAvailable { get; set; } = true;
            internal bool SymbolStoreEnabled { get; set; } = true;
            internal bool UnwindThrows { get; set; }
            internal string DownloadPath { get; set; } = typeof(DataTargetWrapper).Assembly.Location;
            internal ulong LastAddress { get; private set; }
            internal byte[] WrittenBytes { get; private set; }

            internal TestDataServices(bool optionalServices = true)
            {
                Container.AddService<IMemoryService>(this);
                Container.AddService<IModuleService>(this);
                Container.AddService<IThreadService>(this);
                Container.AddService<IThread>(this);
                Container.AddService<ISymbolService>(this);
                if (optionalServices)
                {
                    Container.AddService<IThreadUnwindService>(this);
                    Container.AddService<IRemoteMemoryService>(this);
                    Container.AddService<IClrSymbolProvider>(this);
                }
            }

            int IMemoryService.PointerSize => TargetPointerSize;
            bool IMemoryService.ReadMemory(ulong address, Span<byte> buffer, out int bytesRead)
            {
                LastAddress = address;
                bytesRead = MemoryAvailable ? Math.Min(buffer.Length, 4) : 0;
                new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }.AsSpan(0, bytesRead).CopyTo(buffer);
                return MemoryAvailable;
            }
            bool IMemoryService.WriteMemory(ulong address, Span<byte> buffer, out int bytesWritten)
            {
                LastAddress = address;
                WrittenBytes = buffer.ToArray();
                bytesWritten = MemoryAvailable ? buffer.Length : 0;
                return MemoryAvailable;
            }
            public IEnumerable<IModule> EnumerateModules() => [this];
            public IModule GetModuleFromIndex(int moduleIndex) => throw new NotSupportedException();
            public IModule GetModuleFromBaseAddress(ulong baseAddress) => throw new NotSupportedException();
            public IModule GetModuleFromAddress(ulong address) => throw new NotSupportedException();
            public IEnumerable<IModule> GetModuleFromModuleName(string moduleName) => moduleName == "coreclr.dll" ? [this] : [];
            public IModule CreateModule(int moduleIndex, ulong imageBase, ulong imageSize, string imageName) => throw new NotSupportedException();
            public IModule EntryPointModule => this;
            public IEnumerable<RegisterInfo> Registers => throw new NotSupportedException();
            public int InstructionPointerIndex => 0;
            public int FramePointerIndex => 0;
            public int StackPointerIndex => 0;
            public bool TryGetRegisterIndexByName(string name, out int registerIndex) => throw new NotSupportedException();
            public bool TryGetRegisterInfo(int registerIndex, out RegisterInfo info) => throw new NotSupportedException();
            public IEnumerable<IThread> EnumerateThreads() => [this];
            public IThread GetThreadFromIndex(int threadIndex) => throw new NotSupportedException();
            public IThread GetThreadFromId(uint threadId) => threadId == 42 ? this : throw new DiagnosticsException("Unknown thread");
            public int ThreadIndex => 0;
            public uint ThreadId => 42;
            public bool TryGetRegisterValue(int registerIndex, out ulong value) => throw new NotSupportedException();
            public ReadOnlySpan<byte> GetThreadContext() => new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            public ulong GetThreadTeb() => 0;
            public int Unwind(uint threadId, Span<byte> context)
            {
                if (UnwindThrows)
                {
                    throw new DiagnosticsException("Unable to unwind");
                }
                Assert.Equal(42u, threadId);
                context[^1] = 0xEF;
                return HResult.S_OK;
            }
            public bool AllocateMemory(ulong address, uint size, uint typeFlags, uint protectFlags, out ulong remoteAddress)
            {
                Assert.Equal(16u, size);
                Assert.Equal(2u, typeFlags);
                Assert.Equal(4u, protectFlags);
                remoteAddress = MemoryAvailable ? address : 0;
                return MemoryAvailable;
            }
            public bool FreeMemory(ulong address, uint size, uint typeFlags)
            {
                Assert.Equal(0x1234ul, address);
                Assert.Equal(16u, size);
                Assert.Equal(2u, typeFlags);
                return MemoryAvailable;
            }
            public bool TryGetSymbolName(ulong address, out string symbolName, out ulong displacement)
            {
                LastAddress = address;
                symbolName = SymbolsAvailable ? "symbol-\u03A9" : null;
                displacement = SymbolsAvailable ? 7ul : 0;
                return SymbolsAvailable;
            }
            public bool TryGetSymbolAddress(ulong moduleBase, string symbolName, out ulong address)
            {
                LastAddress = moduleBase;
                Assert.Equal("symbol-\u03A9", symbolName);
                address = 0x1234;
                return SymbolsAvailable;
            }
            public bool TryGetFieldOffset(ulong moduleBase, string typeName, string fieldName, out uint offset)
            {
                LastAddress = moduleBase;
                Assert.Equal("symbol-\u03A9", typeName);
                Assert.Equal("symbol-\u03A9", fieldName);
                offset = 16;
                return SymbolsAvailable;
            }
            public IServiceEvent OnChangeEvent { get; } = new ServiceEvent();
            public bool IsSymbolStoreEnabled => SymbolStoreEnabled;
            public string DefaultSymbolPath => null;
            public string DefaultSymbolCache => null;
            public int DefaultTimeout => 1;
            public int DefaultRetryCount => 0;
            public void Reset() => throw new NotSupportedException();
            public bool ParseSymbolPath(string symbolPath) => throw new NotSupportedException();
            public bool AddSymwebSymbolServer(bool interactive = false, int? timeoutInMinutes = null, int? retryCount = null) => throw new NotSupportedException();
            public bool AddAuthenticatedSymbolServer(string accessToken, string symbolServerPath = null, int? timeoutInMinutes = null, int? retryCount = null) => throw new NotSupportedException();
            public bool AddSymbolServer(string symbolServerPath = null, int? timeoutInMinutes = null, int? retryCount = null) => throw new NotSupportedException();
            public void AddCachePath(string symbolCachePath) => throw new NotSupportedException();
            public void AddDirectoryPath(string symbolDirectoryPath) => throw new NotSupportedException();
            public void DisableSymbolStore() => throw new NotSupportedException();
            public string DownloadModuleFile(IModule module) => throw new NotSupportedException();
            public string DownloadSymbolFile(IModule module) => throw new NotSupportedException();
            public string DownloadFile(string index, string file) => DownloadPath;
            public ISymbolFile OpenSymbolFile(string assemblyPath, bool isFileLayout, Stream peStream) => throw new NotSupportedException();
            public ISymbolFile OpenSymbolFile(Stream pdbStream) => throw new NotSupportedException();
        }
    }
}
