using System;
using System.Collections.Generic;
using System.Linq;
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
            return ServiceProvider.ConfigureServices(serviceCollection);
        }

        [Fact]
        public void Disposes_InReverseOrderOfCreation()
        {
            TestServiceCollection serviceCollection = new TestServiceCollection();
            ServiceCollectionServiceExtensions.AddSingleton<FakeDisposeCallback>(serviceCollection);
            ServiceCollectionServiceExtensions.AddTransient<IFakeOuterService, FakeDisposableCallbackOuterService>(serviceCollection);
            ServiceCollectionServiceExtensions.AddSingleton<IFakeMultipleService, FakeDisposableCallbackInnerService>(serviceCollection);
            ServiceCollectionServiceExtensions.AddScoped<IFakeMultipleService, FakeDisposableCallbackInnerService>(serviceCollection);
            ServiceCollectionServiceExtensions.AddTransient<IFakeMultipleService, FakeDisposableCallbackInnerService>(serviceCollection);
            ServiceCollectionServiceExtensions.AddSingleton<IFakeService, FakeDisposableCallbackInnerService>(serviceCollection);
            IServiceProvider provider1 = this.CreateServiceProvider(serviceCollection);
            FakeDisposeCallback callback = ServiceProviderServiceExtensions.GetService<FakeDisposeCallback>(provider1);
            IFakeOuterService service = ServiceProviderServiceExtensions.GetService<IFakeOuterService>(provider1);
            ((IDisposable)provider1).Dispose();
            Assert.Equal<object>(service, callback.Disposed[0]);
            Assert.Equal<IFakeMultipleService>(Enumerable.Reverse<IFakeMultipleService>(service.MultipleServices), 
                                               Enumerable.OfType<IFakeMultipleService>(Enumerable.Take<object>(Enumerable.Skip<object>((IEnumerable<object>)callback.Disposed, 1), 3)));
            Assert.Equal<object>(service.SingleService, callback.Disposed[4]);
        }

        internal class TestServiceCollection : List<ServiceDescriptor>, 
                                               IServiceCollection, 
                                               IList<ServiceDescriptor>, 
                                               ICollection<ServiceDescriptor>, 
                                               IEnumerable<ServiceDescriptor>
        {
        }

    }
}