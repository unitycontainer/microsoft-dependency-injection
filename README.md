[![Build status](https://ci.appveyor.com/api/projects/status/sevk2yb2jokf8ltr/branch/master?svg=true)](https://ci.appveyor.com/project/IoC-Unity/microsoft-dependency-injection/branch/master)
[![License](https://img.shields.io/badge/license-apache%202.0-60C060.svg)](https://github.com/IoC-Unity/microsoft-dependency-injection/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/dt/Unity.Microsoft.DependencyInjection.svg)](https://www.nuget.org/packages/Unity.Microsoft.DependencyInjection)
[![NuGet](https://img.shields.io/nuget/v/Unity.Microsoft.DependencyInjection.svg)](https://www.nuget.org/packages/Unity.Microsoft.DependencyInjection)

# Unity.Microsoft.DependencyInjection

Unity extension to integrate with [Microsoft.Extensions.DependencyInjection](https://github.com/aspnet/DependencyInjection)  compliant systems

## Getting Started

- Reference the `Unity.Microsoft.DependencyInjection` package from NuGet.

```shell
Install-Package Unity.Microsoft.DependencyInjection
```

## Registration:

- In the `WebHostBuilder` add `UseUnityServiceProvider(...)` method

```C#
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseUnityServiceProvider()   <---- Add this line
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

- In case Unity container configured via application configuration or by convention this container could be used to initialize service provider.

```C#
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseUnityServiceProvider(_container)   <---- Add this line
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

- Add optional method to your `Startup` class

```C#
public void ConfigureContainer(IUnityContainer container)
{
  // Could be used to register more types
  container.RegisterType<IMyService, MyService>();
}
```

### Resolving Controllers from Unity

By default ASP resolves controllers using built in activator. To enable resolution of controllers from Unity you need to add following line to MVC configuration:

```C#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddControllersAsServices(); <-- Add this line
    ...
}
```

## Examples

For example of using Unity with Core 3.1 Web application follow [this link](https://github.com/unitycontainer/examples/tree/master/src/web/ASP.Net.Unity.Example)

## Code of Conduct

This project has adopted the code of conduct defined by the [Contributor Covenant](https://www.contributor-covenant.org/) to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](https://www.dotnetfoundation.org/code-of-conduct)

## Contributing

See the [Contributing guide](https://github.com/unitycontainer/unity/blob/master/CONTRIBUTING.md) for more information.

## .NET Foundation

Unity Container is a [.NET Foundation](https://dotnetfoundation.org/projects/unitycontainer) project
