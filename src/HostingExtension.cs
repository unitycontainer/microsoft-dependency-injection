using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Unity.Microsoft.DependencyInjection
{
    public static class HostingExtension
    {
        public static IWebHostBuilder UseUnityServiceProvider(this IWebHostBuilder hostBuilder, IUnityContainer container = null)
        {
            return UseUnityServiceProvider(hostBuilder, c => c.UnityContainer = container);
        }

        public static IWebHostBuilder UseUnityServiceProvider(this IWebHostBuilder hostBuilder, Action<UnityConfigurationOptions> config)
        {
            var factory = new ServiceProviderFactory(config);

#if NETCOREAPP1_1
            return hostBuilder.ConfigureServices((services) =>
            {
                services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IUnityContainer>>(factory));
                services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IServiceCollection>>(factory));
            });
#else
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IUnityContainer>>(factory));
                services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IServiceCollection>>(factory));
            });
#endif
        }
    }
}
