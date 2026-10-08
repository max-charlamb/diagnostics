// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop;

namespace SOS.Hosting
{
    [GeneratedComClass]
    internal sealed unsafe partial class DataTargetWrapper : ICLRDataTarget2Generated, ICorDebugDataTarget4Generated,
        ICLRMetadataLocatorGenerated, ICLRRuntimeLocatorGenerated, ICLRContractLocatorGenerated, ICLRSymbolProviderGenerated, IDisposable
    {
        // For ClrMD's magic hand shake
        private const ulong MagicCallbackConstant = 0x43;

        private readonly IRuntime _runtime;
        private readonly IContextService _contextService;
        private readonly ISymbolService _symbolService;
        private readonly IMemoryService _memoryService;
        private readonly IThreadService _threadService;
        private readonly IModuleService _moduleService;
        private readonly IThreadUnwindService _threadUnwindService;
        private readonly IRemoteMemoryService _remoteMemoryService;
        private readonly IClrSymbolProvider _symbolProvider;
        private readonly ulong _ignoreAddressBitsMask;

        public IntPtr IDataTarget { get; private set; }

        public DataTargetWrapper(IServiceProvider services, IRuntime runtime)
        {
            Debug.Assert(services != null);
            Debug.Assert(runtime != null);
            _runtime = runtime;
            _contextService = services.GetService<IContextService>();
            _symbolService = services.GetService<ISymbolService>();
            _memoryService = services.GetService<IMemoryService>();
            _threadService = services.GetService<IThreadService>();
            _threadUnwindService = services.GetService<IThreadUnwindService>();
            _moduleService = services.GetService<IModuleService>();
            _remoteMemoryService = services.GetService<IRemoteMemoryService>();
            _symbolProvider = services.GetService<IClrSymbolProvider>();
            _ignoreAddressBitsMask = _memoryService.SignExtensionMask();

            IDataTarget = (IntPtr)ComInterfaceMarshaller<ICLRDataTargetGenerated>.ConvertToUnmanaged(this);
        }

        public void Dispose()
        {
            if (IDataTarget == IntPtr.Zero)
            {
                return;
            }
            Trace.TraceInformation("DataTargetWrapper.Dispose");
            ComInterfaceMarshaller<ICLRDataTargetGenerated>.Free((void*)IDataTarget);
            IDataTarget = IntPtr.Zero;
        }

        #region ICLRDataTarget

        int ICLRDataTargetGenerated.GetMachineType(
            out IMAGE_FILE_MACHINE machineType)
        {
            ITarget target = _runtime.Target;
            Debug.Assert(target != null);
            machineType = target.Architecture switch
            {
                Architecture.X64 => IMAGE_FILE_MACHINE.AMD64,
                Architecture.X86 => IMAGE_FILE_MACHINE.I386,
                Architecture.Arm => IMAGE_FILE_MACHINE.ARMNT,
                Architecture.Arm64 => IMAGE_FILE_MACHINE.ARM64,
                (Architecture)6 /* Architecture.LoongArch64 */ => IMAGE_FILE_MACHINE.LOONGARCH64,
                (Architecture)9 /* Architecture.RiscV64 */ => IMAGE_FILE_MACHINE.RISCV64,
                _ => IMAGE_FILE_MACHINE.UNKNOWN,
            };
            return HResult.S_OK;
        }

        int ICLRDataTargetGenerated.GetPointerSize(
            out uint pointerSize)
        {
            pointerSize = (uint)_memoryService.PointerSize;
            return HResult.S_OK;
        }

        int ICLRDataTargetGenerated.GetImageBase(
            string imagePath,
            out ulong baseAddress)
        {
            IModule module = _moduleService.GetModuleFromModuleName(imagePath).FirstOrDefault();
            if (module != null)
            {
                baseAddress = module.ImageBase;
                return HResult.S_OK;
            }
            baseAddress = 0;
            return HResult.E_FAIL;
        }

        int ICLRDataTargetGenerated.ReadVirtual(
            ulong address,
            byte* buffer,
            uint bytesRequested,
            uint* pbytesRead)
        {
            Debug.Assert(address != MagicCallbackConstant);
            int read = 0;
            if (bytesRequested > 0)
            {
                address &= _ignoreAddressBitsMask;
                if (!_memoryService.ReadMemory(address, new Span<byte>(buffer, unchecked((int)bytesRequested)), out read))
                {
                    Trace.TraceError("DataTargetWrapper.ReadVirtual FAILED address {0:X16} size {1:X8}", address, bytesRequested);
                    SOSHost.Write(pbytesRead);
                    return HResult.E_FAIL;
                }
            }
            SOSHost.Write(pbytesRead, (uint)read);
            return HResult.S_OK;
        }

        int ICLRDataTargetGenerated.WriteVirtual(
            ulong address,
            byte* buffer,
            uint bytesRequested,
            uint* bytesWritten)
        {
            address &= _ignoreAddressBitsMask;
            if (!_memoryService.WriteMemory(address, new Span<byte>(buffer, unchecked((int)bytesRequested)), out int written))
            {
                SOSHost.Write(bytesWritten);
                return HResult.E_FAIL;
            }
            SOSHost.Write(bytesWritten, (uint)written);
            return HResult.S_OK;
        }

        int ICLRDataTargetGenerated.GetTLSValue(
            uint threadId,
            uint index,
            ulong* value)
        {
            return HResult.E_NOTIMPL;
        }

        int ICLRDataTargetGenerated.SetTLSValue(
            uint threadId,
            uint index,
            ulong value)
        {
            return HResult.E_NOTIMPL;
        }

        int ICLRDataTargetGenerated.GetCurrentThreadID(
            out uint threadId)
        {
            uint? id = _contextService.GetCurrentThread()?.ThreadId;
            if (id.HasValue)
            {
                threadId = id.Value;
                return HResult.S_OK;
            }
            threadId = 0;
            return HResult.E_FAIL;
        }

        int ICLRDataTargetGenerated.GetThreadContext(
            uint threadId,
            uint contextFlags,
            uint contextSize,
            byte* context)
        {
            if (contextSize > int.MaxValue)
            {
                Trace.TraceError($"DataTargetWrapper.GetThreadContext: invalid context size {contextSize}");
                return HResult.E_INVALIDARG;
            }
            try
            {
                _threadService.GetThreadFromId(threadId).GetThreadContext(new Span<byte>(context, (int)contextSize));
            }
            catch (Exception ex) when (ex is DiagnosticsException or ArgumentOutOfRangeException)
            {
                Trace.TraceError($"DataTargetWrapper.GetThreadContext({threadId:X8}) FAILED");
                return HResult.E_INVALIDARG;
            }
            return HResult.S_OK;
        }

        int ICLRDataTargetGenerated.SetThreadContext(
            uint threadId,
            uint contextSize,
            byte* context)
        {
            return HResult.E_NOTIMPL;
        }

        int ICLRDataTargetGenerated.Request(
            uint reqCode,
            uint inBufferSize,
            byte* inBuffer,
            uint outBufferSize,
            byte* outBuffer)
        {
            return HResult.E_NOTIMPL;
        }

        #endregion

        #region ICLRDataTarget2

        int ICLRDataTarget2Generated.AllocVirtual(
            ulong address,
            uint size,
            uint typeFlags,
            uint protectFlags,
            ulong* buffer)
        {
            if (_remoteMemoryService == null)
            {
                return HResult.E_NOTIMPL;
            }
            if (!_remoteMemoryService.AllocateMemory(address, size, typeFlags, protectFlags, out ulong remoteAddress))
            {
                return HResult.E_FAIL;
            }
            SOSHost.Write(buffer, remoteAddress);
            return HResult.S_OK;
        }

        int ICLRDataTarget2Generated.FreeVirtual(
            ulong address,
            uint size,
            uint typeFlags)
        {
            if (_remoteMemoryService == null)
            {
                return HResult.E_NOTIMPL;
            }
            if (!_remoteMemoryService.FreeMemory(address, size, typeFlags))
            {
                return HResult.E_FAIL;
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
                Trace.TraceError($"DataTargetWrapper.VirtualUnwind: invalid context size {contextSize}");
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

        #region ICLRMetadataLocator

        int ICLRMetadataLocatorGenerated.GetMetadata(
            string fileName,
            uint imageTimestamp,
            uint imageSize,
            Guid* mvid,
            uint mdRva,
            uint flags,
            uint bufferSize,
            byte* buffer,
            uint* dataSize)
        {
            byte[] mvidBytes = mvid == null ? null : new ReadOnlySpan<byte>(mvid, sizeof(Guid)).ToArray();
            return _symbolService.GetMetadataLocator(fileName, imageTimestamp, imageSize, mvidBytes, mdRva, flags, bufferSize, (IntPtr)buffer, (IntPtr)dataSize);
        }

        #endregion

        #region ICLRRuntimeLocator

        int ICLRRuntimeLocatorGenerated.GetRuntimeBase(
            out ulong address)
        {
            address = _runtime.RuntimeModule.ImageBase;
            return HResult.S_OK;
        }

        #endregion

        #region ICLRContractLocator

        int ICLRContractLocatorGenerated.GetContractDescriptor(
            out ulong address)
        {
            address = 0;
            ClrInfo clrInfo = _runtime.Services.GetService<ClrInfo>();
            if (clrInfo is null)
            {
                return HResult.E_FAIL;
            }
            address = clrInfo.ContractDescriptorAddress;
            if (address == 0)
            {
                return HResult.E_FAIL;
            }
            return HResult.S_OK;
        }

        #endregion

        #region ICLRSymbolProvider

        int ICLRSymbolProviderGenerated.TryGetSymbolName(
            ulong address,
            uint cchName,
            char* pName,
            uint* pcchNameActual,
            ulong* pDisplacement)
        {
            if (cchName > int.MaxValue)
            {
                return HResult.E_INVALIDARG;
            }

            address &= _ignoreAddressBitsMask;

            try
            {
                if (_symbolProvider is null)
                {
                    return HResult.E_NOTIMPL;
                }

                if (!_symbolProvider.TryGetSymbolName(address, out string symbolName, out ulong displacement)
                    || string.IsNullOrEmpty(symbolName))
                {
                    return HResult.E_FAIL;
                }

                if (pcchNameActual != null)
                {
                    *pcchNameActual = (uint)symbolName.Length + 1;
                }
                if (pDisplacement != null)
                {
                    *pDisplacement = displacement;
                }

                if (cchName == 0 || pName == null)
                {
                    return HResult.S_OK;
                }

                int copy = Math.Min(symbolName.Length, (int)cchName - 1);
                for (int i = 0; i < copy; i++)
                {
                    pName[i] = symbolName[i];
                }
                pName[copy] = '\0';
                return copy < symbolName.Length ? HResult.S_FALSE : HResult.S_OK;
            }
            catch
            {
                return HResult.E_FAIL;
            }
        }

        int ICLRSymbolProviderGenerated.TryGetSymbolAddress(
            ulong moduleBase,
            string name,
            ulong* pAddress)
        {
            if (pAddress == null)
            {
                return HResult.E_INVALIDARG;
            }
            *pAddress = 0;

            moduleBase &= _ignoreAddressBitsMask;

            try
            {
                if (_symbolProvider is null)
                {
                    return HResult.E_NOTIMPL;
                }

                if (string.IsNullOrEmpty(name))
                {
                    return HResult.E_INVALIDARG;
                }

                // Bare symbol names only — '!' is reserved as the SOS module
                // separator and is not produced by any of the mangling toolchains
                // we target.
                if (name.IndexOf('!') >= 0)
                {
                    return HResult.E_INVALIDARG;
                }

                if (_symbolProvider.TryGetSymbolAddress(moduleBase, name, out ulong address) && address != 0)
                {
                    *pAddress = address;
                    return HResult.S_OK;
                }
                return HResult.E_FAIL;
            }
            catch
            {
                return HResult.E_FAIL;
            }
        }

        int ICLRSymbolProviderGenerated.TryGetFieldOffset(
            ulong moduleBase,
            string typeName,
            string fieldName,
            uint* pOffset)
        {
            if (pOffset == null)
            {
                return HResult.E_INVALIDARG;
            }
            *pOffset = 0;

            moduleBase &= _ignoreAddressBitsMask;

            try
            {
                if (_symbolProvider is null)
                {
                    return HResult.E_NOTIMPL;
                }

                if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(fieldName))
                {
                    return HResult.E_INVALIDARG;
                }

                if (_symbolProvider.TryGetFieldOffset(moduleBase, typeName, fieldName, out uint offset))
                {
                    *pOffset = offset;
                    return HResult.S_OK;
                }
                return HResult.E_FAIL;
            }
            catch
            {
                return HResult.E_FAIL;
            }
        }

        #endregion
    }
}
