using System;
using System.Collections.Generic;
using System.Text;
using Unity.Resolution;

namespace Unity.Microsoft.DependencyInjection
{
    public class UnityConfigurationOptions
    {
        /// <summary>
        /// The container to use as the root container.
        /// </summary>
        public IUnityContainer UnityContainer { get; set; }
        /// <summary>
        /// Controls the unity resolution parameters when resolving a type via the ServiceProvider.
        /// </summary>
        public Action<ResolutionParameters> ResolveConfiguration { get; set; }
        /// <summary>
        /// Configures the scope options when a new child scope is created.
        /// </summary>
        public Action<UnityConfigurationOptions> CreateScope { get; set; }

        public UnityConfigurationOptions With(IUnityContainer unityContainer = null, Action<ResolutionParameters> resolutionConfiguration = null, Action<UnityConfigurationOptions> createScope = null)
        {
            return new UnityConfigurationOptions
            {
                UnityContainer = unityContainer ?? UnityContainer,
                ResolveConfiguration = resolutionConfiguration ?? ResolveConfiguration,
                CreateScope = createScope ?? CreateScope
            };
        }
    }

    public class ResolutionParameters
    {
        /// <summary>
        /// The type being resolved.
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// The name to use when resolving the type.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The resolver overrides to use when resolving the type.
        /// </summary>
        public ResolverOverride[] ResolverOverrides { get; set; }
        /// <summary>
        /// Action to call when a type resolution fails.
        /// </summary>
        public Action<Type> ResolutionFailureHanlder { get; set; }
    }
}
