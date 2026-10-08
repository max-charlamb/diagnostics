// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop.DbgEng
{
    [GeneratedComInterface]
    [Guid("3A707211-AFDD-4495-AD4F-56FECDF8163F")]
    internal unsafe partial interface IDebugSymbols2Generated : IDebugSymbolsGenerated
    {
        [PreserveSig]
        int GetModuleVersionInformation(uint index, ulong @base, [MarshalAs(UnmanagedType.LPStr)] string item, byte* buffer, uint bufferSize,
            uint* verInfoSize);

        [PreserveSig]
        int GetModuleNameString(DEBUG_MODNAME which, uint index, ulong @base, byte* buffer, uint bufferSize, uint* nameSize);

        [PreserveSig]
        int GetConstantName(ulong module, uint typeId, ulong value, byte* buffer, uint bufferSize, uint* nameSize);

        [PreserveSig]
        int GetFieldName(ulong module, uint typeId, uint fieldIndex, byte* buffer, uint bufferSize, uint* nameSize);

        [PreserveSig]
        int GetTypeOptions(DEBUG_TYPEOPTS* options);

        [PreserveSig]
        int AddTypeOptions(DEBUG_TYPEOPTS options);

        [PreserveSig]
        int RemoveTypeOptions(DEBUG_TYPEOPTS options);

        [PreserveSig]
        int SetTypeOptions(DEBUG_TYPEOPTS options);
    }
}
