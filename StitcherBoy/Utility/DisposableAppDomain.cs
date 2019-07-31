// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Utility
{
    using System;
#if NETSTANDARD2_0
    using System.IO;
#endif
    using System.Reflection;
#if NETSTANDARD2_0
    using System.Runtime.Loader;
#endif

    internal class DisposableAppDomain :
#if NETSTANDARD2_0
        AssemblyLoadContext,
#endif
        IDisposable
    {
#if NET461
        /// <summary>
        /// Gets the application domain.
        /// </summary>
        /// <value>
        /// The application domain.
        /// </value>
        private AppDomain AppDomain { get; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableAppDomain" /> class.
        /// </summary>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <param name="basePath">The base path.</param>
        public DisposableAppDomain(string friendlyName, string basePath)
        {
#if NET461
            var setup = new AppDomainSetup();
            setup.ApplicationBase = basePath;
            AppDomain = AppDomain.CreateDomain(friendlyName, null, setup);
#endif
        }

        public Assembly Load(byte[] assemblyBytes)
        {
#if NET461
            return AppDomain.Load(assemblyBytes);
#elif NETSTANDARD2_0 || NETSTANDARD2_1
            using (var stream = new MemoryStream(assemblyBytes))
            {
                return LoadFromStream(stream);
            }
#endif
        }

        public TInstance CreateInstanceAndUnwrap<TInstance>()
            where TInstance : MarshalByRefObject
        {
#if NET461
            return AppDomainExtensions.CreateInstanceAndUnwrap<TInstance>(AppDomain);
#elif NETSTANDARD2_0 || NETSTANDARD2_1
            var assembly = LoadFromAssemblyName(typeof(TInstance).Assembly.GetName());
            return (TInstance)assembly.CreateInstance(typeof(TInstance).FullName);
#endif
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
#if NET461
            AppDomain.Unload(AppDomain);
#elif NETSTANDARD2_0
            // there is no possibility to unload asemblies in NETSTANDARD v2.0
            // assemblies will be unloaded when GC collects this intstance as a garbage
#elif NETSTANDARD2_1
            Unload();
#endif
        }

#if NETSTANDARD2_0 || NETSTANDARD2_1
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }
#endif
    }
}