using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Unity.Microsoft.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUnity(this IServiceCollection services, Action<IUnityContainer> configurationAction = null)
        {
            return services.AddSingleton<IServiceProviderFactory<IUnityContainer>>(new ServiceProviderFactory(configurationAction));
        }
    }
}
