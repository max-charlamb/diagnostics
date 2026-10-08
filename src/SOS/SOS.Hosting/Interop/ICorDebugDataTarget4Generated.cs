// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("E799DC06-E099-4713-BDD9-906D3CC02CF2")]
    internal unsafe partial interface ICorDebugDataTarget4Generated
    {
        [PreserveSig]
        int VirtualUnwind(uint threadId, uint contextSize, byte* context);
    }
}
