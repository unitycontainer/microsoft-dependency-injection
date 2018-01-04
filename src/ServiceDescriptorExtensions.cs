using Microsoft.Extensions.DependencyInjection;
using System;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    internal static class ServiceDescriptorExtensions
    {
        internal static Type GetImplementationType(this ServiceDescriptor service)
        {
            if (service.ImplementationType != null)
            {
                return service.ImplementationType;
            }
            else if (service.ImplementationInstance != null)
            {
                return service.ImplementationInstance.GetType();
            }
            else if (service.ImplementationFactory != null)
            {
                var typeArguments = service.ImplementationFactory.GetType().GenericTypeArguments;
                return typeArguments[1];
            }
            return null;
        }

        internal static LifetimeManager GetLifetime(this ServiceDescriptor serviceDescriptor, IUnityContainer container)
        {
            switch (serviceDescriptor.Lifetime)
            {
                case ServiceLifetime.Scoped:
                    return new HierarchicalLifetimeManager();
                case ServiceLifetime.Singleton:
                    return new ContainerControlledLifetimeManager();
                case ServiceLifetime.Transient:
                    return new HierarchicalTransientLifetimeManager();
                default:
                    throw new NotImplementedException(
                        $"Unsupported lifetime manager type '{serviceDescriptor.Lifetime}'");
            }
        }
    }
}
