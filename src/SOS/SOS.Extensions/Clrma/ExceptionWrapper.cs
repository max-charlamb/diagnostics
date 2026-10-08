// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.Interop;

namespace SOS.Extensions.Clrma
{
    [GeneratedComClass]
    public sealed partial class ExceptionWrapper : IClrmaExceptionGenerated
    {
        public IException Exception { get; }

        private ExceptionWrapper[] _innerExceptions;

        public ExceptionWrapper(IException exception)
        {
            Debug.Assert(exception != null);
            Exception = exception;
        }

        int IClrmaExceptionGenerated.DebuggerCommand(out string command)
        {
            command = null;
            return HResult.S_FALSE;
        }

        int IClrmaExceptionGenerated.GetAddress(out ulong address)
        {
            address = Exception.Address;
            return HResult.S_OK;
        }

        int IClrmaExceptionGenerated.GetHResult(out uint hresult)
        {
            hresult = Exception.HResult;
            return HResult.S_OK;
        }

        int IClrmaExceptionGenerated.GetType(out string type)
        {
            type = Exception.Type;
            return HResult.S_OK;
        }

        int IClrmaExceptionGenerated.GetMessage(out string message)
        {
            message = Exception.Message;
            return HResult.S_OK;
        }

        int IClrmaExceptionGenerated.FrameCount(out int count)
        {
            count = Exception.Stack.FrameCount;
            return count > 0 ? HResult.S_OK : HResult.S_FALSE;
        }

        int IClrmaExceptionGenerated.Frame(
            int nFrame,
            out ulong ip,
            out ulong sp,
            out string moduleName,
            out string functionName,
            out ulong displacement)
        {
            ip = 0;
            sp = 0;
            moduleName = null;
            functionName = null;
            displacement = 0;
            IStackFrame frame;
            try
            {
                frame = Exception.Stack.GetStackFrame(nFrame);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ClrmaServiceWrapper.E_BOUNDS;
            }
            ip = frame.InstructionPointer;
            sp = frame.StackPointer;
            frame.GetMethodName(out moduleName, out functionName, out displacement);
            moduleName ??= $"module_{frame.ModuleBase:X16}";
            functionName ??= $"function_{frame.InstructionPointer:X16}";
            return HResult.S_OK;
        }

        int IClrmaExceptionGenerated.InnerExceptionCount(out ushort count)
        {
            count = (ushort)InnerExceptions.Length;
            return count > 0 ? HResult.S_OK : HResult.S_FALSE;
        }

        int IClrmaExceptionGenerated.InnerException(
            ushort index,
            out IClrmaExceptionGenerated clrmaClrException)
        {
            clrmaClrException = null;
            if (index >= InnerExceptions.Length)
            {
                return ClrmaServiceWrapper.E_BOUNDS;
            }
            ExceptionWrapper exception = InnerExceptions[index];
            clrmaClrException = exception;
            return HResult.S_OK;
        }

        private ExceptionWrapper[] InnerExceptions
        {
            get { return _innerExceptions ??= Exception.InnerExceptions.Select((exception) => new ExceptionWrapper(exception)).ToArray(); }
        }
    }
}
