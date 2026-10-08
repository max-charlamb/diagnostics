// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.DebugServices.Implementation;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop;
using SOS.Hosting.Interop.DbgEng;

namespace SOS.Extensions
{
    internal sealed unsafe class DebuggerServices : SOSHost.INativeDebugger
    {
        private readonly IDebuggerServicesGenerated _services;

        private readonly HostType _hostType;
        private readonly HResult _symbolOptionsResult = HResult.S_OK;

        /// <summary>
        /// The underlying DbgEng client, if available.
        /// </summary>
        public IDebugClient5Generated DebugClient { get; }

        public IDebugSymbols5Generated DebugSymbols { get; }

        public IRemoteMemoryServiceGenerated RemoteMemoryService => _services as IRemoteMemoryServiceGenerated;

        public IDebuggerThreadStackTraceServiceGenerated ThreadStackTraceService => _services as IDebuggerThreadStackTraceServiceGenerated;

        internal DebuggerServices(IntPtr punk, HostType hostType)
        {
            _hostType = hostType;
            try
            {
                _services = ComInterfaceMarshaller<IDebuggerServicesGenerated>.ConvertToManaged((void*)punk);
            }
            finally
            {
                ComInterfaceMarshaller<IDebuggerServicesGenerated>.Free((void*)punk);
            }
            if (_services == null)
            {
                throw new InvalidCastException("DebuggerServices: IDebuggerServices is not available.");
            }

            if (hostType == HostType.DbgEng && _services is IDebugClient5Generated client)
            {
                DebugClient = client;
                if (_services is IDebugSymbols5Generated symbols)
                {
                    DebugSymbols = symbols;
                    // NativeAOT emits debugger-friendly CodeView display names while retaining stable, reversible
                    // COFF public names used by diagnostic tooling. Prefer the public names for SOS lookups.
                    // See https://github.com/dotnet/runtime/pull/132735.
                    _symbolOptionsResult = DebugSymbols.AddSymbolOptions(SYMOPT.PUBLICS_ONLY);
                }
            }
        }

        #region SOSHost.INativeDebugger

        public IntPtr GetNativeClient()
        {
            Guid iid;
            if (_hostType == HostType.DbgEng)
            {
                iid = typeof(IDebugClientGenerated).GUID;
            }
            else if (_hostType == HostType.Lldb)
            {
                iid = LLDBServices.IID_ILLDBServices;
            }
            else
            {
                throw new InvalidOperationException($"DebuggerServices.GetNativeClient: invalid host type {_hostType}");
            }
            void* services = ComInterfaceMarshaller<IDebuggerServicesGenerated>.ConvertToUnmanaged(_services);
            try
            {
                HResult hr = Marshal.QueryInterface((IntPtr)services, in iid, out IntPtr client);
                return hr.IsOK ? client : IntPtr.Zero;
            }
            finally
            {
                ComInterfaceMarshaller<IDebuggerServicesGenerated>.Free(services);
            }
        }

        #endregion

        public HResult GetOperatingSystem(out IDebuggerServicesGenerated.OperatingSystem operatingSystem)
        {
            return _services.GetOperatingSystem(out operatingSystem);
        }

        public HResult GetDebuggeeType(out DEBUG_CLASS debugClass, out DEBUG_CLASS_QUALIFIER qualifier)
        {
            return _services.GetDebuggeeType(out debugClass, out qualifier);
        }

        public HResult GetProcessorType(out IMAGE_FILE_MACHINE type)
        {
            return _services.GetProcessorType(out type);
        }

        public HResult AddCommand(string command, string help, IEnumerable<string> aliases)
        {
            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(help) || aliases == null)
            {
                throw new ArgumentNullException();
            }

            byte[] commandBytes = Encoding.ASCII.GetBytes(command + "\0");
            byte[] helpBytes = Encoding.ASCII.GetBytes(help + "\0");
            IntPtr[] aliasHandles = aliases.Select((alias) => Marshal.StringToHGlobalAnsi(alias)).ToArray();
            try
            {
                fixed (byte* commandPtr = commandBytes)
                fixed (byte* helpPtr = helpBytes)
                fixed (IntPtr* aliasesPtr = aliasHandles)
                {
                    return _services.AddCommand(commandPtr, helpPtr, aliasesPtr, aliasHandles.Length);
                }
            }
            finally
            {
                foreach (IntPtr handle in aliasHandles)
                {
                    Marshal.FreeHGlobal(handle);
                }
            }
        }

