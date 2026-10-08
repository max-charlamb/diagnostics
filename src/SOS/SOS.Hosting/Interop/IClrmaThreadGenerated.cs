// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("9849CFC9-0868-406E-9059-6B04E9ADBBB8")]
    internal partial interface IClrmaThreadGenerated
    {
        [PreserveSig]
        int DebuggerCommand([MarshalAs(UnmanagedType.BStr)] out string command);

        [PreserveSig]
        int OSThreadId(out uint osThreadId);

        [PreserveSig]
        int FrameCount(out int count);

        [PreserveSig]
        int Frame(int index, out ulong ip, out ulong sp,
            [MarshalAs(UnmanagedType.BStr)] out string moduleName,
            [MarshalAs(UnmanagedType.BStr)] out string functionName, out ulong displacement);

        [PreserveSig]
        int CurrentException(out IClrmaExceptionGenerated exception);

        [PreserveSig]
        int NestedExceptionCount(out ushort count);

        [PreserveSig]
        int NestedException(ushort index, out IClrmaExceptionGenerated exception);
    }
}
