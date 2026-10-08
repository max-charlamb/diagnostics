// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.DebugServices;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("E0CD8534-A88B-40D7-91BA-1B4C925761E9")]
    internal partial interface IHostGenerated
    {
        [PreserveSig]
        HostType GetHostType();

        [PreserveSig]
        int GetService(in Guid serviceId, out IntPtr service);

        [PreserveSig]
        int GetCurrentTarget(out ITargetGenerated target);
    }
}
