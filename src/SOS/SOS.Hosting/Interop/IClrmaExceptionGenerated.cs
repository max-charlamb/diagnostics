// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("7C165652-D539-472E-A6CF-F657FFF31751")]
    internal partial interface IClrmaExceptionGenerated
    {
        [PreserveSig]
        int DebuggerCommand([MarshalAs(UnmanagedType.BStr)] out string command);

        [PreserveSig]
        int GetAddress(out ulong address);

        [PreserveSig]
        int GetHResult(out uint hresult);

        [PreserveSig]
        int GetType([MarshalAs(UnmanagedType.BStr)] out string type);

        [PreserveSig]
        int GetMessage([MarshalAs(UnmanagedType.BStr)] out string message);

        [PreserveSig]
        int FrameCount(out int count);

        [PreserveSig]
        int Frame(int index, out ulong ip, out ulong sp,
            [MarshalAs(UnmanagedType.BStr)] out string moduleName,
            [MarshalAs(UnmanagedType.BStr)] out string functionName, out ulong displacement);

        [PreserveSig]
        int InnerExceptionCount(out ushort count);

        [PreserveSig]
        int InnerException(ushort index, out IClrmaExceptionGenerated exception);
    }
}
