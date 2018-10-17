using Microsoft.Extensions.DependencyInjection;
using System;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    public class ServiceProviderFactory : IServiceProviderFactory<IUnityContainer>,
                                          IServiceProviderFactory<IServiceCollection>
    {
        private readonly Action<UnityConfigurationOptions> _config;

        public ServiceProviderFactory(Action<UnityConfigurationOptions> config)
        {
            _config = config;
        }

        public IServiceProvider CreateServiceProvider(IUnityContainer container)
        {
            return new ServiceProvider(CreateOptions().With(container));
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return new ServiceProvider(CreateServiceProviderContainer(containerBuilder));
        }

        IUnityContainer IServiceProviderFactory<IUnityContainer>.CreateBuilder(IServiceCollection services)
        {
            return CreateServiceProviderContainer(services).UnityContainer;
        }

        IServiceCollection IServiceProviderFactory<IServiceCollection>.CreateBuilder(IServiceCollection services)
        {
            return services;
        }

        private UnityConfigurationOptions CreateServiceProviderContainer(IServiceCollection services)
        {
            var options = CreateOptions();
            options.UnityContainer.AddServices(services);
            return options;
        }

        private UnityConfigurationOptions CreateOptions()
        {
            var options = new UnityConfigurationOptions();
            _config(options);
            options.UnityContainer = options.UnityContainer ?? new UnityContainer();
            ConfigureContainer(options.UnityContainer);

            return options;
        }
        
        private void ConfigureContainer(IUnityContainer container)
        {
            container.AddExtension(new MdiExtension());
            container.RegisterInstance<IServiceProviderFactory<IUnityContainer>>(this, new ContainerControlledLifetimeManager());
            container.RegisterInstance<IServiceProviderFactory<IServiceCollection>>(this, new ExternallyControlledLifetimeManager());
        }
    }
}
