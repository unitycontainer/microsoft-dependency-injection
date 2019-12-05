using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Unity.Microsoft.DependencyInjection.Tests
{
    public class Tests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            return serviceCollection.BuildServiceProvider();
        }

        [Theory]
        [InlineData(typeof(IFakeService), typeof(FakeService), typeof(IFakeService), ServiceLifetime.Scoped)]
        [InlineData(typeof(IFakeService), typeof(FakeService), typeof(IFakeService), ServiceLifetime.Singleton)]
        [InlineData(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), typeof(IFakeOpenGenericService<IServiceProvider>), ServiceLifetime.Scoped)]
        [InlineData(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), typeof(IFakeOpenGenericService<IServiceProvider>), ServiceLifetime.Singleton)]
        public void Resolves_DifferentInstancesForServiceWhenResolvingEnumerable(Type serviceType, Type implementation, Type resolve, ServiceLifetime lifetime)
        {
            // Arrange
            var serviceCollection = new TestServiceCollection
            {
                ServiceDescriptor.Describe(serviceType, implementation, lifetime),
                ServiceDescriptor.Describe(serviceType, implementation, lifetime),
                ServiceDescriptor.Describe(serviceType, implementation, lifetime)
            };

            var serviceProvider = CreateServiceProvider(serviceCollection);
            using (var scope = serviceProvider.CreateScope())
            {
                var type = typeof(IEnumerable<>).MakeGenericType(resolve);
                var enumerable = (scope.ServiceProvider.GetService(type) as IEnumerable)
                    .OfType<object>().ToArray();
                var service = scope.ServiceProvider.GetService(resolve);

                var hash = service.GetHashCode();
                var mhash = enumerable.Select(e => e.GetHashCode()).ToArray();

                // Assert
                Assert.Equal(3, enumerable.Length);
                Assert.NotNull(enumerable[0]);
                Assert.NotNull(enumerable[1]);
                Assert.NotNull(enumerable[2]);

                Assert.NotEqual(enumerable[0], enumerable[1]);
                Assert.NotEqual(enumerable[1], enumerable[2]);
                Assert.Equal(service, enumerable[2]);
            }
        }

        [Fact]
        public void Resolves_MixedOpenClosedGenericsAsEnumerable()
        {
            // Arrange
            var serviceCollection = new TestServiceCollection();
            var instance = new FakeOpenGenericService<PocoClass>(null);

            serviceCollection.AddTransient<PocoClass, PocoClass>();
            serviceCollection.AddSingleton(typeof(IFakeOpenGenericService<PocoClass>), typeof(FakeService));
            serviceCollection.AddSingleton(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>));
            serviceCollection.AddSingleton<IFakeOpenGenericService<PocoClass>>(instance);

            var serviceProvider = CreateServiceProvider(serviceCollection);

            var enumerable = serviceProvider.GetService<IEnumerable<IFakeOpenGenericService<PocoClass>>>().ToArray();

            // Assert
            Assert.Equal(3, enumerable.Length);
            Assert.NotNull(enumerable[0]);
            Assert.NotNull(enumerable[1]);
            Assert.NotNull(enumerable[2]);

            //Assert.Equal(instance, enumerable[2]);
            Assert.IsType<FakeService>(enumerable[0]);
        }
    }


    //public class FakeOpenGenericServic<TVal> : IFakeOpenGenericService<TVal>
    //{
    //    public readonly string id = Guid.NewGuid().ToString();

    //    public FakeOpenGenericServic(TVal value)
    //    {
    //        Value = value;
    //    }

    //    public TVal Value { get; }
    //}

    public class FakeService : IFakeEveryService, IDisposable
    {
        public FakeService(IUnityContainer container)
        {
            Container = container;
        }

        public IUnityContainer Container { get; private set; }

        public PocoClass Value { get; set; }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(FakeService));
            }

            Disposed = true;
        }
    }

    internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
    }
}