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
    [GeneratedComClass]
    public sealed unsafe partial class CorDebugDataTargetWrapper : ICorDebugMutableDataTargetGenerated,
        ICorDebugDataTarget4Generated, ICorDebugMetaDataLocatorGenerated, IDisposable
    {
        private readonly ITarget _target;
        private readonly ISymbolService _symbolService;
        private readonly IMemoryService _memoryService;
        private readonly IThreadService _threadService;
        private readonly IThreadUnwindService _threadUnwindService;
        private readonly ulong _ignoreAddressBitsMask;

        public IntPtr ICorDebugDataTarget { get; private set; }

        public CorDebugDataTargetWrapper(IServiceProvider services, IRuntime runtime)
        {
            Debug.Assert(services != null);
            Debug.Assert(runtime != null);
            _target = runtime.Target;
            _symbolService = services.GetService<ISymbolService>();
            _memoryService = services.GetService<IMemoryService>();
            _threadService = services.GetService<IThreadService>();
            _threadUnwindService = services.GetService<IThreadUnwindService>();
            _ignoreAddressBitsMask = _memoryService.SignExtensionMask();

            ICorDebugDataTarget = (IntPtr)ComInterfaceMarshaller<ICorDebugDataTargetGenerated>.ConvertToUnmanaged(this);
        }

        public void Dispose()
        {
            if (ICorDebugDataTarget == IntPtr.Zero)
            {
                return;
            }
            Trace.TraceInformation("CorDebugDataTargetWrapper.Dispose");
            ComInterfaceMarshaller<ICorDebugDataTargetGenerated>.Free((void*)ICorDebugDataTarget);
            ICorDebugDataTarget = IntPtr.Zero;
        }

        #region ICorDebugDataTarget

        int ICorDebugDataTargetGenerated.GetPlatform(
            out CorDebugPlatform platform)
        {
            platform = CorDebugPlatform.CORDB_PLATFORM_WINDOWS_AMD64;
            if (_target.OperatingSystem == OSPlatform.Windows)
            {
                switch (_target.Architecture)
                {
                    case Architecture.X64:
                        platform = CorDebugPlatform.CORDB_PLATFORM_WINDOWS_AMD64;
                        break;
                    case Architecture.X86:
                        platform = CorDebugPlatform.CORDB_PLATFORM_WINDOWS_X86;
                        break;
                    case Architecture.Arm:
                        platform = CorDebugPlatform.CORDB_PLATFORM_WINDOWS_ARM;
                        break;
                    case Architecture.Arm64:
                        platform = CorDebugPlatform.CORDB_PLATFORM_WINDOWS_ARM64;
                        break;
                    default:
                        return HResult.E_FAIL;
                }
            }
            else if (_target.OperatingSystem == OSPlatform.Linux || _target.OperatingSystem == OSPlatform.OSX)
            {
                switch (_target.Architecture)
                {
                    case Architecture.X64:
                        platform = CorDebugPlatform.CORDB_PLATFORM_POSIX_AMD64;
                        break;
                    case Architecture.X86:
                        platform = CorDebugPlatform.CORDB_PLATFORM_POSIX_X86;
                        break;
                    case Architecture.Arm:
                        platform = CorDebugPlatform.CORDB_PLATFORM_POSIX_ARM;
                        break;
                    case Architecture.Arm64:
                        platform = CorDebugPlatform.CORDB_PLATFORM_POSIX_ARM64;
                        break;
                    case (Architecture)6 /* Architecture.LoongArch64 */:
                        platform = CorDebugPlatform.CORDB_PLATFORM_POSIX_LOONGARCH64;
                        break;
                    case (Architecture)9 /* Architecture.RiscV64 */:
                        platform = CorDebugPlatform.CORDB_PLATFORM_POSIX_RISCV64;
                        break;
                    default:
                        return HResult.E_FAIL;
                }
            }
            else
            {
                return HResult.E_FAIL;
            }
            return HResult.S_OK;
        }

        int ICorDebugDataTargetGenerated.ReadVirtual(
            ulong address,
            byte* buffer,
            uint bytesRequested,
            uint* pbytesRead)
        {
            int read = 0;
            if (bytesRequested > 0)
            {
                address &= _ignoreAddressBitsMask;
                if (!_memoryService.ReadMemory(address, new Span<byte>(buffer, unchecked((int)bytesRequested)), out read))
                {
                    Trace.TraceError("CorDebugDataTargetWrapper.ReadVirtual FAILED address {0:X16} size {1:X8}", address, bytesRequested);
                    return HResult.E_FAIL;
                }
            }
            SOSHost.Write(pbytesRead, (uint)read);
            return HResult.S_OK;
        }

        int ICorDebugDataTargetGenerated.GetThreadContext(
            uint threadId,
            uint contextFlags,
            uint contextSize,
            byte* context)
        {
            if (contextSize > int.MaxValue)
            {
                Trace.TraceError($"CorDebugDataTargetWrapper.GetThreadContext: invalid context size {contextSize}");
                return HResult.E_INVALIDARG;
            }
            try
            {
                _threadService.GetThreadFromId(threadId).GetThreadContext(new Span<byte>(context, (int)contextSize));
            }
            catch (Exception ex) when (ex is DiagnosticsException or ArgumentOutOfRangeException)
            {
                Trace.TraceError($"CorDebugDataTargetWrapper.GetThreadContext({threadId:X8}) FAILED");
                return HResult.E_INVALIDARG;
            }
            return HResult.S_OK;
        }

        #endregion

        #region ICorDebugDataTarget4

        int ICorDebugDataTarget4Generated.VirtualUnwind(
            uint threadId,
            uint contextSize,
            byte* context)
        {
            if (contextSize > int.MaxValue)
            {
                Trace.TraceError($"CorDebugDataTargetWrapper.VirtualUnwind: invalid context size {contextSize}");
                return HResult.E_INVALIDARG;
            }
            try
            {
                if (_threadUnwindService == null)
                {
                    return HResult.E_NOTIMPL;
                }
                return _threadUnwindService.Unwind(threadId, new Span<byte>(context, (int)contextSize));
            }
            catch (DiagnosticsException)
            {
                return HResult.E_INVALIDARG;
            }
        }

        #endregion

        #region ICorDebugMutableDataTarget

        int ICorDebugMutableDataTargetGenerated.WriteVirtual(
            ulong address,
            byte* buffer,
            uint bytesRequested)
        {
            address &= _ignoreAddressBitsMask;
            if (!_memoryService.WriteMemory(address, new Span<byte>(buffer, unchecked((int)bytesRequested)), out _))
            {
                return HResult.E_FAIL;
            }
            return HResult.S_OK;
        }

        int ICorDebugMutableDataTargetGenerated.SetThreadContext(uint threadId, uint contextSize, byte* context)
        {
            return HResult.E_NOTIMPL;
        }

        int ICorDebugMutableDataTargetGenerated.ContinueStatusChanged(uint threadId, uint continueStatus)
        {
            return HResult.E_NOTIMPL;
        }

        #endregion

        #region ICorDebugMetaDataLocator

        int ICorDebugMetaDataLocatorGenerated.GetMetaData(
            string imagePath,
            uint imageTimestamp,
            uint imageSize,
            uint pathBufferSize,
            uint* pPathBufferSize,
            char* pPathBuffer)
        {
            return _symbolService.GetICorDebugMetadataLocator(imagePath, imageTimestamp, imageSize, pathBufferSize, (IntPtr)pPathBufferSize, (IntPtr)pPathBuffer);
        }

        #endregion
    }
}
