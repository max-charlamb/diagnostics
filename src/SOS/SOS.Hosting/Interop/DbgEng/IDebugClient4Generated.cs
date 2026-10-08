// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("CA83C3DE-5089-4CF8-93C8-D892387F2A5E")]
    internal unsafe partial interface IDebugClient4Generated : IDebugClient3Generated
    {
        [PreserveSig]
        int OpenDumpFileWide([MarshalAs(UnmanagedType.LPWStr)] string fileName, ulong fileHandle);

        [PreserveSig]
        int WriteDumpFileWide([MarshalAs(UnmanagedType.LPWStr)] string dumpFile, ulong fileHandle, DEBUG_DUMP qualifier,
            DEBUG_FORMAT formatFlags, [MarshalAs(UnmanagedType.LPWStr)] string comment);

        [PreserveSig]
        int AddDumpInformationFileWide([MarshalAs(UnmanagedType.LPWStr)] string fileName, ulong fileHandle, DEBUG_DUMP_FILE type);

        [PreserveSig]
        int GetNumberDumpFiles(out uint number);

        [PreserveSig]
        int GetDumpFile(uint index, byte* buffer, uint bufferSize, out uint nameSize, out ulong handle, out uint type);

        [PreserveSig]
        int GetDumpFileWide(uint index, char* buffer, uint bufferSize, out uint nameSize, out ulong handle, out uint type);
    }
}
