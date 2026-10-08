// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("1FCF4C14-60C1-44E6-84ED-20506EF3DC60")]
    internal partial interface IClrmaServiceGenerated
    {
        [PreserveSig]
        int AssociateClient(IntPtr client);

        [PreserveSig]
        int GetThread(uint osThreadId, out IClrmaThreadGenerated thread);

        [PreserveSig]
        int GetException(ulong address, out IClrmaExceptionGenerated exception);

        [PreserveSig]
        int GetObjectInspection(out IntPtr objectInspection);

        [PreserveSig]
        int SetModuleEnumerationPolicy(uint moduleEnumerationPolicy);
    }
}
