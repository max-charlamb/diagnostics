// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.DebugServices;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("A5F152B9-BA78-4512-9228-5091A4CB7E35")]
    internal unsafe partial interface IRuntimeGenerated
    {
        [PreserveSig]
        int GetRuntimeConfiguration();

        [PreserveSig]
        ulong GetModuleAddress();

        [PreserveSig]
        ulong GetModuleSize();

        [PreserveSig]
        void SetRuntimeDirectory([MarshalAs(UnmanagedType.LPStr)] string runtimeModuleDirectory);

        [PreserveSig]
        [return: MarshalAs(UnmanagedType.LPStr)]
        string GetRuntimeDirectory();

        [PreserveSig]
        int GetClrDataProcess(CDacLoadPolicy policy, IntPtr* clrDataProcess);

        [PreserveSig]
        int GetCorDebugInterface(IntPtr* corDebugProcess);

        [PreserveSig]
        int GetEEVersion(VS_FIXEDFILEINFO* fileInfo, byte* fileVersionBuffer, int fileVersionBufferSizeInBytes);

        [PreserveSig]
        CDacLoadPolicy GetCDacLoadPolicy();
    }
}
