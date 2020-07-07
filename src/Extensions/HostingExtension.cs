using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Unity.Microsoft.DependencyInjection
{
    public static class HostingExtension
    {
        private static ServiceProviderFactory _factory;


        public static IHostBuilder UseUnityServiceProvider(this IHostBuilder hostBuilder, IUnityContainer container = null)
        {
            _factory = new ServiceProviderFactory(container);

            return hostBuilder.UseServiceProviderFactory<IUnityContainer>(_factory)
                              .ConfigureServices((context, services) =>
                              {
                                  services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IUnityContainer>>(_factory));
                                  services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IServiceCollection>>(_factory));
                              });
        }
    }
}
