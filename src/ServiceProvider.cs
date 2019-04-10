using Microsoft.Extensions.DependencyInjection;
using System;
using Unity.Lifetime;
using Unity.Microsoft.DependencyInjection.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    public class ServiceProvider : IServiceProvider, 
                                   IServiceScopeFactory, 
                                   IServiceScope, 
                                   IDisposable
    {
        private IUnityContainer _container;


        internal ServiceProvider(IUnityContainer container)
        {
            _container = container;
            _container.RegisterInstance<IServiceScope>(this, new ExternallyControlledLifetimeManager());
            _container.RegisterInstance<IServiceProvider>(this, new ServiceProviderLifetimeManager(this));
            _container.RegisterInstance<IServiceScopeFactory>(this, new ExternallyControlledLifetimeManager());
        }

        #region IServiceProvider

        public object GetService(Type serviceType)
        {
            if (null == _container)
            {
                throw new ObjectDisposedException(nameof(IServiceProvider));
            }

            try
            {
                return _container.Resolve(serviceType);
            }
            catch  { /* Ignore */}

            return null;
        }

        #endregion


        #region IServiceScopeFactory

        public IServiceScope CreateScope()
        {
            return new ServiceProvider(_container.CreateChildContainer());
        }

        #endregion


        #region IServiceScope

        IServiceProvider IServiceScope.ServiceProvider => this;

        #endregion


        #region Public Members

        public static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return new ServiceProvider(new UnityContainer()
                .AddExtension(new MdiExtension())
                .AddServices(services));
        }

        public static explicit operator UnityContainer(ServiceProvider c)
        {
            return (UnityContainer)c._container;
        }

        #endregion


        #region Disposable

        public void Dispose()
        {
            IDisposable disposable = _container;
            _container = null;
            disposable?.Dispose();
        }

        #endregion
    }
}
