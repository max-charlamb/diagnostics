// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Microsoft.Diagnostics.DebugServices.Implementation;
using SOS.Hosting;
using SOS.Hosting.Interop;
using Xunit;
using static Microsoft.Diagnostics.DebugServices.UnitTests.ScopedComTests;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe partial class SymbolComTests
    {
        private const int E_FAIL = unchecked((int)0x80004005);
        private const int E_INVALIDARG = unchecked((int)0x80070057);

        [Theory]
        [InlineData(0, false)]
        [InlineData(256, true)]
        public void SymbolInterfacePreservesAbiAndHandleOwnership(int nativeBool, bool isFileLayout)
        {
            TestSymbols symbols = new();
            SymbolServiceWrapper wrapper = new(symbols, symbols);
            using NativeInterface<ISymbolServiceGenerated> native = new(wrapper);
            IntPtr pointer = native.Pointer;
            AssertInterface(pointer, typeof(ISymbolServiceGenerated).GUID, 12);
            IntPtr* vtable = *(IntPtr**)pointer;

            delegate* unmanaged<IntPtr, byte*, byte> parsePath = (delegate* unmanaged<IntPtr, byte*, byte>)vtable[3];
            Assert.Equal((byte)0, parsePath(pointer, null));
            byte[] pathBytes = Encoding.ASCII.GetBytes("cache*test\0");
            fixed (byte* path = pathBytes)
            {
                Assert.Equal((byte)1, parsePath(pointer, path));
            }
            Assert.Equal("cache*test", symbols.SymbolPath);
            Assert.Equal(1, symbols.DisableCount);

            delegate* unmanaged<IntPtr, char*, int, ulong, uint, ulong, uint, IntPtr> load =
                (delegate* unmanaged<IntPtr, char*, int, ulong, uint, ulong, uint, IntPtr>)vtable[4];
            Assert.Equal(IntPtr.Zero, load(pointer, null, nativeBool, 0, 0, 0, 0));
            IntPtr handle;
            fixed (char* assembly = "assembly-\u03a9.dll")
            {
                handle = load(pointer, assembly, nativeBool, 0x1234, 16, 0, 0);
            }
            Assert.NotEqual(IntPtr.Zero, handle);
            Assert.Equal("assembly-\u03a9.dll", symbols.AssemblyPath);
            Assert.Equal(isFileLayout, symbols.IsFileLayout);

            try
            {
                delegate* unmanaged<IntPtr, IntPtr, byte*, int, int*, int*, int> resolve =
                    (delegate* unmanaged<IntPtr, IntPtr, byte*, int, int*, int*, int>)vtable[6];
                int token = 0;
                int offset = 0;
                byte[] fileBytes = Encoding.ASCII.GetBytes("source.cs\0");
                fixed (byte* file = fileBytes)
                {
                    Assert.Equal(1, resolve(pointer, handle, file, 42, &token, &offset));
                }
                Assert.Equal("source.cs", symbols.SourcePath);
                Assert.Equal(0x06000001, token);
                Assert.Equal(7, offset);

                delegate* unmanaged<IntPtr, IntPtr, int, int, IntPtr*, int> local =
                    (delegate* unmanaged<IntPtr, IntPtr, int, int, IntPtr*, int>)vtable[7];
                IntPtr name = IntPtr.Zero;
                Assert.Equal(1, local(pointer, handle, token, 0, &name));
                try
                {
                    Assert.Equal("local-\u03a9\0tail", Marshal.PtrToStringBSTR(name));
                }
                finally
                {
                    Marshal.FreeBSTR(name);
                }

                delegate* unmanaged<IntPtr, IntPtr, int, long, int*, IntPtr*, int> line =
                    (delegate* unmanaged<IntPtr, IntPtr, int, long, int*, IntPtr*, int>)vtable[8];
                int lineNumber = 0;
                const long IlOffset = 0xDEADBEEF1234;
                Assert.Equal(1, line(pointer, handle, token, IlOffset, &lineNumber, &name));
                try
                {
                    Assert.Equal(42, lineNumber);
                    Assert.Equal(IlOffset, symbols.IlOffset);
                    Assert.Equal("source-\u03a9.cs", Marshal.PtrToStringBSTR(name));
                }
                finally
                {
                    Marshal.FreeBSTR(name);
                }

                symbols.ResultsAvailable = false;
                name = new IntPtr(1);
                Assert.Equal(0, local(pointer, handle, token, 0, &name));
                Assert.Equal(IntPtr.Zero, name);
                Assert.Equal(0, line(pointer, handle, token, IlOffset, &lineNumber, &name));
                Assert.Equal(IntPtr.Zero, name);
            }
            finally
            {
                ((delegate* unmanaged<IntPtr, IntPtr, void>)vtable[5])(pointer, handle);
            }
            Assert.Equal(1, symbols.DisposeCount);

            delegate* unmanaged<IntPtr, byte*, ulong> expression = (delegate* unmanaged<IntPtr, byte*, ulong>)vtable[9];
            byte[] expressionBytes = Encoding.ASCII.GetBytes("0xDEADBEEFDEADBEEF\0");
            fixed (byte* value = expressionBytes)
            {
                Assert.Equal(0xDEADBEEFDEADBEEFul, expression(pointer, value));
            }
            Assert.Equal(0ul, expression(pointer, null));

            byte* metadata = stackalloc byte[16];
            byte* mvid = stackalloc byte[16];
            uint size = uint.MaxValue;
            fixed (char* image = "assembly.dll")
            {
                delegate* unmanaged<IntPtr, char*, uint, uint, byte*, uint, uint, uint, IntPtr, IntPtr, int> getMetadata =
                    (delegate* unmanaged<IntPtr, char*, uint, uint, byte*, uint, uint, uint, IntPtr, IntPtr, int>)vtable[10];
                Assert.Equal(E_FAIL, getMetadata(pointer, image, 1, 1, mvid, 0, 0, 16, (IntPtr)metadata, (IntPtr)(&size)));
                Assert.Equal(0u, size);
                Assert.Equal(E_INVALIDARG, getMetadata(pointer, image, 1, 1, mvid, 0, 0, 16, IntPtr.Zero, (IntPtr)(&size)));

                char* pathBuffer = stackalloc char[16];
                size = uint.MaxValue;
                delegate* unmanaged<IntPtr, char*, uint, uint, uint, IntPtr, IntPtr, int> getCorDebugMetadata =
                    (delegate* unmanaged<IntPtr, char*, uint, uint, uint, IntPtr, IntPtr, int>)vtable[11];
                Assert.Equal(E_FAIL, getCorDebugMetadata(pointer, image, 1, 1, 16, (IntPtr)(&size), (IntPtr)pathBuffer));
                Assert.Equal(0u, size);
            }
        }

        [Fact]
        public void MetadataGuidIsExactlySixteenBytes()
        {
            MetadataProbe probe = new();
            using NativeInterface<ISymbolServiceGenerated> native = new(probe);
            IntPtr* vtable = *(IntPtr**)native.Pointer;
            byte[] bytes = [0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0x42];
            fixed (byte* mvid = bytes)
            {
                delegate* unmanaged<IntPtr, char*, uint, uint, byte*, uint, uint, uint, IntPtr, IntPtr, int> getMetadata =
                    (delegate* unmanaged<IntPtr, char*, uint, uint, byte*, uint, uint, uint, IntPtr, IntPtr, int>)vtable[10];
                Assert.Equal(0, getMetadata(native.Pointer, null, 1, 1, mvid, 0, 0, 0, IntPtr.Zero, IntPtr.Zero));
            }
            Assert.Equal(bytes[..16], probe.Mvid);
        }

        [GeneratedComClass]
        private sealed partial class MetadataProbe : ISymbolServiceGenerated
        {
            internal byte[] Mvid { get; private set; }
            public bool ParseSymbolPath(string symbolPath) => throw new NotSupportedException();
            public IntPtr LoadSymbolsForModule(string assemblyPath, bool isFileLayout, ulong loadedPeAddress, uint loadedPeSize, ulong pdbAddress, uint pdbSize)
                => throw new NotSupportedException();
            public void Dispose(IntPtr symbolReaderHandle) => throw new NotSupportedException();
            public bool ResolveSequencePoint(IntPtr handle, string path, int lineNumber, out int token, out int offset) => throw new NotSupportedException();
            public bool GetLocalVariableName(IntPtr handle, int token, int index, out string name) => throw new NotSupportedException();
            public bool GetLineByILOffset(IntPtr handle, int token, long offset, out int line, out string name) => throw new NotSupportedException();
            public ulong GetExpressionValue(string expression) => throw new NotSupportedException();
            public int GetMetadataLocator(string imagePath, uint timestamp, uint size, byte[] mvid, uint mdRva, uint flags,
                uint bufferSize, IntPtr metadata, IntPtr metadataSize)
            {
                Mvid = mvid;
                return 0;
            }
            public int GetICorDebugMetadataLocator(string imagePath, uint timestamp, uint size, uint bufferSize, IntPtr sizeNeeded, IntPtr buffer)
                => throw new NotSupportedException();
        }

        private sealed class TestSymbols : ISymbolService, ISymbolFile, IMemoryService, IDisposable
        {
            internal string SymbolPath { get; private set; }
            internal string AssemblyPath { get; private set; }
            internal string SourcePath { get; private set; }
            internal bool IsFileLayout { get; private set; }
            internal long IlOffset { get; private set; }
            internal int DisableCount { get; private set; }
            internal int DisposeCount { get; private set; }
            internal bool ResultsAvailable { get; set; } = true;
            public IServiceEvent OnChangeEvent { get; } = new ServiceEvent();
            public bool IsSymbolStoreEnabled => false;
            public string DefaultSymbolPath => null;
            public string DefaultSymbolCache => null;
            public int DefaultTimeout => 1;
            public int DefaultRetryCount => 0;
            public void Reset() => throw new NotSupportedException();
            public bool ParseSymbolPath(string symbolPath)
            {
                SymbolPath = symbolPath;
                return true;
            }
            public bool AddSymwebSymbolServer(bool interactive = false, int? timeoutInMinutes = null, int? retryCount = null) => throw new NotSupportedException();
            public bool AddAuthenticatedSymbolServer(string accessToken, string symbolServerPath = null, int? timeoutInMinutes = null, int? retryCount = null)
                => throw new NotSupportedException();
            public bool AddSymbolServer(string symbolServerPath = null, int? timeoutInMinutes = null, int? retryCount = null) => throw new NotSupportedException();
            public void AddCachePath(string symbolCachePath) => throw new NotSupportedException();
            public void AddDirectoryPath(string symbolDirectoryPath) => throw new NotSupportedException();
            public void DisableSymbolStore() => DisableCount++;
            public string DownloadModuleFile(IModule module) => throw new NotSupportedException();
            public string DownloadSymbolFile(IModule module) => throw new NotSupportedException();
            public string DownloadFile(string index, string file) => throw new NotSupportedException();
            public ISymbolFile OpenSymbolFile(string assemblyPath, bool isFileLayout, Stream peStream)
            {
                AssemblyPath = assemblyPath;
                IsFileLayout = isFileLayout;
                peStream.Dispose();
                return this;
            }
            public ISymbolFile OpenSymbolFile(Stream pdbStream) => throw new NotSupportedException();
            public bool ResolveSequencePoint(string filePath, int lineNumber, out int methodToken, out int ilOffset)
            {
                SourcePath = filePath;
                methodToken = 0x06000001;
                ilOffset = 7;
                return ResultsAvailable;
            }
            public bool GetSourceLineByILOffset(int methodToken, long ilOffset, out int lineNumber, out string fileName)
            {
                IlOffset = ilOffset;
                lineNumber = 42;
                fileName = "source-\u03a9.cs";
                return ResultsAvailable;
            }
            public bool GetLocalVariableByIndex(int methodToken, int localIndex, out string localVarName)
            {
                localVarName = "local-\u03a9\0tail";
                return ResultsAvailable;
            }
            public int PointerSize => 8;
            public bool ReadMemory(ulong address, Span<byte> buffer, out int bytesRead) => throw new NotSupportedException();
            public bool WriteMemory(ulong address, Span<byte> buffer, out int bytesWritten) => throw new NotSupportedException();
            public void Dispose() => DisposeCount++;
        }
    }
}
