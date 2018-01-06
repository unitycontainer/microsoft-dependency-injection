using System;
using System.Threading;
using Unity.Exceptions;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    public class InjectionSingletonLifetimeManager : LifetimeManager, IRequiresRecovery
    {
        #region Fields

        protected object _value;
        private ILifetimeContainer _lifetime;
        private readonly object _lockObj = new object();

        #endregion


        public InjectionSingletonLifetimeManager(ILifetimeContainer lifetime)
        {
            _lifetime = lifetime;
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
            var result = SynchronizedGetValue(container);
            if (result != null)
            {
                Monitor.Exit(_lockObj);
            }
            return result;
        }

        /// <summary>
        /// Performs the actual retrieval of a value from the backing store associated 
        /// with this Lifetime policy.
        /// </summary>
        /// <returns>the object desired, or null if no such object is currently stored.</returns>
        /// <remarks>This method is invoked by <see cref="SynchronizedLifetimeManager.GetValue"/>
        /// after it has acquired its lock.</remarks>
        protected virtual object SynchronizedGetValue(ILifetimeContainer container)
        {
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
            SynchronizedSetValue(newValue, container);
            TryExit();
        }

        /// <summary>
        /// Performs the actual storage of the given value into backing store for retrieval later.
        /// </summary>
        /// <param name="newValue">The object being stored.</param>
        /// <param name="container"></param>
        /// <remarks>This method is invoked by <see cref="SynchronizedLifetimeManager.SetValue"/>
        /// before releasing its lock.</remarks>
        protected virtual void SynchronizedSetValue(object newValue, ILifetimeContainer container)
        {
            _value = newValue;
            if (_value is IDisposable disposable) _lifetime.Add(disposable);
        }

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

        public override void RemoveValue(ILifetimeContainer container = null)
        {
            TryExit();
        }

        protected override LifetimeManager OnCreateLifetimeManager()
        {
            return new InjectionSingletonLifetimeManager(_lifetime);
        }
    }
}
