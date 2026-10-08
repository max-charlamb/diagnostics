// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("C65FA83E-1E69-475E-8E0E-B5D79E9CC17E")]
    internal unsafe partial interface IDebugSymbols5Generated : IDebugSymbols4Generated
    {
        [PreserveSig]
        int GetCurrentScopeFrameIndexEx(DEBUG_FRAME flags, out uint index);

        [PreserveSig]
        int SetScopeFrameByIndexEx(DEBUG_FRAME flags, uint index);
    }
}
