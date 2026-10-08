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
        int IDebugRegistersGenerated.GetNumberRegisters(uint* number) => NotImplemented;
        int IDebugRegistersGenerated.GetDescription(uint @register, byte* nameBuffer, uint nameBufferSize, uint* nameSize, IntPtr desc) => NotImplemented;

        int IDebugRegistersGenerated.GetIndexByName(string name, out uint index)
        {
            return _soshost.GetIndexByName(IntPtr.Zero, name, out index);
        }

        int IDebugRegistersGenerated.GetValue(uint @register, out DEBUG_VALUE value)
        {
            return _soshost.GetValue(IntPtr.Zero, @register, out value);
        }

        int IDebugRegistersGenerated.SetValue(uint @register, DEBUG_VALUE* value) => NotImplemented;
        int IDebugRegistersGenerated.GetValues(uint count, uint* indices, uint start, DEBUG_VALUE* values) => NotImplemented;
        int IDebugRegistersGenerated.SetValues(uint count, uint* indices, uint start, DEBUG_VALUE* values) => NotImplemented;
        int IDebugRegistersGenerated.OutputRegisters(DEBUG_OUTCTL outputControl, DEBUG_REGISTERS flags) => NotImplemented;

        int IDebugRegistersGenerated.GetInstructionOffset(out ulong offset)
        {
            return _soshost.GetInstructionOffset(IntPtr.Zero, out offset);
        }

        int IDebugRegistersGenerated.GetStackOffset(out ulong offset)
        {
            return _soshost.GetStackOffset(IntPtr.Zero, out offset);
        }

        int IDebugRegistersGenerated.GetFrameOffset(out ulong offset)
        {
            return _soshost.GetFrameOffset(IntPtr.Zero, out offset);
        }
    }
}
