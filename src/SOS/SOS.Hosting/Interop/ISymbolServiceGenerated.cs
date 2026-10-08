// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("7EE88D46-F8B3-4645-AD3E-01FE7D4F70F1")]
    internal partial interface ISymbolServiceGenerated
    {
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.I1)]
        bool ParseSymbolPath([MarshalAs(UnmanagedType.LPStr)] string symbolPath);

        [PreserveSig]
        IntPtr LoadSymbolsForModule([MarshalAs(UnmanagedType.LPWStr)] string assemblyPath,
            [MarshalAs(UnmanagedType.Bool)] bool isFileLayout, ulong loadedPeAddress, uint loadedPeSize,
            ulong inMemoryPdbAddress, uint inMemoryPdbSize);

        [PreserveSig]
        void Dispose(IntPtr symbolReaderHandle);

        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool ResolveSequencePoint(IntPtr symbolReaderHandle, [MarshalAs(UnmanagedType.LPStr)] string filePath,
            int lineNumber, out int methodToken, out int ilOffset);

        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetLocalVariableName(IntPtr symbolReaderHandle, int methodToken, int localIndex,
            [MarshalAs(UnmanagedType.BStr)] out string localVarName);

        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetLineByILOffset(IntPtr symbolReaderHandle, int methodToken, long ilOffset, out int lineNumber,
            [MarshalAs(UnmanagedType.BStr)] out string fileName);

        [PreserveSig]
        ulong GetExpressionValue([MarshalAs(UnmanagedType.LPStr)] string expression);

        [PreserveSig]
        int GetMetadataLocator([MarshalAs(UnmanagedType.LPWStr)] string imagePath,
            uint imageTimestamp, uint imageSize, [In, MarshalUsing(ConstantElementCount = 16)] byte[] mvid,
            uint mdRva, uint flags, uint bufferSize, IntPtr metadata, IntPtr metadataSize);

        [PreserveSig]
        int GetICorDebugMetadataLocator([MarshalAs(UnmanagedType.LPWStr)] string imagePath,
            uint imageTimestamp, uint imageSize, uint pathBufferSize, IntPtr pathBufferSizeNeeded, IntPtr pathBuffer);
    }
}
