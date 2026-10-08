// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("17D5B8C6-34A9-407F-AF4F-A930201D4E02")]
    internal partial interface ICLRContractLocatorGenerated
    {
        [PreserveSig]
        int GetContractDescriptor(out ulong address);
    }
}
