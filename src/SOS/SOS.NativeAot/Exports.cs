// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SOS.NativeAot;

internal static class Exports
{
    [UnmanagedCallersOnly(EntryPoint = "SOSNativeAotGetVersion", CallConvs = [typeof(CallConvCdecl)])]
    private static uint GetVersion() => 1;
}
