// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("EDBED635-372E-4DAB-BBFE-ED0D2F63BE81")]
    internal unsafe partial interface IDebugClient2Generated : IDebugClientGenerated
    {
        [PreserveSig]
        int WriteDumpFile2([MarshalAs(UnmanagedType.LPStr)] string dumpFile, DEBUG_DUMP qualifier, DEBUG_FORMAT formatFlags,
            [MarshalAs(UnmanagedType.LPStr)] string comment);

        [PreserveSig]
        int AddDumpInformationFile([MarshalAs(UnmanagedType.LPStr)] string infoFile, DEBUG_DUMP_FILE type);

        [PreserveSig]
        int EndProcessServer(ulong server);

        [PreserveSig]
        int WaitForProcessServerEnd(uint timeout);

        [PreserveSig]
        int IsKernelDebuggerEnabled();

        [PreserveSig]
        int TerminateCurrentProcess();

        [PreserveSig]
        int DetachCurrentProcess();

        [PreserveSig]
        int AbandonCurrentProcess();
    }
}
