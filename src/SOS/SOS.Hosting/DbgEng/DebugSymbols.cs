// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop.DbgEng;

namespace SOS.Hosting.DbgEng
{
    internal sealed unsafe partial class DebugClient
    {
        int IDebugSymbolsGenerated.GetSymbolOptions(out SYMOPT options)
        {
            return SOSHost.GetSymbolOptions(IntPtr.Zero, out options);
        }

        int IDebugSymbolsGenerated.AddSymbolOptions(SYMOPT options)
        {
            return HResult.S_OK;
        }

        int IDebugSymbolsGenerated.RemoveSymbolOptions(SYMOPT options)
        {
            return HResult.S_OK;
        }

        int IDebugSymbolsGenerated.SetSymbolOptions(SYMOPT options)
        {
            return HResult.S_OK;
        }

        int IDebugSymbolsGenerated.GetNameByOffset(ulong offset, byte* nameBuffer, uint nameBufferSize, uint* nameSize, ulong* displacement)
        {
            StringBuilder nameBufferBuilder = CreateStringBuilder(nameBuffer, nameBufferSize);
            int result = SOSHost.GetNameByOffset(IntPtr.Zero, offset, nameBufferBuilder, nameBufferSize, nameSize, displacement);
            CopyStringBuffer(nameBufferBuilder, nameBuffer, nameBufferSize);
            return result;
        }

        int IDebugSymbolsGenerated.GetOffsetByName(string symbol, ulong* offset) => NotImplemented;
        int IDebugSymbolsGenerated.GetNearNameByOffset(ulong offset, int delta, byte* nameBuffer, uint nameBufferSize, uint* nameSize, ulong* displacement) => NotImplemented;

        int IDebugSymbolsGenerated.GetLineByOffset(ulong offset, uint* line, byte* fileBuffer, uint fileBufferSize, uint* fileSize, ulong* displacement)
        {
            StringBuilder fileBufferBuilder = CreateStringBuilder(fileBuffer, fileBufferSize);
            int result = SOSHost.GetLineByOffset(IntPtr.Zero, offset, line, fileBufferBuilder, fileBufferSize, fileSize, displacement);
            CopyStringBuffer(fileBufferBuilder, fileBuffer, fileBufferSize);
            return result;
        }

        int IDebugSymbolsGenerated.GetOffsetByLine(uint line, string file, ulong* offset) => NotImplemented;

        int IDebugSymbolsGenerated.GetNumberModules(out uint loaded, out uint unloaded)
        {
            return _soshost.GetNumberModules(IntPtr.Zero, out loaded, out unloaded);
        }

        int IDebugSymbolsGenerated.GetModuleByIndex(uint index, out ulong @base)
        {
            return _soshost.GetModuleByIndex(IntPtr.Zero, index, out @base);
        }

        int IDebugSymbolsGenerated.GetModuleByModuleName(string name, uint startIndex, uint* index, ulong* @base)
        {
            return _soshost.GetModuleByModuleName(IntPtr.Zero, name, startIndex, index, @base);
        }

        int IDebugSymbolsGenerated.GetModuleByOffset(ulong offset, uint startIndex, uint* index, ulong* @base)
        {
            return _soshost.GetModuleByOffset(IntPtr.Zero, offset, startIndex, index, @base);
        }

        int IDebugSymbolsGenerated.GetModuleNames(uint index, ulong @base, byte* imageNameBuffer, uint imageNameBufferSize, uint* imageNameSize, byte* moduleNameBuffer, uint moduleNameBufferSize, uint* moduleNameSize, byte* loadedImageNameBuffer, uint loadedImageNameBufferSize, uint* loadedImageNameSize)
        {
            StringBuilder imageNameBufferBuilder = CreateStringBuilder(imageNameBuffer, imageNameBufferSize);
            StringBuilder moduleNameBufferBuilder = CreateStringBuilder(moduleNameBuffer, moduleNameBufferSize);
            StringBuilder loadedImageNameBufferBuilder = CreateStringBuilder(loadedImageNameBuffer, loadedImageNameBufferSize);
            int result = _soshost.GetModuleNames(IntPtr.Zero, index, @base, imageNameBufferBuilder, imageNameBufferSize, imageNameSize, moduleNameBufferBuilder, moduleNameBufferSize, moduleNameSize, loadedImageNameBufferBuilder, loadedImageNameBufferSize, loadedImageNameSize);
            CopyStringBuffer(imageNameBufferBuilder, imageNameBuffer, imageNameBufferSize);
            CopyStringBuffer(moduleNameBufferBuilder, moduleNameBuffer, moduleNameBufferSize);
            CopyStringBuffer(loadedImageNameBufferBuilder, loadedImageNameBuffer, loadedImageNameBufferSize);
            return result;
        }

