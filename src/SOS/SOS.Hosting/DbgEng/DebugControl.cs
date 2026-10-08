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
        int IDebugControlGenerated.GetInterrupt()
        {
            return _soshost.GetInterrupt(IntPtr.Zero);
        }

        int IDebugControlGenerated.SetInterrupt(DEBUG_INTERRUPT flags)
        {
            return HResult.S_OK;
        }

        int IDebugControlGenerated.GetInterruptTimeout(uint* seconds) => NotImplemented;
        int IDebugControlGenerated.SetInterruptTimeout(uint seconds) => NotImplemented;
        int IDebugControlGenerated.GetLogFile(byte* buffer, uint bufferSize, uint* fileSize, int* append) => NotImplemented;
        int IDebugControlGenerated.OpenLogFile(string file, bool append) => NotImplemented;
        int IDebugControlGenerated.CloseLogFile() => NotImplemented;
        int IDebugControlGenerated.GetLogMask(DEBUG_OUTPUT* mask) => NotImplemented;
        int IDebugControlGenerated.SetLogMask(DEBUG_OUTPUT mask) => NotImplemented;
        int IDebugControlGenerated.Input(byte* buffer, uint bufferSize, uint* inputSize) => NotImplemented;
        int IDebugControlGenerated.ReturnInput(string buffer) => NotImplemented;
        int IDebugControlGenerated.Output(DEBUG_OUTPUT mask, string format) => NotImplemented;

        int IDebugControlGenerated.OutputVaList(DEBUG_OUTPUT mask, string format, IntPtr va_list_Args)
        {
            return _soshost.OutputVaList(IntPtr.Zero, mask, format, va_list_Args);
        }

        int IDebugControlGenerated.ControlledOutput(DEBUG_OUTCTL outputControl, DEBUG_OUTPUT mask, string format) => NotImplemented;
        int IDebugControlGenerated.ControlledOutputVaList(DEBUG_OUTCTL outputControl, DEBUG_OUTPUT mask, string format, IntPtr va_list_Args) => NotImplemented;
        int IDebugControlGenerated.OutputPrompt(DEBUG_OUTCTL outputControl, string format) => NotImplemented;
        int IDebugControlGenerated.OutputPromptVaList(DEBUG_OUTCTL outputControl, string format, IntPtr va_list_Args) => NotImplemented;
        int IDebugControlGenerated.GetPromptText(byte* buffer, uint bufferSize, uint* textSize) => NotImplemented;
        int IDebugControlGenerated.OutputCurrentState(DEBUG_OUTCTL outputControl, DEBUG_CURRENT flags) => NotImplemented;
        int IDebugControlGenerated.OutputVersionInformation(DEBUG_OUTCTL outputControl) => NotImplemented;
        int IDebugControlGenerated.GetNotifyEventHandle(ulong* handle) => NotImplemented;
        int IDebugControlGenerated.SetNotifyEventHandle(ulong handle) => NotImplemented;
        int IDebugControlGenerated.Assemble(ulong offset, string instr, ulong* endOffset) => NotImplemented;

        int IDebugControlGenerated.Disassemble(ulong offset, DEBUG_DISASM flags, byte* buffer, uint bufferSize, uint* disassemblySize, ulong* endOffset)
        {
            StringBuilder bufferBuilder = CreateStringBuilder(buffer, bufferSize);
            int result = SOSHost.Disassemble(IntPtr.Zero, offset, flags, bufferBuilder, bufferSize, disassemblySize, endOffset);
            CopyStringBuffer(bufferBuilder, buffer, bufferSize);
            return result;
        }

        int IDebugControlGenerated.GetDisassembleEffectiveOffset(ulong* offset) => NotImplemented;
        int IDebugControlGenerated.OutputDisassembly(DEBUG_OUTCTL outputControl, ulong offset, DEBUG_DISASM flags, ulong* endOffset) => NotImplemented;
        int IDebugControlGenerated.OutputDisassemblyLines(DEBUG_OUTCTL outputControl, uint previousLines, uint totalLines, ulong offset, DEBUG_DISASM flags, uint* offsetLine, ulong* startOffset, ulong* endOffset, ulong* lineOffsets) => NotImplemented;
        int IDebugControlGenerated.GetNearInstruction(ulong offset, int delta, ulong* nearOffset) => NotImplemented;
        int IDebugControlGenerated.GetStackTrace(ulong frameOffset, ulong stackOffset, ulong instructionOffset, DEBUG_STACK_FRAME* frames, uint frameSize, uint* framesFilled) => NotImplemented;
        int IDebugControlGenerated.GetReturnOffset(ulong* offset) => NotImplemented;
        int IDebugControlGenerated.OutputStackTrace(DEBUG_OUTCTL outputControl, DEBUG_STACK_FRAME* frames, uint framesSize, DEBUG_STACK flags) => NotImplemented;

        int IDebugControlGenerated.GetDebuggeeType(DEBUG_CLASS* @class, DEBUG_CLASS_QUALIFIER* qualifier)
        {
            return _soshost.GetDebuggeeType(IntPtr.Zero, @class, qualifier);
        }

        int IDebugControlGenerated.GetActualProcessorType(IMAGE_FILE_MACHINE* @type)
        {
            return _soshost.GetExecutingProcessorType(IntPtr.Zero, @type);
        }

        int IDebugControlGenerated.GetExecutingProcessorType(IMAGE_FILE_MACHINE* @type)
        {
            return _soshost.GetExecutingProcessorType(IntPtr.Zero, @type);
        }

        int IDebugControlGenerated.GetNumberPossibleExecutingProcessorTypes(uint* number) => NotImplemented;
        int IDebugControlGenerated.GetPossibleExecutingProcessorTypes(uint start, uint count, IMAGE_FILE_MACHINE* types) => NotImplemented;
        int IDebugControlGenerated.GetNumberProcessors(uint* number) => NotImplemented;
        int IDebugControlGenerated.GetSystemVersion(uint* platformId, uint* major, uint* minor, byte* servicePackString, uint servicePackStringSize, uint* servicePackStringUsed, uint* servicePackNumber, byte* buildString, uint buildStringSize, uint* buildStringUsed) => NotImplemented;

        int IDebugControlGenerated.GetPageSize(uint* size)
        {
            return _soshost.GetPageSize(IntPtr.Zero, size);
        }

        int IDebugControlGenerated.IsPointer64Bit() => NotImplemented;
        int IDebugControlGenerated.ReadBugCheckData(uint* code, ulong* arg1, ulong* arg2, ulong* arg3, ulong* arg4) => NotImplemented;
        int IDebugControlGenerated.GetNumberSupportedProcessorTypes(uint* number) => NotImplemented;
        int IDebugControlGenerated.GetSupportedProcessorTypes(uint start, uint count, IMAGE_FILE_MACHINE* types) => NotImplemented;
        int IDebugControlGenerated.GetProcessorTypeNames(IMAGE_FILE_MACHINE @type, byte* fullNameBuffer, uint fullNameBufferSize, uint* fullNameSize, byte* abbrevNameBuffer, uint abbrevNameBufferSize, uint* abbrevNameSize) => NotImplemented;
        int IDebugControlGenerated.GetEffectiveProcessorType(IMAGE_FILE_MACHINE* @type) => NotImplemented;
        int IDebugControlGenerated.SetEffectiveProcessorType(IMAGE_FILE_MACHINE @type) => NotImplemented;
        int IDebugControlGenerated.GetExecutionStatus(DEBUG_STATUS* status) => NotImplemented;
        int IDebugControlGenerated.SetExecutionStatus(DEBUG_STATUS status) => NotImplemented;
        int IDebugControlGenerated.GetCodeLevel(DEBUG_LEVEL* level) => NotImplemented;
        int IDebugControlGenerated.SetCodeLevel(DEBUG_LEVEL level) => NotImplemented;
        int IDebugControlGenerated.GetEngineOptions(DEBUG_ENGOPT* options) => HResult.E_NOTIMPL;
        int IDebugControlGenerated.AddEngineOptions(DEBUG_ENGOPT options) => HResult.E_NOTIMPL;
        int IDebugControlGenerated.RemoveEngineOptions(DEBUG_ENGOPT options) => NotImplemented;
        int IDebugControlGenerated.SetEngineOptions(DEBUG_ENGOPT options) => NotImplemented;
        int IDebugControlGenerated.GetSystemErrorControl(ERROR_LEVEL* outputLevel, ERROR_LEVEL* breakLevel) => NotImplemented;
        int IDebugControlGenerated.SetSystemErrorControl(ERROR_LEVEL outputLevel, ERROR_LEVEL breakLevel) => NotImplemented;
        int IDebugControlGenerated.GetTextMacro(uint slot, byte* buffer, uint bufferSize, uint* macroSize) => NotImplemented;
        int IDebugControlGenerated.SetTextMacro(uint slot, string macro) => NotImplemented;
        int IDebugControlGenerated.GetRadix(uint* radix) => NotImplemented;
        int IDebugControlGenerated.SetRadix(uint radix) => NotImplemented;
        int IDebugControlGenerated.Evaluate(string expression, DEBUG_VALUE_TYPE desiredType, DEBUG_VALUE* value, uint* remainderIndex) => NotImplemented;
        int IDebugControlGenerated.CoerceValue(in DEBUG_VALUE @in, DEBUG_VALUE_TYPE outType, DEBUG_VALUE* @out) => NotImplemented;
        int IDebugControlGenerated.CoerceValues(uint count, DEBUG_VALUE* @in, DEBUG_VALUE_TYPE* outType, DEBUG_VALUE* @out) => NotImplemented;

        int IDebugControlGenerated.Execute(DEBUG_OUTCTL outputControl, string command, DEBUG_EXECUTE flags)
        {
            return SOSHost.Execute(IntPtr.Zero, outputControl, command, flags);
        }

        int IDebugControlGenerated.ExecuteCommandFile(DEBUG_OUTCTL outputControl, string commandFile, DEBUG_EXECUTE flags) => NotImplemented;
        int IDebugControlGenerated.GetNumberBreakpoints(uint* number) => NotImplemented;
        int IDebugControlGenerated.GetBreakpointByIndex(uint index, out IntPtr bp)
        {
            bp = default;
            return NotImplemented;
        }
        int IDebugControlGenerated.GetBreakpointById(uint id, out IntPtr bp)
        {
            bp = default;
            return NotImplemented;
        }
        int IDebugControlGenerated.GetBreakpointParameters(uint count, uint* ids, uint start, DEBUG_BREAKPOINT_PARAMETERS* @params) => NotImplemented;
        int IDebugControlGenerated.AddBreakpoint(DEBUG_BREAKPOINT_TYPE @type, uint desiredId, out IntPtr bp)
        {
            bp = default;
            return NotImplemented;
        }
        int IDebugControlGenerated.RemoveBreakpoint(IntPtr bp) => NotImplemented;
        int IDebugControlGenerated.AddExtension(string path, uint flags, ulong* handle) => NotImplemented;
        int IDebugControlGenerated.RemoveExtension(ulong handle) => NotImplemented;
        int IDebugControlGenerated.GetExtensionByPath(string path, ulong* handle) => NotImplemented;
        int IDebugControlGenerated.CallExtension(ulong handle, string function, string arguments) => NotImplemented;
        int IDebugControlGenerated.GetExtensionFunction(ulong handle, string funcName, IntPtr* function) => NotImplemented;
        int IDebugControlGenerated.GetWindbgExtensionApis32(IntPtr api) => NotImplemented;
        int IDebugControlGenerated.GetWindbgExtensionApis64(IntPtr api) => NotImplemented;
        int IDebugControlGenerated.GetNumberEventFilters(uint* specificEvents, uint* specificExceptions, uint* arbitraryExceptions) => NotImplemented;
        int IDebugControlGenerated.GetEventFilterText(uint index, byte* buffer, uint bufferSize, uint* textSize) => NotImplemented;
        int IDebugControlGenerated.GetEventFilterCommand(uint index, byte* buffer, uint bufferSize, uint* commandSize) => NotImplemented;
        int IDebugControlGenerated.SetEventFilterCommand(uint index, string command) => NotImplemented;
        int IDebugControlGenerated.GetSpecificFilterParameters(uint start, uint count, DEBUG_SPECIFIC_FILTER_PARAMETERS* @params) => NotImplemented;
        int IDebugControlGenerated.SetSpecificFilterParameters(uint start, uint count, DEBUG_SPECIFIC_FILTER_PARAMETERS* @params) => NotImplemented;
        int IDebugControlGenerated.GetSpecificEventFilterArgument(uint index, byte* buffer, uint bufferSize, uint* argumentSize) => NotImplemented;
        int IDebugControlGenerated.SetSpecificEventFilterArgument(uint index, string argument) => NotImplemented;
        int IDebugControlGenerated.GetExceptionFilterParameters(uint count, uint* codes, uint start, DEBUG_EXCEPTION_FILTER_PARAMETERS* @params) => NotImplemented;
        int IDebugControlGenerated.SetExceptionFilterParameters(uint count, DEBUG_EXCEPTION_FILTER_PARAMETERS* @params) => NotImplemented;
        int IDebugControlGenerated.GetExceptionFilterSecondCommand(uint index, byte* buffer, uint bufferSize, uint* commandSize) => NotImplemented;
        int IDebugControlGenerated.SetExceptionFilterSecondCommand(uint index, string command) => NotImplemented;
        int IDebugControlGenerated.WaitForEvent(DEBUG_WAIT flags, uint timeout) => NotImplemented;

        int IDebugControlGenerated.GetLastEventInformation(DEBUG_EVENT* @type, uint* processId, uint* threadId, IntPtr extraInformation, uint extraInformationSize, uint* extraInformationUsed, byte* description, uint descriptionSize, uint* descriptionUsed)
        {
            StringBuilder descriptionBuilder = CreateStringBuilder(description, descriptionSize);
            int result = _soshost.GetLastEventInformation(IntPtr.Zero, @type, processId, threadId, extraInformation, extraInformationSize, extraInformationUsed, descriptionBuilder, descriptionSize, descriptionUsed);
            CopyStringBuffer(descriptionBuilder, description, descriptionSize);
            return result;
        }

        int IDebugControl2Generated.GetCurrentTimeDate(uint* timeDate) => NotImplemented;
        int IDebugControl2Generated.GetCurrentSystemUpTime(uint* upTime) => NotImplemented;

        int IDebugControl2Generated.GetDumpFormatFlags(DEBUG_FORMAT* formatFlags)
        {
            return _soshost.GetDumpFormatFlags(IntPtr.Zero, formatFlags);
        }

        int IDebugControl2Generated.GetNumberTextReplacements(uint* numRepl) => NotImplemented;
        int IDebugControl2Generated.GetTextReplacement(string srcText, uint index, byte* srcBuffer, uint srcBufferSize, uint* srcSize, byte* dstBuffer, uint dstBufferSize, uint* dstSize) => NotImplemented;
        int IDebugControl2Generated.SetTextReplacement(string srcText, string dstText) => NotImplemented;
        int IDebugControl2Generated.RemoveTextReplacements() => NotImplemented;
        int IDebugControl2Generated.OutputTextReplacements(DEBUG_OUTCTL outputControl, DEBUG_OUT_TEXT_REPL flags) => NotImplemented;
    }
}
