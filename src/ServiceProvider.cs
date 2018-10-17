using Microsoft.Extensions.DependencyInjection;
using System;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    public class ServiceProvider : IServiceProvider,
                                   IServiceScopeFactory,
                                   IServiceScope,
                                   IDisposable
    {
        private readonly UnityConfigurationOptions _options;

        internal ServiceProvider(UnityConfigurationOptions options)
        {
            _options = options;
            _options.UnityContainer.RegisterInstance<IServiceScope>(this, new ExternallyControlledLifetimeManager());
            _options.UnityContainer.RegisterInstance<IServiceProvider>(this, new ExternallyControlledLifetimeManager());
            _options.UnityContainer.RegisterInstance<IServiceScopeFactory>(this, new ExternallyControlledLifetimeManager());
        }

        #region IServiceProvider

        public object GetService(Type serviceType)
        {
            ResolutionParameters parameters = new ResolutionParameters { Type = serviceType };

            _options.ResolveConfiguration?.Invoke(parameters);

            try
            {
                return _options.UnityContainer.Resolve(parameters.Type, parameters.Name, parameters.ResolverOverrides ?? Array.Empty<Resolution.ResolverOverride>());
            }
            catch
            {
                parameters.ResolutionFailureHanlder?.Invoke(serviceType);
            }

            return null;
        }

        #endregion


        #region IServiceScopeFactory

        public IServiceScope CreateScope()
        {
            var childOptions = _options.With(_options.UnityContainer.CreateChildContainer());
            childOptions.CreateScope?.Invoke(childOptions);

            return new ServiceProvider(childOptions);
        }

        #endregion


        #region IServiceScope

        IServiceProvider IServiceScope.ServiceProvider => this;

        #endregion


        #region Public Members

        public static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var container = new UnityContainer().AddExtension(new MdiExtension())
                                                           .AddServices(services);

            return new ServiceProvider(new UnityConfigurationOptions { UnityContainer = container });
        }

        public static explicit operator UnityContainer(ServiceProvider c)
        {
            return (UnityContainer)c._options.UnityContainer;
        }

        #endregion


        #region Disposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool _)
        {
            _options.UnityContainer?.Dispose();
        }

        ~ServiceProvider()
        {
            Dispose(false);
        }

        #endregion
    }
}
