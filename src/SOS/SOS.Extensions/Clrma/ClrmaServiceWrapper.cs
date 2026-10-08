// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.Interop;

namespace SOS.Extensions.Clrma
{
    [GeneratedComClass]
    public sealed partial class ClrmaServiceWrapper : IClrmaServiceGenerated
    {
        public const ModuleEnumerationScheme DefaultModuleEnumerationScheme = ModuleEnumerationScheme.EntryPointAndEntryPointDllModule;

        public const int E_BOUNDS = unchecked((int)0x8000000B);
        public const uint DEBUG_ANY_ID = uint.MaxValue;

        private readonly IServiceProvider _serviceProvider;

        public ClrmaServiceWrapper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ModuleEnumerationScheme ModuleEnumerationScheme { get; private set; } = ModuleEnumerationScheme.None;

        int IClrmaServiceGenerated.AssociateClient(IntPtr punk)
        {
            // If the crash info service doesn't exist, then tell Watson/CLRMA to go on to the next provider
            return CrashInfoService is null ? HResult.E_NOINTERFACE : HResult.S_OK;
        }

        int IClrmaServiceGenerated.GetThread(
            uint osThreadId,
            out IClrmaThreadGenerated clrmaClrThread)
        {
            clrmaClrThread = null;
            ICrashInfoService crashInfoService = CrashInfoService;
            if (crashInfoService is null)
            {
                return HResult.E_FAIL;
            }
            IThread thread = ThreadService?.GetThreadFromId(osThreadId);
            if (thread is null)
            {
                return HResult.E_INVALIDARG;
            }
            clrmaClrThread = new ThreadWrapper(crashInfoService, thread);
            return HResult.S_OK;
        }

        int IClrmaServiceGenerated.GetException(
            ulong address,
            out IClrmaExceptionGenerated clrmaClrException)
        {
            clrmaClrException = null;
            IException exception = null;
            try
            {
                exception = CrashInfoService?.GetException(address);
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            if (exception is null)
            {
                return HResult.E_INVALIDARG;
            }
            clrmaClrException = new ExceptionWrapper(exception);
            return HResult.S_OK;
        }

        int IClrmaServiceGenerated.GetObjectInspection(out IntPtr clrmaObjectInspection)
        {
            clrmaObjectInspection = IntPtr.Zero;
            return HResult.E_NOTIMPL;
        }

        int IClrmaServiceGenerated.SetModuleEnumerationPolicy(uint moduleEnumerationPolicy)
        {
            if (moduleEnumerationPolicy > (uint)ModuleEnumerationScheme.All)
            {
                return HResult.E_INVALIDARG;
            }
            ModuleEnumerationScheme = (ModuleEnumerationScheme)moduleEnumerationPolicy;
            return HResult.S_OK;
        }

        private ICrashInfoService CrashInfoService => _serviceProvider.GetService<ICrashInfoService>() ?? _serviceProvider.GetService<ICrashInfoModuleService>()?.Create(ModuleEnumerationScheme);

        private IThreadService ThreadService => _serviceProvider.GetService<IThreadService>();
    }
}
