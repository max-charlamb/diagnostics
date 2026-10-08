// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("5182E668-105E-416E-AD92-24EF800424BA")]
    internal unsafe partial interface IDebugControlGenerated
    {
        [PreserveSig]
        int GetInterrupt();

        [PreserveSig]
        int SetInterrupt(DEBUG_INTERRUPT flags);

        [PreserveSig]
        int GetInterruptTimeout(uint* seconds);

        [PreserveSig]
        int SetInterruptTimeout(uint seconds);

        [PreserveSig]
        int GetLogFile(byte* buffer, uint bufferSize, uint* fileSize, int* append);

        [PreserveSig]
        int OpenLogFile([MarshalAs(UnmanagedType.LPStr)] string file, [MarshalAs(UnmanagedType.Bool)] bool append);

        [PreserveSig]
        int CloseLogFile();

        [PreserveSig]
        int GetLogMask(DEBUG_OUTPUT* mask);

        [PreserveSig]
        int SetLogMask(DEBUG_OUTPUT mask);

        [PreserveSig]
        int Input(byte* buffer, uint bufferSize, uint* inputSize);

        [PreserveSig]
        int ReturnInput([MarshalAs(UnmanagedType.LPStr)] string buffer);

        [PreserveSig]
        int Output(DEBUG_OUTPUT mask, [MarshalAs(UnmanagedType.LPStr)] string format);

        [PreserveSig]
        int OutputVaList(DEBUG_OUTPUT mask, [MarshalAs(UnmanagedType.LPStr)] string format, IntPtr va_list_Args);

        [PreserveSig]
        int ControlledOutput(DEBUG_OUTCTL outputControl, DEBUG_OUTPUT mask, [MarshalAs(UnmanagedType.LPStr)] string format);

        [PreserveSig]
        int ControlledOutputVaList(DEBUG_OUTCTL outputControl, DEBUG_OUTPUT mask, [MarshalAs(UnmanagedType.LPStr)] string format, IntPtr va_list_Args);

        [PreserveSig]
        int OutputPrompt(DEBUG_OUTCTL outputControl, [MarshalAs(UnmanagedType.LPStr)] string format);

        [PreserveSig]
        int OutputPromptVaList(DEBUG_OUTCTL outputControl, [MarshalAs(UnmanagedType.LPStr)] string format, IntPtr va_list_Args);

        [PreserveSig]
        int GetPromptText(byte* buffer, uint bufferSize, uint* textSize);

        [PreserveSig]
        int OutputCurrentState(DEBUG_OUTCTL outputControl, DEBUG_CURRENT flags);

        [PreserveSig]
        int OutputVersionInformation(DEBUG_OUTCTL outputControl);

        [PreserveSig]
        int GetNotifyEventHandle(ulong* handle);

        [PreserveSig]
        int SetNotifyEventHandle(ulong handle);

        [PreserveSig]
        int Assemble(ulong offset, [MarshalAs(UnmanagedType.LPStr)] string instr, ulong* endOffset);

        [PreserveSig]
        int Disassemble(ulong offset, DEBUG_DISASM flags, byte* buffer, uint bufferSize, uint* disassemblySize, ulong* endOffset);

        [PreserveSig]
        int GetDisassembleEffectiveOffset(ulong* offset);

        [PreserveSig]
        int OutputDisassembly(DEBUG_OUTCTL outputControl, ulong offset, DEBUG_DISASM flags, ulong* endOffset);

        [PreserveSig]
        int OutputDisassemblyLines(DEBUG_OUTCTL outputControl, uint previousLines, uint totalLines, ulong offset, DEBUG_DISASM flags, uint* offsetLine, ulong* startOffset, ulong* endOffset, ulong* lineOffsets);

        [PreserveSig]
        int GetNearInstruction(ulong offset, int delta, ulong* nearOffset);

        [PreserveSig]
        int GetStackTrace(ulong frameOffset, ulong stackOffset, ulong instructionOffset, DEBUG_STACK_FRAME* frames, uint frameSize, uint* framesFilled);

        [PreserveSig]
        int GetReturnOffset(ulong* offset);

        [PreserveSig]
        int OutputStackTrace(DEBUG_OUTCTL outputControl, DEBUG_STACK_FRAME* frames, uint framesSize, DEBUG_STACK flags);

        [PreserveSig]
        int GetDebuggeeType(DEBUG_CLASS* @class, DEBUG_CLASS_QUALIFIER* qualifier);

        [PreserveSig]
        int GetActualProcessorType(IMAGE_FILE_MACHINE* @type);

        [PreserveSig]
        int GetExecutingProcessorType(IMAGE_FILE_MACHINE* @type);

        [PreserveSig]
        int GetNumberPossibleExecutingProcessorTypes(uint* number);

        [PreserveSig]
        int GetPossibleExecutingProcessorTypes(uint start, uint count, IMAGE_FILE_MACHINE* types);

        [PreserveSig]
        int GetNumberProcessors(uint* number);

        [PreserveSig]
        int GetSystemVersion(uint* platformId, uint* major, uint* minor, byte* servicePackString, uint servicePackStringSize, uint* servicePackStringUsed, uint* servicePackNumber, byte* buildString, uint buildStringSize, uint* buildStringUsed);

        [PreserveSig]
        int GetPageSize(uint* size);

        [PreserveSig]
        int IsPointer64Bit();

        [PreserveSig]
        int ReadBugCheckData(uint* code, ulong* arg1, ulong* arg2, ulong* arg3, ulong* arg4);

        [PreserveSig]
        int GetNumberSupportedProcessorTypes(uint* number);

        [PreserveSig]
        int GetSupportedProcessorTypes(uint start, uint count, IMAGE_FILE_MACHINE* types);

        [PreserveSig]
        int GetProcessorTypeNames(IMAGE_FILE_MACHINE @type, byte* fullNameBuffer, uint fullNameBufferSize, uint* fullNameSize, byte* abbrevNameBuffer, uint abbrevNameBufferSize, uint* abbrevNameSize);

        [PreserveSig]
        int GetEffectiveProcessorType(IMAGE_FILE_MACHINE* @type);

        [PreserveSig]
        int SetEffectiveProcessorType(IMAGE_FILE_MACHINE @type);

        [PreserveSig]
        int GetExecutionStatus(DEBUG_STATUS* status);

        [PreserveSig]
        int SetExecutionStatus(DEBUG_STATUS status);

        [PreserveSig]
        int GetCodeLevel(DEBUG_LEVEL* level);

        [PreserveSig]
        int SetCodeLevel(DEBUG_LEVEL level);

        [PreserveSig]
        int GetEngineOptions(DEBUG_ENGOPT* options);

        [PreserveSig]
        int AddEngineOptions(DEBUG_ENGOPT options);

        [PreserveSig]
        int RemoveEngineOptions(DEBUG_ENGOPT options);

        [PreserveSig]
        int SetEngineOptions(DEBUG_ENGOPT options);

        [PreserveSig]
        int GetSystemErrorControl(ERROR_LEVEL* outputLevel, ERROR_LEVEL* breakLevel);

        [PreserveSig]
        int SetSystemErrorControl(ERROR_LEVEL outputLevel, ERROR_LEVEL breakLevel);

        [PreserveSig]
        int GetTextMacro(uint slot, byte* buffer, uint bufferSize, uint* macroSize);

        [PreserveSig]
        int SetTextMacro(uint slot, [MarshalAs(UnmanagedType.LPStr)] string macro);

        [PreserveSig]
        int GetRadix(uint* radix);

        [PreserveSig]
        int SetRadix(uint radix);

        [PreserveSig]
        int Evaluate([MarshalAs(UnmanagedType.LPStr)] string expression, DEBUG_VALUE_TYPE desiredType, DEBUG_VALUE* value, uint* remainderIndex);

        [PreserveSig]
        int CoerceValue(in DEBUG_VALUE @in, DEBUG_VALUE_TYPE outType, DEBUG_VALUE* @out);

        [PreserveSig]
        int CoerceValues(uint count, DEBUG_VALUE* @in, DEBUG_VALUE_TYPE* outType, DEBUG_VALUE* @out);

        [PreserveSig]
        int Execute(DEBUG_OUTCTL outputControl, [MarshalAs(UnmanagedType.LPStr)] string command, DEBUG_EXECUTE flags);

        [PreserveSig]
        int ExecuteCommandFile(DEBUG_OUTCTL outputControl, [MarshalAs(UnmanagedType.LPStr)] string commandFile, DEBUG_EXECUTE flags);

        [PreserveSig]
        int GetNumberBreakpoints(uint* number);

        [PreserveSig]
        int GetBreakpointByIndex(uint index, out IntPtr bp);

        [PreserveSig]
        int GetBreakpointById(uint id, out IntPtr bp);

        [PreserveSig]
        int GetBreakpointParameters(uint count, uint* ids, uint start, DEBUG_BREAKPOINT_PARAMETERS* @params);

        [PreserveSig]
        int AddBreakpoint(DEBUG_BREAKPOINT_TYPE @type, uint desiredId, out IntPtr bp);

        [PreserveSig]
        int RemoveBreakpoint(IntPtr bp);

        [PreserveSig]
        int AddExtension([MarshalAs(UnmanagedType.LPStr)] string path, uint flags, ulong* handle);

        [PreserveSig]
        int RemoveExtension(ulong handle);

        [PreserveSig]
        int GetExtensionByPath([MarshalAs(UnmanagedType.LPStr)] string path, ulong* handle);

        [PreserveSig]
        int CallExtension(ulong handle, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string arguments);

        [PreserveSig]
        int GetExtensionFunction(ulong handle, [MarshalAs(UnmanagedType.LPStr)] string funcName, IntPtr* function);

        [PreserveSig]
        int GetWindbgExtensionApis32(IntPtr api);

        [PreserveSig]
        int GetWindbgExtensionApis64(IntPtr api);

        [PreserveSig]
        int GetNumberEventFilters(uint* specificEvents, uint* specificExceptions, uint* arbitraryExceptions);

        [PreserveSig]
        int GetEventFilterText(uint index, byte* buffer, uint bufferSize, uint* textSize);

        [PreserveSig]
        int GetEventFilterCommand(uint index, byte* buffer, uint bufferSize, uint* commandSize);

        [PreserveSig]
        int SetEventFilterCommand(uint index, [MarshalAs(UnmanagedType.LPStr)] string command);

        [PreserveSig]
        int GetSpecificFilterParameters(uint start, uint count, DEBUG_SPECIFIC_FILTER_PARAMETERS* @params);

        [PreserveSig]
        int SetSpecificFilterParameters(uint start, uint count, DEBUG_SPECIFIC_FILTER_PARAMETERS* @params);

        [PreserveSig]
        int GetSpecificEventFilterArgument(uint index, byte* buffer, uint bufferSize, uint* argumentSize);

        [PreserveSig]
        int SetSpecificEventFilterArgument(uint index, [MarshalAs(UnmanagedType.LPStr)] string argument);

        [PreserveSig]
        int GetExceptionFilterParameters(uint count, uint* codes, uint start, DEBUG_EXCEPTION_FILTER_PARAMETERS* @params);

        [PreserveSig]
        int SetExceptionFilterParameters(uint count, DEBUG_EXCEPTION_FILTER_PARAMETERS* @params);

        [PreserveSig]
        int GetExceptionFilterSecondCommand(uint index, byte* buffer, uint bufferSize, uint* commandSize);

        [PreserveSig]
        int SetExceptionFilterSecondCommand(uint index, [MarshalAs(UnmanagedType.LPStr)] string command);

        [PreserveSig]
        int WaitForEvent(DEBUG_WAIT flags, uint timeout);

        [PreserveSig]
        int GetLastEventInformation(DEBUG_EVENT* @type, uint* processId, uint* threadId, IntPtr extraInformation, uint extraInformationSize, uint* extraInformationUsed, byte* description, uint descriptionSize, uint* descriptionUsed);
    }
}
