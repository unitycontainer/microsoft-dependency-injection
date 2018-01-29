using System;
using Microsoft.Extensions.DependencyInjection;

namespace Unity.Microsoft.DependencyInjection
{
    public class ServiceProviderFactory : IServiceProviderFactory<IUnityContainer>
    {
        private readonly IUnityContainer _container;

        public ServiceProviderFactory(IUnityContainer container)
        {
            _container = container ?? new UnityContainer();
        }

        public IUnityContainer CreateBuilder(IServiceCollection services)
        {
            return _container.CreateChildContainer()
                             .AddExtension(new MdiExtension())
                             .AddServices(services);
        }

        public IServiceProvider CreateServiceProvider(IUnityContainer container)
        {
            return new ServiceProvider(container);
        }
    }
}
