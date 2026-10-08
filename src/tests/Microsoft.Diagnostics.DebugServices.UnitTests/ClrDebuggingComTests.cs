// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Diagnostics.Runtime.Utilities;
using SOS.Hosting;
using Xunit;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe class ClrDebuggingComTests
    {
        [Theory]
        [InlineData(HResult.S_OK, false)]
        [InlineData(HResult.S_FALSE, true)]
        [InlineData(HResult.E_FAIL, false)]
        public void OpenVirtualProcessPreservesAbiAndCallerOwnedProcess(int result, bool libraryAvailable)
        {
            using NativeDebugging native = new() { OpenResult = result };
            native.WithDebugging(debugging =>
            {
                Guid iid = RuntimeWrapper.IID_IXCLRDataProcess;
                ClrDebuggingVersion maximum = new() { Major = 4, Minor = 2, Build = 12345, Revision = 7 };
                ClrDebuggingVersion version = new() { Major = 8 };
                IntPtr library = libraryAvailable ? (IntPtr)0x5678 : IntPtr.Zero;
                int hr = debugging.OpenVirtualProcess(0xFEDCBA9800000000, (IntPtr)0x1234, library,
                    in maximum, in iid, out IntPtr process, ref version, out ClrDebuggingProcessFlags flags);
                Assert.Equal(result, hr);
                Assert.Equal(0xFEDCBA9800000000ul, native.ModuleBase);
                Assert.Equal((IntPtr)0x1234, native.DataTarget);
                Assert.Equal(library, native.LibraryProvider);
                Assert.Equal(iid, native.ProcessId);
                Assert.Equal(maximum, native.MaximumVersion);
                Assert.Equal((short)0, native.InputVersion.StructVersion);
                Assert.Equal((short)8, native.InputVersion.Major);
                Assert.Equal((short)11, version.Major);
                Assert.Equal((short)2, version.Minor);
                Assert.Equal((short)123, version.Build);
                Assert.Equal((short)456, version.Revision);
                Assert.Equal(ClrDebuggingProcessFlags.ManagedDebugEventDebuggerLaunch, flags);
                if (result >= 0)
                {
                    Assert.Equal((IntPtr)native.Process, process);
                    Assert.Equal(2, native.Process->References);
                    Assert.Equal(1, Marshal.Release(process));
                }
                else
                {
                    Assert.Equal(IntPtr.Zero, process);
                    Assert.Equal(1, native.Process->References);
                }
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(DbgShimCDacLoadPolicy.PreferCDac)]
        [InlineData(DbgShimCDacLoadPolicy.CDacOnly)]
        [InlineData(DbgShimCDacLoadPolicy.LegacyDacOnly)]
        public void PolicyCastsShareProxyAndDispatchBothSlots(DbgShimCDacLoadPolicy requested)
        {
            using NativeDebugging native = new();
            native.WithDebugging(debugging =>
            {
                ICLRDebuggingPolicy policy = (ICLRDebuggingPolicy)debugging;
                Assert.Same(debugging, policy);
                Assert.IsType<ComObject>(debugging);
                Assert.False(debugging is IDisposable);
                Assert.Equal(HResult.S_OK, policy.SetCDacLoadPolicy(requested));
                Assert.Equal(HResult.S_OK, policy.GetCDacLoadPolicy(out DbgShimCDacLoadPolicy actual));
                Assert.Equal(requested, actual);
                Assert.Equal(requested, native.Policy);
            });
            native.AssertBaselineReferences();
        }

        [Theory]
        [InlineData(HResult.S_OK)]
        [InlineData(HResult.S_FALSE)]
        [InlineData(HResult.E_FAIL)]
        public void PolicyAndCanUnloadPreserveRawHResults(int result)
        {
            using NativeDebugging native = new() { PolicyResult = result, UnloadResult = result };
            native.WithDebugging(debugging =>
            {
                ICLRDebuggingPolicy policy = (ICLRDebuggingPolicy)debugging;
                Assert.Equal(result, policy.SetCDacLoadPolicy(DbgShimCDacLoadPolicy.CDacOnly));
                Assert.Equal(result, policy.GetCDacLoadPolicy(out DbgShimCDacLoadPolicy actual));
                Assert.Equal(DbgShimCDacLoadPolicy.CDacOnly, actual);
                Assert.Equal(result, debugging.CanUnloadNow((IntPtr)0x76543210));
                Assert.Equal((IntPtr)0x76543210, native.UnloadModule);
            });
            native.AssertBaselineReferences();
        }

        [Fact]
        public void MissingPolicyDoesNotPreventUsingDebuggingInterface()
        {
            using NativeDebugging native = new() { PolicyAvailable = false };
            native.WithDebugging(debugging =>
            {
                Assert.False(debugging is ICLRDebuggingPolicy);
                Assert.Equal(HResult.S_FALSE, debugging.CanUnloadNow(IntPtr.Zero));
            });
            native.AssertBaselineReferences();
        }

        [Fact]
        public void ImportFailureBalancesIncomingOwnedReference()
        {
            using NativeDebugging native = new() { DebuggingAvailable = false };
            Assert.Throws<InvalidCastException>(() => native.CreateDebugging());
            native.AssertBaselineReferences();
        }

        [Fact]
        public void ReimportUsesCachedProxyWithoutInvalidatingEarlierInterface()
        {
            using NativeDebugging native = new();
            native.WithDebugging(first =>
            {
                native.WithDebugging(second => Assert.Same(first, second));
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Assert.Equal(HResult.S_FALSE, first.CanUnloadNow(IntPtr.Zero));
            });
            native.AssertBaselineReferences();
        }

        private sealed class NativeDebugging : IDisposable
        {
            internal struct Instance
            {
                internal IntPtr* Vtable;
                internal int References;
                internal IntPtr Context;
            }

            private static readonly Guid s_unknownId = new("00000000-0000-0000-C000-000000000046");
            private readonly Instance* _debugging;
            private readonly Instance* _policy;
            internal readonly Instance* Process;
            private readonly GCHandle _handle;
            private int _liveInstances = 3;
            internal bool DebuggingAvailable { get; set; } = true;
            internal bool PolicyAvailable { get; set; } = true;
            internal int OpenResult { get; set; } = HResult.S_OK;
            internal int PolicyResult { get; set; } = HResult.S_OK;
            internal int UnloadResult { get; set; } = HResult.S_FALSE;
            internal DbgShimCDacLoadPolicy Policy { get; private set; }
            internal ulong ModuleBase { get; private set; }
            internal IntPtr DataTarget { get; private set; }
            internal IntPtr LibraryProvider { get; private set; }
            internal Guid ProcessId { get; private set; }
            internal ClrDebuggingVersion MaximumVersion { get; private set; }
            internal ClrDebuggingVersion InputVersion { get; private set; }
            internal IntPtr UnloadModule { get; private set; }

            internal NativeDebugging()
            {
                _handle = GCHandle.Alloc(this);
                _debugging = Allocate(5);
                _policy = Allocate(5);
                Process = Allocate(3);
                _debugging->Vtable[3] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, ulong, IntPtr, IntPtr,
                    ClrDebuggingVersion*, Guid*, IntPtr*, ClrDebuggingVersion*, ClrDebuggingProcessFlags*, int>)&OpenVirtualProcess;
                _debugging->Vtable[4] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, IntPtr, int>)&CanUnloadNow;
                _policy->Vtable[3] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, DbgShimCDacLoadPolicy, int>)&SetPolicy;
                _policy->Vtable[4] = (IntPtr)(delegate* unmanaged[MemberFunction]<Instance*, DbgShimCDacLoadPolicy*, int>)&GetPolicy;
            }

            private Instance* Allocate(int slots)
            {
                Instance* instance = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
                instance->Vtable = (IntPtr*)NativeMemory.AllocZeroed((nuint)(slots * sizeof(IntPtr)));
                instance->References = 1;
                instance->Context = GCHandle.ToIntPtr(_handle);
                instance->Vtable[0] = (IntPtr)(delegate* unmanaged[Stdcall]<Instance*, Guid*, void**, int>)&Query;
                instance->Vtable[1] = (IntPtr)(delegate* unmanaged[Stdcall]<Instance*, uint>)&Retain;
                instance->Vtable[2] = (IntPtr)(delegate* unmanaged[Stdcall]<Instance*, uint>)&Drop;
                return instance;
            }

            internal ICLRDebugging CreateDebugging()
            {
                Marshal.AddRef((IntPtr)_debugging);
                try
                {
                    return ComInterfaceMarshaller<ICLRDebugging>.ConvertToManaged(_debugging);
                }
                finally
                {
                    ComInterfaceMarshaller<ICLRDebugging>.Free(_debugging);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            internal void WithDebugging(Action<ICLRDebugging> action)
            {
                ICLRDebugging debugging = CreateDebugging();
                action(debugging);
            }

            internal void AssertBaselineReferences()
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Assert.Equal(1, _debugging->References);
                Assert.Equal(1, _policy->References);
                Assert.Equal(1, Process->References);
            }

            public void Dispose()
            {
                Marshal.Release((IntPtr)_debugging);
                Marshal.Release((IntPtr)_policy);
                Marshal.Release((IntPtr)Process);
            }

            private static NativeDebugging Context(Instance* self) => (NativeDebugging)GCHandle.FromIntPtr(self->Context).Target;

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
            private static int Query(Instance* self, Guid* iid, void** result)
            {
                NativeDebugging native = Context(self);
                Instance* instance = null;
                *result = null;
                if (*iid == s_unknownId)
                    instance = self == native.Process ? self : native._debugging;
                else if (self != native.Process && *iid == typeof(ICLRDebugging).GUID && native.DebuggingAvailable)
                    instance = native._debugging;
                else if (self != native.Process && *iid == typeof(ICLRDebuggingPolicy).GUID && native.PolicyAvailable)
                    instance = native._policy;
                if (instance == null)
                    return HResult.E_NOINTERFACE;
                instance->References++;
                *result = instance;
                return HResult.S_OK;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
            private static uint Retain(Instance* self) => (uint)++self->References;

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
            private static uint Drop(Instance* self)
            {
                uint references = (uint)--self->References;
                if (references == 0)
                {
                    NativeDebugging native = Context(self);
                    NativeMemory.Free(self->Vtable);
                    NativeMemory.Free(self);
                    if (System.Threading.Interlocked.Decrement(ref native._liveInstances) == 0)
                        native._handle.Free();
                }
                return references;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int OpenVirtualProcess(Instance* self, ulong moduleBase, IntPtr target, IntPtr library,
                ClrDebuggingVersion* maximum, Guid* iid, IntPtr* process, ClrDebuggingVersion* version, ClrDebuggingProcessFlags* flags)
            {
                NativeDebugging native = Context(self);
                native.ModuleBase = moduleBase;
                native.DataTarget = target;
                native.LibraryProvider = library;
                native.ProcessId = *iid;
                native.MaximumVersion = *maximum;
                native.InputVersion = *version;
                *process = IntPtr.Zero;
                if (native.OpenResult >= 0)
                {
                    native.Process->References++;
                    *process = (IntPtr)native.Process;
                }
                *version = new ClrDebuggingVersion { Major = 11, Minor = 2, Build = 123, Revision = 456 };
                *flags = ClrDebuggingProcessFlags.ManagedDebugEventDebuggerLaunch;
                return native.OpenResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int CanUnloadNow(Instance* self, IntPtr module)
            {
                NativeDebugging native = Context(self);
                native.UnloadModule = module;
                return native.UnloadResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int SetPolicy(Instance* self, DbgShimCDacLoadPolicy policy)
            {
                NativeDebugging native = Context(self);
                native.Policy = policy;
                return native.PolicyResult;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetPolicy(Instance* self, DbgShimCDacLoadPolicy* policy)
            {
                NativeDebugging native = Context(self);
                *policy = native.Policy;
                return native.PolicyResult;
            }
        }
    }
}
