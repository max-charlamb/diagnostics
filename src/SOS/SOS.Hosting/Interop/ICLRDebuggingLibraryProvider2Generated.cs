// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("E04E2FF1-DCFD-45D5-BCD1-16FFF2FAF7BA")]
    internal partial interface ICLRDebuggingLibraryProvider2Generated
    {
        [PreserveSig]
        int ProvideLibrary2([MarshalAs(UnmanagedType.LPWStr)] string fileName, uint timeStamp, uint sizeOfImage,
            [MarshalAs(UnmanagedType.LPWStr)] out string modulePath);
    }
}
