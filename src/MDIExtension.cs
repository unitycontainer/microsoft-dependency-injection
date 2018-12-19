using Unity.Extension;
using Unity.Microsoft.DependencyInjection.Policy;
using Unity.Policy;

namespace Unity.Microsoft.DependencyInjection
{
    internal class MdiExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Context.Policies.Set<IConstructorSelectorPolicy>(new ConstructorSelectorPolicy());
        }

        public ILifetimeContainer Lifetime => Context.Lifetime;
    }
}
