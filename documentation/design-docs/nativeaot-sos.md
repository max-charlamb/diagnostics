# NativeAOT SOS

## Summary

SOS will ship as one native binary per platform and architecture. The binary will
contain the existing native debugger integration and Strike commands together
with a statically linked NativeAOT image containing the managed SOS
infrastructure, commands, services, ClrMD, and SymbolStore.

The final debugger extension will not:

- discover, load, or initialize CoreCLR;
- load a managed SOS payload;
- select a host runtime or target framework;
- support operation without the managed SOS infrastructure;
- load arbitrary managed extensions at runtime; or
- use runtime-generated COM vtables or built-in COM interop.

dotnet-dump is outside the initial cutover. It may continue to build shared SOS
code as a regular managed dependency until its hosting model is addressed
separately.

## Architecture

```text
Debugger
   |
   v
sos.dll / libsosplugin
+---------------------------------------+
| Native debugger exports and adapters  |
| Native Strike commands                |
|                                       |
| Statically linked NativeAOT image     |
| - HostServices                        |
| - managed SOS commands                |
| - DebugServices                       |
| - ClrMD and SymbolStore               |
+---------------------------------------+
```

The initial work focuses on making the managed graph NativeAOT-compatible,
starting with its COM boundaries. Native loading and final packaging are
deliberately deferred until the managed implementation can be published and
tested without dynamic-code requirements.

## Implementation plan

### 0. Lock down the interop ABI

Inventory the interface GUIDs, vtable slot ordering, calling conventions,
marshalling, identity, and ownership rules shared between native and managed
SOS. Add representative native tests for `QueryInterface`, `AddRef`, `Release`,
multiple interfaces, strings, buffers, structs, callbacks, and teardown.

### 1. Establish a NativeAOT validation project

Add a minimal net8.0 NativeAOT project that roots the managed SOS graph and is
used to expose trimming and AOT diagnostics. Native loading and linking are not
part of this phase.

The managed SOS payload and dotnet-dump also retain the repository's net8.0
minimum target. Source-generated COM does not require raising that minimum.
The validation project currently publishes with remaining reflection, trimming,
and single-file warnings; clearing those warnings and integrating a native
loader are separate follow-up steps.

### 2. Define generated COM conventions

Use `GeneratedComInterface`, `GeneratedComClass`, and
`StrategyBasedComWrappers` for SOS IUnknown-style interfaces. Preserve HRESULTs,
interface inheritance, GUIDs, vtable ordering, and explicit string marshalling.
Use custom source-generated marshallers when required. If an interface cannot
be represented by generated COM, use a static `ComWrappers` vtable built from
`UnmanagedCallersOnly` methods. Never fall back to delegate-generated vtables.

Define explicit ownership helpers for borrowed pointers, owned pointers,
`QueryInterface` results, and generated interface factories.

`HostServices`, `HostWrapper`, `TargetWrapper`, and `RuntimeWrapper` now use
generated COM interfaces separate from the managed DebugServices interfaces.
`GetHost` and `GetCurrentTarget` return typed generated interfaces; generated
stubs perform marshalling and AddRef without adapter-owned self-pointers.
Interface IDs come from `typeof(IInterface).GUID`, using the interface's
`GuidAttribute` rather than duplicate IID fields on adapter implementations.
Explicit marshalling is restricted to raw native entrypoints, IID-based service
lookup, and borrowed-pointer interfaces. Resource teardown is idempotent and
belongs to the managed service scope, independently of generated COM reference
counts; native callers must not invoke services after scope disposal.
`HostWrapper` and `TargetWrapper` own no unmanaged resources and do not require disposal.

`GetHost`, `GetCurrentTarget`, and `GetService` return AddRef-owned pointers.
`GetRuntime`, `GetClrDataProcess`, and `GetCorDebugInterface` retain the existing
borrowed-pointer ABI. `RuntimeWrapper` creates one interface reference in its constructor for
`GetRuntime` and releases it at runtime-scope teardown; returning a normally
marshalled interface here would add references that existing native callers do
not release. `GetRuntimeDirectory` returns an ANSI-marshalled `string`, preserving
the original delegate-based adapter's return marshalling rather than caching an
adapter-owned native buffer.
The unused trailing temporary-directory slot from the original managed host
is removed; the generated host interface matches the native `host.h` contract.

