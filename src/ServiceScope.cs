using System;

using Microsoft.Extensions.DependencyInjection;

namespace Unity.Microsoft.DependencyInjection
{
    public class ServiceScope : IServiceScope
    {
        private IUnityContainer _container;
        private ServiceProvider _provider;

        IServiceProvider IServiceScope.ServiceProvider
        {
            get
            {
                if (_provider == null)
                    _provider = new ServiceProvider(_container.CreateChildContainer());
                return _provider;
            }
        }

        public ServiceScope(IUnityContainer container)
        {
            _container = container;
        }

        public void Dispose()
        {
            var disposable = _provider;
            _container = null;
            _provider = null;
            disposable?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}