// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("8C31E98C-983A-48A5-9016-6FE5D667A950")]
    internal unsafe partial interface IDebugSymbolsGenerated
    {
        [PreserveSig]
        int GetSymbolOptions(out SYMOPT options);

        [PreserveSig]
        int AddSymbolOptions(SYMOPT options);

        [PreserveSig]
        int RemoveSymbolOptions(SYMOPT options);

        [PreserveSig]
        int SetSymbolOptions(SYMOPT options);

        [PreserveSig]
        int GetNameByOffset(ulong offset, byte* nameBuffer, uint nameBufferSize, uint* nameSize, ulong* displacement);

        [PreserveSig]
        int GetOffsetByName([MarshalAs(UnmanagedType.LPStr)] string symbol, ulong* offset);

        [PreserveSig]
        int GetNearNameByOffset(ulong offset, int delta, byte* nameBuffer, uint nameBufferSize, uint* nameSize, ulong* displacement);

        [PreserveSig]
        int GetLineByOffset(ulong offset, uint* line, byte* fileBuffer, uint fileBufferSize, uint* fileSize, ulong* displacement);

        [PreserveSig]
        int GetOffsetByLine(uint line, [MarshalAs(UnmanagedType.LPStr)] string file, ulong* offset);

        [PreserveSig]
        int GetNumberModules(out uint loaded, out uint unloaded);

        [PreserveSig]
        int GetModuleByIndex(uint index, out ulong @base);

        [PreserveSig]
        int GetModuleByModuleName([MarshalAs(UnmanagedType.LPStr)] string name, uint startIndex, uint* index, ulong* @base);

        [PreserveSig]
        int GetModuleByOffset(ulong offset, uint startIndex, uint* index, ulong* @base);

        [PreserveSig]
        int GetModuleNames(uint index, ulong @base, byte* imageNameBuffer, uint imageNameBufferSize, uint* imageNameSize,
            byte* moduleNameBuffer, uint moduleNameBufferSize, uint* moduleNameSize, byte* loadedImageNameBuffer,
            uint loadedImageNameBufferSize, uint* loadedImageNameSize);

        [PreserveSig]
        int GetModuleParameters(uint count, ulong* bases, uint start, DEBUG_MODULE_PARAMETERS* @params);

        [PreserveSig]
        int GetSymbolModule([MarshalAs(UnmanagedType.LPStr)] string symbol, ulong* @base);

        [PreserveSig]
        int GetTypeName(ulong module, uint typeId, byte* nameBuffer, uint nameBufferSize, uint* nameSize);

        [PreserveSig]
        int GetTypeId(ulong module, [MarshalAs(UnmanagedType.LPStr)] string name, uint* typeId);

        [PreserveSig]
        int GetTypeSize(ulong module, uint typeId, uint* size);

        [PreserveSig]
        int GetFieldOffset(ulong module, uint typeId, [MarshalAs(UnmanagedType.LPStr)] string field, uint* offset);

        [PreserveSig]
        int GetSymbolTypeId([MarshalAs(UnmanagedType.LPStr)] string symbol, uint* typeId, ulong* module);

        [PreserveSig]
        int GetOffsetTypeId(ulong offset, uint* typeId, ulong* module);

        [PreserveSig]
        int ReadTypedDataVirtual(ulong offset, ulong module, uint typeId, byte* buffer, uint bufferSize, uint* bytesRead);

        [PreserveSig]
        int WriteTypedDataVirtual(ulong offset, ulong module, uint typeId, IntPtr buffer, uint bufferSize, uint* bytesWritten);

        [PreserveSig]
        int OutputTypedDataVirtual(DEBUG_OUTCTL outputControl, ulong offset, ulong module, uint typeId, DEBUG_TYPEOPTS flags);

        [PreserveSig]
        int ReadTypedDataPhysical(ulong offset, ulong module, uint typeId, IntPtr buffer, uint bufferSize, uint* bytesRead);

        [PreserveSig]
        int WriteTypedDataPhysical(ulong offset, ulong module, uint typeId, IntPtr buffer, uint bufferSize, uint* bytesWritten);

        [PreserveSig]
        int OutputTypedDataPhysical(DEBUG_OUTCTL outputControl, ulong offset, ulong module, uint typeId, DEBUG_TYPEOPTS flags);

        [PreserveSig]
        int GetScope(ulong* instructionOffset, DEBUG_STACK_FRAME* scopeFrame, IntPtr scopeContext, uint scopeContextSize);

        [PreserveSig]
        int SetScope(ulong instructionOffset, in DEBUG_STACK_FRAME scopeFrame, IntPtr scopeContext, uint scopeContextSize);

        [PreserveSig]
        int ResetScope();

        [PreserveSig]
        int GetScopeSymbolGroup(DEBUG_SCOPE_GROUP flags, IntPtr update, out IntPtr symbols);

        [PreserveSig]
        int CreateSymbolGroup(out IntPtr group);

        [PreserveSig]
        int StartSymbolMatch([MarshalAs(UnmanagedType.LPStr)] string pattern, ulong* handle);

        [PreserveSig]
        int GetNextSymbolMatch(ulong handle, byte* buffer, uint bufferSize, uint* matchSize, ulong* offset);

        [PreserveSig]
        int EndSymbolMatch(ulong handle);

        [PreserveSig]
        int Reload([MarshalAs(UnmanagedType.LPStr)] string module);

        [PreserveSig]
        int GetSymbolPath(byte* buffer, uint bufferSize, uint* pathSize);

        [PreserveSig]
        int SetSymbolPath([MarshalAs(UnmanagedType.LPStr)] string path);

        [PreserveSig]
        int AppendSymbolPath([MarshalAs(UnmanagedType.LPStr)] string addition);

        [PreserveSig]
        int GetImagePath(byte* buffer, uint bufferSize, uint* pathSize);

        [PreserveSig]
        int SetImagePath([MarshalAs(UnmanagedType.LPStr)] string path);

        [PreserveSig]
        int AppendImagePath([MarshalAs(UnmanagedType.LPStr)] string addition);

        [PreserveSig]
        int GetSourcePath(byte* buffer, uint bufferSize, uint* pathSize);

        [PreserveSig]
        int GetSourcePathElement(uint index, byte* buffer, uint bufferSize, uint* elementSize);

        [PreserveSig]
        int SetSourcePath([MarshalAs(UnmanagedType.LPStr)] string path);

        [PreserveSig]
        int AppendSourcePath([MarshalAs(UnmanagedType.LPStr)] string addition);

        [PreserveSig]
        int FindSourceFile(uint startElement, [MarshalAs(UnmanagedType.LPStr)] string file, DEBUG_FIND_SOURCE flags, uint* foundElement,
            byte* buffer, uint bufferSize, uint* foundSize);

        [PreserveSig]
        int GetSourceFileLineOffsets([MarshalAs(UnmanagedType.LPStr)] string file, ulong* buffer, uint bufferLines, uint* fileLines);
    }
}