        public void OutputString(DEBUG_OUTPUT mask, string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            byte[] messageBytes = Encoding.ASCII.GetBytes(message + "\0");
            fixed (byte* messagePtr = messageBytes)
            {
                _services.OutputString(mask, messagePtr);
            }
        }

        public HResult ReadVirtual(ulong offset, Span<byte> buffer, out int bytesRead)
        {
            fixed (byte* bufferPtr = buffer)
            {
                return _services.ReadVirtual(offset, bufferPtr, (uint)buffer.Length, out bytesRead);
            }
        }

        public HResult WriteVirtual(ulong offset, Span<byte> buffer, out int bytesWritten)
        {
            fixed (byte* bufferPtr = buffer)
            {
                return _services.WriteVirtual(offset, bufferPtr, (uint)buffer.Length, out bytesWritten);
            }
        }

        public HResult GetNumberModules(out uint loaded, out uint unloaded)
        {
            return _services.GetNumberModules(out loaded, out unloaded);
        }

        public HResult GetModuleName(int index, out string imageName)
        {
            imageName = null;

            // GetModuleNames under lldb doesn't support querying just the
            // path length (imageNameBufferPtr = null) so use a fix size
            // image name buffer.
            byte[] imageNameBuffer = new byte[1024];
            fixed (byte* imageNameBufferPtr = imageNameBuffer)
            {
                HResult hr = _services.GetModuleNames(
                    (uint)index,
                    0,
                    imageNameBufferPtr,
                    (uint)imageNameBuffer.Length,
                    out uint imageNameSize,
                    null,
                    0,
                    null,
                    null,
                    0,
                    null);

                if (hr >= HResult.S_OK)
                {
                    if (imageNameSize > 0)
                    {
                        imageName = Encoding.ASCII.GetString(imageNameBufferPtr, (int)imageNameSize - 1);
                    }
                    else
                    {
                        hr = HResult.E_INVALIDARG;
                    }
                }
                return hr;
            }
        }

        public HResult GetModuleInfo(int index, out ulong moduleBase, out ulong moduleSize, out uint timestamp, out uint checksum)
        {
            return _services.GetModuleInfo((uint)index, out moduleBase, out moduleSize, out timestamp, out checksum);
        }

        private static readonly byte[] s_getVersionInfo = Encoding.ASCII.GetBytes("\\\0");

        public HResult GetModuleVersionInformation(int index, out VS_FIXEDFILEINFO fileInfo)
        {
            int versionBufferSize = Marshal.SizeOf<VS_FIXEDFILEINFO>();
            byte[] versionBuffer = new byte[versionBufferSize];
            fileInfo = default;

            fixed (byte* getVersionInfoPtr = s_getVersionInfo)
            fixed (byte* versionBufferPtr = versionBuffer)
            {
                HResult hr = _services.GetModuleVersionInformation((uint)index, 0, getVersionInfoPtr, versionBufferPtr, (uint)versionBufferSize, null);
                if (hr == HResult.S_OK)
                {
                    fileInfo = *((VS_FIXEDFILEINFO*)versionBufferPtr);
                }
                return hr;
            }
        }

        private static readonly byte[] s_getVersionString = Encoding.ASCII.GetBytes("\\StringFileInfo\\040904B0\\FileVersion\0");

        public HResult GetModuleVersionString(int index, out string version)
        {
            byte[] versionBuffer = new byte[1024];
            version = default;

            fixed (byte* getVersionStringPtr = s_getVersionString)
            fixed (byte* versionBufferPtr = versionBuffer)
            {
                int hr = _services.GetModuleVersionInformation((uint)index, 0, getVersionStringPtr, versionBufferPtr, (uint)versionBuffer.Length, null);
                if (hr == HResult.S_OK)
                {
                    version = Marshal.PtrToStringAnsi(new IntPtr(versionBufferPtr));
                }
                return hr;
            }
        }

        public HResult GetNumberThreads(out uint number)
        {
            return _services.GetNumberThreads(out number);
        }