        int IDebugSymbolsGenerated.GetModuleParameters(uint count, ulong* bases, uint start, DEBUG_MODULE_PARAMETERS* @params)
        {
            return _soshost.GetModuleParameters(IntPtr.Zero, count, bases, start, @params);
        }

        int IDebugSymbolsGenerated.GetSymbolModule(string symbol, ulong* @base) => NotImplemented;
        int IDebugSymbolsGenerated.GetTypeName(ulong module, uint typeId, byte* nameBuffer, uint nameBufferSize, uint* nameSize) => NotImplemented;
        int IDebugSymbolsGenerated.GetTypeId(ulong module, string name, uint* typeId) => NotImplemented;
        int IDebugSymbolsGenerated.GetTypeSize(ulong module, uint typeId, uint* size) => NotImplemented;
        int IDebugSymbolsGenerated.GetFieldOffset(ulong module, uint typeId, string field, uint* offset) => NotImplemented;
        int IDebugSymbolsGenerated.GetSymbolTypeId(string symbol, uint* typeId, ulong* module) => NotImplemented;
        int IDebugSymbolsGenerated.GetOffsetTypeId(ulong offset, uint* typeId, ulong* module) => NotImplemented;
        int IDebugSymbolsGenerated.ReadTypedDataVirtual(ulong offset, ulong module, uint typeId, byte* buffer, uint bufferSize, uint* bytesRead) => NotImplemented;
        int IDebugSymbolsGenerated.WriteTypedDataVirtual(ulong offset, ulong module, uint typeId, IntPtr buffer, uint bufferSize, uint* bytesWritten) => NotImplemented;
        int IDebugSymbolsGenerated.OutputTypedDataVirtual(DEBUG_OUTCTL outputControl, ulong offset, ulong module, uint typeId, DEBUG_TYPEOPTS flags) => NotImplemented;
        int IDebugSymbolsGenerated.ReadTypedDataPhysical(ulong offset, ulong module, uint typeId, IntPtr buffer, uint bufferSize, uint* bytesRead) => NotImplemented;
        int IDebugSymbolsGenerated.WriteTypedDataPhysical(ulong offset, ulong module, uint typeId, IntPtr buffer, uint bufferSize, uint* bytesWritten) => NotImplemented;
        int IDebugSymbolsGenerated.OutputTypedDataPhysical(DEBUG_OUTCTL outputControl, ulong offset, ulong module, uint typeId, DEBUG_TYPEOPTS flags) => NotImplemented;
        int IDebugSymbolsGenerated.GetScope(ulong* instructionOffset, DEBUG_STACK_FRAME* scopeFrame, IntPtr scopeContext, uint scopeContextSize) => NotImplemented;
        int IDebugSymbolsGenerated.SetScope(ulong instructionOffset, in DEBUG_STACK_FRAME scopeFrame, IntPtr scopeContext, uint scopeContextSize) => NotImplemented;
        int IDebugSymbolsGenerated.ResetScope() => NotImplemented;
        int IDebugSymbolsGenerated.GetScopeSymbolGroup(DEBUG_SCOPE_GROUP flags, IntPtr update, out IntPtr symbols)
        {
            symbols = default;
            return NotImplemented;
        }
        int IDebugSymbolsGenerated.CreateSymbolGroup(out IntPtr group)
        {
            group = default;
            return NotImplemented;
        }
        int IDebugSymbolsGenerated.StartSymbolMatch(string pattern, ulong* handle) => NotImplemented;
        int IDebugSymbolsGenerated.GetNextSymbolMatch(ulong handle, byte* buffer, uint bufferSize, uint* matchSize, ulong* offset) => NotImplemented;
        int IDebugSymbolsGenerated.EndSymbolMatch(ulong handle) => NotImplemented;
        int IDebugSymbolsGenerated.Reload(string module) => NotImplemented;

