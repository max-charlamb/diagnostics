// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("B760BF44-9377-4597-8BE7-58083BDC5146")]
    internal partial interface ICLRRuntimeLocatorGenerated
    {
        [PreserveSig]
        int GetRuntimeBase(out ulong address);
    }
}