Native service lookup is fixed: the host exposes only `IHostServices`, and each
target exposes only `ISymbolService` and the optional `ICLRMAService`.
Adapters are created lazily, without a general-purpose registry. The symbol
factory defaults to `SymbolServiceWrapper` in the target adapter's constructor;
native service lookup and LLDB expression evaluation share the cached instance.
The ClrMA implementation remains in SOS.Extensions and is supplied through a dedicated
factory configured by debugger-backed targets. ClrMA still declines clients
when crash information is unavailable. Both adapters use typed generated
interfaces and remain bound to their target's lifetime.

The ClrMA service, thread, and exception adapters use generated COM, including
BSTR outputs and typed child-interface returns. Thread and exception adapters
hold only managed objects; their cached children no longer need creator
references or explicit COM teardown. Module-discovered crash information is
cached in `CrashInfoModuleService` per enumeration scheme, including misses.
Target flush discards that service instance
and all its cached results. ClrMA resolves through the current target services
without its own cache or flush subscription and owns no disposable resources.

The symbol adapter also uses generated COM. Its ABI distinguishes native
one-byte `bool` from four-byte `BOOL`, preserves the 16-byte metadata GUID
buffer, and returns caller-owned BSTRs through generated string marshalling.
Symbol reader handles still use GCHandles and the native `Dispose(handle)`
method to release the managed symbol file.

### Prepared interface declarations

The runtime callback contracts are declared under
`src/SOS/SOS.Hosting/Interop/`. DbgEng client and symbol contracts live under its
`DbgEng/` subdirectory, in `SOS.Hosting.Interop.DbgEng`. The `Generated` suffix
distinguishes these contracts from existing delegate-based adapters and
`ComImport` declarations. `DataTargetWrapper`, `CorDebugDataTargetWrapper`, and
`RuntimeLibraryProvider` now implement their generated contracts. The managed
DbgEng and LLDB clients exported by `SOSHost` also use generated COM classes.

The runtime declarations cover `ICLRDataTarget`/`ICLRDataTarget2`, metadata,
runtime and contract locators, the CLR symbol provider, CorDebug data targets,
and `ICLRDebuggingLibraryProvider2`. Native inheritance and signatures follow
`src/shared/inc/clrdata.idl`, `cordebug.idl`, and `metahost.idl`. The former
`DataTargetWrapper` name `ICLRDataTarget4` referred to the same IID and unwind
contract as `ICorDebugDataTarget4`; both adapters use the single generated
`ICorDebugDataTarget4` declaration. The converted `DataTargetWrapper` exposes
all seven original IIDs, including inherited `ICLRDataTarget2` slots, without
delegate-built vtables. Unwind callbacks operate directly on the native context
buffer. `IDataTarget` owns one constructor-created COM reference, released
idempotently by `Dispose`; references retained by the DAC remain valid until
native callers release them. Disposing that creator reference does not shut
down the target's services.

`CorDebugDataTargetWrapper` exposes its four original IIDs, with the mutable
interface now including inherited base-target slots. `ICorDebugDataTarget`
also owns one constructor-created reference released by `Dispose`; DBI-held
references remain valid after that release. Thread-context callbacks use the
shared span-based copying helper, and unwind callbacks modify the native buffer
directly. Existing platform mappings, memory semantics, metadata-path buffer
handling, and unsupported-method HRESULTs are preserved.

`RuntimeLibraryProvider` uses the generated library-provider contract and owns
one constructor-created `ILibraryProvider` reference. Its idempotent disposal
releases both that reference and retained Authenticode file locks. DAC/DBI path
selection and signature-verification policy are unchanged. Generated UTF-16
output marshalling allocates each returned path with caller-owned CoTaskMem
storage, preserving the native library-provider contract.

