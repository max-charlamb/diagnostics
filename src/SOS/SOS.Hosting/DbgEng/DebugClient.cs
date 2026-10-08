// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop.DbgEng;

namespace SOS.Hosting.DbgEng
{
    [GeneratedComClass]
    internal sealed unsafe partial class DebugClient : IDebugClientGenerated, IDebugAdvancedGenerated, IDebugControl2Generated, IDebugDataSpaces2Generated, IDebugRegistersGenerated, IDebugSymbols3Generated, IDebugSystemObjectsGenerated
    {
        private readonly SOSHost _soshost;

        /// <summary>
        /// Create an instance of the service wrapper SOS uses.
        /// The host exports and owns the native client reference.
        /// </summary>
        public DebugClient(SOSHost soshost)
        {
            _soshost = soshost;
        }

        internal static int NotImplemented
        {
            get
            {
                System.Diagnostics.Debugger.Break();
                return HResult.E_NOTIMPL;
            }
        }

        private static StringBuilder CreateStringBuilder(void* buffer, uint bufferSize)
        {
            return buffer == null ? null : new StringBuilder((int)Math.Min(bufferSize, 256u));
        }

        private static void CopyStringBuffer(StringBuilder builder, byte* buffer, uint bufferSize)
        {
            if (builder == null || buffer == null || bufferSize == 0)
            {
                return;
            }
            // Match LPStr's platform encoding rather than Encoding.Default's UTF-8 encoding.
            IntPtr text = Marshal.StringToCoTaskMemAnsi(builder.ToString());
            try
            {
                byte* source = (byte*)text;
                int length = 0;
                while (source[length] != 0)
                {
                    length++;
                }
                int count = (int)Math.Min((uint)length, bufferSize - 1);
                new ReadOnlySpan<byte>(source, count).CopyTo(new Span<byte>(buffer, count));
                buffer[count] = 0;
            }
            finally
            {
                Marshal.FreeCoTaskMem(text);
            }
        }

        private static void CopyStringBuffer(StringBuilder builder, char* buffer, uint bufferSize)
        {
            if (builder == null || buffer == null || bufferSize == 0)
            {
                return;
            }
            string text = builder.ToString();
            int count = (int)Math.Min((uint)text.Length, bufferSize - 1);
            text.AsSpan(0, count).CopyTo(new Span<char>(buffer, count));
            buffer[count] = '\0';
        }

