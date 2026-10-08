// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop;

namespace SOS.Hosting
{
    [GeneratedComClass]
    public sealed unsafe partial class LLDBServices : ILLDBServicesGenerated, ILLDBServices2Generated
    {
        public static readonly Guid IID_ILLDBServices = typeof(ILLDBServicesGenerated).GUID;
        public static readonly Guid IID_ILLDBServices2 = typeof(ILLDBServices2Generated).GUID;

        private readonly SOSHost _soshost;

        internal LLDBServices(SOSHost soshost)
        {
            _soshost = soshost;
        }

        string ILLDBServicesGenerated.GetCoreClrDirectory()
        {
            IRuntime currentRuntime = _soshost.ContextService.GetCurrentRuntime();
            return currentRuntime is not null ? Path.GetDirectoryName(currentRuntime.RuntimeModule.FileName) : null;
        }

        ulong ILLDBServicesGenerated.GetExpression(string expression) =>
            _soshost.Target.Services.GetService<TargetWrapper>().GetSymbolService().GetExpressionValue(expression);

        int ILLDBServicesGenerated.VirtualUnwind(uint threadId, uint contextSize, byte* context) => HResult.E_NOTIMPL;

        int ILLDBServicesGenerated.SetExceptionCallback(IntPtr callback) => HResult.S_OK;

        int ILLDBServicesGenerated.ClearExceptionCallback() => HResult.S_OK;

        int ILLDBServicesGenerated.GetInterrupt() => _soshost.GetInterrupt(IntPtr.Zero);

        int ILLDBServicesGenerated.OutputVaList(DEBUG_OUTPUT mask, string format, IntPtr args) =>
            _soshost.OutputVaList(IntPtr.Zero, mask, format, args);

        int ILLDBServicesGenerated.GetDebuggeeType(DEBUG_CLASS* debugClass, DEBUG_CLASS_QUALIFIER* qualifier) =>
            _soshost.GetDebuggeeType(IntPtr.Zero, debugClass, qualifier);

        int ILLDBServicesGenerated.GetPageSize(uint* size) => _soshost.GetPageSize(IntPtr.Zero, size);

        int ILLDBServicesGenerated.GetProcessorType(IMAGE_FILE_MACHINE* type) => _soshost.GetExecutingProcessorType(IntPtr.Zero, type);

        int ILLDBServicesGenerated.Execute(DEBUG_OUTCTL outputControl, string command, DEBUG_EXECUTE flags) =>
            SOSHost.Execute(IntPtr.Zero, outputControl, command, flags);

        int ILLDBServicesGenerated.GetLastEventInformation(DEBUG_EVENT* type, uint* processId, uint* threadId, IntPtr extraInformation,
            uint extraInformationSize, uint* extraInformationUsed, byte* description, uint descriptionSize, uint* descriptionUsed)
        {
            StringBuilder text = new();
            int hr = _soshost.GetLastEventInformation(IntPtr.Zero, type, processId, threadId, extraInformation, extraInformationSize,
                extraInformationUsed, text, descriptionSize, descriptionUsed);
            CopyOutputString(text, description, descriptionSize);
            return hr;
        }

        int ILLDBServicesGenerated.Disassemble(ulong offset, DEBUG_DISASM flags, byte* buffer, uint bufferSize,
            uint* disassemblySize, ulong* endOffset)
        {
            StringBuilder text = new();
            int hr = SOSHost.Disassemble(IntPtr.Zero, offset, flags, text, bufferSize, disassemblySize, endOffset);
            CopyOutputString(text, buffer, bufferSize);
            return hr;
        }

        int ILLDBServicesGenerated.GetContextStackTrace(IntPtr startContext, uint startContextSize, DEBUG_STACK_FRAME* frames,
            uint framesSize, IntPtr frameContexts, uint frameContextsSize, uint frameContextsEntrySize, uint* framesFilled)
        {
            // No native frames: "clrstack -f" must still print managed frames.
            SOSHost.Write(framesFilled);
            return HResult.S_OK;
        }

        int ILLDBServicesGenerated.ReadVirtual(ulong address, IntPtr buffer, uint bufferSize, uint* bytesRead) =>
            _soshost.ReadVirtual(IntPtr.Zero, address, buffer, bufferSize, bytesRead);

        int ILLDBServicesGenerated.WriteVirtual(ulong address, IntPtr buffer, uint bufferSize, uint* bytesWritten) =>
            _soshost.WriteVirtual(IntPtr.Zero, address, buffer, bufferSize, bytesWritten);

