// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using Microsoft.Diagnostics.DebugServices;
using Microsoft.Diagnostics.DebugServices.Implementation;
using SOS.Extensions.Clrma;
using SOS.Hosting.Interop;
using Xunit;

namespace Microsoft.Diagnostics.DebugServices.UnitTests
{
    public class CrashInfoServiceTests
    {
        private const string TriageJson = """{"version":"1.0.0","runtime_base":"0x7FFF80270000","runtime_type":"4","runtime_version":"9.0.16","reason":"2","thread":"0x7050","message":"Delegate_GarbageCollected"}""";
        private static readonly ModuleEnumerationScheme[] s_moduleSchemes =
        [
            ModuleEnumerationScheme.EntryPointModule,
            ModuleEnumerationScheme.EntryPointAndEntryPointDllModule,
            ModuleEnumerationScheme.All,
        ];

        [Fact]
        public void CreateAllowsMessageOnlyNativeAotCrashInfo()
        {
            ICrashInfoService crashInfo = CrashInfoService.Create(0, Encoding.UTF8.GetBytes(TriageJson), null!);

            Assert.NotNull(crashInfo);
            Assert.Equal(CrashReason.EnvironmentFailFast, crashInfo.CrashReason);
            Assert.Equal(RuntimeType.NativeAOT, crashInfo.RuntimeType);
            Assert.Equal((uint)0x7050, crashInfo.ThreadId);
            Assert.Equal("Delegate_GarbageCollected", crashInfo.Message);
            Assert.Null(crashInfo.GetException(0));
            Assert.Null(crashInfo.GetThreadException(crashInfo.ThreadId));
            Assert.Empty(crashInfo.GetNestedExceptions(crashInfo.ThreadId));
        }

        [Fact]
        public void ModuleServiceCachesSuccessfulResultsPerScheme()
        {
            TestCrashModule module = new();
            CrashInfoModuleService service = new(module.Services);
            Dictionary<ModuleEnumerationScheme, ICrashInfoService> results = new();
            foreach (ModuleEnumerationScheme scheme in s_moduleSchemes)
            {
                ICrashInfoService crashInfo = service.Create(scheme);
                Assert.NotNull(crashInfo);
                Assert.Equal("Delegate_GarbageCollected", crashInfo.Message);
                foreach (ICrashInfoService previous in results.Values)
                {
                    Assert.NotSame(previous, crashInfo);
                }
                results.Add(scheme, crashInfo);
            }
            Assert.Equal(3, module.ExportLookups);
            foreach (ModuleEnumerationScheme scheme in s_moduleSchemes)
            {
                Assert.Same(results[scheme], service.Create(scheme));
            }
            Assert.Null(service.Create(ModuleEnumerationScheme.None));
            Assert.Equal(3, module.ExportLookups);
        }

        [Theory]
        [InlineData(ModuleEnumerationScheme.EntryPointModule)]
        [InlineData(ModuleEnumerationScheme.EntryPointAndEntryPointDllModule)]
        [InlineData(ModuleEnumerationScheme.All)]
        public void ModuleServiceCachesMissingCrashInfoPerScheme(ModuleEnumerationScheme scheme)
        {
            TestCrashModule module = new() { HasCrashInfo = false };
            CrashInfoModuleService service = new(module.Services);
            Assert.Null(service.Create(scheme));
            Assert.Null(service.Create(scheme));
            Assert.Equal(1, module.ExportLookups);

            module.HasCrashInfo = true;
            Assert.Null(service.Create(scheme));
            Assert.Equal(1, module.ExportLookups);
            ModuleEnumerationScheme otherScheme = scheme == ModuleEnumerationScheme.All
                ? ModuleEnumerationScheme.EntryPointModule : ModuleEnumerationScheme.All;
            ICrashInfoService crashInfo = service.Create(otherScheme);
            Assert.NotNull(crashInfo);
            Assert.Same(crashInfo, service.Create(otherScheme));
            Assert.Equal(2, module.ExportLookups);
        }

        [Fact]
        public void TargetFlushDiscardsCachedMisses()
        {
            TestCrashModule module = new() { HasCrashInfo = false };
            ServiceContainer services = CreateTargetServices(module);
            using IDisposable registration = module.OnFlushEvent.Register(() => services.RemoveService(typeof(ICrashInfoModuleService)));
            ICrashInfoModuleService service = services.GetService<ICrashInfoModuleService>();
            Assert.Null(service.Create(ModuleEnumerationScheme.EntryPointModule));
            Assert.Null(service.Create(ModuleEnumerationScheme.All));
            Assert.Equal(2, module.ExportLookups);

            module.HasCrashInfo = true;
            Assert.Null(service.Create(ModuleEnumerationScheme.EntryPointModule));
            Assert.Null(service.Create(ModuleEnumerationScheme.All));
            Assert.Equal(2, module.ExportLookups);

            module.OnFlushEvent.Fire();
            ICrashInfoModuleService replacement = services.GetService<ICrashInfoModuleService>();
            Assert.NotSame(service, replacement);
            Assert.NotNull(replacement.Create(ModuleEnumerationScheme.EntryPointModule));
            Assert.NotNull(replacement.Create(ModuleEnumerationScheme.All));
            Assert.Equal(4, module.ExportLookups);
        }

