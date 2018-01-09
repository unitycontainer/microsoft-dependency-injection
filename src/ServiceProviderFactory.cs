using System;
using Microsoft.Extensions.DependencyInjection;

namespace Unity.Microsoft.DependencyInjection
{
    internal class ServiceProviderFactory : IServiceProviderFactory<IUnityContainer>
    {
        private readonly Action<IUnityContainer> _configurationAction;

        public ServiceProviderFactory(Action<IUnityContainer> configurationAction = null)
        {
            _configurationAction = configurationAction ?? (container => { });
        }

        public IUnityContainer CreateBuilder(IServiceCollection serviceCollection)
        {
            var unityContainer = new UnityContainer().AddNewExtension<MdiExtension>()
                                                     .AddServices(serviceCollection);
            _configurationAction(unityContainer);

            return unityContainer;
        }

        public IServiceProvider CreateServiceProvider(IUnityContainer unityContainer)
        {
            return new ServiceProvider(unityContainer);
        }
    }
}
