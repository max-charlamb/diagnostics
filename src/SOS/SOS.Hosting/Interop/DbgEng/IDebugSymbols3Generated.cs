// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("F02FBECC-50AC-4F36-9AD9-C975E8F32FF8")]
    internal unsafe partial interface IDebugSymbols3Generated : IDebugSymbols2Generated
    {
        [PreserveSig]
        int GetNameByOffsetWide(ulong offset, char* nameBuffer, uint nameBufferSize, uint* nameSize, ulong* displacement);

        [PreserveSig]
        int GetOffsetByNameWide([MarshalAs(UnmanagedType.LPWStr)] string symbol, ulong* offset);

        [PreserveSig]
        int GetNearNameByOffsetWide(ulong offset, int delta, char* nameBuffer, uint nameBufferSize, uint* nameSize,
            ulong* displacement);

        [PreserveSig]
        int GetLineByOffsetWide(ulong offset, uint* line, char* fileBuffer, uint fileBufferSize, uint* fileSize,
            ulong* displacement);

        [PreserveSig]
        int GetOffsetByLineWide(uint line, [MarshalAs(UnmanagedType.LPWStr)] string file, ulong* offset);

        [PreserveSig]
        int GetModuleByModuleNameWide([MarshalAs(UnmanagedType.LPWStr)] string name, uint startIndex, uint* index, ulong* @base);

        [PreserveSig]
        int GetSymbolModuleWide([MarshalAs(UnmanagedType.LPWStr)] string symbol, ulong* @base);

        [PreserveSig]
        int GetTypeNameWide(ulong module, uint typeId, char* nameBuffer, uint nameBufferSize, uint* nameSize);

        [PreserveSig]
        int GetTypeIdWide(ulong module, [MarshalAs(UnmanagedType.LPWStr)] string name, uint* typeId);

        [PreserveSig]
        int GetFieldOffsetWide(ulong module, uint typeId, [MarshalAs(UnmanagedType.LPWStr)] string field, uint* offset);

        [PreserveSig]
        int GetSymbolTypeIdWide([MarshalAs(UnmanagedType.LPWStr)] string symbol, uint* typeId, ulong* module);

        [PreserveSig]
        int GetScopeSymbolGroup2(DEBUG_SCOPE_GROUP flags, IntPtr update, out IntPtr symbols);

        [PreserveSig]
        int CreateSymbolGroup2(out IntPtr group);

        [PreserveSig]
        int StartSymbolMatchWide([MarshalAs(UnmanagedType.LPWStr)] string pattern, ulong* handle);

        [PreserveSig]
        int GetNextSymbolMatchWide(ulong handle, char* buffer, uint bufferSize, uint* matchSize, ulong* offset);

        [PreserveSig]
        int ReloadWide([MarshalAs(UnmanagedType.LPWStr)] string module);

        [PreserveSig]
        int GetSymbolPathWide(char* buffer, uint bufferSize, uint* pathSize);

        [PreserveSig]
        int SetSymbolPathWide([MarshalAs(UnmanagedType.LPWStr)] string path);

        [PreserveSig]
        int AppendSymbolPathWide([MarshalAs(UnmanagedType.LPWStr)] string addition);

        [PreserveSig]
        int GetImagePathWide(char* buffer, uint bufferSize, uint* pathSize);

        [PreserveSig]
        int SetImagePathWide([MarshalAs(UnmanagedType.LPWStr)] string path);

        [PreserveSig]
        int AppendImagePathWide([MarshalAs(UnmanagedType.LPWStr)] string addition);

        [PreserveSig]
        int GetSourcePathWide(char* buffer, uint bufferSize, uint* pathSize);

        [PreserveSig]
        int GetSourcePathElementWide(uint index, char* buffer, uint bufferSize, uint* elementSize);

        [PreserveSig]
        int SetSourcePathWide([MarshalAs(UnmanagedType.LPWStr)] string path);

        [PreserveSig]
        int AppendSourcePathWide([MarshalAs(UnmanagedType.LPWStr)] string addition);

        [PreserveSig]
        int FindSourceFileWide(uint startElement, [MarshalAs(UnmanagedType.LPWStr)] string file, DEBUG_FIND_SOURCE flags,
            uint* foundElement, char* buffer, uint bufferSize, uint* foundSize);

        [PreserveSig]
        int GetSourceFileLineOffsetsWide([MarshalAs(UnmanagedType.LPWStr)] string file, ulong* buffer, uint bufferLines, uint* fileLines);

        [PreserveSig]
        int GetModuleVersionInformationWide(uint index, ulong @base, [MarshalAs(UnmanagedType.LPWStr)] string item, IntPtr buffer,
            uint bufferSize, uint* verInfoSize);

        [PreserveSig]
        int GetModuleNameStringWide(DEBUG_MODNAME which, uint index, ulong @base, char* buffer, uint bufferSize, uint* nameSize);

        [PreserveSig]
        int GetConstantNameWide(ulong module, uint typeId, ulong value, char* buffer, uint bufferSize, uint* nameSize);

        [PreserveSig]
        int GetFieldNameWide(ulong module, uint typeId, uint fieldIndex, char* buffer, uint bufferSize, uint* nameSize);

        [PreserveSig]
        int IsManagedModule(uint index, ulong @base);

        [PreserveSig]
        int GetModuleByModuleName2([MarshalAs(UnmanagedType.LPStr)] string name, uint startIndex, DEBUG_GETMOD flags, uint* index,
            ulong* @base);

        [PreserveSig]
        int GetModuleByModuleName2Wide([MarshalAs(UnmanagedType.LPWStr)] string name, uint startIndex, DEBUG_GETMOD flags, uint* index,
            ulong* @base);

        [PreserveSig]
        int GetModuleByOffset2(ulong offset, uint startIndex, DEBUG_GETMOD flags, uint* index, ulong* @base);

        [PreserveSig]
        int AddSyntheticModule(ulong @base, uint size, [MarshalAs(UnmanagedType.LPStr)] string imagePath,
            [MarshalAs(UnmanagedType.LPStr)] string moduleName, DEBUG_ADDSYNTHMOD flags);

        [PreserveSig]
        int AddSyntheticModuleWide(ulong @base, uint size, [MarshalAs(UnmanagedType.LPWStr)] string imagePath,
            [MarshalAs(UnmanagedType.LPWStr)] string moduleName, DEBUG_ADDSYNTHMOD flags);

        [PreserveSig]
        int RemoveSyntheticModule(ulong @base);

        [PreserveSig]
        int GetCurrentScopeFrameIndex(uint* index);

        [PreserveSig]
        int SetScopeFrameByIndex(uint index);

        [PreserveSig]
        int SetScopeFromJitDebugInfo(uint outputControl, ulong infoOffset);

        [PreserveSig]
        int SetScopeFromStoredEvent();

        [PreserveSig]
        int OutputSymbolByOffset(uint outputControl, DEBUG_OUTSYM flags, ulong offset);

        [PreserveSig]
        int GetFunctionEntryByOffset(ulong offset, DEBUG_GETFNENT flags, IntPtr buffer, uint bufferSize, uint* bufferNeeded);

        [PreserveSig]
        int GetFieldTypeAndOffset(ulong module, uint containerTypeId, [MarshalAs(UnmanagedType.LPStr)] string field, uint* fieldTypeId,
            uint* offset);

        [PreserveSig]
        int GetFieldTypeAndOffsetWide(ulong module, uint containerTypeId, [MarshalAs(UnmanagedType.LPWStr)] string field,
            uint* fieldTypeId, uint* offset);

        [PreserveSig]
        int AddSyntheticSymbol(ulong offset, uint size, [MarshalAs(UnmanagedType.LPStr)] string name, DEBUG_ADDSYNTHSYM flags,
            DEBUG_MODULE_AND_ID* id);

        [PreserveSig]
        int AddSyntheticSymbolWide(ulong offset, uint size, [MarshalAs(UnmanagedType.LPWStr)] string name, DEBUG_ADDSYNTHSYM flags,
            DEBUG_MODULE_AND_ID* id);

        [PreserveSig]
        int RemoveSyntheticSymbol(DEBUG_MODULE_AND_ID* id);

        [PreserveSig]
        int GetSymbolEntriesByOffset(ulong offset, uint flags, DEBUG_MODULE_AND_ID* ids, ulong* displacements, uint idsCount,
            uint* entries);

        [PreserveSig]
        int GetSymbolEntriesByName([MarshalAs(UnmanagedType.LPStr)] string symbol, uint flags, DEBUG_MODULE_AND_ID* ids, uint idsCount,
            uint* entries);

        [PreserveSig]
        int GetSymbolEntriesByNameWide([MarshalAs(UnmanagedType.LPWStr)] string symbol, uint flags, DEBUG_MODULE_AND_ID* ids, uint idsCount,
            uint* entries);

        [PreserveSig]
        int GetSymbolEntryByToken(ulong moduleBase, uint token, DEBUG_MODULE_AND_ID* id);

        [PreserveSig]
        int GetSymbolEntryInformation(DEBUG_MODULE_AND_ID* id, DEBUG_SYMBOL_ENTRY* info);

        [PreserveSig]
        int GetSymbolEntryString(DEBUG_MODULE_AND_ID* id, uint which, byte* buffer, uint bufferSize, uint* stringSize);

        [PreserveSig]
        int GetSymbolEntryStringWide(DEBUG_MODULE_AND_ID* id, uint which, char* buffer, uint bufferSize, uint* stringSize);

        [PreserveSig]
        int GetSymbolEntryOffsetRegions(DEBUG_MODULE_AND_ID* id, uint flags, DEBUG_OFFSET_REGION* regions, uint regionsCount,
            uint* regionsAvail);

        [PreserveSig]
        int GetSymbolEntryBySymbolEntry(DEBUG_MODULE_AND_ID* fromId, uint flags, DEBUG_MODULE_AND_ID* toId);

        [PreserveSig]
        int GetSourceEntriesByOffset(ulong offset, uint flags, DEBUG_SYMBOL_SOURCE_ENTRY* entries, uint entriesCount, uint* entriesAvail);

        [PreserveSig]
        int GetSourceEntriesByLine(uint line, [MarshalAs(UnmanagedType.LPStr)] string file, uint flags, DEBUG_SYMBOL_SOURCE_ENTRY* entries,
            uint entriesCount, uint* entriesAvail);

        [PreserveSig]
        int GetSourceEntriesByLineWide(uint line, [MarshalAs(UnmanagedType.LPWStr)] string file, uint flags,
            DEBUG_SYMBOL_SOURCE_ENTRY* entries, uint entriesCount, uint* entriesAvail);

        [PreserveSig]
        int GetSourceEntryString(in DEBUG_SYMBOL_SOURCE_ENTRY entry, uint which, byte* buffer, uint bufferSize, uint* stringSize);

        [PreserveSig]
        int GetSourceEntryStringWide(in DEBUG_SYMBOL_SOURCE_ENTRY entry, uint which, char* buffer, uint bufferSize, uint* stringSize);

        [PreserveSig]
        int GetSourceEntryOffsetRegions(DEBUG_SYMBOL_SOURCE_ENTRY* entry, uint flags, DEBUG_OFFSET_REGION* regions, uint regionsCount,
            uint* regionsAvail);

        [PreserveSig]
        int GetSourceEntryBySourceEntry(DEBUG_SYMBOL_SOURCE_ENTRY* fromEntry, uint flags, DEBUG_SYMBOL_SOURCE_ENTRY* toEntry);
    }
}
