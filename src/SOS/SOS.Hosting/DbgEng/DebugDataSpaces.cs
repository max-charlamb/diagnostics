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
        int IDebugDataSpacesGenerated.ReadVirtual(ulong offset, IntPtr buffer, uint bufferSize, uint* bytesRead)
        {
            return _soshost.ReadVirtual(IntPtr.Zero, offset, buffer, bufferSize, bytesRead);
        }

        int IDebugDataSpacesGenerated.WriteVirtual(ulong offset, byte* buffer, uint bufferSize, uint* bytesWritten)
        {
            return _soshost.WriteVirtual(IntPtr.Zero, offset, (IntPtr)buffer, bufferSize, bytesWritten);
        }

        int IDebugDataSpacesGenerated.SearchVirtual(ulong offset, ulong length, byte* pattern, uint patternSize, uint patternGranularity, ulong* matchOffset) => NotImplemented;
        int IDebugDataSpacesGenerated.ReadVirtualUncached(ulong offset, byte* buffer, uint bufferSize, uint* bytesRead) => NotImplemented;
        int IDebugDataSpacesGenerated.WriteVirtualUncached(ulong offset, byte* buffer, uint bufferSize, uint* bytesWritten) => NotImplemented;
        int IDebugDataSpacesGenerated.ReadPointersVirtual(uint count, ulong offset, ulong* ptrs) => NotImplemented;
        int IDebugDataSpacesGenerated.WritePointersVirtual(uint count, ulong offset, ulong* ptrs) => NotImplemented;
        int IDebugDataSpacesGenerated.ReadPhysical(ulong offset, byte* buffer, uint bufferSize, uint* bytesRead) => NotImplemented;
        int IDebugDataSpacesGenerated.WritePhysical(ulong offset, byte* buffer, uint bufferSize, uint* bytesWritten) => NotImplemented;
        int IDebugDataSpacesGenerated.ReadControl(uint processor, ulong offset, byte* buffer, uint bufferSize, uint* bytesRead) => NotImplemented;
        int IDebugDataSpacesGenerated.WriteControl(uint processor, ulong offset, byte* buffer, uint bufferSize, uint* bytesWritten) => NotImplemented;
        int IDebugDataSpacesGenerated.ReadIo(INTERFACE_TYPE interfaceType, uint busNumber, uint addressSpace, ulong offset, byte* buffer, uint bufferSize, uint* bytesRead) => NotImplemented;
        int IDebugDataSpacesGenerated.WriteIo(INTERFACE_TYPE interfaceType, uint busNumber, uint addressSpace, ulong offset, byte* buffer, uint bufferSize, uint* bytesWritten) => NotImplemented;
        int IDebugDataSpacesGenerated.ReadMsr(uint msr, ulong* msrValue) => NotImplemented;
        int IDebugDataSpacesGenerated.WriteMsr(uint msr, ulong msrValue) => NotImplemented;
        int IDebugDataSpacesGenerated.ReadBusData(BUS_DATA_TYPE busDataType, uint busNumber, uint slotNumber, uint offset, byte* buffer, uint bufferSize, uint* bytesRead) => NotImplemented;
        int IDebugDataSpacesGenerated.WriteBusData(BUS_DATA_TYPE busDataType, uint busNumber, uint slotNumber, uint offset, byte* buffer, uint bufferSize, uint* bytesWritten) => NotImplemented;
        int IDebugDataSpacesGenerated.CheckLowMemory() => NotImplemented;
        int IDebugDataSpacesGenerated.ReadDebuggerData(uint index, byte* buffer, uint bufferSize, uint* dataSize) => HResult.E_NOTIMPL;
        int IDebugDataSpacesGenerated.ReadProcessorSystemData(uint processor, DEBUG_DATA index, byte* buffer, uint bufferSize, uint* dataSize) => NotImplemented;
        int IDebugDataSpaces2Generated.VirtualToPhysical(ulong @virtual, ulong* physical) => NotImplemented;
        int IDebugDataSpaces2Generated.GetVirtualTranslationPhysicalOffsets(ulong @virtual, ulong* offsets, uint offsetsSize, uint* levels) => NotImplemented;
        int IDebugDataSpaces2Generated.ReadHandleData(ulong handle, DEBUG_HANDLE_DATA_TYPE dataType, byte* buffer, uint bufferSize, uint* dataSize) => NotImplemented;
        int IDebugDataSpaces2Generated.FillVirtual(ulong start, uint size, byte* buffer, uint patternSize, uint* filled) => NotImplemented;
        int IDebugDataSpaces2Generated.FillPhysical(ulong start, uint size, byte* buffer, uint patternSize, uint* filled) => NotImplemented;
        int IDebugDataSpaces2Generated.QueryVirtual(ulong offset, MEMORY_BASIC_INFORMATION64* info) => NotImplemented;
    }
}
