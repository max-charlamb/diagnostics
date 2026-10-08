// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("AA8FA804-BC05-4642-B2C5-C353ED22FC63")]
    internal unsafe partial interface ICLRMetadataLocatorGenerated
    {
        [PreserveSig]
        int GetMetadata([MarshalAs(UnmanagedType.LPWStr)] string imagePath, uint imageTimestamp, uint imageSize,
            Guid* mvid, uint metadataRva, uint flags, uint bufferSize, byte* buffer, uint* dataSize);
    }
}
