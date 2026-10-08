// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("CE289126-9E84-45A7-937E-67BB18691493")]
    internal unsafe partial interface IDebugRegistersGenerated
    {
        [PreserveSig]
        int GetNumberRegisters(uint* number);

        [PreserveSig]
        // The legacy managed register-description layout is not the native DEBUG_REGISTER_DESCRIPTION layout.
        int GetDescription(uint @register, byte* nameBuffer, uint nameBufferSize, uint* nameSize, IntPtr desc);

        [PreserveSig]
        int GetIndexByName([MarshalAs(UnmanagedType.LPStr)] string name, out uint index);

        [PreserveSig]
        int GetValue(uint @register, out DEBUG_VALUE value);

        [PreserveSig]
        int SetValue(uint @register, DEBUG_VALUE* value);

        [PreserveSig]
        int GetValues(uint count, uint* indices, uint start, DEBUG_VALUE* values);

        [PreserveSig]
        int SetValues(uint count, uint* indices, uint start, DEBUG_VALUE* values);

        [PreserveSig]
        int OutputRegisters(DEBUG_OUTCTL outputControl, DEBUG_REGISTERS flags);

        [PreserveSig]
        int GetInstructionOffset(out ulong offset);

        [PreserveSig]
        int GetStackOffset(out ulong offset);

        [PreserveSig]
        int GetFrameOffset(out ulong offset);
    }
}
