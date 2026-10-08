// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities;
using Microsoft.FileFormats.ELF;
using SOS.Hosting.DbgEng.Interop;
using SOS.Hosting.Interop;

namespace SOS.Hosting
{
    [ServiceExport(Scope = ServiceScope.Runtime)]
    [GeneratedComClass]
    public sealed unsafe partial class RuntimeWrapper : IRuntimeGenerated, IDisposable
    {
        /// <summary>
        /// The runtime OS and type. Must match IRuntime::RuntimeConfiguration in runtime.h.
        /// </summary>
        private enum RuntimeConfiguration
        {
            WindowsDesktop = 0,
            WindowsCore = 1,
            UnixCore = 2,
            OSXCore = 3,
            Unknown = 4
        }

        public static Guid IID_IXCLRDataProcess = new("5c552ab6-fc09-4cb3-8e36-22fa03c798b7");
        public static Guid IID_ICorDebugProcess = new("3d6f5f64-7538-11d3-8d5b-00104b35e7ef");

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int DllMainDelegate(
            IntPtr instance,
            int reason,
            IntPtr reserved);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int CLRDataCreateInstanceDelegate(
            in Guid riid,
            IntPtr dacDataInterface,
            out IntPtr ppObj);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int OpenVirtualProcessImpl2Delegate(
            ulong clrInstanceId,
            IntPtr dataTarget,
            [MarshalAs(UnmanagedType.LPWStr)] string dacModulePath,
            ref ClrDebuggingVersion maxDebuggerSupportedVersion,
            ref Guid riid,
            out IntPtr instance,
            out ClrDebuggingProcessFlags flags);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int OpenVirtualProcessImplDelegate(
            ulong clrInstanceId,
            IntPtr dataTarget,
            IntPtr dacHandle,
            ref ClrDebuggingVersion maxDebuggerSupportedVersion,
            ref Guid riid,
            out IntPtr instance,
            out ClrDebuggingProcessFlags flags);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int OpenVirtualProcessDelegate(
            ulong clrInstanceId,
            IntPtr dataTarget,
            IntPtr dacHandle,
            ref Guid riid,
            out IntPtr instance,
            out ClrDebuggingProcessFlags flags);

        private readonly IServiceProvider _services;
        private readonly IRuntime _runtime;
        private IntPtr _clrDataProcess = IntPtr.Zero;
        private IntPtr _corDebugProcess = IntPtr.Zero;
        private IntPtr _dacHandle = IntPtr.Zero;
        private IntPtr _dbiHandle = IntPtr.Zero;
        private bool _disposed;

        internal bool IsDisposed => _disposed;

        // GetRuntime's existing ABI returns a borrowed pointer, not an AddRef-owned interface.
        public IntPtr IRuntime { get; private set; }

        public RuntimeWrapper(IServiceProvider services, IRuntime runtime)
        {
            Debug.Assert(services != null);
            Debug.Assert(runtime != null);
            _services = services;
            _runtime = runtime;
            IRuntime = (IntPtr)ComInterfaceMarshaller<IRuntimeGenerated>.ConvertToUnmanaged(this);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            Trace.TraceInformation("RuntimeWrapper.Dispose");
            _disposed = true;
            if (_corDebugProcess != IntPtr.Zero)
            {
                Marshal.Release(_corDebugProcess);
                _corDebugProcess = IntPtr.Zero;
            }
            if (_clrDataProcess != IntPtr.Zero)
            {
                Marshal.Release(_clrDataProcess);
                _clrDataProcess = IntPtr.Zero;
            }
            if (_dacHandle != IntPtr.Zero)
            {
                // Previously, the DAC was freed here, but as we transition to the cDAC which uses NativeAOT,
                // it is no longer possible to free the DAC library when it is using the shimmed cDAC.
                _dacHandle = IntPtr.Zero;
            }
            if (_dbiHandle != IntPtr.Zero)
            {
                DataTarget.PlatformFunctions.FreeLibrary(_dbiHandle);
                _dbiHandle = IntPtr.Zero;
            }
            ComInterfaceMarshaller<IRuntimeGenerated>.Free((void*)IRuntime);
            IRuntime = IntPtr.Zero;
        }

        int IRuntimeGenerated.GetRuntimeConfiguration()
        {
            switch (_runtime.RuntimeType)
            {
                case RuntimeType.Desktop:
                    return (int)RuntimeConfiguration.WindowsDesktop;

                case RuntimeType.NetCore:
                case RuntimeType.SingleFile:
                    if (_runtime.Target.OperatingSystem == OSPlatform.Windows)
                    {
                        return (int)RuntimeConfiguration.WindowsCore;
                    }
                    else if (_runtime.Target.OperatingSystem == OSPlatform.Linux || _runtime.Target.OperatingSystem == OSPlatform.OSX)
                    {
                        return (int)RuntimeConfiguration.UnixCore;
                    }
                    break;
            }
            return (int)RuntimeConfiguration.Unknown;
        }

