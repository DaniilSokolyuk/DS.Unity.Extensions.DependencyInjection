# DS.Unity.Extensions.DependencyInjection
Unity implementation of the interfaces in Microsoft.Extensions.DependencyInjection.Abstractions

## Get Packages

You can get started with `DS.Unity.Extensions.DependencyInjection` by [grabbing the latest NuGet package](https://www.nuget.org/packages/DS.Unity.Extensions.DependencyInjection).

Example usage

```C#
public IServiceProvider ConfigureServices(IServiceCollection services)
{
  services.AddMvc();
  services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
  services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

  var container = new UnityContainer();
  container.Populate(services);
  return new UnityServiceProvider(container);
}
```
