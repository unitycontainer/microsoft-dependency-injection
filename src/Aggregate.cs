using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Injection;

namespace Unity.Microsoft.DependencyInjection
{
    public class Aggregate
    {
        public Type Type { get; private set; }

        private List<ServiceDescriptor> Services { get; set; } = new List<ServiceDescriptor>();
        private ServiceDescriptor Last;
        private IUnityContainer Container;

        public Aggregate(Type type, IUnityContainer container)
        {
            Type = type;
            Container = container;
        }

        public void AddService(ServiceDescriptor service)
        {
            Services.Add(service);
            Last = service;
        }

        public void Register()
        {
            foreach (var serv in Services)
            {
                var qualifier = serv.GetImplementationType().FullName;
                Container.Register(serv, qualifier);
            }

            Container.RegisterType(Type, Last.GetLifetime(Container),
                new InjectionFactory((c, t, s) =>
                {
                    if (Last.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
                        return c.Resolve(t, Last.GetImplementationType().FullName);
                    var instance = Resolve(c);
                    return instance;
                }));

            var enumType = typeof(IEnumerable<>).MakeGenericType(Type);
            Container.RegisterType(enumType, new HierarchicalTransientLifetimeManager(),
                new InjectionFactory(c =>
                {
                    List<object> instances = new List<object>();
                    foreach (var serv in Services)
                    {
                        if (!serv.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
                        {
                            var qualifier = serv.GetImplementationType().FullName;
                            var instance = Container.Resolve(serv.ServiceType, qualifier);
                            instances.Add(instance);
                        }
                    }
                    return typeof(Enumerable)
                        .GetTypeInfo()
                        .GetDeclaredMethod("Cast")
                        .MakeGenericMethod(Type)
                        .Invoke(null, new[] { instances });
                }));
        }

        public object Resolve(IUnityContainer container)
        {
            return container.Resolve(Type, Last.GetImplementationType().FullName);
        }
    }
}
