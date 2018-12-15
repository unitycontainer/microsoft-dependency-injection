using System;
using Unity.Policy;

namespace Unity.Microsoft.DependencyInjection.Lifetime
{
    public class InjectionSingletonLifetimeManager : SynchronizedLifetimeManager

    {
        #region Fields

        private readonly ILifetimeContainer _container;
        private object _value;

        #endregion


        #region Constructors

        public InjectionSingletonLifetimeManager(ILifetimeContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        #endregion


        #region LifetimeManager

        protected override object SynchronizedGetValue(ILifetimeContainer container) => _value;

        protected override void SynchronizedSetValue(object newValue, ILifetimeContainer container)
        {
            _value = newValue;
            if (_value is IDisposable) _container.Add(new DisposableAction(() => RemoveValue(_container)));
        }

        /// <summary>
        /// Remove the given object from backing store.
        /// </summary>
        /// <param name="container">Instance of container</param>
        public override void RemoveValue(ILifetimeContainer container = null)
        {
            if (_value == null) return;
            if (_value is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _value = null;
        }

        protected override LifetimeManager OnCreateLifetimeManager()
        {
            return new InjectionSingletonLifetimeManager(_container);
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