        int IDebugClientGenerated.AttachKernel(DEBUG_ATTACH flags, string connectOptions) => NotImplemented;
        int IDebugClientGenerated.GetKernelConnectionOptions(byte* buffer, uint bufferSize, uint* optionsSize) => NotImplemented;
        int IDebugClientGenerated.SetKernelConnectionOptions(string options) => NotImplemented;
        int IDebugClientGenerated.StartProcessServer(DEBUG_CLASS flags, string options, IntPtr reserved) => NotImplemented;
        int IDebugClientGenerated.ConnectProcessServer(string remoteOptions, ulong* server) => NotImplemented;
        int IDebugClientGenerated.DisconnectProcessServer(ulong server) => NotImplemented;
        int IDebugClientGenerated.GetRunningProcessSystemIds(ulong server, uint* ids, uint count, uint* actualCount) => NotImplemented;
        int IDebugClientGenerated.GetRunningProcessSystemIdByExecutableName(ulong server, string exeName, DEBUG_GET_PROC flags, uint* id) => NotImplemented;
        int IDebugClientGenerated.GetRunningProcessDescription(ulong server, uint systemId, DEBUG_PROC_DESC flags, byte* exeName, uint exeNameSize, uint* actualExeNameSize, byte* description, uint descriptionSize, uint* actualDescriptionSize) => NotImplemented;
        int IDebugClientGenerated.AttachProcess(ulong server, uint processID, DEBUG_ATTACH attachFlags) => NotImplemented;
        int IDebugClientGenerated.CreateProcess(ulong server, string commandLine, DEBUG_CREATE_PROCESS flags) => NotImplemented;
        int IDebugClientGenerated.CreateProcessAndAttach(ulong server, string commandLine, DEBUG_CREATE_PROCESS flags, uint processId, DEBUG_ATTACH attachFlags) => NotImplemented;
        int IDebugClientGenerated.GetProcessOptions(DEBUG_PROCESS* options) => NotImplemented;
        int IDebugClientGenerated.AddProcessOptions(DEBUG_PROCESS options) => NotImplemented;
        int IDebugClientGenerated.RemoveProcessOptions(DEBUG_PROCESS options) => NotImplemented;
        int IDebugClientGenerated.SetProcessOptions(DEBUG_PROCESS options) => NotImplemented;
        int IDebugClientGenerated.OpenDumpFile(string dumpFile) => NotImplemented;
        int IDebugClientGenerated.WriteDumpFile(string dumpFile, DEBUG_DUMP qualifier) => NotImplemented;
        int IDebugClientGenerated.ConnectSession(DEBUG_CONNECT_SESSION flags, uint historyLimit) => NotImplemented;
        int IDebugClientGenerated.StartServer(string options) => NotImplemented;
        int IDebugClientGenerated.OutputServers(DEBUG_OUTCTL outputControl, string machine, DEBUG_SERVERS flags) => NotImplemented;
        int IDebugClientGenerated.TerminateProcesses() => NotImplemented;
        int IDebugClientGenerated.DetachProcesses() => NotImplemented;
        int IDebugClientGenerated.EndSession(DEBUG_END flags) => NotImplemented;
        int IDebugClientGenerated.GetExitCode(uint* code) => NotImplemented;
        int IDebugClientGenerated.DispatchCallbacks(uint timeout) => NotImplemented;
        int IDebugClientGenerated.ExitDispatch(IDebugClientGenerated client) => NotImplemented;
        int IDebugClientGenerated.CreateClient(out IDebugClientGenerated client)
        {
            client = default;
            return NotImplemented;
        }
        int IDebugClientGenerated.GetInputCallbacks(out IntPtr callbacks)
        {
            callbacks = default;
            return NotImplemented;
        }
        int IDebugClientGenerated.SetInputCallbacks(IntPtr callbacks) => NotImplemented;
        int IDebugClientGenerated.GetOutputCallbacks(out IntPtr callbacks)
        {
            callbacks = default;
            return NotImplemented;
        }
        int IDebugClientGenerated.SetOutputCallbacks(IntPtr callbacks) => NotImplemented;
        int IDebugClientGenerated.GetOutputMask(DEBUG_OUTPUT* mask) => NotImplemented;
        int IDebugClientGenerated.SetOutputMask(DEBUG_OUTPUT mask) => NotImplemented;
        int IDebugClientGenerated.GetOtherOutputMask(IDebugClientGenerated client, DEBUG_OUTPUT* mask) => NotImplemented;
        int IDebugClientGenerated.SetOtherOutputMask(IDebugClientGenerated client, DEBUG_OUTPUT mask) => NotImplemented;
        int IDebugClientGenerated.GetOutputWidth(uint* columns) => NotImplemented;
        int IDebugClientGenerated.SetOutputWidth(uint columns) => NotImplemented;
        int IDebugClientGenerated.GetOutputLinePrefix(byte* buffer, uint bufferSize, uint* prefixSize) => NotImplemented;
        int IDebugClientGenerated.SetOutputLinePrefix(string prefix) => NotImplemented;
        int IDebugClientGenerated.GetIdentity(byte* buffer, uint bufferSize, uint* identitySize) => NotImplemented;
        int IDebugClientGenerated.OutputIdentity(DEBUG_OUTCTL outputControl, uint flags, string format) => NotImplemented;
        int IDebugClientGenerated.GetEventCallbacks(out IntPtr callbacks)
        {
            callbacks = default;
            return NotImplemented;
        }
        int IDebugClientGenerated.SetEventCallbacks(IntPtr callbacks) => NotImplemented;
        int IDebugClientGenerated.FlushCallbacks() => NotImplemented;
    }
}
