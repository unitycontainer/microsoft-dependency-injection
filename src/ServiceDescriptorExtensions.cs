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

        internal static string GetRegistrationName(this ServiceDescriptor service)
        {
            if (service.ImplementationType != null)
            {
                return service.ImplementationType.FullName;
            }
            else if (service.ImplementationInstance != null)
            {
                return service.ImplementationInstance.GetType().FullName;
            }
            else if (service.ImplementationFactory != null)
            {
                var typeArguments = service.ImplementationFactory.GetType().GenericTypeArguments;
                return typeArguments[1].FullName;
            }

            return null;
        }
    }
}
