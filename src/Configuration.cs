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
    public static void Register(IServiceCollection services, IUnityContainer _container)
    {
      _container.RegisterType<IServiceScopeFactory, ServiceScopeFactory>();
      _container.RegisterType<IServiceScope, ServiceScope>();
      _container.RegisterType<IServiceProvider, ServiceProvider>();

      RegisterEnumerable(_container);

      HashSet<Type> aggregateTypes = GetAggregateTypes(services);

      MethodInfo registerInstance = RegisterInstance();

      Func<ServiceDescriptor, LifetimeManager> lifetime = GetLifetime();

      // Configure all registrations into Unity
      foreach (ServiceDescriptor serviceDescriptor in services)
      {
        RegisterType(
          _container,
          lifetime,
          serviceDescriptor,
          aggregateTypes,
          registerInstance);
      }
    }

    private static MethodInfo RegisterInstance()
    {
      return
        typeof(UnityContainerExtensions)
          .GetRuntimeMethods()
          .Single(o =>
            o.IsStatic &&
            o.IsPublic &&
            o.IsGenericMethod &&
            o.Name == "RegisterInstance" &&
            o.GetParameters().Length == 4);
    }

    private static HashSet<Type> GetAggregateTypes(IServiceCollection services)
    {
      var aggregateTypes = new HashSet<Type>
      (
        services.GroupBy
          (
            serviceDescriptor => serviceDescriptor.ServiceType,
            serviceDescriptor => serviceDescriptor
          )
          .Where(typeGrouping => typeGrouping.Count() > 1)
          .Select(type => type.Key)
      );

      return aggregateTypes;
    }

    private static void RegisterEnumerable(IUnityContainer _container)
    {
      _container.RegisterType
      (
        typeof(IEnumerable<>),
        new InjectionFactory
        (
          (container, enumerableType, name) =>
          {
            Type type = enumerableType.GenericTypeArguments.Single();
            Type genericType = type.GetGenericTypeDefinition();

            object[] allInstances = container.ResolveAll(type).Concat
            (
              _container.IsRegistered(type) ||
              type.GenericTypeArguments.Any() &&
              _container.IsRegistered(genericType)
                ? new[] {container.Resolve(type)}
                : new object[] { }
            )
            .ToArray();

            return
              typeof(Enumerable).GetRuntimeMethod("OfType", new[] {typeof(object[])})
                .MakeGenericMethod(type)
                .Invoke(null, new object[] {allInstances});
          }
        )
      );
    }

    private static Func<ServiceDescriptor, LifetimeManager> GetLifetime()
    {
      return serviceDescriptor =>
      {
        switch (serviceDescriptor.Lifetime)
        {
          case ServiceLifetime.Scoped:
            return new HierarchicalLifetimeManager();

          case ServiceLifetime.Singleton:
            return new ContainerControlledLifetimeManager();

          case ServiceLifetime.Transient:
            return new TransientLifetimeManager();

          default:
            throw new NotImplementedException($"Unsupported lifetime manager type '{serviceDescriptor.Lifetime}'");
        }
      };
    }

    private static void RegisterType(
      IUnityContainer _container,
      Func<ServiceDescriptor, LifetimeManager> fetchLifetime,
      ServiceDescriptor serviceDescriptor,
      ICollection<Type> aggregateTypes,
      MethodInfo miRegisterInstanceOpen)
    {
      LifetimeManager lifetimeManager = fetchLifetime(serviceDescriptor);
      bool isAggregateType = aggregateTypes.Contains(serviceDescriptor.ServiceType);

      if (serviceDescriptor.ImplementationType != null)
      {
        RegisterImplementation(_container, serviceDescriptor, isAggregateType, lifetimeManager);
      }
      else if (serviceDescriptor.ImplementationFactory != null)
      {
        RegisterFactory(_container, serviceDescriptor, isAggregateType, lifetimeManager);
      }
      else if (serviceDescriptor.ImplementationInstance != null)
      {
        RegisterSingleton(_container, serviceDescriptor, miRegisterInstanceOpen, isAggregateType, lifetimeManager);
      }
      else
      {
        throw new InvalidOperationException("Unsupported registration type");
      }
    }

    private static void RegisterImplementation(
      IUnityContainer _container,
      ServiceDescriptor serviceDescriptor,
      bool isAggregateType,
      LifetimeManager lifetimeManager)
    {
      if (isAggregateType)
      {
        _container.RegisterType(
          serviceDescriptor.ServiceType,
          serviceDescriptor.ImplementationType,
          serviceDescriptor.ImplementationType.AssemblyQualifiedName,
          lifetimeManager);
      }
      else
      {
        _container.RegisterType(
          serviceDescriptor.ServiceType,
          serviceDescriptor.ImplementationType,
          lifetimeManager);
      }
    }

    private static void RegisterFactory(
      IUnityContainer _container,
      ServiceDescriptor serviceDescriptor,
      bool isAggregateType,
      LifetimeManager lifetimeManager)
    {
      if (isAggregateType)
      {
        _container.RegisterType
        (
          serviceDescriptor.ServiceType,
          serviceDescriptor.ImplementationType.AssemblyQualifiedName,
          lifetimeManager,
          new InjectionFactory
          (
            container =>
            {
              var serviceProvider = container.Resolve<IServiceProvider>();
              object instance = serviceDescriptor.ImplementationFactory(serviceProvider);
              return instance;
            }
          )
        );
      }
      else
      {
        _container.RegisterType
        (
          serviceDescriptor.ServiceType,
          lifetimeManager,
          new InjectionFactory
          (
            container =>
            {
              var serviceProvider = container.Resolve<IServiceProvider>();
              object instance = serviceDescriptor.ImplementationFactory(serviceProvider);
              return instance;
            }
          )
        );
      }
    }

    private static void RegisterSingleton(
      IUnityContainer _container,
      ServiceDescriptor serviceDescriptor,
      MethodInfo miRegisterInstanceOpen,
      bool isAggregateType,
      LifetimeManager lifetimeManager)
    {
      if (isAggregateType)
      {
        miRegisterInstanceOpen
          .MakeGenericMethod(serviceDescriptor.ServiceType)
          .Invoke(null,
            new[]
            {
              _container, serviceDescriptor.ImplementationType.AssemblyQualifiedName,
              serviceDescriptor.ImplementationInstance, lifetimeManager
            });
      }
      else
      {
        _container.RegisterInstance(
          serviceDescriptor.ServiceType,
          serviceDescriptor.ImplementationInstance,
          lifetimeManager);
      }
    }
  }
}