`DebuggerServices` imports the main `IDebuggerServicesGenerated` interface with
`ComInterfaceMarshaller<IDebuggerServicesGenerated>.ConvertToManaged`, using
the framework's shared default `StrategyBasedComWrappers` instance and normal
identity caching.
Casts on that proxy to `IDebugClient5Generated` and
`IDebugSymbols5Generated` query the native adapter, which forwards DbgEng
interface queries to its client. All three interfaces share the managed proxy,
without a separate client import. The incoming owned reference is released
after import; proxy-owned references are released
automatically when the proxy is collected, without `FinalRelease`.
`DebuggerServices` is not disposable: host shutdown drops its managed references,
while native callback shutdown remains explicit. The proxy retains the
SOS-native adapter's identity and the queried DbgEng interface references.
Consequently, collecting the proxy can call `Release` in the
SOS-native DLL. Coordinating DLL unload with outstanding proxies remains a
debugger-host integration requirement, not something automatic COM release
replaces. `GetNativeClient` still returns a caller-owned native client reference
for `SOSHost` to pass to native SOS commands. Symbol options and module
symbol-status loading use generated calls, preserving the existing reload
fallback and per-module status cache.

The optional remote-memory and debugger-stack interfaces also come from casts
on that same proxy. `HostServices` registers their managed service adapters only
when the corresponding interface is available. `RemoteMemoryService` and
`ThreadStackTraceService` retain generated interface references without
`IDisposable`, handwritten vtables, or manual `AddRef`/`Release`. Their generated
contracts preserve native HRESULTs, unsigned 32-bit buffer sizes, and the
16-byte debugger stack-frame layout. Stack results retain the existing
validation of frame counts and zero for unavailable stack pointers.

`ICLRDebugging` and `ICLRDebuggingPolicy` are generated interfaces in
`SOS.Hosting/Interop`, retaining their existing public type names and namespaces.
The CLR debugging contract includes both `OpenVirtualProcess` and
`CanUnloadNow` in native slot order. Its version parameter is `ref`, matching
the native input/output contract; callers initialize the version structure to
zero before calling. `ClrDataProcessActivator` imports the factory's owned
pointer into the default cached proxy and releases the incoming reference.
Policy selection uses an ordinary cast on that proxy, without separate wrappers,
`SuppressRelease`, or manually released policy/debugging references. Returned
process pointers retain their existing caller-owned lifetime. The dbgshim library
remains loaded, as before, so proxy cleanup can still call its native `Release`.
No `CallableCOMWrapper` usage remains under `src/SOS`; the DbgShim test-only
`ICorDebug` wrappers are outside this production migration.

The DbgEng declarations include `IDebugClient` through `IDebugClient6` and
`IDebugSymbols` through `IDebugSymbols5`, plus the advanced, control, data-spaces,
registers, and system-objects contracts implemented by the managed client,
following the Windows SDK `DbgEng.h`.
Derived interfaces declare only their additional methods; generated COM
provides the inherited vtable slots. Input strings have explicit ANSI or UTF-16
marshalling. Caller-owned output buffers and arrays use native pointers rather
than `StringBuilder` or runtime array marshalling. DbgEng callback and symbol-group
interfaces remain raw `IntPtr`/`IntPtr*` parameters until their own declarations are
migrated: getter results are caller-owned COM references that must be released,
while setter inputs are borrowed for the duration of the call. Returned client
interfaces and runtime-library path strings use generated ownership marshalling.
Output parameters previously represented by native pointers remain pointers,
including optional transfer counts, module indices, names, and version sizes.
The generated servers forward them directly so `NULL` remains valid where the
native helpers allow it, and unsupported or early-return paths do not overwrite
outputs that the legacy implementation left untouched.

`DebugClient` and `LLDBServices` no longer inherit `COMCallableIUnknown` or build
vtables from delegates. Their generated classes expose the same native
capabilities and forward supported calls to the existing `SOSHost` helpers.
The client implementations contain no pointers to themselves. `SOSHost` exports
one caller-owned client reference, passes that pointer to native commands, and
releases it at target disposal. Additional
native references keep the generated server alive independently of that creator
reference. This explicit exported-pointer ownership is separate from the
automatic lifetime of imported native proxies.
Host disposal is idempotent; command execution and help lookup reject a disposed
host with `ObjectDisposedException`. Callbacks held through additional native
references retain their existing lifetime semantics.

