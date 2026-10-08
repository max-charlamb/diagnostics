// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("7A5E852F-96E9-468F-AC1B-0B3ADDC4A049")]
    internal unsafe partial interface IDebugDataSpaces2Generated : IDebugDataSpacesGenerated
    {
        [PreserveSig]
        int VirtualToPhysical(ulong @virtual, ulong* physical);

        [PreserveSig]
        int GetVirtualTranslationPhysicalOffsets(ulong @virtual, ulong* offsets, uint offsetsSize, uint* levels);

        [PreserveSig]
        int ReadHandleData(ulong handle, DEBUG_HANDLE_DATA_TYPE dataType, byte* buffer, uint bufferSize, uint* dataSize);

        [PreserveSig]
        int FillVirtual(ulong start, uint size, byte* buffer, uint patternSize, uint* filled);

        [PreserveSig]
        int FillPhysical(ulong start, uint size, byte* buffer, uint patternSize, uint* filled);

        [PreserveSig]
        int QueryVirtual(ulong offset, MEMORY_BASIC_INFORMATION64* info);
    }
}
