using System;
using System.Threading;
using Unity.Exceptions;
using Unity.Lifetime;
using Unity.Policy.Lifetime;

namespace Unity.Microsoft.DependencyInjection.Lifetime
{
    public class InjectionSingletonLifetimeManager : LifetimeManager, IRequiresRecovery

    {
        #region Fields

        private readonly object _lockObj = new object();
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


        /// <summary>
        /// Retrieve a value from the backing store associated with this Lifetime policy.
        /// </summary>
        /// <returns>the object desired, or null if no such object is currently stored.</returns>
        /// <remarks>Calls to this method acquire a lock which is released only if a non-null value
        /// has been set for the lifetime manager.</remarks>
        public override object GetValue(ILifetimeContainer container = null)
        {
            Monitor.Enter(_lockObj);
            if (_value != null)
            {
                Monitor.Exit(_lockObj);
            }
            return _value;
        }


        /// <summary>
        /// Stores the given value into backing store for retrieval later.
        /// </summary>
        /// <param name="newValue">The object being stored.</param>
        /// <param name="container">The container this value belongs to.</param>
        /// <remarks>Setting a value will attempt to release the lock acquired by 
        /// <see cref="SynchronizedLifetimeManager.GetValue"/>.</remarks>
        public override void SetValue(object newValue, ILifetimeContainer container = null)
        {
            _value = newValue;
            if (_value is IDisposable) _container.Add(new DisposableAction(() => RemoveValue(_container)));
            TryExit();
        }

        #endregion


        #region IRequiresRecovery

        /// <summary>
        /// A method that does whatever is needed to clean up
        /// as part of cleaning up after an exception.
        /// </summary>
        /// <remarks>
        /// Don't do anything that could throw in this method,
        /// it will cause later recover operations to get skipped
        /// and play real havoc with the stack trace.
        /// </remarks>
        public void Recover()
        {
            TryExit();
        }

        protected virtual void TryExit()
        {
#if !NET40
            // Prevent first chance exception when abandoning a lock that has not been entered
            if (!Monitor.IsEntered(_lockObj)) return;
#endif
            try
            {
                Monitor.Exit(_lockObj);
            }
            catch (SynchronizationLockException)
            {
                // Noop here - we don't hold the lock and that's ok.
            }
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