        public HResult GetThreadIdsByIndex(uint start, uint count, uint[] ids, uint[] sysIds)
        {
            if (ids != null && (start >= ids.Length || start + count > ids.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(ids));
            }

            if (sysIds != null && (start >= sysIds.Length || start + count > sysIds.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(sysIds));
            }

            fixed (uint* pids = ids)
            {
                fixed (uint* psysIds = sysIds)
                {
                    return _services.GetThreadIdsByIndex(start, count, pids, psysIds);
                }
            }
        }

        public HResult GetThreadContext(uint threadId, uint contextFlags, byte[] context)
        {
            fixed (byte* contextPtr = context)
            {
                return _services.GetThreadContextBySystemId(threadId, contextFlags, (uint)context.Length, contextPtr);
            }
        }

        public HResult GetCurrentProcessId(out uint processId)
        {
            return _services.GetCurrentProcessSystemId(out processId);
        }

        public HResult GetCurrentThreadId(out uint threadId)
        {
            return _services.GetCurrentThreadSystemId(out threadId);
        }

        public HResult SetCurrentThreadId(uint threadId)
        {
            return _services.SetCurrentThreadSystemId(threadId);
        }

        public HResult GetThreadTeb(uint threadId, out ulong teb)
        {
            // The native code may not zero out this return pointer
            teb = 0;
            return _services.GetThreadTeb(threadId, ref teb);
        }

        public HResult VirtualUnwind(uint threadId, Span<byte> context)
        {
            fixed (byte* contextPtr = context)
            {
                return _services.VirtualUnwind(threadId, (uint)context.Length, contextPtr);
            }
        }

        public HResult GetSymbolPath(out string symbolPath)
        {
            symbolPath = null;

            // Get the path length first
            HResult hr = _services.GetSymbolPath(null, 0, out uint pathSize);
            if (hr == HResult.S_OK)
            {
                if (pathSize > 0)
                {
                    // Now get the symbol path
                    byte[] buffer = new byte[pathSize];
                    fixed (byte* bufferPtr = buffer)
                    {
                        hr = _services.GetSymbolPath(bufferPtr, (uint)buffer.Length, out pathSize);
                        if (hr == HResult.S_OK)
                        {
                            symbolPath = Encoding.ASCII.GetString(bufferPtr, (int)pathSize - 1);
                        }
                    }
                }
                else
                {
                    hr = HResult.E_INVALIDARG;
                }
            }
            return hr;
        }

        public HResult GetSymbolByOffset(int moduleIndex, ulong address, out string symbol, out ulong displacement)
        {
            if (!_symbolOptionsResult.IsOK)
            {
                symbol = null;
                displacement = 0;
                return _symbolOptionsResult;
            }

            symbol = null;

            // Get the symbol length first
            HResult hr = _services.GetSymbolByOffset((uint)moduleIndex, address, null, 0, out uint symbolSize, out displacement);
            if (hr == HResult.S_OK)
            {
                if (symbolSize > 0)
                {
                    // Now get the symbol
                    byte[] symbolBuffer = new byte[symbolSize];
                    fixed (byte* symbolBufferPtr = symbolBuffer)
                    {
                        hr = _services.GetSymbolByOffset((uint)moduleIndex, address, symbolBufferPtr, (uint)symbolBuffer.Length, out symbolSize, out displacement);
                        if (hr == HResult.S_OK)
                        {
                            symbol = Encoding.ASCII.GetString(symbolBufferPtr, (int)symbolSize - 1);
                            if (_hostType == HostType.DbgEng)
                            {
                                int index = symbol.IndexOf('!');
                                if (index != -1)
                                {
                                    symbol = symbol.Remove(0, index + 1);
                                }
                            }
                        }
                    }
                }
                else
                {
                    hr = HResult.E_INVALIDARG;
                }
            }
            return hr;
        }

