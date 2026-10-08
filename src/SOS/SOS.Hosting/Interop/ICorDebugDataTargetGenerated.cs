// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("FE06DC28-49FB-4636-A4A3-E80DB4AE116C")]
    internal unsafe partial interface ICorDebugDataTargetGenerated
    {
        [PreserveSig]
        int GetPlatform(out CorDebugPlatform platform);

        [PreserveSig]
        int ReadVirtual(ulong address, byte* buffer, uint bytesRequested, uint* bytesRead);

        [PreserveSig]
        int GetThreadContext(uint threadId, uint contextFlags, uint contextSize, byte* context);
    }
}
