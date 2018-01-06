using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Unity.Injection;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    internal static class Configuration
    {

        internal static IUnityContainer AddServices(this IUnityContainer container, IServiceCollection services)
        {
            var lifetime = container.Configure<MDIExtension>()
                                    .Lifetime;

            foreach (var group in services.GroupBy(serviceDescriptor => serviceDescriptor.ServiceType,
                                                   serviceDescriptor => serviceDescriptor)
                                          .Select(group => group.ToArray()))
            {
                // Register named types
                for (var i = 0; i < group.Length - 1; i++)
                {
                    var descriptor = group[i];
                    container.Register(descriptor, descriptor.GetRegistrationName(), lifetime);
                }

                // Register default types
                container.Register(group[group.Length - 1], null, lifetime);
            }

            return container;
        }

        internal static void Register(this IUnityContainer container,
            ServiceDescriptor serviceDescriptor, string qualifier, ILifetimeContainer lifetime)
        {
            if (serviceDescriptor.ImplementationType != null)
            {
                container.RegisterType(serviceDescriptor.ServiceType,
                                       serviceDescriptor.ImplementationType,
                                       qualifier,
                                       serviceDescriptor.GetLifetime(lifetime));
            }
            else if (serviceDescriptor.ImplementationFactory != null)
            {
                container.RegisterType(serviceDescriptor.ServiceType, 
                                       qualifier, 
                                       serviceDescriptor.GetLifetime(lifetime),
                                        new InjectionFactory(scope =>
                                        {
                                            var serviceProvider = serviceDescriptor.Lifetime == ServiceLifetime.Scoped
                                                ? scope.Resolve<IServiceProvider>()
                                                : container.Resolve<IServiceProvider>();
                                            var instance = serviceDescriptor.ImplementationFactory(serviceProvider);
                                            return instance;
                                        }));
            }
            else if (serviceDescriptor.ImplementationInstance != null)
            {
                container.RegisterInstance(serviceDescriptor.ServiceType,
                                           qualifier,
                                           serviceDescriptor.ImplementationInstance,
                                           serviceDescriptor.GetLifetime(lifetime));
            }
            else
            {
                throw new InvalidOperationException("Unsupported registration type");
            }
        }


        internal static LifetimeManager GetLifetime(this ServiceDescriptor serviceDescriptor, ILifetimeContainer lifetime)
        {
            switch (serviceDescriptor.Lifetime)
            {
                case ServiceLifetime.Scoped:
                    return new HierarchicalLifetimeManager();
                case ServiceLifetime.Singleton:
                    return new InjectionSingletonLifetimeManager(lifetime);
                case ServiceLifetime.Transient:
                    return new InjectionTransientLifetimeManager();
                default:
                    throw new NotImplementedException(
                        $"Unsupported lifetime manager type '{serviceDescriptor.Lifetime}'");
            }
        }


        internal static bool CanResolve(this IUnityContainer container, Type type)
        {
            var info = type.GetTypeInfo();

            if (info.IsClass && !info.IsAbstract)
            {
                if (typeof(Delegate).GetTypeInfo().IsAssignableFrom(info) || typeof(string) == type || info.IsEnum
                    || type.IsArray || info.IsPrimitive)
                {
                    return container.IsRegistered(type);
                }
                return true;
            }

            if (info.IsGenericType)
            {
                var gerericType = type.GetGenericTypeDefinition();
                if ((gerericType == typeof(IEnumerable<>)) ||
                    container.IsRegistered(gerericType))
                {
                    return true;
                }
            }

            return container.IsRegistered(type);
        }
    }
}