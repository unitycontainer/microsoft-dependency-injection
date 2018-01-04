using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Unity.Injection;

namespace Unity.Microsoft.DependencyInjection
{
    internal static class Configuration
    {
        static List<Aggregate> _aggregates;

        internal static void Configure(this IUnityContainer container, IServiceCollection services)
        {
            var aggregateTypes = GetAggregateTypes(services);

            _aggregates = aggregateTypes.Select(t => new Aggregate(t, container)).ToList();
            container.RegisterInstance(_aggregates);

            // Configure all registrations into Unity
            foreach (var serviceDescriptor in services)
            {
                container.RegisterType(serviceDescriptor, _aggregates);
            }

            foreach (var type in _aggregates)
            {
                type.Register();
            }
        }

        internal static void Register(this IUnityContainer container,
            ServiceDescriptor service, string qualifier)
        {
            if (service.ImplementationType != null)
            {
                RegisterImplementation(container, service, qualifier);
            }
            else if (service.ImplementationFactory != null)
            {
                RegisterFactory(container, service, qualifier);
            }
            else if (service.ImplementationInstance != null)
            {
                RegisterSingleton(container, service, qualifier);
            }
            else
            {
                throw new InvalidOperationException("Unsupported registration type");
            }
        }

        private static HashSet<Type> GetAggregateTypes(IServiceCollection services)
        {
            var enumerable = services.GroupBy(serviceDescriptor => serviceDescriptor.ServiceType,
                                              serviceDescriptor => serviceDescriptor)
                                     .Where(typeGrouping => typeGrouping.Count() > 1)
                                     .Select(type => type.Key);

            return new HashSet<Type>(enumerable);
        }

        

        private static void RegisterType(this IUnityContainer container,
            ServiceDescriptor serviceDescriptor, List<Aggregate> aggregates)
        {
            var aggregate = aggregates.FirstOrDefault(a => a.Type == serviceDescriptor.ServiceType);
            if (aggregate != null)
                aggregate.AddService(serviceDescriptor);
            else
                container.Register(serviceDescriptor, null);
        }

        private static void RegisterImplementation(this IUnityContainer container,
            ServiceDescriptor serviceDescriptor, string qualifier)
        {
            container.RegisterType(serviceDescriptor.ServiceType,
                serviceDescriptor.ImplementationType,
                qualifier,
                serviceDescriptor.GetLifetime(container));
        }

        private static void RegisterFactory(this IUnityContainer container,
            ServiceDescriptor serviceDescriptor, string qualifier)
        {
            container.RegisterType(serviceDescriptor.ServiceType, qualifier, serviceDescriptor.GetLifetime(container),
                new InjectionFactory(scope =>
                {
                    var serviceProvider = serviceDescriptor.Lifetime == ServiceLifetime.Scoped
                        ? scope.Resolve<IServiceProvider>()
                        : container.Resolve<IServiceProvider>();
                    var instance = serviceDescriptor.ImplementationFactory(serviceProvider);
                    return instance;
                }));
        }

        private static void RegisterSingleton(this IUnityContainer container,
            ServiceDescriptor serviceDescriptor, string qualifier)
        {
            container.RegisterInstance(serviceDescriptor.ServiceType,
                qualifier,
                serviceDescriptor.ImplementationInstance,
                serviceDescriptor.GetLifetime(container));
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