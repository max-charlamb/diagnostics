// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.Interop;

namespace SOS.Hosting
{
    [ServiceExport(Scope = ServiceScope.Target)]
    [GeneratedComClass]
    public sealed unsafe partial class TargetWrapper : ITargetGenerated
    {
        internal Func<IClrmaServiceGenerated> ClrmaServiceFactory { get; set; }

        internal Func<ISymbolServiceGenerated> SymbolServiceFactory { get; set; }

        private readonly ITarget _target;
        private readonly IContextService _contextService;
        private ISymbolServiceGenerated _symbolServiceWrapper;
        private IClrmaServiceGenerated _clrmaServiceWrapper;

        public TargetWrapper(
            ITarget target,
            IContextService contextService,
            ISymbolService symbolService,
            IMemoryService memoryService)
        {
            _target = target;
            _contextService = contextService;
            SymbolServiceFactory = () => new SymbolServiceWrapper(symbolService, memoryService);
        }

        internal ISymbolServiceGenerated GetSymbolService() => _symbolServiceWrapper ??= SymbolServiceFactory();

        ITargetGenerated.OperatingSystem ITargetGenerated.GetOperatingSystem()
        {
            if (_target.OperatingSystem == OSPlatform.Windows)
            {
                return ITargetGenerated.OperatingSystem.Windows;
            }
            else if (_target.OperatingSystem == OSPlatform.Linux)
            {
                return ITargetGenerated.OperatingSystem.Linux;
            }
            else if (_target.OperatingSystem == OSPlatform.OSX)
            {
                return ITargetGenerated.OperatingSystem.OSX;
            }
            return ITargetGenerated.OperatingSystem.Unknown;
        }

        int ITargetGenerated.GetService(in Guid serviceId, out IntPtr service)
        {
            service = IntPtr.Zero;
            if (serviceId == typeof(ISymbolServiceGenerated).GUID && SymbolServiceFactory != null)
            {
                _symbolServiceWrapper ??= SymbolServiceFactory();
                service = (IntPtr)ComInterfaceMarshaller<ISymbolServiceGenerated>.ConvertToUnmanaged(_symbolServiceWrapper);
                return HResult.S_OK;
            }
            else if (serviceId == typeof(IClrmaServiceGenerated).GUID && ClrmaServiceFactory != null)
            {
                _clrmaServiceWrapper ??= ClrmaServiceFactory();
                service = (IntPtr)ComInterfaceMarshaller<IClrmaServiceGenerated>.ConvertToUnmanaged(_clrmaServiceWrapper);
                return HResult.S_OK;
            }
            else
            {
                return HResult.E_NOINTERFACE;
            }
        }

        int ITargetGenerated.GetRuntime(IntPtr* ppRuntime)
        {
            if (ppRuntime == null)
            {
                return HResult.E_INVALIDARG;
            }
            *ppRuntime = IntPtr.Zero;
            try
            {
                IRuntime runtime = _contextService.GetCurrentRuntime();
                if (runtime is null)
                {
                    return HResult.E_NOINTERFACE;
                }
                RuntimeWrapper wrapper = runtime.Services.GetService<RuntimeWrapper>();
                if (wrapper is null || wrapper.IsDisposed)
                {
                    return HResult.E_NOINTERFACE;
                }
                *ppRuntime = wrapper.IRuntime;
                return HResult.S_OK;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return HResult.E_NOINTERFACE;
            }
        }

        void ITargetGenerated.Flush()
        {
            _target.Flush();
        }
    }
}
