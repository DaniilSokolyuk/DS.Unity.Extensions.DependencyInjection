# DS.Unity.Extensions.DependencyInjection
Unity implementation of the interfaces in Microsoft.Extensions.DependencyInjection.Abstractions

## Get Packages

You can get started with `DS.Unity.Extensions.DependencyInjection` by [grabbing the latest NuGet package](https://www.nuget.org/packages/DS.Unity.Extensions.DependencyInjection).


## Get Started

- Reference the `DS.Unity.Extensions.DependencyInjection` package from NuGet.
- In the `ConfigureServices` method of your `Startup` class...
  - Register services from the `IServiceCollection`.
  - Build your container.
  - Create an `UnityServiceProvider` using the container and return it.

```C#
public IServiceProvider ConfigureServices(IServiceCollection services)
{
  services.AddMvc().AddDataAnnotationsLocalization(x => { });
  services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
  services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

  var container = new UnityContainer();
  container.Populate(services);
  return new UnityServiceProvider(container);
}
```