        int IDebugSymbolsGenerated.GetSymbolPath(byte* buffer, uint bufferSize, uint* pathSize)
        {
            StringBuilder bufferBuilder = CreateStringBuilder(buffer, bufferSize);
            int result = SOSHost.GetSymbolPath(IntPtr.Zero, bufferBuilder, unchecked((int)bufferSize), pathSize);
            CopyStringBuffer(bufferBuilder, buffer, bufferSize);
            return result;
        }

        int IDebugSymbolsGenerated.SetSymbolPath(string path) => NotImplemented;
        int IDebugSymbolsGenerated.AppendSymbolPath(string addition) => NotImplemented;
        int IDebugSymbolsGenerated.GetImagePath(byte* buffer, uint bufferSize, uint* pathSize) => NotImplemented;
        int IDebugSymbolsGenerated.SetImagePath(string path) => NotImplemented;
        int IDebugSymbolsGenerated.AppendImagePath(string addition) => NotImplemented;
        int IDebugSymbolsGenerated.GetSourcePath(byte* buffer, uint bufferSize, uint* pathSize) => NotImplemented;
        int IDebugSymbolsGenerated.GetSourcePathElement(uint index, byte* buffer, uint bufferSize, uint* elementSize) => NotImplemented;
        int IDebugSymbolsGenerated.SetSourcePath(string path) => NotImplemented;
        int IDebugSymbolsGenerated.AppendSourcePath(string addition) => NotImplemented;

        int IDebugSymbolsGenerated.FindSourceFile(uint startElement, string file, DEBUG_FIND_SOURCE flags, uint* foundElement, byte* buffer, uint bufferSize, uint* foundSize)
        {
            StringBuilder bufferBuilder = CreateStringBuilder(buffer, bufferSize);
            int result = SOSHost.FindSourceFile(IntPtr.Zero, startElement, file, flags, foundElement, bufferBuilder, bufferSize, foundSize);
            CopyStringBuffer(bufferBuilder, buffer, bufferSize);
            return result;
        }

        int IDebugSymbolsGenerated.GetSourceFileLineOffsets(string file, ulong* buffer, uint bufferLines, uint* fileLines) => NotImplemented;

        int IDebugSymbols2Generated.GetModuleVersionInformation(uint index, ulong @base, string item, byte* buffer, uint bufferSize, uint* verInfoSize)
        {
            return _soshost.GetModuleVersionInformation(IntPtr.Zero, index, @base, item, buffer, bufferSize, verInfoSize);
        }

        int IDebugSymbols2Generated.GetModuleNameString(DEBUG_MODNAME which, uint index, ulong @base, byte* buffer, uint bufferSize, uint* nameSize) => NotImplemented;
        int IDebugSymbols2Generated.GetConstantName(ulong module, uint typeId, ulong value, byte* buffer, uint bufferSize, uint* nameSize) => NotImplemented;
        int IDebugSymbols2Generated.GetFieldName(ulong module, uint typeId, uint fieldIndex, byte* buffer, uint bufferSize, uint* nameSize) => NotImplemented;
        int IDebugSymbols2Generated.GetTypeOptions(DEBUG_TYPEOPTS* options) => NotImplemented;
        int IDebugSymbols2Generated.AddTypeOptions(DEBUG_TYPEOPTS options) => NotImplemented;
        int IDebugSymbols2Generated.RemoveTypeOptions(DEBUG_TYPEOPTS options) => NotImplemented;
        int IDebugSymbols2Generated.SetTypeOptions(DEBUG_TYPEOPTS options) => NotImplemented;

