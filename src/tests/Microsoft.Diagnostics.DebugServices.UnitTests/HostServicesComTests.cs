// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SOS.Hosting.Interop;
using Xunit;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe partial class HostServicesComTests
    {
        private const int S_OK = 0;

        [Fact]
        public void GeneratedInterfacePreservesAbi()
        {
            TestHostServices services = new();
            void* hostServices = ComInterfaceMarshaller<IHostServicesGenerated>.ConvertToUnmanaged(services);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)hostServices);

            IntPtr* vtable = *(IntPtr**)hostServices;
            for (int slot = 0; slot < 11; slot++)
            {
                Assert.NotEqual(IntPtr.Zero, vtable[slot]);
            }

            Guid iid = typeof(IHostServicesGenerated).GUID;
            IntPtr queried = IntPtr.Zero;
            delegate* unmanaged<IntPtr, Guid*, IntPtr*, int> queryInterface =
                (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
            Assert.Equal(S_OK, queryInterface((IntPtr)hostServices, &iid, &queried));
            Assert.Equal((IntPtr)hostServices, queried);

            delegate* unmanaged<IntPtr, IntPtr*, int> getHost =
                (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[3];
            IntPtr host = IntPtr.Zero;
            Assert.Equal(S_OK, getHost((IntPtr)hostServices, &host));
            try
            {
                Assert.NotEqual(IntPtr.Zero, host);
                IntPtr* hostVtable = *(IntPtr**)host;
                delegate* unmanaged<IntPtr, HostType> getHostType = (delegate* unmanaged<IntPtr, HostType>)hostVtable[3];
                Assert.Equal(HostType.DbgEng, getHostType(host));
            }
            finally
            {
                IntPtr* hostVtable = *(IntPtr**)host;
                ((delegate* unmanaged<IntPtr, uint>)hostVtable[2])(host);
            }

            delegate* unmanaged<IntPtr, uint, int> updateTarget =
                (delegate* unmanaged<IntPtr, uint, int>)vtable[6];
            Assert.Equal(S_OK, updateTarget((IntPtr)hostServices, 42));
            Assert.Equal((uint)42, services.ProcessId);

            delegate* unmanaged<IntPtr, void> flushTarget =
                (delegate* unmanaged<IntPtr, void>)vtable[7];
            flushTarget((IntPtr)hostServices);
            Assert.True(services.Flushed);

            delegate* unmanaged<IntPtr, void> destroyTarget =
                (delegate* unmanaged<IntPtr, void>)vtable[8];
            destroyTarget((IntPtr)hostServices);
            Assert.True(services.Destroyed);

            delegate* unmanaged<IntPtr, uint> release =
                (delegate* unmanaged<IntPtr, uint>)vtable[2];
            release(queried);
            ComInterfaceMarshaller<IHostServicesGenerated>.Free(hostServices);
        }

        [GeneratedComClass]
        internal sealed partial class TestHostServices : IHostServicesGenerated
        {
            private readonly TestHost _host = new();

            internal uint ProcessId { get; private set; }

            internal bool Flushed { get; private set; }

            internal bool Destroyed { get; private set; }

            public int GetHost(out IHostGenerated host)
            {
                host = _host;
                return S_OK;
            }

            public int RegisterDebuggerServices(IntPtr debuggerServices) => S_OK;

            public int CreateTarget() => S_OK;

            public int UpdateTarget(uint processId)
            {
                ProcessId = processId;
                return S_OK;
            }

            public void FlushTarget() => Flushed = true;

            public void DestroyTarget() => Destroyed = true;

            public int DispatchCommand(string commandName, string arguments, bool displayCommandNotFound) => S_OK;

            public void Uninitialize()
            {
            }
        }

        [GeneratedComClass]
        private sealed partial class TestHost : IHostGenerated
        {
            public HostType GetHostType() => HostType.DbgEng;

            public int GetService(in Guid serviceId, out IntPtr service)
            {
                service = IntPtr.Zero;
                return unchecked((int)0x80004002);
            }

            public int GetCurrentTarget(out ITargetGenerated target)
            {
                target = null;
                return unchecked((int)0x80004002);
            }
        }
    }
}
