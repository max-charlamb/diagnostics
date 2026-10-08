// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("7CEF8BA9-2EF7-42BF-973F-4171474F87D9")]
    internal unsafe partial interface ICorDebugMetaDataLocatorGenerated
    {
        [PreserveSig]
        int GetMetaData([MarshalAs(UnmanagedType.LPWStr)] string imagePath, uint imageTimestamp, uint imageSize,
            uint pathBufferSize, uint* pathSize, char* pathBuffer);
    }
}
