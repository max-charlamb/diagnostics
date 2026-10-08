// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("B4640016-6CA0-468E-BA2C-1FFF28DE7B72")]
    internal unsafe partial interface ITargetGenerated
    {
        // Must be the same as ITarget::OperatingSystem
        public enum OperatingSystem
        {
            Unknown = 0,
            Windows = 1,
            Linux = 2,
            OSX = 3,
        }

        [PreserveSig]
        OperatingSystem GetOperatingSystem();

        [PreserveSig]
        int GetService(in Guid serviceId, out IntPtr service);

        [PreserveSig]
        int GetRuntime(IntPtr* runtime);

        [PreserveSig]
        void Flush();
    }
}
