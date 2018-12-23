using System;
using Microsoft.Extensions.DependencyInjection;

namespace Unity.Microsoft.DependencyInjection
{
    public class ServiceProviderFactory : IServiceProviderFactory<IUnityContainer>,
                                          IServiceProviderFactory<IServiceCollection>
    {
        private readonly IUnityContainer _container;

        public ServiceProviderFactory(IUnityContainer container)
        {
            _container = container ?? new UnityContainer();

            _container.RegisterInstance<IServiceProviderFactory<IUnityContainer>>(this, new ContainerControlledLifetimeManager());
            _container.RegisterInstance<IServiceProviderFactory<IServiceCollection>>(this, new ExternallyControlledLifetimeManager());
        }

        public IServiceProvider CreateServiceProvider(IUnityContainer container)
        {
            return new ServiceProvider(container);
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return new ServiceProvider(CreateServiceProviderContainer(containerBuilder));
        }

        IUnityContainer IServiceProviderFactory<IUnityContainer>.CreateBuilder(IServiceCollection services)
        {
            return CreateServiceProviderContainer(services);
        }

        IServiceCollection IServiceProviderFactory<IServiceCollection>.CreateBuilder(IServiceCollection services)
        {
            return services;
        }


        private IUnityContainer CreateServiceProviderContainer(IServiceCollection services)
        {
            var container = _container.CreateChildContainer();
            new ServiceProviderFactory(container);

            return ((UnityContainer)container).AddExtension(new MdiExtension())
                            .AddServices(services);
        }
    }
}
