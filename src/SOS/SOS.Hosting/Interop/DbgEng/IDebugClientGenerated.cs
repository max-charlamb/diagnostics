// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("27FE5639-8407-4F47-8364-EE118FB08AC8")]
    internal unsafe partial interface IDebugClientGenerated
    {
        [PreserveSig]
        int AttachKernel(DEBUG_ATTACH flags, [MarshalAs(UnmanagedType.LPStr)] string connectOptions);

        [PreserveSig]
        int GetKernelConnectionOptions(byte* buffer, uint bufferSize, uint* optionsSize);

        [PreserveSig]
        int SetKernelConnectionOptions([MarshalAs(UnmanagedType.LPStr)] string options);

        [PreserveSig]
        int StartProcessServer(DEBUG_CLASS flags, [MarshalAs(UnmanagedType.LPStr)] string options, IntPtr reserved);

        [PreserveSig]
        int ConnectProcessServer([MarshalAs(UnmanagedType.LPStr)] string remoteOptions, ulong* server);

        [PreserveSig]
        int DisconnectProcessServer(ulong server);

        [PreserveSig]
        int GetRunningProcessSystemIds(ulong server, uint* ids, uint count, uint* actualCount);

        [PreserveSig]
        int GetRunningProcessSystemIdByExecutableName(ulong server, [MarshalAs(UnmanagedType.LPStr)] string exeName, DEBUG_GET_PROC flags,
            uint* id);

        [PreserveSig]
        int GetRunningProcessDescription(ulong server, uint systemId, DEBUG_PROC_DESC flags, byte* exeName, uint exeNameSize,
            uint* actualExeNameSize, byte* description, uint descriptionSize, uint* actualDescriptionSize);

        [PreserveSig]
        int AttachProcess(ulong server, uint processID, DEBUG_ATTACH attachFlags);

        [PreserveSig]
        int CreateProcess(ulong server, [MarshalAs(UnmanagedType.LPStr)] string commandLine, DEBUG_CREATE_PROCESS flags);

        [PreserveSig]
        int CreateProcessAndAttach(ulong server, [MarshalAs(UnmanagedType.LPStr)] string commandLine, DEBUG_CREATE_PROCESS flags,
            uint processId, DEBUG_ATTACH attachFlags);

        [PreserveSig]
        int GetProcessOptions(DEBUG_PROCESS* options);

        [PreserveSig]
        int AddProcessOptions(DEBUG_PROCESS options);

        [PreserveSig]
        int RemoveProcessOptions(DEBUG_PROCESS options);

        [PreserveSig]
        int SetProcessOptions(DEBUG_PROCESS options);

        [PreserveSig]
        int OpenDumpFile([MarshalAs(UnmanagedType.LPStr)] string dumpFile);

        [PreserveSig]
        int WriteDumpFile([MarshalAs(UnmanagedType.LPStr)] string dumpFile, DEBUG_DUMP qualifier);

        [PreserveSig]
        int ConnectSession(DEBUG_CONNECT_SESSION flags, uint historyLimit);

        [PreserveSig]
        int StartServer([MarshalAs(UnmanagedType.LPStr)] string options);

        [PreserveSig]
        int OutputServers(DEBUG_OUTCTL outputControl, [MarshalAs(UnmanagedType.LPStr)] string machine, DEBUG_SERVERS flags);

        [PreserveSig]
        int TerminateProcesses();

        [PreserveSig]
        int DetachProcesses();

        [PreserveSig]
        int EndSession(DEBUG_END flags);

        [PreserveSig]
        int GetExitCode(uint* code);

        [PreserveSig]
        int DispatchCallbacks(uint timeout);

        [PreserveSig]
        int ExitDispatch(IDebugClientGenerated client);

        [PreserveSig]
        int CreateClient(out IDebugClientGenerated client);

        [PreserveSig]
        int GetInputCallbacks(out IntPtr callbacks);

        [PreserveSig]
        int SetInputCallbacks(IntPtr callbacks);

        [PreserveSig]
        int GetOutputCallbacks(out IntPtr callbacks);

        [PreserveSig]
        int SetOutputCallbacks(IntPtr callbacks);

        [PreserveSig]
        int GetOutputMask(DEBUG_OUTPUT* mask);

        [PreserveSig]
        int SetOutputMask(DEBUG_OUTPUT mask);

        [PreserveSig]
        int GetOtherOutputMask(IDebugClientGenerated client, DEBUG_OUTPUT* mask);

        [PreserveSig]
        int SetOtherOutputMask(IDebugClientGenerated client, DEBUG_OUTPUT mask);

        [PreserveSig]
        int GetOutputWidth(uint* columns);

        [PreserveSig]
        int SetOutputWidth(uint columns);

        [PreserveSig]
        int GetOutputLinePrefix(byte* buffer, uint bufferSize, uint* prefixSize);

        [PreserveSig]
        int SetOutputLinePrefix([MarshalAs(UnmanagedType.LPStr)] string prefix);

        [PreserveSig]
        int GetIdentity(byte* buffer, uint bufferSize, uint* identitySize);

        [PreserveSig]
        int OutputIdentity(DEBUG_OUTCTL outputControl, uint flags, [MarshalAs(UnmanagedType.LPStr)] string format);

        [PreserveSig]
        int GetEventCallbacks(out IntPtr callbacks);

        [PreserveSig]
        int SetEventCallbacks(IntPtr callbacks);

        [PreserveSig]
        int FlushCallbacks();
    }
}
