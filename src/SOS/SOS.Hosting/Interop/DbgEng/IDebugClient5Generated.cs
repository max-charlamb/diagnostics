// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("E3ACB9D7-7EC2-4F0C-A0DA-E81E0CBBE628")]
    internal unsafe partial interface IDebugClient5Generated : IDebugClient4Generated
    {
        [PreserveSig]
        int AttachKernelWide(DEBUG_ATTACH flags, [MarshalAs(UnmanagedType.LPWStr)] string connectOptions);

        [PreserveSig]
        int GetKernelConnectionOptionsWide(char* buffer, uint bufferSize, out uint optionsSize);

        [PreserveSig]
        int SetKernelConnectionOptionsWide([MarshalAs(UnmanagedType.LPWStr)] string options);

        [PreserveSig]
        int StartProcessServerWide(DEBUG_CLASS flags, [MarshalAs(UnmanagedType.LPWStr)] string options, IntPtr reserved);

        [PreserveSig]
        int ConnectProcessServerWide([MarshalAs(UnmanagedType.LPWStr)] string remoteOptions, out ulong server);

        [PreserveSig]
        int StartServerWide([MarshalAs(UnmanagedType.LPWStr)] string options);

        [PreserveSig]
        int OutputServersWide(DEBUG_OUTCTL outputControl, [MarshalAs(UnmanagedType.LPWStr)] string machine, DEBUG_SERVERS flags);

        [PreserveSig]
        int GetOutputCallbacksWide(out IntPtr callbacks);

        [PreserveSig]
        int SetOutputCallbacksWide(IntPtr callbacks);

        [PreserveSig]
        int GetOutputLinePrefixWide(char* buffer, uint bufferSize, out uint prefixSize);

        [PreserveSig]
        int SetOutputLinePrefixWide([MarshalAs(UnmanagedType.LPWStr)] string prefix);

        [PreserveSig]
        int GetIdentityWide(char* buffer, uint bufferSize, out uint identitySize);

        [PreserveSig]
        int OutputIdentityWide(DEBUG_OUTCTL outputControl, uint flags, [MarshalAs(UnmanagedType.LPWStr)] string machine);

        [PreserveSig]
        int GetEventCallbacksWide(out IntPtr callbacks);

        [PreserveSig]
        int SetEventCallbacksWide(IntPtr callbacks);

        [PreserveSig]
        int CreateProcess2(ulong server, [MarshalAs(UnmanagedType.LPStr)] string commandLine, void* optionsBuffer, uint optionsBufferSize,
            [MarshalAs(UnmanagedType.LPStr)] string initialDirectory, [MarshalAs(UnmanagedType.LPStr)] string environment);

        [PreserveSig]
        int CreateProcess2Wide(ulong server, [MarshalAs(UnmanagedType.LPWStr)] string commandLine, void* optionsBuffer,
            uint optionsBufferSize, [MarshalAs(UnmanagedType.LPWStr)] string initialDirectory,
            [MarshalAs(UnmanagedType.LPWStr)] string environment);

        [PreserveSig]
        int CreateProcessAndAttach2(ulong server, [MarshalAs(UnmanagedType.LPStr)] string commandLine, void* optionsBuffer,
            uint optionsBufferSize, [MarshalAs(UnmanagedType.LPStr)] string initialDirectory,
            [MarshalAs(UnmanagedType.LPStr)] string environment, uint processId, DEBUG_ATTACH attachFlags);

        [PreserveSig]
        int CreateProcessAndAttach2Wide(ulong server, [MarshalAs(UnmanagedType.LPWStr)] string commandLine, void* optionsBuffer,
            uint optionsBufferSize, [MarshalAs(UnmanagedType.LPWStr)] string initialDirectory,
            [MarshalAs(UnmanagedType.LPWStr)] string environment, uint processId, DEBUG_ATTACH attachFlags);

        [PreserveSig]
        int PushOutputLinePrefix([MarshalAs(UnmanagedType.LPStr)] string newPrefix, out ulong handle);

        [PreserveSig]
        int PushOutputLinePrefixWide([MarshalAs(UnmanagedType.LPWStr)] string newPrefix, out ulong handle);

        [PreserveSig]
        int PopOutputLinePrefix(ulong handle);

        [PreserveSig]
        int GetNumberInputCallbacks(out uint count);

        [PreserveSig]
        int GetNumberOutputCallbacks(out uint count);

        [PreserveSig]
        int GetNumberEventCallbacks(DEBUG_EVENT flags, out uint count);

        [PreserveSig]
        int GetQuitLockString(byte* buffer, uint bufferSize, out uint stringSize);

        [PreserveSig]
        int SetQuitLockString([MarshalAs(UnmanagedType.LPStr)] string lockString);

        [PreserveSig]
        int GetQuitLockStringWide(char* buffer, uint bufferSize, out uint stringSize);

        [PreserveSig]
        int SetQuitLockStringWide([MarshalAs(UnmanagedType.LPWStr)] string lockString);
    }
}
