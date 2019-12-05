using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Lifetime;
using Unity.Microsoft.DependencyInjection.Lifetime;
using Unity.Policy;
using Unity.Registration;

namespace Unity.Microsoft.DependencyInjection
{
    internal static class Configuration
    {
        internal static IUnityContainer AddServices(this IUnityContainer container, IServiceCollection services)
        {
            var lifetime = ((UnityContainer)container).Configure<MdiExtension>().Lifetime;

            Func<Type, string, InternalRegistration, IPolicySet> registerFunc = ((UnityContainer)container).Register;

            ((UnityContainer)container).Register = OnRegister;


            foreach (var descriptor in services)
            {
                container.Register(descriptor, lifetime);
            }

            ((UnityContainer)container).Register = registerFunc;

            return container;


            IPolicySet OnRegister(Type type, string name, InternalRegistration registration)
            {
                registerFunc(type, null, registration);
                registerFunc(type, Guid.NewGuid().ToString(), registration);
                return null;
            }
        }


        internal static void Register(this IUnityContainer container,
            ServiceDescriptor serviceDescriptor, ILifetimeContainer lifetime)
        {
            if (serviceDescriptor.ImplementationType != null)
            {
                container.RegisterType(serviceDescriptor.ServiceType,
                                       serviceDescriptor.ImplementationType,
                                       null,
                                       (ITypeLifetimeManager)serviceDescriptor.GetLifetime(lifetime));
            }
            else if (serviceDescriptor.ImplementationFactory != null)
            {
                container.RegisterFactory(serviceDescriptor.ServiceType, 
                                        null,
                                        scope =>
                                        {
                                            var serviceProvider = serviceDescriptor.Lifetime == ServiceLifetime.Scoped
                                                ? scope.Resolve<IServiceProvider>()
                                                : container.Resolve<IServiceProvider>();
                                            var instance = serviceDescriptor.ImplementationFactory(serviceProvider);
                                            return instance;
                                        },
                                       (IFactoryLifetimeManager)serviceDescriptor.GetLifetime(lifetime));
            }
            else if (serviceDescriptor.ImplementationInstance != null)
            {
                container.RegisterInstance(serviceDescriptor.ServiceType,
                                           null,
                                           serviceDescriptor.ImplementationInstance,
                                           (IInstanceLifetimeManager)serviceDescriptor.GetLifetime(lifetime));
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