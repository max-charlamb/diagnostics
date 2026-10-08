// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SOS.Extensions.Clrma;
using SOS.Hosting.Interop;
using Xunit;
using static Microsoft.Diagnostics.DebugServices.UnitTests.ScopedComTests;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public unsafe class ClrmaComTests
    {
        private const int S_OK = 0;
        private const int S_FALSE = 1;
        private const int E_NOINTERFACE = unchecked((int)0x80004002);
        private const int E_NOTIMPL = unchecked((int)0x80004001);
        private const int E_FAIL = unchecked((int)0x80004005);
        private const int E_INVALIDARG = unchecked((int)0x80070057);
        private const int E_BOUNDS = unchecked((int)0x8000000B);

        [Fact]
        public void ServicePreservesAbiAndResolvesCurrentCrashInfo()
        {
            TestServices target = new();
            CrashData data = new(target);
            target.Container.AddService<ICrashInfoService>(data);
            target.Container.AddService<IThreadService>(data);
            ClrmaServiceWrapper service = new(target.Container);
            using NativeInterface<IClrmaServiceGenerated> native = new(service);
            IntPtr pointer = native.Pointer;
            AssertInterface(pointer, typeof(IClrmaServiceGenerated).GUID, 8);
            IntPtr* vtable = *(IntPtr**)pointer;

            delegate* unmanaged<IntPtr, IntPtr, int> associate = (delegate* unmanaged<IntPtr, IntPtr, int>)vtable[3];
            Assert.Equal(S_OK, associate(pointer, IntPtr.Zero));

            delegate* unmanaged<IntPtr, uint, IntPtr*, int> getThread = (delegate* unmanaged<IntPtr, uint, IntPtr*, int>)vtable[4];
            IntPtr thread = IntPtr.Zero;
            Assert.Equal(S_OK, getThread(pointer, data.ThreadId, &thread));
            try
            {
                AssertInterface(thread, typeof(IClrmaThreadGenerated).GUID, 10);
            }
            finally
            {
                Release(thread);
            }
            Assert.Equal(E_INVALIDARG, getThread(pointer, 999, &thread));
            Assert.Equal(IntPtr.Zero, thread);

            delegate* unmanaged<IntPtr, ulong, IntPtr*, int> getException = (delegate* unmanaged<IntPtr, ulong, IntPtr*, int>)vtable[5];
            IntPtr exception = IntPtr.Zero;
            Assert.Equal(S_OK, getException(pointer, data.Address, &exception));
            try
            {
                AssertInterface(exception, typeof(IClrmaExceptionGenerated).GUID, 12);
            }
            finally
            {
                Release(exception);
            }
            Assert.Equal(E_INVALIDARG, getException(pointer, ulong.MaxValue, &exception));
            Assert.Equal(IntPtr.Zero, exception);

            delegate* unmanaged<IntPtr, IntPtr*, int> getInspection = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[6];
            IntPtr inspection = new(1);
            Assert.Equal(E_NOTIMPL, getInspection(pointer, &inspection));
            Assert.Equal(IntPtr.Zero, inspection);

            delegate* unmanaged<IntPtr, uint, int> setPolicy = (delegate* unmanaged<IntPtr, uint, int>)vtable[7];
            for (uint policy = 0; policy <= 3; policy++)
            {
                Assert.Equal(S_OK, setPolicy(pointer, policy));
                Assert.Equal((ModuleEnumerationScheme)policy, service.ModuleEnumerationScheme);
            }
            Assert.Equal(E_INVALIDARG, setPolicy(pointer, 4));
            Assert.Equal(E_INVALIDARG, setPolicy(pointer, uint.MaxValue));

            target.Container.RemoveService(typeof(ICrashInfoService));
            Assert.Equal(E_NOINTERFACE, associate(pointer, IntPtr.Zero));
            Assert.Equal(E_FAIL, getThread(pointer, data.ThreadId, &thread));
            Assert.Equal(IntPtr.Zero, thread);
            target.Container.AddService<ICrashInfoService>(data);
            Assert.Equal(S_OK, associate(pointer, IntPtr.Zero));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ThreadPreservesAbiAndTypedExceptionReturns(bool hasException)
        {
            CrashData data = new(new TestServices()) { HasException = hasException };
            ThreadWrapper wrapper = new(data, data);
            using NativeInterface<IClrmaThreadGenerated> native = new(wrapper);
            IntPtr pointer = native.Pointer;
            AssertInterface(pointer, typeof(IClrmaThreadGenerated).GUID, 10);
            IntPtr* vtable = *(IntPtr**)pointer;
            IntPtr command = new(1);
            Assert.Equal(S_FALSE, ((delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[3])(pointer, &command));
            Assert.Equal(IntPtr.Zero, command);
            uint threadId = 0;
            Assert.Equal(S_OK, ((delegate* unmanaged<IntPtr, uint*, int>)vtable[4])(pointer, &threadId));
            Assert.Equal(data.ThreadId, threadId);
            int frames = -1;
            Assert.Equal(E_NOTIMPL, ((delegate* unmanaged<IntPtr, int*, int>)vtable[5])(pointer, &frames));
            Assert.Equal(0, frames);
            AssertFrame(pointer, vtable[6], 0, E_NOTIMPL, 0, 0, null, null, 0);

            delegate* unmanaged<IntPtr, IntPtr*, int> current = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[7];
            IntPtr exception = IntPtr.Zero;
            Assert.Equal(hasException ? S_OK : S_FALSE, current(pointer, &exception));
            if (hasException)
            {
                try
                {
                    AssertInterface(exception, typeof(IClrmaExceptionGenerated).GUID, 12);
                    IntPtr repeated = IntPtr.Zero;
                    Assert.Equal(S_OK, current(pointer, &repeated));
                    try
                    {
                        Assert.Equal(exception, repeated);
                    }
                    finally
                    {
                        Release(repeated);
                    }
                }
                finally
                {
                    Release(exception);
                }
            }
            else
            {
                Assert.Equal(IntPtr.Zero, exception);
            }

            ushort count = 99;
            Assert.Equal(hasException ? S_OK : S_FALSE, ((delegate* unmanaged<IntPtr, ushort*, int>)vtable[8])(pointer, &count));
            Assert.Equal(hasException ? (ushort)1 : (ushort)0, count);
            delegate* unmanaged<IntPtr, ushort, IntPtr*, int> nested = (delegate* unmanaged<IntPtr, ushort, IntPtr*, int>)vtable[9];
            if (hasException)
            {
                Assert.Equal(S_OK, nested(pointer, 0, &exception));
                try
                {
                    AssertExceptionAddress(exception, data.Address);
                }
                finally
                {
                    Release(exception);
                }
            }
            exception = new IntPtr(1);
            Assert.Equal(E_BOUNDS, nested(pointer, count, &exception));
            Assert.Equal(IntPtr.Zero, exception);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ExceptionPreservesBstrFrameAndChildAbi(bool namedFrame)
        {
            CrashData data = new(new TestServices()) { NamedFrame = namedFrame };
            ExceptionWrapper wrapper = new(data);
            using NativeInterface<IClrmaExceptionGenerated> native = new(wrapper);
            IntPtr pointer = native.Pointer;
            AssertInterface(pointer, typeof(IClrmaExceptionGenerated).GUID, 12);
            IntPtr* vtable = *(IntPtr**)pointer;
            IntPtr command = new(1);
            Assert.Equal(S_FALSE, ((delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[3])(pointer, &command));
            Assert.Equal(IntPtr.Zero, command);
            AssertExceptionAddress(pointer, data.Address);
            uint hresult = 0;
            Assert.Equal(S_OK, ((delegate* unmanaged<IntPtr, uint*, int>)vtable[5])(pointer, &hresult));
            Assert.Equal(data.HResult, hresult);
            AssertBstr(pointer, vtable[6], data.Type);
            AssertBstr(pointer, vtable[7], data.Message);

            int frames = 0;
            Assert.Equal(S_OK, ((delegate* unmanaged<IntPtr, int*, int>)vtable[8])(pointer, &frames));
            Assert.Equal(1, frames);
            AssertFrame(pointer, vtable[9], 0, S_OK, data.InstructionPointer, data.StackPointer,
                namedFrame ? "module-\u03a9" : $"module_{data.ModuleBase:X16}",
                namedFrame ? "method-\u03a9" : $"function_{data.InstructionPointer:X16}", 7);
            AssertFrame(pointer, vtable[9], -1, E_BOUNDS, 0, 0, null, null, 0);
            AssertFrame(pointer, vtable[9], 1, E_BOUNDS, 0, 0, null, null, 0);

            ushort count = 0;
            Assert.Equal(S_OK, ((delegate* unmanaged<IntPtr, ushort*, int>)vtable[10])(pointer, &count));
            Assert.Equal((ushort)1, count);
            delegate* unmanaged<IntPtr, ushort, IntPtr*, int> inner = (delegate* unmanaged<IntPtr, ushort, IntPtr*, int>)vtable[11];
            IntPtr exception = IntPtr.Zero;
            Assert.Equal(S_OK, inner(pointer, 0, &exception));
            try
            {
                AssertExceptionAddress(exception, data.Address);
                IntPtr repeated = IntPtr.Zero;
                Assert.Equal(S_OK, inner(pointer, 0, &repeated));
                try
                {
                    Assert.Equal(exception, repeated);
                }
                finally
                {
                    Release(repeated);
                }
            }
            finally
            {
                Release(exception);
            }
            Assert.Equal(E_BOUNDS, inner(pointer, count, &exception));
            Assert.Equal(IntPtr.Zero, exception);
        }

        private static void AssertExceptionAddress(IntPtr pointer, ulong expected)
        {
            IntPtr* vtable = *(IntPtr**)pointer;
            ulong address = 0;
            Assert.Equal(S_OK, ((delegate* unmanaged<IntPtr, ulong*, int>)vtable[4])(pointer, &address));
            Assert.Equal(expected, address);
        }

        private static void AssertBstr(IntPtr pointer, IntPtr method, string expected)
        {
            IntPtr value = IntPtr.Zero;
            Assert.Equal(S_OK, ((delegate* unmanaged<IntPtr, IntPtr*, int>)method)(pointer, &value));
            try
            {
                Assert.Equal(expected, Marshal.PtrToStringBSTR(value));
            }
            finally
            {
                Marshal.FreeBSTR(value);
            }
        }

        private static void AssertFrame(IntPtr pointer, IntPtr method, int index, int expectedResult,
            ulong expectedIp, ulong expectedSp, string expectedModule, string expectedFunction, ulong expectedDisplacement)
        {
            ulong ip = ulong.MaxValue;
            ulong sp = ulong.MaxValue;
            ulong displacement = ulong.MaxValue;
            IntPtr module = IntPtr.Zero;
            IntPtr function = IntPtr.Zero;
            Assert.Equal(expectedResult,
                ((delegate* unmanaged<IntPtr, int, ulong*, ulong*, IntPtr*, IntPtr*, ulong*, int>)method)
                    (pointer, index, &ip, &sp, &module, &function, &displacement));
            try
            {
                Assert.Equal(expectedIp, ip);
                Assert.Equal(expectedSp, sp);
                Assert.Equal(expectedDisplacement, displacement);
                Assert.Equal(expectedModule, module == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(module));
                Assert.Equal(expectedFunction, function == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(function));
            }
            finally
            {
                Marshal.FreeBSTR(module);
                Marshal.FreeBSTR(function);
            }
        }

        internal sealed class CrashData : ICrashInfoService, IThreadService, IThread, IException, IStack, IStackFrame
        {
            internal bool HasException { get; set; } = true;
            internal bool NamedFrame { get; set; } = true;
            public ITarget Target { get; }
            internal CrashData(ITarget target) => Target = target;
            public CrashReason CrashReason => CrashReason.UnhandledException;
            public uint ThreadId => 42;
            public uint HResult => 0x80131500;
            public RuntimeType RuntimeType => RuntimeType.NetCore;
            public ulong RuntimeBaseAddress => 0xDEADBEEF00000000;
            public string RuntimeVersion => "10.0";
            public string Message => "message-\u03a9\0tail";
            public IException GetException(ulong address) => address == Address ? this : throw new ArgumentOutOfRangeException(nameof(address));
            public IException GetThreadException(uint threadId) => threadId == ThreadId
                ? (HasException ? this : null) : throw new ArgumentOutOfRangeException(nameof(threadId));
            public IEnumerable<IException> GetNestedExceptions(uint threadId) => new IException[] { this };

            public IEnumerable<RegisterInfo> Registers => Array.Empty<RegisterInfo>();
            public int InstructionPointerIndex => 0;
            public int FramePointerIndex => 1;
            public int StackPointerIndex => 2;
            public bool TryGetRegisterIndexByName(string name, out int registerIndex) => throw new NotSupportedException();
            public bool TryGetRegisterInfo(int registerIndex, out RegisterInfo info) => throw new NotSupportedException();
            public IEnumerable<IThread> EnumerateThreads() => new IThread[] { this };
            public IThread GetThreadFromIndex(int threadIndex) => throw new NotSupportedException();
            public IThread GetThreadFromId(uint threadId) => threadId == ThreadId ? this : null;
            public int ThreadIndex => 0;
            public IServiceProvider Services => Target.Services;
            public bool TryGetRegisterValue(int registerIndex, out ulong value) => throw new NotSupportedException();
            public ReadOnlySpan<byte> GetThreadContext() => throw new NotSupportedException();
            public ulong GetThreadTeb() => 0;

            public ulong Address => 0xDEADBEEF12345678;
            public string Type => "Test.Exception-\u03a9";
            public IStack Stack => this;
            public IEnumerable<IException> InnerExceptions => new IException[] { this };
            public int FrameCount => 1;
            public IStackFrame GetStackFrame(int index) => index == 0 ? this : throw new ArgumentOutOfRangeException(nameof(index));
            public ulong InstructionPointer => 0x123456789ABCDEF0;
            public ulong StackPointer => 0xABCDEF0123456789;
            public ulong ModuleBase => 0x1234567800000000;
            public void GetMethodName(out string moduleName, out string methodName, out ulong displacement)
            {
                moduleName = NamedFrame ? "module-\u03a9" : null;
                methodName = NamedFrame ? "method-\u03a9" : null;
                displacement = 7;
            }
        }
    }
}
