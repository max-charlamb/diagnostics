// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("012F32F0-33BA-4E8E-BC01-037D382D8A5E")]
    internal unsafe partial interface ILLDBServices2Generated
    {
        [PreserveSig]
        int LoadNativeSymbols([MarshalAs(UnmanagedType.I1)] bool runtimeOnly, IntPtr callback);
        [PreserveSig]
        int AddModuleSymbol(IntPtr parameter, [MarshalAs(UnmanagedType.LPStr)] string symbolFilename);
        [PreserveSig]
        int GetModuleInfo(uint index, ulong* moduleBase, ulong* moduleSize, uint* timestamp, uint* checksum);
        [PreserveSig]
        int GetModuleVersionInformation(uint index, ulong moduleBase, [MarshalAs(UnmanagedType.LPStr)] string item,
            byte* buffer, uint bufferSize, uint* versionInfoSize);
        [PreserveSig]
        int SetRuntimeLoadedCallback(IntPtr callback);
    }
}
