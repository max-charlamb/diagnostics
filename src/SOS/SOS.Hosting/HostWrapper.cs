// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.Interop;

namespace SOS.Hosting
{
    [GeneratedComClass]
    public sealed partial class HostWrapper : IHostGenerated
    {
        private readonly IHost _host;
        private readonly IHostServicesGenerated _hostServices;

        public HostWrapper(IHost host)
            : this(host, null)
        {
        }

        internal HostWrapper(IHost host, IHostServicesGenerated hostServices)
        {
            _host = host;
            _hostServices = hostServices;
        }

        /// <summary>
        /// Returns the host type
        /// </summary>
        HostType IHostGenerated.GetHostType() => _host.HostType;

        unsafe int IHostGenerated.GetService(in Guid serviceId, out IntPtr service)
        {
            service = IntPtr.Zero;
            if (serviceId != typeof(IHostServicesGenerated).GUID || _hostServices is null)
            {
                return HResult.E_NOINTERFACE;
            }
            service = (IntPtr)ComInterfaceMarshaller<IHostServicesGenerated>.ConvertToUnmanaged(_hostServices);
            return HResult.S_OK;
        }

        /// <summary>
        /// Returns the current target wrapper or null
        /// </summary>
        /// <param name="targetWrapper">target wrapper address returned</param>
        /// <returns>S_OK</returns>
        int IHostGenerated.GetCurrentTarget(out ITargetGenerated targetWrapper)
        {
            IContextService contextService = _host.Services.GetService<IContextService>();
            ITarget target = contextService.GetCurrentTarget();
            TargetWrapper wrapper = target?.Services.GetService<TargetWrapper>();
            if (wrapper == null)
            {
                targetWrapper = null;
                return HResult.E_NOINTERFACE;
            }
            targetWrapper = wrapper;
            return HResult.S_OK;
        }
    }
}
