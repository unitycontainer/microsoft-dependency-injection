using Unity.Extension;
using Unity.Lifetime;
using Unity.Policy;

namespace Unity.Microsoft.DependencyInjection
{
    internal class MDIExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Context.Policies.SetDefault<IConstructorSelectorPolicy>(new ConstructorSelectorPolicy());
        }

        public ILifetimeContainer Lifetime => Context.Lifetime;
    }
}
