// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("E391BBD8-9D8C-4418-840B-C006592A1752")]
    internal unsafe partial interface IDebugSymbols4Generated : IDebugSymbols3Generated
    {
        [PreserveSig]
        int GetScopeEx(out ulong instructionOffset, out DEBUG_STACK_FRAME_EX scopeFrame, IntPtr scopeContext, uint scopeContextSize);

        [PreserveSig]
        int SetScopeEx(ulong instructionOffset, in DEBUG_STACK_FRAME_EX scopeFrame, IntPtr scopeContext, uint scopeContextSize);

        [PreserveSig]
        int GetNameByInlineContext(ulong offset, uint inlineContext, byte* nameBuffer, uint nameBufferSize, out uint nameSize,
            out ulong displacement);

        [PreserveSig]
        int GetNameByInlineContextWide(ulong offset, uint inlineContext, char* nameBuffer, uint nameBufferSize, out uint nameSize,
            out ulong displacement);

        [PreserveSig]
        int GetLineByInlineContext(ulong offset, uint inlineContext, out uint line, byte* fileBuffer, uint fileBufferSize, out uint fileSize,
            out ulong displacement);

        [PreserveSig]
        int GetLineByInlineContextWide(ulong offset, uint inlineContext, out uint line, char* fileBuffer, uint fileBufferSize,
            out uint fileSize, out ulong displacement);

        [PreserveSig]
        int OutputSymbolByInlineContext(uint outputControl, uint flags, ulong offset, uint inlineContext);
    }
}