        [Fact]
        public void TargetFlushDiscardsEveryCachedScheme()
        {
            TestCrashModule module = new();
            ServiceContainer services = CreateTargetServices(module);
            using IDisposable registration = module.OnFlushEvent.Register(() => services.RemoveService(typeof(ICrashInfoModuleService)));
            ICrashInfoModuleService service = services.GetService<ICrashInfoModuleService>();
            Dictionary<ModuleEnumerationScheme, ICrashInfoService> results = new();
            foreach (ModuleEnumerationScheme scheme in s_moduleSchemes)
            {
                results.Add(scheme, service.Create(scheme));
                Assert.NotNull(results[scheme]);
            }
            Assert.Equal(3, module.ExportLookups);

            module.OnFlushEvent.Fire();
            ICrashInfoModuleService replacement = services.GetService<ICrashInfoModuleService>();
            Assert.NotSame(service, replacement);
            foreach (ModuleEnumerationScheme scheme in s_moduleSchemes)
            {
                ICrashInfoService crashInfo = replacement.Create(scheme);
                Assert.NotNull(crashInfo);
                Assert.NotSame(results[scheme], crashInfo);
                Assert.Same(crashInfo, replacement.Create(scheme));
            }
            Assert.Equal(6, module.ExportLookups);
        }

        [Fact]
        public void ClrmaUsesCurrentModuleServiceAndEnumerationScheme()
        {
            TestCrashModule module = new();
            ServiceContainer services = CreateTargetServices(module);
            using IDisposable registration = module.OnFlushEvent.Register(() => services.RemoveService(typeof(ICrashInfoModuleService)));
            ClrmaServiceWrapper wrapper = new(services);
            IClrmaServiceGenerated service = wrapper;
            Assert.Equal(unchecked((int)0x80004002), service.AssociateClient(IntPtr.Zero));
            foreach (ModuleEnumerationScheme scheme in s_moduleSchemes)
            {
                Assert.Equal(0, service.SetModuleEnumerationPolicy((uint)scheme));
                Assert.Equal(0, service.AssociateClient(IntPtr.Zero));
            }
            Assert.Equal(3, module.ExportLookups);
            Assert.Equal(0, service.SetModuleEnumerationPolicy((uint)ModuleEnumerationScheme.EntryPointModule));
            Assert.Equal(0, service.AssociateClient(IntPtr.Zero));
            Assert.Equal(3, module.ExportLookups);

            module.OnFlushEvent.Fire();
            Assert.Equal(0, service.AssociateClient(IntPtr.Zero));
            Assert.Equal(4, module.ExportLookups);

            services.AddService<ICrashInfoService>(CrashInfoService.Create(0, Encoding.UTF8.GetBytes(TriageJson), null!));
            Assert.Equal(0, service.SetModuleEnumerationPolicy((uint)ModuleEnumerationScheme.All));
            Assert.Equal(0, service.AssociateClient(IntPtr.Zero));
            Assert.Equal(4, module.ExportLookups);
        }

        private static ServiceContainer CreateTargetServices(TestCrashModule module) => new(module.Services,
            new Dictionary<Type, ServiceFactory>
            {
                [typeof(ICrashInfoModuleService)] = services => new CrashInfoModuleService(services),
            });

        private sealed class TestCrashModule : ScopedComTests.TestServices, IModule, IModuleService, IExportSymbols, IMemoryService
        {
            private const ulong HeaderAddress = 0x1000;
            private const ulong EntriesAddress = 0x2000;
            private const ulong NameAddress = 0x3000;
            private const ulong BufferAddress = 0x4000;
            private readonly Dictionary<ulong, byte[]> _memory = new();

            internal bool HasCrashInfo { get; set; } = true;
            internal int ExportLookups { get; private set; }

            internal TestCrashModule()
            {
                Container.AddService<IModuleService>(this);
                Container.AddService<IMemoryService>(this);

                byte[] header = new byte[32];
                BinaryPrimitives.WriteUInt32LittleEndian(header, CrashInfoService.DOTNET_RUNTIME_DEBUG_HEADER_COOKIE);
                BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4), 4);
                BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(8), 1);
                BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(24), EntriesAddress);
                _memory.Add(HeaderAddress, header);

                byte[] entries = new byte[16];
                BinaryPrimitives.WriteUInt64LittleEndian(entries, NameAddress);
                BinaryPrimitives.WriteUInt64LittleEndian(entries.AsSpan(8), BufferAddress);
                _memory.Add(EntriesAddress, entries);
                _memory.Add(NameAddress, Encoding.ASCII.GetBytes("g_CrashInfoBuffer\0"));
                _memory.Add(BufferAddress, Encoding.UTF8.GetBytes(TriageJson + "\0"));
            }

            public bool TryGetSymbolAddress(string name, out ulong offset)
            {
                ExportLookups++;
                offset = HeaderAddress;
                return HasCrashInfo && name == CrashInfoService.DOTNET_RUNTIME_DEBUG_HEADER_NAME;
            }

            bool IMemoryService.ReadMemory(ulong address, Span<byte> buffer, out int bytesRead)
            {
                foreach (KeyValuePair<ulong, byte[]> region in _memory)
                {
                    if (address >= region.Key && address - region.Key < (ulong)region.Value.Length)
                    {
                        int offset = (int)(address - region.Key);
                        bytesRead = Math.Min(buffer.Length, region.Value.Length - offset);
                        region.Value.AsSpan(offset, bytesRead).CopyTo(buffer);
                        return true;
                    }
                }
                bytesRead = 0;
                return false;
            }

            string IModule.FileName => "test.exe";
            public IModule EntryPointModule => this;
            public IEnumerable<IModule> EnumerateModules() => new IModule[] { this };
            public IModule GetModuleFromIndex(int moduleIndex) => throw new NotSupportedException();
            public IModule GetModuleFromBaseAddress(ulong baseAddress) => throw new NotSupportedException();
            public IModule GetModuleFromAddress(ulong address) => this;
            public IEnumerable<IModule> GetModuleFromModuleName(string moduleName) => Array.Empty<IModule>();
            public IModule CreateModule(int moduleIndex, ulong imageBase, ulong imageSize, string imageName) => throw new NotSupportedException();
        }
    }
}
