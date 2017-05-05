# DS.Unity.Extensions.DependencyInjection [![NuGet Version](https://buildstats.info/nuget/DS.Unity.Extensions.DependencyInjection)](https://www.nuget.org/packages/DS.Unity.Extensions.DependencyInjection/)

An unofficial [Unity](https://www.nuget.org/packages/Unity/) implementation of the interfaces in [Microsoft.Extensions.DependencyInjection.Abstractions](https://github.com/aspnet/DependencyInjection) 

## Get Started
- Reference the `DS.Unity.Extensions.DependencyInjection` package from NuGet.

## First way:
- In the `WebHostBuilder` add `ConfigureServices(services => services.AddUnity())` method

```C#
var host = new WebHostBuilder()
  .UseKestrel()
  .ConfigureServices(services => services.AddUnity())
  .UseContentRoot(Directory.GetCurrentDirectory())
  .UseIISIntegration()
  .UseStartup<Startup>()
  .UseApplicationInsights()
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
  - Create an `UnityServiceProvider` using the container and return it.

```C#
public IServiceProvider ConfigureServices(IServiceCollection services)
{
  services.AddMvc();
  
  var container = new UnityContainer();
  container.Populate(services);
  
  container.RegisterType<IMyService, MyService>();
  
  return new UnityServiceProvider(container);
}
```