        ulong IRuntimeGenerated.GetModuleAddress()
        {
            return _runtime.RuntimeModule.ImageBase;
        }

        ulong IRuntimeGenerated.GetModuleSize()
        {
            return _runtime.RuntimeModule.ImageSize;
        }

        void IRuntimeGenerated.SetRuntimeDirectory(string runtimeModuleDirectory)
        {
            _runtime.RuntimeModuleDirectory = runtimeModuleDirectory;
        }

        string IRuntimeGenerated.GetRuntimeDirectory()
        {
            return _runtime.RuntimeModuleDirectory ?? Path.GetDirectoryName(_runtime.RuntimeModule.FileName);
        }

        int IRuntimeGenerated.GetClrDataProcess(
            CDacLoadPolicy policy,
            IntPtr* ppClrDataProcess)
        {
            if (ppClrDataProcess == null)
            {
                return HResult.E_INVALIDARG;
            }
            *ppClrDataProcess = IntPtr.Zero;
            bool cdacOnly = policy == CDacLoadPolicy.OnlyUseCDac;

            int cdacActivationResult = HResult.E_NOINTERFACE;
            if (policy != CDacLoadPolicy.UseLegacyDac)
            {
                try
                {
                    Trace.TraceInformation($"Runtime #{_runtime.Id} native data-access: requesting an IXCLRDataProcess (cDAC preferred)");
                    cdacActivationResult = _runtime.GetClrDataProcessFromCDac(out IntPtr cdacDataProcess);
                    *ppClrDataProcess = cdacDataProcess;
                    if (cdacActivationResult >= 0 && cdacDataProcess != IntPtr.Zero)
                    {
                        Trace.TraceInformation($"Runtime #{_runtime.Id} native data-access: received an IXCLRDataProcess");
                    }
                    else
                    {
                        Trace.TraceInformation(cdacOnly
                            ? $"Runtime #{_runtime.Id} native data-access: no IXCLRDataProcess was created under forced cDAC policy"
                            : $"Runtime #{_runtime.Id} native data-access: no IXCLRDataProcess was created; falling back to the in-box DAC");
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                    cdacActivationResult = ex.HResult;
                }
            }
            if (*ppClrDataProcess == IntPtr.Zero && cdacOnly)
            {
                Trace.TraceError($"Runtime #{_runtime.Id} native data-access: cDAC was forced but could not service this runtime; not falling back to the DAC");
                return cdacActivationResult;
            }
            if (*ppClrDataProcess == IntPtr.Zero)
            {
                if (_clrDataProcess == IntPtr.Zero)
                {
                    try
                    {
                        Trace.TraceInformation($"Runtime #{_runtime.Id} native data-access: creating IXCLRDataProcess from the in-box DAC");
                        _clrDataProcess = CreateClrDataProcessFromDac(GetDacHandle());
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }
                }
                *ppClrDataProcess = _clrDataProcess;
            }
            if (*ppClrDataProcess == IntPtr.Zero)
            {
                return HResult.E_NOINTERFACE;
            }
            return HResult.S_OK;
        }

        CDacLoadPolicy IRuntimeGenerated.GetCDacLoadPolicy()
        {
            return _services.GetService<ISettingsService>()?.CDacLoadPolicy ?? CDacLoadPolicy.PreferCDac;
        }

        int IRuntimeGenerated.GetCorDebugInterface(
            IntPtr* ppCorDebugProcess)
        {
            if (ppCorDebugProcess == null)
            {
                return HResult.E_INVALIDARG;
            }
            int result = HResult.S_OK;
            if (_corDebugProcess == IntPtr.Zero)
            {
                try
                {
                    result = CreateCorDebugProcess(out _corDebugProcess);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                    result = ex.HResult;
                }
            }
            *ppCorDebugProcess = _corDebugProcess;
            if (*ppCorDebugProcess == IntPtr.Zero)
            {
                return result < 0 ? result : HResult.E_NOINTERFACE;
            }
            return HResult.S_OK;
        }

        int IRuntimeGenerated.GetEEVersion(
            VS_FIXEDFILEINFO* pFileInfo,
            byte* fileVersionBuffer,
            int fileVersionBufferSizeInBytes)
        {
            if (pFileInfo == null)
            {
                return HResult.E_INVALIDARG;
            }
            pFileInfo->dwSignature = 0;
            pFileInfo->dwStrucVersion = 0;
            pFileInfo->dwFileFlagsMask = 0;
            pFileInfo->dwFileFlags = 0;
            pFileInfo->dwFileVersionMS = 0;
            pFileInfo->dwFileVersionLS = 0;

            Version version = _runtime.RuntimeVersion;
            if (version is not null)
            {
                pFileInfo->dwFileVersionMS = (uint)version.Minor & 0xffff | (uint)version.Major << 16;
                pFileInfo->dwFileVersionLS = (uint)version.Revision & 0xffff | (uint)version.Build << 16;
            }

            // Attempt to get the FileVersion string that contains version and the "built by" and commit id info
            if (fileVersionBuffer != null)
            {
                if (fileVersionBufferSizeInBytes > 0)
                {
                    *fileVersionBuffer = 0;
                }
                string versionString = _runtime.RuntimeModule.GetVersionString();
                if (versionString != null)
                {
                    try
                    {
                        byte[] source = Encoding.ASCII.GetBytes(versionString + '\0');
                        Marshal.Copy(source, 0, new IntPtr(fileVersionBuffer), Math.Min(source.Length, fileVersionBufferSizeInBytes));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                    }
                }
            }
            return HResult.S_OK;
        }

        private IntPtr CreateClrDataProcessFromDac(IntPtr dacHandle)
        {
            if (dacHandle == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            CLRDataCreateInstanceDelegate createInstance = SOSHost.GetDelegateFunction<CLRDataCreateInstanceDelegate>(dacHandle, "CLRDataCreateInstance");
            if (createInstance == null)
            {
                Trace.TraceError("Failed to obtain DAC CLRDataCreateInstance");
                return IntPtr.Zero;
            }
            DataTargetWrapper dataTarget = new(_services, _runtime);
            try
            {
                int hr = createInstance(IID_IXCLRDataProcess, dataTarget.IDataTarget, out IntPtr unk);
                if (hr != 0)
                {
                    Trace.TraceError($"CLRDataCreateInstance FAILED {hr:X8}");
                    return IntPtr.Zero;
                }
                return unk;
            }
            finally
            {
                dataTarget.Dispose();
            }
        }

        private int CreateCorDebugProcess(out IntPtr corDebugProcess)
        {
            corDebugProcess = IntPtr.Zero;
            CDacLoadPolicy policy =
                _services.GetService<ISettingsService>()?.CDacLoadPolicy ?? CDacLoadPolicy.PreferCDac;
            if (_runtime.RuntimeType == RuntimeType.Desktop)
            {
                return policy == CDacLoadPolicy.OnlyUseCDac
                    ? HResult.E_NOINTERFACE
                    : CreateDesktopCorDebugProcess(out corDebugProcess);
            }

            using RuntimeLibraryProvider libraryProvider = new(
                _runtime.GetDbiFilePath,
                () => _runtime.GetDacFilePath(out _),
                _services.GetService<ISettingsService>()?.DacSignatureVerificationEnabled ?? true);

            IClrDataProcessActivator activator = _services.GetService<IClrDataProcessActivator>();
            if (activator is null)
            {
                return HResult.E_NOINTERFACE;
            }

            return activator.CreateCorDebugProcess(
                _runtime,
                libraryProvider.ILibraryProvider,
                policy,
                out corDebugProcess);
        }

        private int CreateDesktopCorDebugProcess(out IntPtr corDebugProcess)
        {
            corDebugProcess = IntPtr.Zero;
            string dacFilePath = _runtime.GetDacFilePath(out bool verifySignature);
            string dbiFilePath = _runtime.GetDbiFilePath();
            if (string.IsNullOrEmpty(dacFilePath) || string.IsNullOrEmpty(dbiFilePath))
            {
                Trace.TraceError($"Could not find matching Desktop DAC or DBI for this runtime: {_runtime.RuntimeModule.FileName}");
                return HResult.E_NOINTERFACE;
            }

            IntPtr dacHandle = GetDacHandle();
            if (dacHandle == IntPtr.Zero)
            {
                return HResult.E_NOINTERFACE;
            }
            if (_dbiHandle == IntPtr.Zero)
            {
                _dbiHandle = LoadLibraryWithSignatureVerification(dbiFilePath, verifySignature);
                if (_dbiHandle == IntPtr.Zero)
                {
                    return HResult.E_NOINTERFACE;
                }
            }

            ClrDebuggingVersion maxDebuggerSupportedVersion = new()
            {
                StructVersion = 0,
                Major = 4,
                Minor = 0,
                Build = 0,
                Revision = 0,
            };
            CorDebugDataTargetWrapper dataTarget = new(_services, _runtime);
            ulong clrInstanceId = _runtime.RuntimeModule.ImageBase;
            Guid iid = IID_ICorDebugProcess;
            try
            {
                OpenVirtualProcessImpl2Delegate openVirtualProcessImpl2 =
                    SOSHost.GetDelegateFunction<OpenVirtualProcessImpl2Delegate>(_dbiHandle, "OpenVirtualProcessImpl2");
                if (openVirtualProcessImpl2 is not null)
                {
                    int hr = openVirtualProcessImpl2(
                        clrInstanceId,
                        dataTarget.ICorDebugDataTarget,
                        dacFilePath,
                        ref maxDebuggerSupportedVersion,
                        ref iid,
                        out IntPtr process,
                        out _);
                    return CompleteDesktopCorDebugActivation(hr, process, out corDebugProcess);
                }

                OpenVirtualProcessImplDelegate openVirtualProcessImpl =
                    SOSHost.GetDelegateFunction<OpenVirtualProcessImplDelegate>(_dbiHandle, "OpenVirtualProcessImpl");
                if (openVirtualProcessImpl is not null)
                {
                    int hr = openVirtualProcessImpl(
                        clrInstanceId,
                        dataTarget.ICorDebugDataTarget,
                        dacHandle,
                        ref maxDebuggerSupportedVersion,
                        ref iid,
                        out IntPtr process,
                        out _);
                    return CompleteDesktopCorDebugActivation(hr, process, out corDebugProcess);
                }

                OpenVirtualProcessDelegate openVirtualProcess =
                    SOSHost.GetDelegateFunction<OpenVirtualProcessDelegate>(_dbiHandle, "OpenVirtualProcess");
                if (openVirtualProcess is not null)
                {
                    int hr = openVirtualProcess(
                        clrInstanceId,
                        dataTarget.ICorDebugDataTarget,
                        dacHandle,
                        ref iid,
                        out IntPtr process,
                        out _);
                    return CompleteDesktopCorDebugActivation(hr, process, out corDebugProcess);
                }
                Trace.TraceError("Desktop DBI OpenVirtualProcess export not found");
                return HResult.E_NOINTERFACE;
            }
            finally
            {
                dataTarget.Dispose();
            }
        }

        private static int CompleteDesktopCorDebugActivation(
            int hr,
            IntPtr process,
            out IntPtr corDebugProcess)
        {
            corDebugProcess = IntPtr.Zero;
            if (hr < 0 || process == IntPtr.Zero)
            {
                if (process != IntPtr.Zero)
                {
                    Marshal.Release(process);
                }
                return hr < 0 ? hr : HResult.E_NOINTERFACE;
            }
            corDebugProcess = process;
            return HResult.S_OK;
        }

        private IntPtr GetDacHandle()
        {
            if (_dacHandle == IntPtr.Zero)
            {
                string dacFilePath = _runtime.GetDacFilePath(out bool verifySignature);
                if (dacFilePath == null)
                {
                    Trace.TraceError($"Could not find matching DAC for this runtime: {_runtime.RuntimeModule.FileName}");
                    return IntPtr.Zero;
                }
                _dacHandle = LoadDacLibrary(dacFilePath, verifySignature);
            }
            return _dacHandle;
        }

        private static IntPtr LoadDacLibrary(string dacFilePath, bool verifySignature)
        {
            IntPtr dacHandle = LoadLibraryWithSignatureVerification(dacFilePath, verifySignature);
            if (dacHandle == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                DllMainDelegate dllmain = SOSHost.GetDelegateFunction<DllMainDelegate>(dacHandle, "DllMain");
                dllmain?.Invoke(dacHandle, 1, IntPtr.Zero);
            }
            return dacHandle;
        }

        internal static IntPtr LoadLibraryWithSignatureVerification(string libraryPath, bool verifySignature)
        {
            IntPtr libraryHandle = IntPtr.Zero;
            IDisposable fileLock = null;
            try
            {
                if (verifySignature)
                {
                    Trace.TraceInformation($"Verifying library signing and cert {libraryPath}");

                    if (!AuthenticodeUtil.VerifyDacDll(libraryPath, out fileLock))
                    {
                        return IntPtr.Zero;
                    }
                }
                try
                {
                    libraryHandle = DataTarget.PlatformFunctions.LoadLibrary(libraryPath);
                }
                catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException)
                {
                    Trace.TraceError($"LoadLibrary({libraryPath}) FAILED {ex}");
                    return IntPtr.Zero;
                }
            }
            finally
            {
                // Keep the verified file locked until it is loaded.
                fileLock?.Dispose();
            }
            Debug.Assert(libraryHandle != IntPtr.Zero);
            return libraryHandle;
        }
    }
}
