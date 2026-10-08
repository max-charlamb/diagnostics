// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("6B86FE2C-2C4F-4F0C-9DA2-174311ACC327")]
    internal unsafe partial interface IDebugSystemObjectsGenerated
    {
        [PreserveSig]
        int GetEventThread(uint* id);

        [PreserveSig]
        int GetEventProcess(uint* id);

        [PreserveSig]
        int GetCurrentThreadId(out uint id);

        [PreserveSig]
        int SetCurrentThreadId(uint id);

        [PreserveSig]
        int GetCurrentProcessId(uint* id);

        [PreserveSig]
        int SetCurrentProcessId(uint id);

        [PreserveSig]
        int GetNumberThreads(out uint number);

        [PreserveSig]
        int GetTotalNumberThreads(out uint total, out uint largestProcess);

        [PreserveSig]
        int GetThreadIdsByIndex(uint start, uint count, uint* ids, uint* sysIds);

        [PreserveSig]
        int GetThreadIdByProcessor(uint processor, uint* id);

        [PreserveSig]
        int GetCurrentThreadDataOffset(ulong* offset);

        [PreserveSig]
        int GetThreadIdByDataOffset(ulong offset, uint* id);

        [PreserveSig]
        int GetCurrentThreadTeb(ulong* offset);

        [PreserveSig]
        int GetThreadIdByTeb(ulong offset, uint* id);

        [PreserveSig]
        int GetCurrentThreadSystemId(out uint sysId);

        [PreserveSig]
        int GetThreadIdBySystemId(uint sysId, out uint id);

        [PreserveSig]
        int GetCurrentThreadHandle(ulong* handle);

        [PreserveSig]
        int GetThreadIdByHandle(ulong handle, uint* id);

        [PreserveSig]
        int GetNumberProcesses(uint* number);

        [PreserveSig]
        int GetProcessIdsByIndex(uint start, uint count, uint* ids, uint* sysIds);

        [PreserveSig]
        int GetCurrentProcessDataOffset(ulong* offset);

        [PreserveSig]
        int GetProcessIdByDataOffset(ulong offset, uint* id);

        [PreserveSig]
        int GetCurrentProcessPeb(ulong* offset);

        [PreserveSig]
        int GetProcessIdByPeb(ulong offset, uint* id);

        [PreserveSig]
        int GetCurrentProcessSystemId(out uint sysId);

        [PreserveSig]
        int GetProcessIdBySystemId(uint sysId, uint* id);

        [PreserveSig]
        int GetCurrentProcessHandle(ulong* handle);

        [PreserveSig]
        int GetProcessIdByHandle(ulong handle, uint* id);

        [PreserveSig]
        int GetCurrentProcessExecutableName(byte* buffer, uint bufferSize, uint* exeSize);
    }
}