        public HResult GetOffsetBySymbol(int moduleIndex, string symbol, out ulong address)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            byte[] symbolBytes = Encoding.ASCII.GetBytes(symbol + "\0");
            fixed (byte* symbolPtr = symbolBytes)
            {
                return _services.GetOffsetBySymbol((uint)moduleIndex, symbolPtr, out address);
            }
        }

        public HResult GetTypeId(int moduleIndex, string typeName, out ulong typeId)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException(nameof(typeName));
            }

            byte[] typeNameBytes = Encoding.ASCII.GetBytes(typeName + "\0");
            fixed (byte* typeNamePtr = typeNameBytes)
            {
                return _services.GetTypeId((uint)moduleIndex, typeNamePtr, out typeId);
            }
        }

        public HResult GetFieldOffset(int moduleIndex, ulong typeId, string typeName, string fieldName, out uint offset)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException(nameof(fieldName));
            }

            byte[] typeNameBytes = Encoding.ASCII.GetBytes(typeName + "\0");
            byte[] fieldNameBytes = Encoding.ASCII.GetBytes(fieldName + "\0");
            fixed (byte* typeNamePtr = typeNameBytes)
            fixed (byte* fieldNamePtr = fieldNameBytes)
            {
                return _services.GetFieldOffset((uint)moduleIndex, typeNamePtr, typeId, fieldNamePtr, out offset);
            }
        }

        public int GetOutputWidth() => (int)_services.GetOutputWidth();

        public bool SupportsDml
        {
            get
            {
                uint supported = 0;
                _services.SupportsDml(&supported);
                return supported != 0;
            }
        }

        public void OutputDmlString(DEBUG_OUTPUT mask, string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            byte[] messageBytes = Encoding.ASCII.GetBytes(message + "\0");
            fixed (byte* messagePtr = messageBytes)
            {
                _services.OutputDmlString(mask, messagePtr);
            }
        }

        public HResult AddModuleSymbol(string symbolFileName)
        {
            if (symbolFileName == null)
            {
                throw new ArgumentNullException(nameof(symbolFileName));
            }
            byte[] symbolFileNameBytes = Encoding.ASCII.GetBytes(symbolFileName + "\0");
            fixed (byte* ptr = symbolFileNameBytes)
            {
                return _services.AddModuleSymbol(IntPtr.Zero, ptr);
            }
        }

        public HResult GetLastException(out uint processId, out int threadId, out EXCEPTION_RECORD64 exceptionRecord)
        {
            exceptionRecord = default;

            uint type;
            HResult hr = _services.GetLastEventInformation(out type, out processId, out threadId, null, 0, null, null, 0, null);
            if (hr.IsOK)
            {
                if (type != (uint)DEBUG_EVENT.EXCEPTION)
                {
                    return HResult.E_FAIL;
                }
            }

            DEBUG_LAST_EVENT_INFO_EXCEPTION exceptionInfo;
            hr = _services.GetLastEventInformation(
                out _,
                out processId,
                out threadId,
                &exceptionInfo,
                (uint)Unsafe.SizeOf<DEBUG_LAST_EVENT_INFO_EXCEPTION>(),
                null,
                null,
                0,
                null);

            if (hr.IsOK)
            {
                exceptionRecord = exceptionInfo.ExceptionRecord;
            }
            Debug.Assert(hr != HResult.S_FALSE);
            return hr;
        }

        public void FlushCheck()
        {
            _services.FlushCheck();
        }

        public IReadOnlyList<string> ExecuteHostCommand(string commandLine, DEBUG_OUTPUT interestMask = DEBUG_OUTPUT.NORMAL)
        {
            CaptureConsoleService console = new();
            ExecuteHostCommand(commandLine, (DEBUG_OUTPUT mask, string text) =>
            {
                if ((mask & interestMask) != 0)
                {
                    console.Write(text);
                }
            });
            return console.OutputLines;
        }

        public delegate void ExecuteHostCommandCallback(DEBUG_OUTPUT mask, string text);

        public void ExecuteHostCommand(string commandLine, ExecuteHostCommandCallback outputCallback)
        {
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(outputCallback);
            byte[] commandLineBytes = Encoding.ASCII.GetBytes(commandLine + "\0");
            fixed (byte* ptr = commandLineBytes)
            {
                HResult hr = _services.ExecuteHostCommand(ptr, callbackPtr);
                if (!hr.IsOK)
                {
                    throw new DiagnosticsException($"{commandLine} FAILED {hr}");
                }
            }
        }

        public HResult GetDacSignatureVerificationSettings(out bool value)
        {
            value = false;
            int dacSignatureVerificationEnabled = 0;
            HResult hr = _services.GetDacSignatureVerificationSettings(&dacSignatureVerificationEnabled);
            if (!hr.IsOK)
            {
                return hr;
            }
            value = dacSignatureVerificationEnabled != 0;
            return HResult.S_OK;
        }
    }
}
