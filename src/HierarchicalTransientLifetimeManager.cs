using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    /// <summary>
    /// A special lifetime manager which works like <see cref="TransienLifetimeManager"/>,
    /// except that in the presence of child containers, each child gets it's own instance
    /// of the object, instead of sharing one in the common parent.
    /// </summary>
    internal class HierarchicalTransientLifetimeManager : HierarchicalLifetimeManager
    {
        private IUnityContainer _container;

        public HierarchicalTransientLifetimeManager(IUnityContainer container)
        {
            _container = container;
        }

        public override void SetValue(object newValue, ILifetimeContainer container = null)
        {
            _container.Resolve<TransientObjectPool>().Add(newValue);
        }

        public override object GetValue(ILifetimeContainer container = null)
        {
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            _container = null;
            base.Dispose(disposing);
        }
    }
}