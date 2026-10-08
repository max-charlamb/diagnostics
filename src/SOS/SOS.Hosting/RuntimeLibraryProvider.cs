// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.Interop;

namespace SOS.Hosting
{
    [GeneratedComClass]
    internal sealed partial class RuntimeLibraryProvider : ICLRDebuggingLibraryProvider2Generated, IDisposable
    {
        private readonly Func<string> _getDbiPath;
        private readonly Func<string> _getDacPath;
        private readonly bool _verifySignature;
        private readonly List<IDisposable> _verifiedFiles = [];

        public IntPtr ILibraryProvider { get; private set; }

        public RuntimeLibraryProvider(
            Func<string> getDbiPath,
            Func<string> getDacPath,
            bool verifySignature)
        {
            _getDbiPath = getDbiPath ?? throw new ArgumentNullException(nameof(getDbiPath));
            _getDacPath = getDacPath ?? throw new ArgumentNullException(nameof(getDacPath));
            _verifySignature = verifySignature;

            unsafe
            {
                ILibraryProvider = (IntPtr)ComInterfaceMarshaller<ICLRDebuggingLibraryProvider2Generated>.ConvertToUnmanaged(this);
            }
        }

        void IDisposable.Dispose()
        {
            if (ILibraryProvider == IntPtr.Zero)
            {
                return;
            }
            foreach (IDisposable verifiedFile in _verifiedFiles)
            {
                verifiedFile.Dispose();
            }
            _verifiedFiles.Clear();
            unsafe
            {
                ComInterfaceMarshaller<ICLRDebuggingLibraryProvider2Generated>.Free((void*)ILibraryProvider);
            }
            ILibraryProvider = IntPtr.Zero;
        }

        int ICLRDebuggingLibraryProvider2Generated.ProvideLibrary2(
            string fileName,
            uint timeStamp,
            uint sizeOfImage,
            out string modulePath)
        {
            modulePath = null;

            try
            {
                string path = fileName?.IndexOf("mscordbi", StringComparison.OrdinalIgnoreCase) >= 0
                    ? _getDbiPath()
                    : _getDacPath();
                if (string.IsNullOrEmpty(path))
                {
                    Trace.TraceError($"RuntimeLibraryProvider: could not resolve {fileName}");
                    return HResult.E_NOINTERFACE;
                }

                if (_verifySignature && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Trace.TraceInformation($"RuntimeLibraryProvider: verifying Authenticode signature {path}");
                    if (!AuthenticodeUtil.VerifyDacDll(path, out IDisposable fileLock))
                    {
                        fileLock?.Dispose();
                        Trace.TraceError($"RuntimeLibraryProvider: Authenticode verification failed for {path}");
                        return HResult.E_NOINTERFACE;
                    }
                    _verifiedFiles.Add(fileLock);
                }

                modulePath = path;
                Trace.TraceInformation($"RuntimeLibraryProvider: resolved {fileName} to {path}");
                return HResult.S_OK;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"RuntimeLibraryProvider: resolving {fileName} failed: {ex}");
                return HResult.E_NOINTERFACE;
            }
        }

    }
}
