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
    internal unsafe partial interface IDebugClient6Generated : IDebugClient5Generated
    {
        [PreserveSig]
        int SetEventContextCallbacks(IntPtr callbacks);
    }
}
