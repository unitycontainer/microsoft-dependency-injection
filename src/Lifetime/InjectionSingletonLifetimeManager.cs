using System;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection.Lifetime
{
    public class InjectionSingletonLifetimeManager : ContainerControlledLifetimeManager
    {
        #region Fields

        private readonly ILifetimeContainer _container;

        #endregion


        #region Constructors

        public InjectionSingletonLifetimeManager(ILifetimeContainer container)
            : base()
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            Scope = _container.Container;
        }

        #endregion


        #region LifetimeManager

        protected override void SynchronizedSetValue(object newValue, ILifetimeContainer container)
        {
            base.SynchronizedSetValue(newValue, container);
            if (newValue is IDisposable) _container.Add(new DisposableAction(() => RemoveValue(_container) ));
        }

        protected override LifetimeManager OnCreateLifetimeManager()
        {
            return new InjectionSingletonLifetimeManager(_container);
        }

        #endregion


        #region Overrides

        /// <summary>
        /// This method provides human readable representation of the lifetime
        /// </summary>
        /// <returns>Name of the lifetime</returns>
        public override string ToString() => "Lifetime:InjectionPerContainer";

        #endregion


        #region Nested Types

        private class DisposableAction : IDisposable
        {
            private readonly Action _action;

            public DisposableAction(Action action)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }

            public void Dispose()
            {
                _action();
            }
        }

        #endregion
    }
}
