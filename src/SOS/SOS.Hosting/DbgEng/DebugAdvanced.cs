// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop.DbgEng;

namespace SOS.Hosting.DbgEng
{
    internal sealed unsafe partial class DebugClient
    {
        int IDebugAdvancedGenerated.GetThreadContext(IntPtr context, uint contextSize)
        {
            return _soshost.GetThreadContext(IntPtr.Zero, context, unchecked((int)contextSize));
        }

        int IDebugAdvancedGenerated.SetThreadContext(IntPtr context, uint contextSize) => NotImplemented;
    }
}
