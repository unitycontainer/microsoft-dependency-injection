using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Unity.Microsoft.DependencyInjection.Tests
{
    public class Tests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            return serviceCollection.BuildServiceProvider();
        }
    }
}