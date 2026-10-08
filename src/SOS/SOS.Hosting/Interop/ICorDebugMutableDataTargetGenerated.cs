// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("A1B8A756-3CB6-4CCB-979F-3DF999673A59")]
    internal unsafe partial interface ICorDebugMutableDataTargetGenerated : ICorDebugDataTargetGenerated
    {
        [PreserveSig]
        int WriteVirtual(ulong address, byte* buffer, uint bytesRequested);

        [PreserveSig]
        int SetThreadContext(uint threadId, uint contextSize, byte* context);

        [PreserveSig]
        int ContinueStatusChanged(uint threadId, uint continueStatus);
    }
}