        int ILLDBServicesGenerated.GetSymbolOptions(out SYMOPT options) => SOSHost.GetSymbolOptions(IntPtr.Zero, out options);

        int ILLDBServicesGenerated.GetNameByOffset(ulong offset, byte* nameBuffer, uint nameBufferSize, uint* nameSize, ulong* displacement)
        {
            StringBuilder text = new();
            int hr = SOSHost.GetNameByOffset(IntPtr.Zero, offset, text, nameBufferSize, nameSize, displacement);
            CopyOutputString(text, nameBuffer, nameBufferSize);
            return hr;
        }

        int ILLDBServicesGenerated.GetNumberModules(out uint loaded, out uint unloaded) =>
            _soshost.GetNumberModules(IntPtr.Zero, out loaded, out unloaded);

        int ILLDBServicesGenerated.GetModuleByIndex(uint index, out ulong baseAddress) =>
            _soshost.GetModuleByIndex(IntPtr.Zero, index, out baseAddress);

        int ILLDBServicesGenerated.GetModuleByModuleName(string name, uint startIndex, uint* index, ulong* baseAddress) =>
            _soshost.GetModuleByModuleName(IntPtr.Zero, name, startIndex, index, baseAddress);

        int ILLDBServicesGenerated.GetModuleByOffset(ulong offset, uint startIndex, uint* index, ulong* baseAddress) =>
            _soshost.GetModuleByOffset(IntPtr.Zero, offset, startIndex, index, baseAddress);

        int ILLDBServicesGenerated.GetModuleNames(uint index, ulong baseAddress, byte* imageNameBuffer, uint imageNameBufferSize,
            uint* imageNameSize, byte* moduleNameBuffer, uint moduleNameBufferSize, uint* moduleNameSize,
            byte* loadedImageNameBuffer, uint loadedImageNameBufferSize, uint* loadedImageNameSize)
        {
            StringBuilder image = imageNameBuffer != null ? new StringBuilder() : null;
            StringBuilder module = moduleNameBuffer != null ? new StringBuilder() : null;
            StringBuilder loaded = loadedImageNameBuffer != null ? new StringBuilder() : null;
            int hr = _soshost.GetModuleNames(IntPtr.Zero, index, baseAddress, image, imageNameBufferSize, imageNameSize,
                module, moduleNameBufferSize, moduleNameSize, loaded, loadedImageNameBufferSize, loadedImageNameSize);
            CopyOutputString(image, imageNameBuffer, imageNameBufferSize);
            CopyOutputString(module, moduleNameBuffer, moduleNameBufferSize);
            CopyOutputString(loaded, loadedImageNameBuffer, loadedImageNameBufferSize);
            return hr;
        }

        int ILLDBServicesGenerated.GetLineByOffset(ulong offset, uint* line, byte* fileBuffer, uint fileBufferSize,
            uint* fileSize, ulong* displacement)
        {
            StringBuilder text = new();
            int hr = SOSHost.GetLineByOffset(IntPtr.Zero, offset, line, text, fileBufferSize, fileSize, displacement);
            CopyOutputString(text, fileBuffer, fileBufferSize);
            return hr;
        }

        int ILLDBServicesGenerated.GetSourceFileLineOffsets(string file, ulong* buffer, uint bufferLines, uint* fileLines) =>
            SOSHost.GetSourceFileLineOffsets(IntPtr.Zero, file, null, bufferLines, fileLines);

        int ILLDBServicesGenerated.FindSourceFile(uint startElement, string file, DEBUG_FIND_SOURCE flags, uint* foundElement,
            byte* buffer, uint bufferSize, uint* foundSize)
        {
            StringBuilder text = new();
            int hr = SOSHost.FindSourceFile(IntPtr.Zero, startElement, file, flags, foundElement, text, bufferSize, foundSize);
            CopyOutputString(text, buffer, bufferSize);
            return hr;
        }

        int ILLDBServicesGenerated.GetCurrentProcessSystemId(out uint id) => _soshost.GetCurrentProcessSystemId(IntPtr.Zero, out id);

        int ILLDBServicesGenerated.GetCurrentThreadId(out uint id) => _soshost.GetCurrentThreadId(IntPtr.Zero, out id);

        int ILLDBServicesGenerated.SetCurrentThreadId(uint id) => _soshost.SetCurrentThreadId(IntPtr.Zero, id);

        int ILLDBServicesGenerated.GetCurrentThreadSystemId(out uint sysId) => _soshost.GetCurrentThreadSystemId(IntPtr.Zero, out sysId);

