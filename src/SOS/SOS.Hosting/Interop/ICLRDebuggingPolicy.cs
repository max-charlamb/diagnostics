// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Microsoft.Diagnostics
{
    public enum DbgShimCDacLoadPolicy : uint
    {
        PreferCDac = 0,
        CDacOnly = 1,
        LegacyDacOnly = 2,
    }

    [GeneratedComInterface]
    [Guid("2D3B4F6A-1C7E-4B2A-9E5D-7F1A6C0B8D34")]
    public partial interface ICLRDebuggingPolicy
    {
        [PreserveSig]
        int SetCDacLoadPolicy(DbgShimCDacLoadPolicy policy);

        [PreserveSig]
        int GetCDacLoadPolicy(out DbgShimCDacLoadPolicy policy);
    }
}
