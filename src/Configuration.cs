using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Unity.Injection;
using Unity.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    public static class Configuration
    {
        static Aggregates _aggregates;

        public static IServiceProvider Configure(this IUnityContainer container, IServiceCollection services)
        {
            container.AddNewExtension<MDIExtension>();

            var provider = new ServiceProvider(container);

            var aggregateTypes = GetAggregateTypes(services);

            var aggregateList = aggregateTypes.Select(t => new Aggregate(t, container)).ToList();
            _aggregates = new Aggregates(aggregateList);
            container.RegisterInstance(_aggregates);

            // Configure all registrations into Unity
            foreach (var serviceDescriptor in services)
            {
                container.RegisterType(serviceDescriptor, _aggregates);
            }
            _aggregates.Register();

            container.RegisterInstance<IServiceScopeFactory>(provider);
            container.RegisterType<TransientObjectPool>(new HierarchicalLifetimeManager());
            
            return provider;
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
            var aggregateTypes = new HashSet<Type>
                (
                services.
                    GroupBy
                    (
                        serviceDescriptor => serviceDescriptor.ServiceType,
                        serviceDescriptor => serviceDescriptor
                    ).
                    Where(typeGrouping => typeGrouping.Count() > 1).
                    Select(type => type.Key)
                );
            return aggregateTypes;
        }

        

        private static void RegisterType(this IUnityContainer container,
            ServiceDescriptor serviceDescriptor, Aggregates aggregates)
        {
            var aggregate = aggregates.Get(serviceDescriptor.ServiceType);
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
            if (type.IsClass && !type.IsAbstract)
            {
                if (typeof(Delegate).IsAssignableFrom(type) || typeof(string) == type || type.IsEnum
                    || type.IsArray || type.IsPrimitive)
                {
                    return container.IsRegistered(type);
                }
                return true;
            }

            if (type.IsGenericType)
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