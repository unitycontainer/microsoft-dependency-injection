using System;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection.Lifetime
{
    internal class ServiceProviderLifetimeManager : LifetimeManager, IInstanceLifetimeManager
    {
        IServiceProvider _serviceProvider;

        public ServiceProviderLifetimeManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object GetValue(ILifetimeContainer container = null)
        {
            return _serviceProvider;
        }

        protected override LifetimeManager OnCreateLifetimeManager()
        {
            throw new NotImplementedException();
        }
    }
}
