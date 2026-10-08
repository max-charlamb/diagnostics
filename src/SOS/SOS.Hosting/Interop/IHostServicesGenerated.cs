// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("27B2CB8D-BDEE-4CBD-B6EF-75880D76D46F")]
    internal partial interface IHostServicesGenerated
    {
        [PreserveSig]
        int GetHost(out IHostGenerated host);

        [PreserveSig]
        int RegisterDebuggerServices(IntPtr debuggerServices);

        [PreserveSig]
        int CreateTarget();

        [PreserveSig]
        int UpdateTarget(uint processId);

        [PreserveSig]
        void FlushTarget();

        [PreserveSig]
        void DestroyTarget();

        [PreserveSig]
        int DispatchCommand(
            [MarshalAs(UnmanagedType.LPStr)] string commandName,
            [MarshalAs(UnmanagedType.LPStr)] string arguments,
            [MarshalAs(UnmanagedType.I1)] bool displayCommandNotFound);

        [PreserveSig]
        void Uninitialize();
    }
}