        int ILLDBServicesGenerated.GetThreadIdBySystemId(uint sysId, out uint id) => _soshost.GetThreadIdBySystemId(IntPtr.Zero, sysId, out id);

        int ILLDBServicesGenerated.GetThreadContextBySystemId(uint threadId, uint contextFlags, uint contextSize, byte* context)
        {
            if (contextSize > int.MaxValue)
            {
                Trace.TraceError($"LLDBServices.GetThreadContextBySystemId: invalid context size {contextSize}.");
                return HResult.E_INVALIDARG;
            }
            return _soshost.GetThreadContextBySystemId(IntPtr.Zero, threadId, contextFlags, (int)contextSize, (IntPtr)context);
        }

        int ILLDBServicesGenerated.GetValueByName(string name, out UIntPtr value)
        {
            int hr = _soshost.GetRegister(name, out ulong register);
            value = new UIntPtr(register);
            return hr;
        }

        int ILLDBServicesGenerated.GetInstructionOffset(out ulong offset) => _soshost.GetInstructionOffset(IntPtr.Zero, out offset);

        int ILLDBServicesGenerated.GetStackOffset(out ulong offset) => _soshost.GetStackOffset(IntPtr.Zero, out offset);

        int ILLDBServicesGenerated.GetFrameOffset(out ulong offset) => _soshost.GetFrameOffset(IntPtr.Zero, out offset);

        int ILLDBServices2Generated.LoadNativeSymbols(bool runtimeOnly, IntPtr callback)
        {
            if (callback == IntPtr.Zero)
            {
                Trace.TraceError("LLDBServices.LoadNativeSymbols: callback is null.");
                return HResult.E_INVALIDARG;
            }
            IEnumerable<IModule> modules = runtimeOnly
                ? _soshost.ModuleService.GetModuleFromModuleName(_soshost.Target.GetPlatformModuleName("coreclr"))
                : _soshost.ModuleService.EnumerateModules();
            delegate* unmanaged[Cdecl]<IntPtr, byte*, ulong, int, void> moduleLoad =
                (delegate* unmanaged[Cdecl]<IntPtr, byte*, ulong, int, void>)callback;
            foreach (IModule module in modules)
            {
                IntPtr path = Marshal.StringToCoTaskMemAnsi(module.FileName);
                try
                {
                    moduleLoad(IntPtr.Zero, (byte*)path, module.ImageBase, unchecked((int)module.ImageSize));
                }
                finally
                {
                    Marshal.FreeCoTaskMem(path);
                }
            }
            return HResult.S_OK;
        }

        int ILLDBServices2Generated.AddModuleSymbol(IntPtr parameter, string symbolFilename) => HResult.S_OK;

        int ILLDBServices2Generated.GetModuleInfo(uint index, ulong* moduleBase, ulong* moduleSize, uint* timestamp, uint* checksum)
        {
            try
            {
                IModule module = _soshost.ModuleService.GetModuleFromIndex((int)index);
                SOSHost.Write(moduleBase, module.ImageBase);
                SOSHost.Write(moduleSize, module.ImageSize);
                SOSHost.Write(timestamp, module.IndexTimeStamp.GetValueOrDefault(SOSHost.InvalidTimeStamp));
                SOSHost.Write(checksum, SOSHost.InvalidChecksum);
            }
            catch (DiagnosticsException)
            {
                return HResult.E_FAIL;
            }
            return HResult.S_OK;
        }

        int ILLDBServices2Generated.GetModuleVersionInformation(uint index, ulong moduleBase, string item, byte* buffer,
            uint bufferSize, uint* versionInfoSize) =>
            _soshost.GetModuleVersionInformation(IntPtr.Zero, index, moduleBase, item, buffer, bufferSize, versionInfoSize);

        int ILLDBServices2Generated.SetRuntimeLoadedCallback(IntPtr callback) => HResult.E_NOTIMPL;

        private static void CopyOutputString(StringBuilder text, byte* buffer, uint capacity)
        {
            if (buffer == null || capacity == 0)
            {
                return;
            }
            IntPtr source = Marshal.StringToCoTaskMemAnsi(text?.ToString() ?? string.Empty);
            try
            {
                byte* bytes = (byte*)source;
                int length = 0;
                while (bytes[length] != 0)
                {
                    length++;
                }
                int count = (int)Math.Min((uint)length, capacity - 1);
                new ReadOnlySpan<byte>(bytes, count).CopyTo(new Span<byte>(buffer, count));
                buffer[count] = 0;
            }
            finally
            {
                Marshal.FreeCoTaskMem(source);
            }
        }
    }
}
