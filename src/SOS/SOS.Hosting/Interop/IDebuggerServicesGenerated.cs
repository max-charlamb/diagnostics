// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.DbgEng.Interop;

namespace SOS.Hosting.Interop
{
    [GeneratedComInterface]
    [Guid("B4640016-6CA0-468E-BA2C-1FFF28DE7B72")]
    internal unsafe partial interface IDebuggerServicesGenerated
    {
        public enum OperatingSystem
        {
            Unknown = 0,
            Windows = 1,
            Linux = 2,
            OSX = 3,
        }

        [PreserveSig]
        int GetOperatingSystem(out OperatingSystem operatingSystem);

        [PreserveSig]
        int GetDebuggeeType(out DEBUG_CLASS debugClass, out DEBUG_CLASS_QUALIFIER qualifier);

        [PreserveSig]
        int GetProcessorType(out IMAGE_FILE_MACHINE type);

        [PreserveSig]
        int AddCommand(byte* command, byte* help, IntPtr* aliases, int numberOfAliases);

        [PreserveSig]
        void OutputString(DEBUG_OUTPUT mask, byte* message);

        [PreserveSig]
        int ReadVirtual(ulong offset, byte* buffer, uint bufferSize, out int bytesRead);

        [PreserveSig]
        int WriteVirtual(ulong offset, byte* buffer, uint bufferSize, out int bytesWritten);

        [PreserveSig]
        int GetNumberModules(out uint loaded, out uint unloaded);

        [PreserveSig]
        int GetModuleByIndex(uint index, out ulong moduleBase);

        [PreserveSig]
        int GetModuleNames(uint index, ulong moduleBase, byte* imageNameBuffer, uint imageNameBufferSize, out uint imageNameSize,
            byte* moduleNameBuffer, uint moduleNameBufferSize, uint* moduleNameSize,
            byte* loadedImageNameBuffer, uint loadedImageNameBufferSize, uint* loadedImageNameSize);

        [PreserveSig]
        int GetModuleInfo(uint index, out ulong moduleBase, out ulong moduleSize, out uint timestamp, out uint checksum);

        [PreserveSig]
        int GetModuleVersionInformation(uint index, ulong moduleBase, byte* item, byte* buffer, uint bufferSize, uint* versionInfoSize);

        [PreserveSig]
        int GetModuleByModuleName(byte* name, uint startIndex, out uint index, out ulong moduleBase);

        [PreserveSig]
        int GetNumberThreads(out uint number);

        [PreserveSig]
        int GetThreadIdsByIndex(uint start, uint count, uint* ids, uint* sysIds);

        [PreserveSig]
        int GetThreadContextBySystemId(uint sysId, uint contextFlags, uint contextSize, byte* context);

        [PreserveSig]
        int GetCurrentProcessSystemId(out uint sysId);

        [PreserveSig]
        int GetCurrentThreadSystemId(out uint sysId);

        [PreserveSig]
        int SetCurrentThreadSystemId(uint sysId);

        [PreserveSig]
        int GetThreadTeb(uint sysId, ref ulong teb);

        [PreserveSig]
        int VirtualUnwind(uint threadId, uint contextSize, byte* context);

        [PreserveSig]
        int GetSymbolPath(byte* buffer, uint bufferSize, out uint pathSize);

        [PreserveSig]
        int GetSymbolByOffset(uint moduleIndex, ulong offset, byte* nameBuffer, uint nameBufferSize, out uint nameSize, out ulong displacement);

        [PreserveSig]
        int GetOffsetBySymbol(uint moduleIndex, byte* name, out ulong offset);

        [PreserveSig]
        int GetTypeId(uint moduleIndex, byte* typeName, out ulong typeId);

        [PreserveSig]
        int GetFieldOffset(uint moduleIndex, byte* typeName, ulong typeId, byte* fieldName, out uint offset);

        [PreserveSig]
        uint GetOutputWidth();

        [PreserveSig]
        int SupportsDml(uint* supported);

        [PreserveSig]
        void OutputDmlString(DEBUG_OUTPUT mask, byte* message);

        [PreserveSig]
        int AddModuleSymbol(IntPtr param, byte* symbolFileName);

        [PreserveSig]
        int GetLastEventInformation(out uint type, out uint processId, out int threadId, void* extraInformation,
            uint extraInformationSize, uint* extraInformationUsed, byte* description, uint descriptionSize, uint* descriptionUsed);

        [PreserveSig]
        void FlushCheck();

        [PreserveSig]
        int ExecuteHostCommand(byte* commandLine, IntPtr callback);

        [PreserveSig]
        int GetDacSignatureVerificationSettings(int* enabled);
    }
}
