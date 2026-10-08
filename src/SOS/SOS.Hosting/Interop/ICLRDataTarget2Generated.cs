// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("6D05FAE3-189C-4630-A6DC-1C251E1C01AB")]
    internal unsafe partial interface ICLRDataTarget2Generated : ICLRDataTargetGenerated
    {
        [PreserveSig]
        int AllocVirtual(ulong address, uint size, uint typeFlags, uint protectFlags, ulong* buffer);

        [PreserveSig]
        int FreeVirtual(ulong address, uint size, uint typeFlags);
    }
}
