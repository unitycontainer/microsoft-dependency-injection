using System;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection.Lifetime
{
    public class InjectionSingletonLifetimeManager : ContainerControlledLifetimeManager
    {
        #region Fields

        private ILifetimeContainer _lifetime;
        
        #endregion


        public InjectionSingletonLifetimeManager(ILifetimeContainer lifetime)
        {
            _lifetime = lifetime;
        }


        protected override void SynchronizedSetValue(object newValue, ILifetimeContainer container = null)
        {
            base.SynchronizedSetValue(newValue, container);
            _lifetime.Add(new DisposableAction(() => RemoveValue(_lifetime)));
        }

        protected override LifetimeManager OnCreateLifetimeManager()
        {
            return new InjectionSingletonLifetimeManager(_lifetime);
        }


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
