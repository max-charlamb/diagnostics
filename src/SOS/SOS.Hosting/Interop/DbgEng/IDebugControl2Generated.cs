// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("D4366723-44DF-4BED-8C7E-4C05424F4588")]
    internal unsafe partial interface IDebugControl2Generated : IDebugControlGenerated
    {
        [PreserveSig]
        int GetCurrentTimeDate(uint* timeDate);

        [PreserveSig]
        int GetCurrentSystemUpTime(uint* upTime);

        [PreserveSig]
        int GetDumpFormatFlags(DEBUG_FORMAT* formatFlags);

        [PreserveSig]
        int GetNumberTextReplacements(uint* numRepl);

        [PreserveSig]
        int GetTextReplacement([MarshalAs(UnmanagedType.LPStr)] string srcText, uint index, byte* srcBuffer, uint srcBufferSize, uint* srcSize, byte* dstBuffer, uint dstBufferSize, uint* dstSize);

        [PreserveSig]
        int SetTextReplacement([MarshalAs(UnmanagedType.LPStr)] string srcText, [MarshalAs(UnmanagedType.LPStr)] string dstText);

        [PreserveSig]
        int RemoveTextReplacements();

        [PreserveSig]
        int OutputTextReplacements(DEBUG_OUTCTL outputControl, DEBUG_OUT_TEXT_REPL flags);
    }
}
