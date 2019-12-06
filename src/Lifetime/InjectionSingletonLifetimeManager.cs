using System;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection.Lifetime
{
    public class InjectionSingletonLifetimeManager : ContainerControlledLifetimeManager
    {
        #region Fields

        private readonly ILifetimeContainer _container;
        private bool _disposed;

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
            if (newValue is IDisposable) _container.Add(new DisposableAction(() => 
            { 
                RemoveValue(_container); 
                _disposed = true; 
            }));
        }

        protected override LifetimeManager OnCreateLifetimeManager()
        {
            return new InjectionSingletonLifetimeManager(_container);
        }

        public override void RemoveValue(ILifetimeContainer container = null)
        {
            _disposed = true;
            base.RemoveValue(container);
        }

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
