using System;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection.Lifetime
{
    /// <summary>
    /// A special lifetime manager which works like <see cref="TransienLifetimeManager"/>,
    /// except it makes container remember all Disposable objects it created. Once container
    /// is disposed all these objects are disposed as well.
    /// </summary>
    public class InjectionTransientLifetimeManager : LifetimeManager, 
                                                     IFactoryLifetimeManager,
                                                     ITypeLifetimeManager
    {
        public override void SetValue(object newValue, ILifetimeContainer container = null)
        {
            if (newValue is IDisposable disposable)
                container?.Add(disposable);
        }

        protected override LifetimeManager OnCreateLifetimeManager() => this;

        public override bool InUse
        {
            get => false;
            set { }
        }



        #region Overrides

        /// <summary>
        /// This method provides human readable representation of the lifetime
        /// </summary>
        /// <returns>Name of the lifetime</returns>
        public override string ToString() => "Lifetime:InjectionTransient";

        #endregion
    }
}
