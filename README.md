[![Build status](https://ci.appveyor.com/api/projects/status/sevk2yb2jokf8ltr/branch/master?svg=true)](https://ci.appveyor.com/project/IoC-Unity/microsoft-dependency-injection/branch/master)
[![License](https://img.shields.io/badge/license-apache%202.0-60C060.svg)](https://github.com/IoC-Unity/microsoft-dependency-injection/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/dt/Unity.Microsoft.DependencyInjection.svg)](https://www.nuget.org/packages/Unity.Microsoft.DependencyInjection)
[![NuGet](https://img.shields.io/nuget/v/Unity.Microsoft.DependencyInjection.svg)](https://www.nuget.org/packages/Unity.Microsoft.DependencyInjection)

# Unity.Microsoft.DependencyInjection
Unity extension to integrate with [Microsoft.Extensions.DependencyInjection](https://github.com/aspnet/DependencyInjection)  compliant systems

## Getting Started
- Reference the `Unity.Microsoft.DependencyInjection` package from NuGet.
```
Install-Package Unity.Microsoft.DependencyInjection
```

## Registration:
- In the `WebHostBuilder` add `UseUnityServiceProvider(...)` method

```C#
public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
           .UseUnityServiceProvider()   <---- Add this line
           .UseStartup<Startup>()
           .Build();
```

- In case Unity container configured via application configuration or by convention this container could be used to initialize service provider.

```C#
public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
           .UseUnityServiceProvider(_container)   <---- or add this line
           .UseStartup<Startup>()
           .Build();
```

- Add optional method to your `Startup` class
```C#
public void ConfigureContainer(IUnityContainer container)
{
  // Could be used to register more types
  container.RegisterType<IMyService, MyService>();
}
```

### Startup

At the moment it is not possible to resolve Startup class from Unity even if it is configured as default container. [An Issue](https://github.com/aspnet/Hosting/issues/1309) has been filed with ASPNET team to fix it. Once it is resolved it would be possible to resolve the Startup class itself from the Unity container.

## Examples

For example of using Unity with Core 2.0 Web application follow [this link](https://github.com/unitycontainer/examples/tree/master/src/AspNetCoreExample)

