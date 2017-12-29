using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace Unity.Microsoft.DependencyInjection.Tests
{
    public class Tests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var container = new UnityContainer();
            return container.Configure(serviceCollection);
        }

        internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection, IList<ServiceDescriptor>, ICollection<ServiceDescriptor>, IEnumerable<ServiceDescriptor>, IEnumerable
        {
        }

        [Fact]
        public void Disposing_ScopeDisposesService()
        {

            // Arrange

            var collection = new TestServiceCollection();

            collection.AddSingleton<IFakeSingletonService, FakeService>();
            collection.AddScoped<IFakeScopedService, FakeService>();
            collection.AddTransient<IFakeService, FakeService>();



            var provider = CreateServiceProvider(collection);

            FakeService disposableService;
            FakeService transient1;
            FakeService transient2;
            FakeService singleton;



            // Act and Assert

            var transient3 = Assert.IsType<FakeService>(provider.GetService<IFakeService>());

            using (var scope = provider.CreateScope())
            {
                disposableService = (FakeService)scope.ServiceProvider.GetService<IFakeScopedService>();
                transient1 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                transient2 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                singleton = (FakeService)scope.ServiceProvider.GetService<IFakeSingletonService>();


                Assert.False(disposableService.Disposed);
                Assert.False(transient1.Disposed);
                Assert.False(transient2.Disposed);
                Assert.False(singleton.Disposed);

            }

            Assert.True(disposableService.Disposed);
            Assert.True(transient1.Disposed);
            Assert.True(transient2.Disposed);
            Assert.False(singleton.Disposed);


            var disposableProvider = provider as IDisposable;
            if (disposableProvider != null)
            {
                disposableProvider.Dispose();
                Assert.True(singleton.Disposed);
                Assert.True(transient3.Disposed);
            }

        }
    }
}