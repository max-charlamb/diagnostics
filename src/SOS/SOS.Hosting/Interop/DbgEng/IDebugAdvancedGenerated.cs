// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("F2DF5F53-071F-47BD-9DE6-5734C3FED689")]
    internal unsafe partial interface IDebugAdvancedGenerated
    {
        [PreserveSig]
        int GetThreadContext(IntPtr context, uint contextSize);

        [PreserveSig]
        int SetThreadContext(IntPtr context, uint contextSize);
    }
}
