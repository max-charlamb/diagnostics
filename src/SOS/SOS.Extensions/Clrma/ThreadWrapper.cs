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
    public sealed partial class ThreadWrapper : IClrmaThreadGenerated
    {
        public uint ThreadId => _thread.ThreadId;

        private readonly ICrashInfoService _crashInfoService;
        private readonly IThread _thread;
        private ExceptionWrapper _currentException;
        private ExceptionWrapper[] _nestedExceptions;

        public ThreadWrapper(ICrashInfoService crashInfoService, IThread thread)
        {
            Debug.Assert(crashInfoService != null);
            _crashInfoService = crashInfoService;
            _thread = thread;
        }

        int IClrmaThreadGenerated.DebuggerCommand(out string command)
        {
            command = null;
            return HResult.S_FALSE;
        }

        int IClrmaThreadGenerated.OSThreadId(out uint osThreadId)
        {
            osThreadId = ThreadId;
            return osThreadId > 0 ? HResult.S_OK : HResult.S_FALSE;
        }

        int IClrmaThreadGenerated.FrameCount(out int count)
        {
            count = 0;
            return HResult.E_NOTIMPL;
        }

        int IClrmaThreadGenerated.Frame(
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
            return HResult.E_NOTIMPL;
        }

        int IClrmaThreadGenerated.CurrentException(out IClrmaExceptionGenerated clrmaClrException)
        {
            clrmaClrException = null;
            if (_currentException is null)
            {
                IException exception = null;
                try
                {
                    exception = _crashInfoService.GetThreadException(ThreadId);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                if (exception is null)
                {
                    return HResult.S_FALSE;
                }
                _currentException ??= new ExceptionWrapper(exception);
            }
            clrmaClrException = _currentException;
            return HResult.S_OK;
        }

        int IClrmaThreadGenerated.NestedExceptionCount(out ushort count)
        {
            count = (ushort)NestedExceptions.Length;
            return count > 0 ? HResult.S_OK : HResult.S_FALSE;
        }

        int IClrmaThreadGenerated.NestedException(
            ushort index,
            out IClrmaExceptionGenerated clrmaClrException)
        {
            clrmaClrException = null;
            if (index >= NestedExceptions.Length)
            {
                return ClrmaServiceWrapper.E_BOUNDS;
            }
            ExceptionWrapper exception = NestedExceptions[index];
            clrmaClrException = exception;
            return HResult.S_OK;
        }

        private ExceptionWrapper[] NestedExceptions
        {
            get
            {
                if (_nestedExceptions is null)
                {
                    try
                    {
                        if (_crashInfoService.GetThreadException(ThreadId) is null)
                        {
                            _nestedExceptions = Array.Empty<ExceptionWrapper>();
                        }
                        else
                        {
                            _nestedExceptions = _crashInfoService.GetNestedExceptions(ThreadId).Select((exception) => new ExceptionWrapper(exception)).ToArray();
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        _nestedExceptions = Array.Empty<ExceptionWrapper>();
                    }
                }
                return _nestedExceptions;
            }
        }
    }
}
