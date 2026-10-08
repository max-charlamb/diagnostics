// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting;
using SOS.Hosting.Interop;
using Xunit;
using static Microsoft.Diagnostics.DebugServices.UnitTests.DataTargetComTests;
using static Microsoft.Diagnostics.DebugServices.UnitTests.ScopedComTests;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe class CorDebugDataTargetComTests
    {
        [Fact]
        public void InterfacesShareIdentityAndRetainNativeReferences()
        {
            TestDataServices services = new();
            using CorDebugDataTargetWrapper wrapper = new(services.Container, services);
            IntPtr pointer = wrapper.ICorDebugDataTarget;
            Assert.Equal(2u, AddRef(pointer));
            Assert.Equal(1u, ReleaseReference(pointer));
            Guid[] interfaces =
            [
                typeof(ICorDebugDataTargetGenerated).GUID,
                typeof(ICorDebugMutableDataTargetGenerated).GUID,
                typeof(ICorDebugDataTarget4Generated).GUID,
                typeof(ICorDebugMetaDataLocatorGenerated).GUID,
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

            IntPtr retained = QueryInterface(pointer, typeof(ICorDebugMutableDataTargetGenerated).GUID);
            try
            {
                wrapper.Dispose();
                wrapper.Dispose();
                Assert.Equal(IntPtr.Zero, wrapper.ICorDebugDataTarget);
                CorDebugPlatform platform = default;
                Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, CorDebugPlatform*, int>)VTable(retained)[3])(retained, &platform));
                Assert.Equal(CorDebugPlatform.CORDB_PLATFORM_WINDOWS_AMD64, platform);
                Assert.Equal(2u, AddRef(retained));
                Assert.Equal(1u, ReleaseReference(retained));
            }
            finally
            {
                Assert.Equal(0u, ReleaseReference(retained));
            }
        }

        [Theory]
        [InlineData("WINDOWS", Architecture.X64, CorDebugPlatform.CORDB_PLATFORM_WINDOWS_AMD64)]
        [InlineData("WINDOWS", Architecture.X86, CorDebugPlatform.CORDB_PLATFORM_WINDOWS_X86)]
        [InlineData("WINDOWS", Architecture.Arm, CorDebugPlatform.CORDB_PLATFORM_WINDOWS_ARM)]
        [InlineData("WINDOWS", Architecture.Arm64, CorDebugPlatform.CORDB_PLATFORM_WINDOWS_ARM64)]
        [InlineData("LINUX", Architecture.X64, CorDebugPlatform.CORDB_PLATFORM_POSIX_AMD64)]
        [InlineData("LINUX", Architecture.X86, CorDebugPlatform.CORDB_PLATFORM_POSIX_X86)]
        [InlineData("LINUX", Architecture.Arm, CorDebugPlatform.CORDB_PLATFORM_POSIX_ARM)]
        [InlineData("LINUX", Architecture.Arm64, CorDebugPlatform.CORDB_PLATFORM_POSIX_ARM64)]
        [InlineData("LINUX", (Architecture)6, CorDebugPlatform.CORDB_PLATFORM_POSIX_LOONGARCH64)]
        [InlineData("LINUX", (Architecture)9, CorDebugPlatform.CORDB_PLATFORM_POSIX_RISCV64)]
        [InlineData("OSX", Architecture.X64, CorDebugPlatform.CORDB_PLATFORM_POSIX_AMD64)]
        [InlineData("OSX", Architecture.X86, CorDebugPlatform.CORDB_PLATFORM_POSIX_X86)]
        [InlineData("OSX", Architecture.Arm, CorDebugPlatform.CORDB_PLATFORM_POSIX_ARM)]
        [InlineData("OSX", Architecture.Arm64, CorDebugPlatform.CORDB_PLATFORM_POSIX_ARM64)]
        [InlineData("OSX", (Architecture)6, CorDebugPlatform.CORDB_PLATFORM_POSIX_LOONGARCH64)]
        [InlineData("OSX", (Architecture)9, CorDebugPlatform.CORDB_PLATFORM_POSIX_RISCV64)]
        public void PlatformMappingPreservesNativeValues(string operatingSystem, Architecture architecture, CorDebugPlatform expected)
        {
            TestDataServices services = new() { OperatingSystem = OSPlatform.Create(operatingSystem), Architecture = architecture };
            using CorDebugDataTargetWrapper wrapper = new(services.Container, services);
            IntPtr pointer = wrapper.ICorDebugDataTarget;
            CorDebugPlatform platform = default;
            Assert.Equal(HResult.S_OK, ((delegate* unmanaged<IntPtr, CorDebugPlatform*, int>)VTable(pointer)[3])(pointer, &platform));
            Assert.Equal(expected, platform);
        }

        [Theory]
        [InlineData("WINDOWS", (Architecture)6)]
        [InlineData("WINDOWS", (Architecture)9)]
        [InlineData("LINUX", (Architecture)127)]
        [InlineData("OSX", (Architecture)127)]
        [InlineData("UNKNOWN", Architecture.X64)]
        public void UnsupportedPlatformsFail(string operatingSystem, Architecture architecture)
        {
            TestDataServices services = new() { OperatingSystem = OSPlatform.Create(operatingSystem), Architecture = architecture };
            using CorDebugDataTargetWrapper wrapper = new(services.Container, services);
            IntPtr pointer = wrapper.ICorDebugDataTarget;
            CorDebugPlatform platform = default;
            Assert.Equal(HResult.E_FAIL, ((delegate* unmanaged<IntPtr, CorDebugPlatform*, int>)VTable(pointer)[3])(pointer, &platform));
        }

        [Theory]
        [InlineData(4)]
        [InlineData(8)]
        public void MutableTargetIncludesBaseSlotsAndPreservesMemoryBehavior(int pointerSize)
        {
            TestDataServices services = new() { TargetPointerSize = pointerSize };
            using CorDebugDataTargetWrapper wrapper = new(services.Container, services);
            using NativeInterface<ICorDebugMutableDataTargetGenerated> mutable = new(wrapper);
            IntPtr pointer = mutable.Pointer;
            IntPtr* table = VTable(pointer);
            byte* buffer = stackalloc byte[8];
            new Span<byte>(buffer, 8).Fill(0xCC);
            uint count = uint.MaxValue;
            delegate* unmanaged<IntPtr, ulong, byte*, uint, uint*, int> read = (delegate* unmanaged<IntPtr, ulong, byte*, uint, uint*, int>)table[4];
            delegate* unmanaged<IntPtr, ulong, byte*, uint, int> write = (delegate* unmanaged<IntPtr, ulong, byte*, uint, int>)table[6];
            Assert.Equal(HResult.S_OK, read(pointer, ulong.MaxValue, buffer, 8, &count));
            Assert.Equal(4u, count);
            Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, new ReadOnlySpan<byte>(buffer, 4).ToArray());
            Assert.Equal(0xCC, buffer[4]);
            Assert.Equal(pointerSize == 4 ? uint.MaxValue : ulong.MaxValue, services.LastAddress);
            Assert.Equal(HResult.S_OK, write(pointer, ulong.MaxValue, buffer, 4));
            Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, services.WrittenBytes);
            Assert.Equal(pointerSize == 4 ? uint.MaxValue : ulong.MaxValue, services.LastAddress);
            Assert.Equal(HResult.S_OK, read(pointer, 0, buffer, 4, null));
            Assert.Equal(HResult.S_OK, read(pointer, 0, null, 0, &count));
            Assert.Equal(0u, count);
            Assert.Equal(HResult.S_OK, write(pointer, 0, null, 0));
            Assert.Empty(services.WrittenBytes);
            services.MemoryAvailable = false;
            count = uint.MaxValue;
            Assert.Equal(HResult.E_FAIL, read(pointer, 0, buffer, 4, &count));
            Assert.Equal(uint.MaxValue, count);
            Assert.Equal(HResult.E_FAIL, write(pointer, 0, buffer, 4));
            Assert.Equal(HResult.S_OK, read(pointer, 0, null, 0, &count));
            Assert.Equal(0u, count);
            Assert.Equal(HResult.E_NOTIMPL, ((delegate* unmanaged<IntPtr, uint, uint, byte*, int>)table[7])(pointer, 42, 4, buffer));
            Assert.Equal(HResult.E_NOTIMPL, ((delegate* unmanaged<IntPtr, uint, uint, int>)table[8])(pointer, 42, 0xDEADBEEF));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        public void ThreadContextTruncatesAndZeroFills(int contextSize)
        {
            TestDataServices services = new();
            using CorDebugDataTargetWrapper wrapper = new(services.Container, services);
            using NativeInterface<ICorDebugMutableDataTargetGenerated> mutable = new(wrapper);
            IntPtr pointer = mutable.Pointer;
            delegate* unmanaged<IntPtr, uint, uint, uint, byte*, int> getContext = (delegate* unmanaged<IntPtr, uint, uint, uint, byte*, int>)VTable(pointer)[5];
            byte* context = stackalloc byte[9];
            new Span<byte>(context, 9).Fill(0xCC);
            Assert.Equal(HResult.S_OK, getContext(pointer, 42, 0, (uint)contextSize, context));
            byte[] expected = new byte[contextSize];
            new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }.AsSpan(0, Math.Min(4, contextSize)).CopyTo(expected);
            Assert.Equal(expected, new ReadOnlySpan<byte>(context, contextSize).ToArray());
            Assert.Equal(0xCC, context[contextSize]);
            Assert.Equal(HResult.S_OK, getContext(pointer, 42, 0, 0, null));
            Assert.Equal(HResult.E_INVALIDARG, getContext(pointer, 13, 0, 4, context));
            Assert.Equal(HResult.E_INVALIDARG, getContext(pointer, 42, 0, uint.MaxValue, context));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UnwindUpdatesNativeBufferAndPreservesErrors(bool enabled)
        {
            TestDataServices services = new(enabled);
            using CorDebugDataTargetWrapper wrapper = new(services.Container, services);
            using NativeInterface<ICorDebugDataTarget4Generated> unwind = new(wrapper);
            IntPtr pointer = unwind.Pointer;
            delegate* unmanaged<IntPtr, uint, uint, byte*, int> virtualUnwind = (delegate* unmanaged<IntPtr, uint, uint, byte*, int>)VTable(pointer)[3];
            byte* context = stackalloc byte[5];
            new Span<byte>(context, 5).Fill(0xCC);
            Assert.Equal(enabled ? HResult.S_OK : HResult.E_NOTIMPL, virtualUnwind(pointer, 42, 4, context));
            Assert.Equal(enabled ? (byte)0xEF : (byte)0xCC, context[3]);
            Assert.Equal(0xCC, context[4]);
            Assert.Equal(HResult.E_INVALIDARG, virtualUnwind(pointer, 42, uint.MaxValue, context));
            services.UnwindThrows = true;
            Assert.Equal(enabled ? HResult.E_INVALIDARG : HResult.E_NOTIMPL, virtualUnwind(pointer, 42, 4, context));
        }

        [Fact]
        public void MetadataLocatorMarshalsUtf16PathsAndBufferSizes()
        {
            TestDataServices services = new() { DownloadPath = "C:\\symbols\\module-\u03A9.dll" };
            using CorDebugDataTargetWrapper wrapper = new(services.Container, services);
            using NativeInterface<ICorDebugMetaDataLocatorGenerated> metadata = new(wrapper);
            IntPtr pointer = metadata.Pointer;
            delegate* unmanaged<IntPtr, char*, uint, uint, uint, uint*, char*, int> getMetadata =
                (delegate* unmanaged<IntPtr, char*, uint, uint, uint, uint*, char*, int>)VTable(pointer)[3];
            uint expectedSize = (uint)services.DownloadPath.Length + 1;
            char* buffer = stackalloc char[(int)expectedSize + 1];
            new Span<char>(buffer, (int)expectedSize + 1).Fill('?');
            uint size = 0;
            fixed (char* path = "module-\u03A9.dll")
            {
                Assert.Equal(HResult.S_OK, getMetadata(pointer, path, 1, 1, expectedSize + 1, &size, buffer));
                Assert.Equal(expectedSize, size);
                Assert.Equal(services.DownloadPath, new string(buffer));
                Assert.Equal('?', buffer[expectedSize]);
                Assert.Equal(HResult.S_OK, getMetadata(pointer, path, 1, 1, expectedSize + 1, null, buffer));
                buffer[0] = '?';
                Assert.Equal(unchecked((int)0x8007007A), getMetadata(pointer, path, 1, 1, expectedSize, &size, buffer));
                Assert.Equal(expectedSize, size);
                Assert.Equal('?', buffer[0]);
                services.DownloadPath = null;
                Assert.Equal(HResult.E_FAIL, getMetadata(pointer, path, 1, 1, expectedSize + 1, &size, buffer));
                Assert.Equal(0u, size);
                services.SymbolStoreEnabled = false;
                Assert.Equal(HResult.E_FAIL, getMetadata(pointer, path, 1, 1, expectedSize + 1, &size, buffer));
                Assert.Equal(0u, size);
            }
        }
    }
}
