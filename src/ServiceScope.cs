using System;

using Microsoft.Extensions.DependencyInjection;

namespace Unity.DependencyInjection
{
  public class ServiceScope : IServiceScope
  {
    private readonly IUnityContainer container;

    public ServiceScope(IUnityContainer container)
    {
      this.container = container.CreateChildContainer();
    }

    public IServiceProvider ServiceProvider =>
      container.Resolve<IServiceProvider>();

    public void Dispose()
    {
      container.Dispose();
    }
  }
}