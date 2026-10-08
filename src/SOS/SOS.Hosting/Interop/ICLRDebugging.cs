// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting;

namespace Microsoft.Diagnostics
{
    [GeneratedComInterface]
    [Guid("D28F3C5A-9634-4206-A509-477552EEFB10")]
    public partial interface ICLRDebugging
    {
        public static readonly Guid CLSID_ICLRDebugging = new("BACC578D-FBDD-48A4-969F-02D932B74634");

        [PreserveSig]
        int OpenVirtualProcess(
            ulong moduleBaseAddress,
            IntPtr dataTarget,
            IntPtr libraryProvider,
            in ClrDebuggingVersion maxDebuggerSupportedVersion,
            in Guid riidProcess,
            out IntPtr process,
            ref ClrDebuggingVersion version,
            out ClrDebuggingProcessFlags flags);

        [PreserveSig]
        int CanUnloadNow(IntPtr module);
    }
}
