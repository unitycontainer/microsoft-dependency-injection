using System;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection.Lifetime
{
    /// <summary>
    /// A special lifetime manager which works like <see cref="TransienLifetimeManager"/>,
    /// except it makes container remember all Disposable objects it created. Once container
    /// is disposed all these objects are disposed as well.
    /// </summary>
    internal class InjectionTransientLifetimeManager : LifetimeManager, 
                                                       IFactoryLifetimeManager,
                                                       ITypeLifetimeManager
    {
        public override void SetValue(object newValue, ILifetimeContainer container = null)
        {
            if (newValue is IDisposable disposable)
                container?.Add(disposable);
        }

        protected override LifetimeManager OnCreateLifetimeManager() => this;
    }
}