The LLDB contracts are separate IUnknown-based interfaces in native header
order. They preserve the non-HRESULT directory and expression returns, the
one-byte module-load boolean, and the Cdecl module callback. Output strings
are copied into bounded native buffers with platform ANSI encoding and a NUL
terminator. Module paths remain valid for the duration of each callback.
Native stack tracing still reports zero frames so managed stack output is not
suppressed. `ILLDBServices2` now includes its previously omitted final
`SetRuntimeLoadedCallback` slot and returns `E_NOTIMPL` for that unsupported
capability. Test-only legacy COM callback implementations remain outside this
production migration.

The declarations intentionally follow native contracts rather than reproducing
legacy adapter discrepancies. In particular, `ICLRDataTarget.Request` takes a
32-bit output-buffer size and a byte-buffer pointer, and
`ICorDebugMutableDataTarget` inherits `ICorDebugDataTarget` and declares
`ContinueStatusChanged(threadId, continueStatus)`. Both adapters now implement
these corrected signatures and inheritance.

### 3. Convert representative interfaces

Convert `HostServices`, `DataTargetWrapper`, and one native debugger-services
path first. These cover the top-level lifetime boundary, multiple-interface
objects, complex parameters, and native interfaces consumed by managed code.
Validate each under both CoreCLR and NativeAOT while the old CoreCLR host remains
only as a development scaffold.

### 4. Convert managed COM servers

Convert the host, target, runtime, symbol, data-target, CorDebug, DbgEng, LLDB,
and ClrMA wrappers. Keep the fixed native-service surface and migrate its
remaining legacy wrappers to generated COM.

### 5. Convert managed COM clients

Replace `CallableCOMWrapper`, the DbgEng `ComImport` interfaces,
`Marshal.GetObjectForIUnknown`, and ClrMD `COMHelper` usage with generated
interfaces and explicit ownership.

### 6. Remove legacy COM infrastructure

Delete all SOS usage of `COMCallableIUnknown`, `VTableBuilder`,
`CallableCOMWrapper`, `ComImport`, and runtime-generated marshaling. Remove the
COM-related IL3050 suppressions and enable runtime-marshalling restrictions for
the NativeAOT root.

### 7. Make commands and services closed-world

Generate or explicitly define command metadata, service registrations,
factories, and service imports. Remove reflection-based assembly scanning,
constructor activation, runtime generic construction, and managed extension
loading from the NativeAOT graph.

### 8. Clear remaining AOT warnings

Use source-generated JSON, replace assembly-location assumptions and dynamic
marshal APIs, and preserve or generate Microsoft.FileFormats layouts. The fully
rooted NativeAOT publish must complete without trimming, single-file, or AOT
warnings.

### 9. Add the production NativeAOT entrypoint

Export a narrow initialization ABI that receives the SOS path and debugger
services and returns `IHostServices`. No managed exception may cross this
boundary. Initialization and uninitialization must have explicit lifetime and
error-reporting behavior.

### 10. Integrate NativeAOT with the production SOS target

Choose the supported native-library packaging model, build it with the native
SOS target, load or link its exported entrypoints, and produce symbols for both
portions. This decision is made after the managed graph is NativeAOT-compatible.

### 11. Cut over

Switch production initialization to the linked NativeAOT entrypoint and remove
`hostcoreclr.cpp`, CoreCLR probing and initialization, TPA construction, managed
payload probing, host-runtime selection, `SetHostRuntime`, and the no-host
fallback.

### 12. Simplify packaging

Ship only the combined binary and its symbols for each RID. Remove the managed
SOS payload and assert that the package has no dependency on CoreCLR, hostfxr,
or an installed shared framework.

## Validation

The cutover requires live-process and dump testing under WinDbg/CDB, LLDB, and
dotnet-dump; DAC and cDAC coverage; load/unload and target-lifecycle testing;
COM identity and reference-count validation; native and managed command
coverage; symbol testing; and package installation testing on all shipping
platforms and architectures.
