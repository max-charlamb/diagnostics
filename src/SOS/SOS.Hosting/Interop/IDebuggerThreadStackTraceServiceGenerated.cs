// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct DebuggerStackFrame
    {
        public readonly ulong InstructionPointer;
        public readonly ulong StackPointer;
    }

    [GeneratedComInterface]
    [Guid("3F0DEFDA-A8A3-43B2-9209-935147C89B58")]
    internal unsafe partial interface IDebuggerThreadStackTraceServiceGenerated
    {
        [PreserveSig]
        int GetDebuggerStackTrace(uint sysId, DebuggerStackFrame* frames, uint framesSize, out uint framesFilled);
    }
}