        int IDebugSymbols3Generated.GetNameByOffsetWide(ulong offset, char* nameBuffer, uint nameBufferSize, uint* nameSize, ulong* displacement)
        {
            StringBuilder nameBufferBuilder = CreateStringBuilder(nameBuffer, nameBufferSize);
            int result = SOSHost.GetNameByOffset(IntPtr.Zero, offset, nameBufferBuilder, nameBufferSize, nameSize, displacement);
            CopyStringBuffer(nameBufferBuilder, nameBuffer, nameBufferSize);
            return result;
        }

        int IDebugSymbols3Generated.GetOffsetByNameWide(string symbol, ulong* offset) => NotImplemented;
        int IDebugSymbols3Generated.GetNearNameByOffsetWide(ulong offset, int delta, char* nameBuffer, uint nameBufferSize, uint* nameSize, ulong* displacement) => NotImplemented;
        int IDebugSymbols3Generated.GetLineByOffsetWide(ulong offset, uint* line, char* fileBuffer, uint fileBufferSize, uint* fileSize, ulong* displacement) => NotImplemented;
        int IDebugSymbols3Generated.GetOffsetByLineWide(uint line, string file, ulong* offset) => NotImplemented;

        int IDebugSymbols3Generated.GetModuleByModuleNameWide(string name, uint startIndex, uint* index, ulong* @base)
        {
            return _soshost.GetModuleByModuleName(IntPtr.Zero, name, startIndex, index, @base);
        }

        int IDebugSymbols3Generated.GetSymbolModuleWide(string symbol, ulong* @base) => NotImplemented;
        int IDebugSymbols3Generated.GetTypeNameWide(ulong module, uint typeId, char* nameBuffer, uint nameBufferSize, uint* nameSize) => NotImplemented;
        int IDebugSymbols3Generated.GetTypeIdWide(ulong module, string name, uint* typeId) => NotImplemented;
        int IDebugSymbols3Generated.GetFieldOffsetWide(ulong module, uint typeId, string field, uint* offset) => NotImplemented;
        int IDebugSymbols3Generated.GetSymbolTypeIdWide(string symbol, uint* typeId, ulong* module) => NotImplemented;
        int IDebugSymbols3Generated.GetScopeSymbolGroup2(DEBUG_SCOPE_GROUP flags, IntPtr update, out IntPtr symbols)
        {
            symbols = default;
            return NotImplemented;
        }
        int IDebugSymbols3Generated.CreateSymbolGroup2(out IntPtr group)
        {
            group = default;
            return NotImplemented;
        }
        int IDebugSymbols3Generated.StartSymbolMatchWide(string pattern, ulong* handle) => NotImplemented;
        int IDebugSymbols3Generated.GetNextSymbolMatchWide(ulong handle, char* buffer, uint bufferSize, uint* matchSize, ulong* offset) => NotImplemented;
        int IDebugSymbols3Generated.ReloadWide(string module) => NotImplemented;

        int IDebugSymbols3Generated.GetSymbolPathWide(char* buffer, uint bufferSize, uint* pathSize)
        {
            StringBuilder bufferBuilder = CreateStringBuilder(buffer, bufferSize);
            int result = SOSHost.GetSymbolPath(IntPtr.Zero, bufferBuilder, unchecked((int)bufferSize), pathSize);
            CopyStringBuffer(bufferBuilder, buffer, bufferSize);
            return result;
        }

        int IDebugSymbols3Generated.SetSymbolPathWide(string path) => NotImplemented;
        int IDebugSymbols3Generated.AppendSymbolPathWide(string addition) => NotImplemented;
        int IDebugSymbols3Generated.GetImagePathWide(char* buffer, uint bufferSize, uint* pathSize) => NotImplemented;
        int IDebugSymbols3Generated.SetImagePathWide(string path) => NotImplemented;
        int IDebugSymbols3Generated.AppendImagePathWide(string addition) => NotImplemented;
        int IDebugSymbols3Generated.GetSourcePathWide(char* buffer, uint bufferSize, uint* pathSize) => NotImplemented;
        int IDebugSymbols3Generated.GetSourcePathElementWide(uint index, char* buffer, uint bufferSize, uint* elementSize) => NotImplemented;
        int IDebugSymbols3Generated.SetSourcePathWide(string path) => NotImplemented;
        int IDebugSymbols3Generated.AppendSourcePathWide(string addition) => NotImplemented;

