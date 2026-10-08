// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop.DbgEng;

namespace SOS.Hosting.DbgEng
{
    internal sealed unsafe partial class DebugClient
    {
        int IDebugSystemObjectsGenerated.GetEventThread(uint* id) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetEventProcess(uint* id) => NotImplemented;

        int IDebugSystemObjectsGenerated.GetCurrentThreadId(out uint id)
        {
            return _soshost.GetCurrentThreadId(IntPtr.Zero, out id);
        }

        int IDebugSystemObjectsGenerated.SetCurrentThreadId(uint id)
        {
            return _soshost.SetCurrentThreadId(IntPtr.Zero, id);
        }

        int IDebugSystemObjectsGenerated.GetCurrentProcessId(uint* id) => NotImplemented;
        int IDebugSystemObjectsGenerated.SetCurrentProcessId(uint id) => NotImplemented;

        int IDebugSystemObjectsGenerated.GetNumberThreads(out uint number)
        {
            return _soshost.GetNumberThreads(IntPtr.Zero, out number);
        }

        int IDebugSystemObjectsGenerated.GetTotalNumberThreads(out uint total, out uint largestProcess)
        {
            return _soshost.GetTotalNumberThreads(IntPtr.Zero, out total, out largestProcess);
        }

        int IDebugSystemObjectsGenerated.GetThreadIdsByIndex(uint start, uint count, uint* ids, uint* sysIds)
        {
            return _soshost.GetThreadIdsByIndex(IntPtr.Zero, start, count, ids, sysIds);
        }

        int IDebugSystemObjectsGenerated.GetThreadIdByProcessor(uint processor, uint* id) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetCurrentThreadDataOffset(ulong* offset) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetThreadIdByDataOffset(ulong offset, uint* id) => NotImplemented;

        int IDebugSystemObjectsGenerated.GetCurrentThreadTeb(ulong* offset)
        {
            return _soshost.GetCurrentThreadTeb(IntPtr.Zero, offset);
        }

        int IDebugSystemObjectsGenerated.GetThreadIdByTeb(ulong offset, uint* id) => NotImplemented;

        int IDebugSystemObjectsGenerated.GetCurrentThreadSystemId(out uint sysId)
        {
            return _soshost.GetCurrentThreadSystemId(IntPtr.Zero, out sysId);
        }

        int IDebugSystemObjectsGenerated.GetThreadIdBySystemId(uint sysId, out uint id)
        {
            return _soshost.GetThreadIdBySystemId(IntPtr.Zero, sysId, out id);
        }

        int IDebugSystemObjectsGenerated.GetCurrentThreadHandle(ulong* handle) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetThreadIdByHandle(ulong handle, uint* id) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetNumberProcesses(uint* number) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetProcessIdsByIndex(uint start, uint count, uint* ids, uint* sysIds) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetCurrentProcessDataOffset(ulong* offset) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetProcessIdByDataOffset(ulong offset, uint* id) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetCurrentProcessPeb(ulong* offset) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetProcessIdByPeb(ulong offset, uint* id) => NotImplemented;

        int IDebugSystemObjectsGenerated.GetCurrentProcessSystemId(out uint sysId)
        {
            return _soshost.GetCurrentProcessSystemId(IntPtr.Zero, out sysId);
        }

        int IDebugSystemObjectsGenerated.GetProcessIdBySystemId(uint sysId, uint* id) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetCurrentProcessHandle(ulong* handle) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetProcessIdByHandle(ulong handle, uint* id) => NotImplemented;
        int IDebugSystemObjectsGenerated.GetCurrentProcessExecutableName(byte* buffer, uint bufferSize, uint* exeSize) => NotImplemented;
    }
}
