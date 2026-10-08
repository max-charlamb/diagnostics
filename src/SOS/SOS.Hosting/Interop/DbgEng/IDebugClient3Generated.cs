// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("DD492D7F-71B8-4AD6-A8DC-1C887479FF91")]
    internal unsafe partial interface IDebugClient3Generated : IDebugClient2Generated
    {
        [PreserveSig]
        int GetRunningProcessSystemIdByExecutableNameWide(ulong server, [MarshalAs(UnmanagedType.LPWStr)] string exeName,
            DEBUG_GET_PROC flags, out uint id);

        [PreserveSig]
        int GetRunningProcessDescriptionWide(ulong server, uint systemId, DEBUG_PROC_DESC flags, char* exeName, uint exeNameSize,
            out uint actualExeNameSize, char* description, uint descriptionSize, out uint actualDescriptionSize);

        [PreserveSig]
        int CreateProcessWide(ulong server, [MarshalAs(UnmanagedType.LPWStr)] string commandLine, DEBUG_CREATE_PROCESS createFlags);

        [PreserveSig]
        int CreateProcessAndAttachWide(ulong server, [MarshalAs(UnmanagedType.LPWStr)] string commandLine, DEBUG_CREATE_PROCESS createFlags,
            uint processId, DEBUG_ATTACH attachFlags);
    }
}
