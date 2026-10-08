// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("88F7DFAB-3EA7-4C3A-AEFB-C4E8106173AA")]
    internal unsafe partial interface IDebugDataSpacesGenerated
    {
        [PreserveSig]
        int ReadVirtual(ulong offset, IntPtr buffer, uint bufferSize, uint* bytesRead);

        [PreserveSig]
        int WriteVirtual(ulong offset, byte* buffer, uint bufferSize, uint* bytesWritten);

        [PreserveSig]
        int SearchVirtual(ulong offset, ulong length, byte* pattern, uint patternSize, uint patternGranularity, ulong* matchOffset);

        [PreserveSig]
        int ReadVirtualUncached(ulong offset, byte* buffer, uint bufferSize, uint* bytesRead);

        [PreserveSig]
        int WriteVirtualUncached(ulong offset, byte* buffer, uint bufferSize, uint* bytesWritten);

        [PreserveSig]
        int ReadPointersVirtual(uint count, ulong offset, ulong* ptrs);

        [PreserveSig]
        int WritePointersVirtual(uint count, ulong offset, ulong* ptrs);

        [PreserveSig]
        int ReadPhysical(ulong offset, byte* buffer, uint bufferSize, uint* bytesRead);

        [PreserveSig]
        int WritePhysical(ulong offset, byte* buffer, uint bufferSize, uint* bytesWritten);

        [PreserveSig]
        int ReadControl(uint processor, ulong offset, byte* buffer, uint bufferSize, uint* bytesRead);

        [PreserveSig]
        int WriteControl(uint processor, ulong offset, byte* buffer, uint bufferSize, uint* bytesWritten);

        [PreserveSig]
        int ReadIo(INTERFACE_TYPE interfaceType, uint busNumber, uint addressSpace, ulong offset, byte* buffer, uint bufferSize, uint* bytesRead);

        [PreserveSig]
        int WriteIo(INTERFACE_TYPE interfaceType, uint busNumber, uint addressSpace, ulong offset, byte* buffer, uint bufferSize, uint* bytesWritten);

        [PreserveSig]
        int ReadMsr(uint msr, ulong* msrValue);

        [PreserveSig]
        int WriteMsr(uint msr, ulong msrValue);

        [PreserveSig]
        int ReadBusData(BUS_DATA_TYPE busDataType, uint busNumber, uint slotNumber, uint offset, byte* buffer, uint bufferSize, uint* bytesRead);

        [PreserveSig]
        int WriteBusData(BUS_DATA_TYPE busDataType, uint busNumber, uint slotNumber, uint offset, byte* buffer, uint bufferSize, uint* bytesWritten);

        [PreserveSig]
        int CheckLowMemory();

        [PreserveSig]
        int ReadDebuggerData(uint index, byte* buffer, uint bufferSize, uint* dataSize);

        [PreserveSig]
        int ReadProcessorSystemData(uint processor, DEBUG_DATA index, byte* buffer, uint bufferSize, uint* dataSize);
    }
}
