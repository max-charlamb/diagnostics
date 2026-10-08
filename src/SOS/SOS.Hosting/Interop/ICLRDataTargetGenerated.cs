// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("3E11CCEE-D08B-43E5-AF01-32717A64DA03")]
    internal unsafe partial interface ICLRDataTargetGenerated
    {
        [PreserveSig]
        int GetMachineType(out IMAGE_FILE_MACHINE machineType);

        [PreserveSig]
        int GetPointerSize(out uint pointerSize);

        [PreserveSig]
        int GetImageBase([MarshalAs(UnmanagedType.LPWStr)] string imagePath, out ulong baseAddress);

        [PreserveSig]
        int ReadVirtual(ulong address, byte* buffer, uint bytesRequested, uint* bytesRead);

        [PreserveSig]
        int WriteVirtual(ulong address, byte* buffer, uint bytesRequested, uint* bytesWritten);

        [PreserveSig]
        int GetTLSValue(uint threadId, uint index, ulong* value);

        [PreserveSig]
        int SetTLSValue(uint threadId, uint index, ulong value);

        [PreserveSig]
        int GetCurrentThreadID(out uint threadId);

        [PreserveSig]
        int GetThreadContext(uint threadId, uint contextFlags, uint contextSize, byte* context);

        [PreserveSig]
        int SetThreadContext(uint threadId, uint contextSize, byte* context);

        [PreserveSig]
        int Request(uint requestCode, uint inputBufferSize, byte* inputBuffer, uint outputBufferSize, byte* outputBuffer);
    }
}
