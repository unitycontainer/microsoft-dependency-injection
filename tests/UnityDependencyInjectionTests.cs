using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Unity.Microsoft.DependencyInjection.Tests
{
  public sealed class UnityDependencyInjectionTests : DependencyInjectionSpecificationTests
  {
    /// <inheritdoc />
    protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
    {
      var container = new UnityContainer();
      var provider = new ServiceProvider(container);

      foreach (ServiceDescriptor service in serviceCollection)
      {
        container.RegisterType(
          service.ServiceType,
          service.ImplementationType);
      }

      return provider;
    }
  }
}