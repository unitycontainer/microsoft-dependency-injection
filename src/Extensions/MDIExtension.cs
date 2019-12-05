using Unity.Extension;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    internal class MdiExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
        }

        public ILifetimeContainer Lifetime => Context.Lifetime;
    }
}
