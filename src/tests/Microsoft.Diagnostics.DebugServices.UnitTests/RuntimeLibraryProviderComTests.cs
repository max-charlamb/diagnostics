// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting;
using SOS.Hosting.Interop;
using Xunit;
using static Microsoft.Diagnostics.DebugServices.UnitTests.DataTargetComTests;
using static Microsoft.Diagnostics.DebugServices.UnitTests.ScopedComTests;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe class RuntimeLibraryProviderComTests
    {
        [Fact]
        public void DisposeReleasesOnlyTheCreatorReference()
        {
            using RuntimeLibraryProvider provider = new(() => "dbi.dll", () => "dac.dll", false);
            IntPtr pointer = provider.ILibraryProvider;
            Assert.Equal(2u, AddRef(pointer));
            Assert.Equal(1u, ReleaseReference(pointer));
            IntPtr retained = QueryInterface(pointer, typeof(ICLRDebuggingLibraryProvider2Generated).GUID);
            try
            {
                Assert.Equal(pointer, retained);
                Guid unknown = new("00000000-0000-0000-C000-000000000046");
                IntPtr identity = QueryInterface(pointer, unknown);
                Release(identity);
                ((IDisposable)provider).Dispose();
                ((IDisposable)provider).Dispose();
                Assert.Equal(IntPtr.Zero, provider.ILibraryProvider);
                Assert.Equal(2u, AddRef(retained));
                Assert.Equal(1u, ReleaseReference(retained));
            }
            finally
            {
                Assert.Equal(0u, ReleaseReference(retained));
            }
        }

        [Theory]
        [InlineData("mscordbi.dll", true)]
        [InlineData("MSCORDBI.DLL", true)]
        [InlineData("libmscordbi.so", true)]
        [InlineData("mscordaccore.dll", false)]
        [InlineData("mscordacwks.dll", false)]
        [InlineData("unknown.dll", false)]
        [InlineData(null, false)]
        public void SelectsPathsAndReturnsCallerOwnedUtf16Strings(string fileName, bool useDbi)
        {
            int dbiCalls = 0;
            int dacCalls = 0;
            string dbiPath = "C:\\runtime-DBI-\u03A9\\mscordbi.dll";
            string dacPath = "C:\\runtime-DAC-\u03A9\\mscordaccore.dll";
            using RuntimeLibraryProvider provider = new(
                () => { dbiCalls++; return dbiPath; },
                () => { dacCalls++; return dacPath; },
                false);
            IntPtr pointer = provider.ILibraryProvider;
            delegate* unmanaged<IntPtr, char*, uint, uint, IntPtr*, int> provideLibrary =
                (delegate* unmanaged<IntPtr, char*, uint, uint, IntPtr*, int>)VTable(pointer)[3];
            IntPtr first = IntPtr.Zero;
            IntPtr second = IntPtr.Zero;
            try
            {
                fixed (char* name = fileName)
                {
                    Assert.Equal(fileName, Marshal.PtrToStringUni((IntPtr)name));
                    Assert.Equal(HResult.S_OK, provideLibrary(pointer, name, 0xDEADBEEF, 0xDEADBEEF, &first));
                    Assert.Equal(HResult.S_OK, provideLibrary(pointer, name, 0, 0, &second));
                }
                Assert.NotEqual(IntPtr.Zero, first);
                Assert.NotEqual(IntPtr.Zero, second);
                Assert.NotEqual(first, second);
                Assert.Equal(useDbi ? 2 : 0, dbiCalls);
                Assert.Equal(useDbi ? 0 : 2, dacCalls);
                ((IDisposable)provider).Dispose();
                Assert.Equal(useDbi ? dbiPath : dacPath, Marshal.PtrToStringUni(first));
                Assert.Equal(useDbi ? dbiPath : dacPath, Marshal.PtrToStringUni(second));
            }
            finally
            {
                Marshal.FreeCoTaskMem(first);
                Marshal.FreeCoTaskMem(second);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingPathsReturnNoInterfaceAndNullOutput(string path)
        {
            using RuntimeLibraryProvider provider = new(() => path, () => path, false);
            IntPtr pointer = provider.ILibraryProvider;
            IntPtr output = new(1);
            fixed (char* name = "mscordaccore.dll")
            {
                Assert.Equal(HResult.E_NOINTERFACE,
                    ((delegate* unmanaged<IntPtr, char*, uint, uint, IntPtr*, int>)VTable(pointer)[3])(pointer, name, 0, 0, &output));
            }
            Assert.Equal(IntPtr.Zero, output);
        }

        [Fact]
        public void ResolverExceptionsReturnNoInterfaceAndNullOutput()
        {
            using RuntimeLibraryProvider provider = new(
                () => throw new IOException("Unable to resolve DBI"),
                () => throw new IOException("Unable to resolve DAC"),
                false);
            IntPtr pointer = provider.ILibraryProvider;
            delegate* unmanaged<IntPtr, char*, uint, uint, IntPtr*, int> provideLibrary =
                (delegate* unmanaged<IntPtr, char*, uint, uint, IntPtr*, int>)VTable(pointer)[3];
            IntPtr output = new(1);
            fixed (char* name = "mscordbi.dll")
            {
                Assert.Equal(HResult.E_NOINTERFACE, provideLibrary(pointer, name, 0, 0, &output));
            }
            Assert.Equal(IntPtr.Zero, output);
            output = new(1);
            fixed (char* name = "mscordaccore.dll")
            {
                Assert.Equal(HResult.E_NOINTERFACE, provideLibrary(pointer, name, 0, 0, &output));
            }
            Assert.Equal(IntPtr.Zero, output);
        }

        [Fact]
        public void VerifiedFilesStayLockedUntilDispose()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Skip("Authenticode verification is Windows-only");
            }
            string dacPath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "mscordaccore.dll");
            if (!File.Exists(dacPath))
            {
                Assert.Skip($"Runtime DAC not found: {dacPath}");
            }
            using (FileStream stream = File.OpenRead(dacPath))
            using (PEReader reader = new(stream))
            {
                if (reader.PEHeaders.PEHeader?.CertificateTableDirectory.Size is not > 0)
                {
                    Assert.Skip("Runtime DAC is unsigned");
                }
            }
            string path = Path.Combine(Path.GetTempPath(), $"sos-library-provider-{Guid.NewGuid():N}.dll");
            try
            {
                File.Copy(dacPath, path);
                using RuntimeLibraryProvider provider = new(() => path, () => path, true);
                IntPtr pointer = provider.ILibraryProvider;
                delegate* unmanaged<IntPtr, char*, uint, uint, IntPtr*, int> provideLibrary =
                    (delegate* unmanaged<IntPtr, char*, uint, uint, IntPtr*, int>)VTable(pointer)[3];
                for (int i = 0; i < 2; i++)
                {
                    IntPtr output = IntPtr.Zero;
                    try
                    {
                        fixed (char* name = "mscordaccore.dll")
                        {
                            Assert.Equal(HResult.S_OK, provideLibrary(pointer, name, 0, 0, &output));
                        }
                        Assert.Equal(path, Marshal.PtrToStringUni(output));
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(output);
                    }
                }
                Assert.Throws<IOException>(() =>
                {
                    using FileStream exclusive = new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                });
                ((IDisposable)provider).Dispose();
                ((IDisposable)provider).Dispose();
                Assert.Equal(IntPtr.Zero, provider.ILibraryProvider);
                using FileStream unlocked = new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void FailedVerificationReleasesItsFileLock()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Skip("Authenticode verification is Windows-only");
            }
            string path = Path.Combine(Path.GetTempPath(), $"sos-library-provider-{Guid.NewGuid():N}.dll");
            try
            {
                File.Copy(typeof(RuntimeLibraryProviderComTests).Assembly.Location, path);
                using RuntimeLibraryProvider provider = new(() => path, () => path, true);
                IntPtr pointer = provider.ILibraryProvider;
                IntPtr output = new(1);
                fixed (char* name = "mscordaccore.dll")
                {
                    Assert.Equal(HResult.E_NOINTERFACE,
                        ((delegate* unmanaged<IntPtr, char*, uint, uint, IntPtr*, int>)VTable(pointer)[3])(pointer, name, 0, 0, &output));
                }
                Assert.Equal(IntPtr.Zero, output);
                using FileStream unlocked = new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
