// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.Interop;

namespace SOS.Extensions
{
    internal sealed unsafe class ThreadStackTraceService : IThreadStackTraceService
    {
        private readonly IDebuggerThreadStackTraceServiceGenerated _services;

        internal ThreadStackTraceService(IDebuggerThreadStackTraceServiceGenerated services)
        {
            _services = services;
        }

        public IStack GetDebuggerStackTrace(uint threadId, int maxFrames)
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfNegative(maxFrames);
#else
            if (maxFrames < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFrames));
            }
#endif
            if (maxFrames == 0)
            {
                return new Stack(Array.Empty<DebuggerStackFrame>(), 0);
            }

            DebuggerStackFrame[] debuggerFrames = new DebuggerStackFrame[maxFrames];
            HResult result;
            uint framesFilled;
            fixed (DebuggerStackFrame* framesPtr = debuggerFrames)
            {
                result = _services.GetDebuggerStackTrace(threadId, framesPtr, (uint)debuggerFrames.Length, out framesFilled);
            }
            if (!result.IsOK)
            {
                throw new DiagnosticsException($"Failed to get the debugger stack trace for thread {threadId:x8}: {result}");
            }
            if (framesFilled > debuggerFrames.Length)
            {
                throw new DiagnosticsException(
                    $"Debugger stack trace for thread {threadId:x8} returned {framesFilled} frames for a {debuggerFrames.Length}-frame buffer.");
            }

            return new Stack(debuggerFrames, (int)framesFilled);
        }

        private sealed class Stack : IStack
        {
            private readonly DebuggerStackFrame[] _frames;

            internal Stack(DebuggerStackFrame[] frames, int frameCount)
            {
                _frames = frames;
                FrameCount = frameCount;
            }

            public int FrameCount { get; }

            public IStackFrame GetStackFrame(int index)
            {
                if (index < 0 || index >= FrameCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return new StackFrame(_frames, index);
            }
        }

        private sealed class StackFrame : IStackFrame
        {
            private readonly DebuggerStackFrame[] _frames;
            private readonly int _index;

            internal StackFrame(DebuggerStackFrame[] frames, int index)
            {
                _frames = frames;
                _index = index;
            }

            public ulong InstructionPointer => _frames[_index].InstructionPointer;
            public ulong StackPointer => _frames[_index].StackPointer;
            public ulong ModuleBase => throw new NotImplementedException();

            public void GetMethodName(out string moduleName, out string methodName, out ulong displacement)
            {
                throw new NotImplementedException();
            }
        }
    }
}
