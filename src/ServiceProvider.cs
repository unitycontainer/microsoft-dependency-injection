using System;

namespace Unity.DependencyInjection
{
  public class ServiceProvider : IServiceProvider
  {
    private readonly IUnityContainer container;

    public ServiceProvider(IUnityContainer container)
    {
      this.container = container;
    }

    public object GetService(Type serviceType)
    {
      return container.Resolve(serviceType);
    }
  }
}