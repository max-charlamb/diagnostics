// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("2E6C569A-9E14-4DA4-9DFC-CDB73A532566")]
    internal unsafe partial interface ILLDBServicesGenerated
    {
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.LPStr)]
        string GetCoreClrDirectory();
        [PreserveSig]
        ulong GetExpression([MarshalAs(UnmanagedType.LPStr)] string expression);
        [PreserveSig]
        int VirtualUnwind(uint threadId, uint contextSize, byte* context);
        [PreserveSig]
        int SetExceptionCallback(IntPtr callback);
        [PreserveSig]
        int ClearExceptionCallback();
        [PreserveSig]
        int GetInterrupt();
        [PreserveSig]
        int OutputVaList(DEBUG_OUTPUT mask, [MarshalAs(UnmanagedType.LPStr)] string format, IntPtr args);
        [PreserveSig]
        int GetDebuggeeType(DEBUG_CLASS* debugClass, DEBUG_CLASS_QUALIFIER* qualifier);
        [PreserveSig]
        int GetPageSize(uint* size);
        [PreserveSig]
        int GetProcessorType(IMAGE_FILE_MACHINE* type);
        [PreserveSig]
        int Execute(DEBUG_OUTCTL outputControl, [MarshalAs(UnmanagedType.LPStr)] string command, DEBUG_EXECUTE flags);
        [PreserveSig]
        int GetLastEventInformation(DEBUG_EVENT* type, uint* processId, uint* threadId, IntPtr extraInformation,
            uint extraInformationSize, uint* extraInformationUsed, byte* description, uint descriptionSize, uint* descriptionUsed);
        [PreserveSig]
        int Disassemble(ulong offset, DEBUG_DISASM flags, byte* buffer, uint bufferSize, uint* disassemblySize, ulong* endOffset);
        [PreserveSig]
        int GetContextStackTrace(IntPtr startContext, uint startContextSize, DEBUG_STACK_FRAME* frames, uint framesSize,
            IntPtr frameContexts, uint frameContextsSize, uint frameContextsEntrySize, uint* framesFilled);
        [PreserveSig]
        int ReadVirtual(ulong address, IntPtr buffer, uint bufferSize, uint* bytesRead);
        [PreserveSig]
        int WriteVirtual(ulong address, IntPtr buffer, uint bufferSize, uint* bytesWritten);
        [PreserveSig]
        int GetSymbolOptions(out SYMOPT options);
        [PreserveSig]
        int GetNameByOffset(ulong offset, byte* nameBuffer, uint nameBufferSize, uint* nameSize, ulong* displacement);
        [PreserveSig]
        int GetNumberModules(out uint loaded, out uint unloaded);
        [PreserveSig]
        int GetModuleByIndex(uint index, out ulong baseAddress);
        [PreserveSig]
        int GetModuleByModuleName([MarshalAs(UnmanagedType.LPStr)] string name, uint startIndex, uint* index, ulong* baseAddress);
        [PreserveSig]
        int GetModuleByOffset(ulong offset, uint startIndex, uint* index, ulong* baseAddress);
        [PreserveSig]
        int GetModuleNames(uint index, ulong baseAddress, byte* imageNameBuffer, uint imageNameBufferSize, uint* imageNameSize,
            byte* moduleNameBuffer, uint moduleNameBufferSize, uint* moduleNameSize,
            byte* loadedImageNameBuffer, uint loadedImageNameBufferSize, uint* loadedImageNameSize);
        [PreserveSig]
        int GetLineByOffset(ulong offset, uint* line, byte* fileBuffer, uint fileBufferSize, uint* fileSize, ulong* displacement);
        [PreserveSig]
        int GetSourceFileLineOffsets([MarshalAs(UnmanagedType.LPStr)] string file, ulong* buffer, uint bufferLines, uint* fileLines);
        [PreserveSig]
        int FindSourceFile(uint startElement, [MarshalAs(UnmanagedType.LPStr)] string file, DEBUG_FIND_SOURCE flags,
            uint* foundElement, byte* buffer, uint bufferSize, uint* foundSize);
        [PreserveSig]
        int GetCurrentProcessSystemId(out uint id);
        [PreserveSig]
        int GetCurrentThreadId(out uint id);
        [PreserveSig]
        int SetCurrentThreadId(uint id);
        [PreserveSig]
        int GetCurrentThreadSystemId(out uint sysId);
        [PreserveSig]
        int GetThreadIdBySystemId(uint sysId, out uint id);
        [PreserveSig]
        int GetThreadContextBySystemId(uint threadId, uint contextFlags, uint contextSize, byte* context);
        [PreserveSig]
        int GetValueByName([MarshalAs(UnmanagedType.LPStr)] string name, out UIntPtr value);
        [PreserveSig]
        int GetInstructionOffset(out ulong offset);
        [PreserveSig]
        int GetStackOffset(out ulong offset);
        [PreserveSig]
        int GetFrameOffset(out ulong offset);
    }
}
