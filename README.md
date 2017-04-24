# DS.Unity.Extensions.DependencyInjection
Unity implementation of the interfaces in Microsoft.Extensions.DependencyInjection.Abstractions

Nuget https://www.nuget.org/packages/DS.Unity.Extensions.DependencyInjection

Example usage


public IServiceProvider ConfigureServices(IServiceCollection services)
{
  services.AddMvc();
  services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
  services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

  var container = new UnityContainer();
  container.Populate(services);
  return new UnityServiceProvider(container);
}
