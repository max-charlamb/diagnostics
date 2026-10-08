// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("C4F8B7E2-9D3A-4F6C-B1E5-8A2D7C3F9B1E")]
    internal unsafe partial interface ICLRSymbolProviderGenerated
    {
        [PreserveSig]
        int TryGetSymbolName(ulong address, uint nameBufferSize, char* name, uint* nameSize, ulong* displacement);

        [PreserveSig]
        int TryGetSymbolAddress(ulong moduleBase, [MarshalAs(UnmanagedType.LPWStr)] string name, ulong* address);

        [PreserveSig]
        int TryGetFieldOffset(ulong moduleBase, [MarshalAs(UnmanagedType.LPWStr)] string typeName,
            [MarshalAs(UnmanagedType.LPWStr)] string fieldName, uint* offset);
    }
}
