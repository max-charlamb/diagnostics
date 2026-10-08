// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting.Interop;

namespace SOS.Extensions
{
    internal sealed class RemoteMemoryService : IRemoteMemoryService
    {
        private readonly IRemoteMemoryServiceGenerated _services;

        internal RemoteMemoryService(IRemoteMemoryServiceGenerated services)
        {
            _services = services;
        }

        public bool AllocateMemory(ulong address, uint size, uint typeFlags, uint protectFlags, out ulong remoteAddress)
        {
            return _services.AllocVirtual(address, size, typeFlags, protectFlags, out remoteAddress) == HResult.S_OK;
        }

        public bool FreeMemory(ulong address, uint size, uint typeFlags)
        {
            return _services.FreeVirtual(address, size, typeFlags) == HResult.S_OK;
        }
    }
}
