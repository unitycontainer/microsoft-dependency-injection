using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    public class ServiceProvider : IServiceProvider, IServiceScopeFactory, IDisposable
    {
        protected IUnityContainer container;
        private static Type enumerableType = typeof(IEnumerable<>);

        public ServiceProvider(IUnityContainer container)
        {
            this.container = container; //ExternallyControlledLifetimeManager
            container.RegisterInstance<IServiceProvider>(this, new ExternallyControlledLifetimeManager());
        }

        #region IServiceProvider

        public object GetService(Type serviceType)
        {
            if (!container.CanResolve(serviceType))
                return null;
            var instance = container.Resolve(serviceType);
            return instance;
        }

        #endregion


        #region IServiceScopeFactory

        public IServiceScope CreateScope()
        {
            return CreateScope(container);
        }

        internal static IServiceScope CreateScope(IUnityContainer container)
        {
            return new ServiceScope(container);
        }

        #endregion

        public void Dispose()
        {
            IDisposable disposable = container;
            container = null;
            disposable?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}