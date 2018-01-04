using Microsoft.Extensions.DependencyInjection;
using System;

namespace Unity.Microsoft.DependencyInjection
{
    public class ServiceProvider : IServiceProvider, IServiceScopeFactory, IServiceScope, IDisposable
    {
        protected IUnityContainer _container;


        internal ServiceProvider(IUnityContainer container)
        {
            _container = container;
            _container.AddNewExtension<MDIExtension>();
            _container.RegisterInstance<IServiceScope>(this);
            _container.RegisterInstance<IServiceProvider>(this);
            _container.RegisterInstance<IServiceScopeFactory>(this);
        }

        #region IServiceProvider

        public object GetService(Type serviceType)
        {
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


        #region ConfigureServices

        public static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var unity = new UnityContainer();
            unity.Configure(services);    
            return new ServiceProvider(unity);
        }

        #endregion


        #region Disposable

        ~ServiceProvider()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool mode)
        {
            IDisposable disposable = _container;
            _container = null;
            disposable?.Dispose();
        }

        #endregion
    }


    public static class ServiceProviderExtension
    {
        public static IServiceProvider ConfigureServices(this IUnityContainer container, IServiceCollection services)
        {
            var unity = container.CreateChildContainer();
            unity.Configure(services);
            return new ServiceProvider(unity);
        }
    }
}