        int IDebugSymbols3Generated.FindSourceFileWide(uint startElement, string file, DEBUG_FIND_SOURCE flags, uint* foundElement, char* buffer, uint bufferSize, uint* foundSize)
        {
            StringBuilder bufferBuilder = CreateStringBuilder(buffer, bufferSize);
            int result = SOSHost.FindSourceFile(IntPtr.Zero, startElement, file, flags, foundElement, bufferBuilder, bufferSize, foundSize);
            CopyStringBuffer(bufferBuilder, buffer, bufferSize);
            return result;
        }

        int IDebugSymbols3Generated.GetSourceFileLineOffsetsWide(string file, ulong* buffer, uint bufferLines, uint* fileLines) => NotImplemented;
        int IDebugSymbols3Generated.GetModuleVersionInformationWide(uint index, ulong @base, string item, IntPtr buffer, uint bufferSize, uint* verInfoSize) => NotImplemented;
        int IDebugSymbols3Generated.GetModuleNameStringWide(DEBUG_MODNAME which, uint index, ulong @base, char* buffer, uint bufferSize, uint* nameSize) => NotImplemented;
        int IDebugSymbols3Generated.GetConstantNameWide(ulong module, uint typeId, ulong value, char* buffer, uint bufferSize, uint* nameSize) => NotImplemented;
        int IDebugSymbols3Generated.GetFieldNameWide(ulong module, uint typeId, uint fieldIndex, char* buffer, uint bufferSize, uint* nameSize) => NotImplemented;
        int IDebugSymbols3Generated.IsManagedModule(uint index, ulong @base) => NotImplemented;
        int IDebugSymbols3Generated.GetModuleByModuleName2(string name, uint startIndex, DEBUG_GETMOD flags, uint* index, ulong* @base) => NotImplemented;
        int IDebugSymbols3Generated.GetModuleByModuleName2Wide(string name, uint startIndex, DEBUG_GETMOD flags, uint* index, ulong* @base) => NotImplemented;
        int IDebugSymbols3Generated.GetModuleByOffset2(ulong offset, uint startIndex, DEBUG_GETMOD flags, uint* index, ulong* @base) => NotImplemented;
        int IDebugSymbols3Generated.AddSyntheticModule(ulong @base, uint size, string imagePath, string moduleName, DEBUG_ADDSYNTHMOD flags) => NotImplemented;
        int IDebugSymbols3Generated.AddSyntheticModuleWide(ulong @base, uint size, string imagePath, string moduleName, DEBUG_ADDSYNTHMOD flags) => NotImplemented;
        int IDebugSymbols3Generated.RemoveSyntheticModule(ulong @base) => NotImplemented;
        int IDebugSymbols3Generated.GetCurrentScopeFrameIndex(uint* index) => NotImplemented;
        int IDebugSymbols3Generated.SetScopeFrameByIndex(uint index) => NotImplemented;
        int IDebugSymbols3Generated.SetScopeFromJitDebugInfo(uint outputControl, ulong infoOffset) => NotImplemented;
        int IDebugSymbols3Generated.SetScopeFromStoredEvent() => NotImplemented;
        int IDebugSymbols3Generated.OutputSymbolByOffset(uint outputControl, DEBUG_OUTSYM flags, ulong offset) => NotImplemented;
        int IDebugSymbols3Generated.GetFunctionEntryByOffset(ulong offset, DEBUG_GETFNENT flags, IntPtr buffer, uint bufferSize, uint* bufferNeeded) => NotImplemented;
        int IDebugSymbols3Generated.GetFieldTypeAndOffset(ulong module, uint containerTypeId, string field, uint* fieldTypeId, uint* offset) => NotImplemented;
        int IDebugSymbols3Generated.GetFieldTypeAndOffsetWide(ulong module, uint containerTypeId, string field, uint* fieldTypeId, uint* offset) => NotImplemented;
        int IDebugSymbols3Generated.AddSyntheticSymbol(ulong offset, uint size, string name, DEBUG_ADDSYNTHSYM flags, DEBUG_MODULE_AND_ID* id) => NotImplemented;
        int IDebugSymbols3Generated.AddSyntheticSymbolWide(ulong offset, uint size, string name, DEBUG_ADDSYNTHSYM flags, DEBUG_MODULE_AND_ID* id) => NotImplemented;
        int IDebugSymbols3Generated.RemoveSyntheticSymbol(DEBUG_MODULE_AND_ID* id) => NotImplemented;
        int IDebugSymbols3Generated.GetSymbolEntriesByOffset(ulong offset, uint flags, DEBUG_MODULE_AND_ID* ids, ulong* displacements, uint idsCount, uint* entries) => NotImplemented;
        int IDebugSymbols3Generated.GetSymbolEntriesByName(string symbol, uint flags, DEBUG_MODULE_AND_ID* ids, uint idsCount, uint* entries) => NotImplemented;
        int IDebugSymbols3Generated.GetSymbolEntriesByNameWide(string symbol, uint flags, DEBUG_MODULE_AND_ID* ids, uint idsCount, uint* entries) => NotImplemented;
        int IDebugSymbols3Generated.GetSymbolEntryByToken(ulong moduleBase, uint token, DEBUG_MODULE_AND_ID* id) => NotImplemented;
        int IDebugSymbols3Generated.GetSymbolEntryInformation(DEBUG_MODULE_AND_ID* id, DEBUG_SYMBOL_ENTRY* info) => NotImplemented;
        int IDebugSymbols3Generated.GetSymbolEntryString(DEBUG_MODULE_AND_ID* id, uint which, byte* buffer, uint bufferSize, uint* stringSize) => NotImplemented;
        int IDebugSymbols3Generated.GetSymbolEntryStringWide(DEBUG_MODULE_AND_ID* id, uint which, char* buffer, uint bufferSize, uint* stringSize) => NotImplemented;
        int IDebugSymbols3Generated.GetSymbolEntryOffsetRegions(DEBUG_MODULE_AND_ID* id, uint flags, DEBUG_OFFSET_REGION* regions, uint regionsCount, uint* regionsAvail) => NotImplemented;
        int IDebugSymbols3Generated.GetSymbolEntryBySymbolEntry(DEBUG_MODULE_AND_ID* fromId, uint flags, DEBUG_MODULE_AND_ID* toId) => NotImplemented;
        int IDebugSymbols3Generated.GetSourceEntriesByOffset(ulong offset, uint flags, DEBUG_SYMBOL_SOURCE_ENTRY* entries, uint entriesCount, uint* entriesAvail) => NotImplemented;
        int IDebugSymbols3Generated.GetSourceEntriesByLine(uint line, string file, uint flags, DEBUG_SYMBOL_SOURCE_ENTRY* entries, uint entriesCount, uint* entriesAvail) => NotImplemented;
        int IDebugSymbols3Generated.GetSourceEntriesByLineWide(uint line, string file, uint flags, DEBUG_SYMBOL_SOURCE_ENTRY* entries, uint entriesCount, uint* entriesAvail) => NotImplemented;
        int IDebugSymbols3Generated.GetSourceEntryString(in DEBUG_SYMBOL_SOURCE_ENTRY entry, uint which, byte* buffer, uint bufferSize, uint* stringSize) => NotImplemented;
        int IDebugSymbols3Generated.GetSourceEntryStringWide(in DEBUG_SYMBOL_SOURCE_ENTRY entry, uint which, char* buffer, uint bufferSize, uint* stringSize) => NotImplemented;
        int IDebugSymbols3Generated.GetSourceEntryOffsetRegions(DEBUG_SYMBOL_SOURCE_ENTRY* entry, uint flags, DEBUG_OFFSET_REGION* regions, uint regionsCount, uint* regionsAvail) => NotImplemented;
        int IDebugSymbols3Generated.GetSourceEntryBySourceEntry(DEBUG_SYMBOL_SOURCE_ENTRY* fromEntry, uint flags, DEBUG_SYMBOL_SOURCE_ENTRY* toEntry) => NotImplemented;
    }
}
