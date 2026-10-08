// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("CD6A0F22-8BCF-4297-9366-F440C2D1C781")]
    internal partial interface IRemoteMemoryServiceGenerated
    {
        [PreserveSig]
        int AllocVirtual(ulong address, uint size, uint typeFlags, uint protectFlags, out ulong remoteAddress);

        [PreserveSig]
        int FreeVirtual(ulong address, uint size, uint typeFlags);
    }
}
