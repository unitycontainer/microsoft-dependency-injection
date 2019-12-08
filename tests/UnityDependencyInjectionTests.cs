using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Unity.Microsoft.DependencyInjection.Specification.Tests
{
    public class Tests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            return serviceCollection.BuildServiceProvider();
        }

        [Fact]
#pragma warning disable xUnit1024 // Test methods cannot have overloads
        public new void ResolvesMixedOpenClosedGenericsAsEnumerable()
#pragma warning restore xUnit1024 // Test methods cannot have overloads
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
            var service = serviceProvider.GetService<IFakeOpenGenericService<PocoClass>>();

            // Assert
            Assert.Equal(3, enumerable.Length);
            Assert.NotNull(enumerable[0]);
            Assert.NotNull(enumerable[1]);
            Assert.NotNull(enumerable[2]);

            Assert.Contains(instance, enumerable);
            Assert.Equal(instance, service);
            Assert.IsType<FakeService>(enumerable[0]);
        }
    }

    internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
    }
}