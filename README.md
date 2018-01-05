[![Build status](https://ci.appveyor.com/api/projects/status/sevk2yb2jokf8ltr/branch/master?svg=true)](https://ci.appveyor.com/project/IoC-Unity/microsoft-dependency-injection/branch/master)
[![License](https://img.shields.io/badge/license-apache%202.0-60C060.svg)](https://github.com/IoC-Unity/microsoft-dependency-injection/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/dt/Unity.Microsoft.DependencyInjection.svg)](https://www.nuget.org/packages/Unity.Microsoft.DependencyInjection)
[![NuGet](https://img.shields.io/nuget/v/Unity.Microsoft.DependencyInjection.svg)](https://www.nuget.org/packages/Unity.Microsoft.DependencyInjection)

# Unity.Microsoft.DependencyInjection
Unity extension to integrate with [Microsoft.Extensions.DependencyInjection.Abstractions](https://github.com/aspnet/DependencyInjection)  compliant systems

## Get Started
- Reference the `Unity.Microsoft.DependencyInjection` package from NuGet.
```
Install-Package Unity.Microsoft.DependencyInjection
```

## First way:
- In the `WebHostBuilder` add `ConfigureServices(services => services.AddUnity())` method

```C#
public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .ConfigureServices(services => services.AddUnity())
        .UseStartup<Startup>()
        .Build();
```
- Add method to your `Startup` class
```C#
public void ConfigureContainer(IUnityContainer container)
{
  container.RegisterType<IMyService, MyService>();
}
```

## Second way:
- In the `ConfigureServices` method of your `Startup` class...
  - Register services from the `IServiceCollection`.
  - Build your container.
  - Call `ConfigureServices` extension on `IUnityContainer` and return it.

```C#
public IServiceProvider ConfigureServices(IServiceCollection services)
{
  services.AddMvc();
  
  var container = new UnityContainer();
  
  container.RegisterType<IMyService, MyService>();
  
  return container.ConfigureServices(services);
}
```
