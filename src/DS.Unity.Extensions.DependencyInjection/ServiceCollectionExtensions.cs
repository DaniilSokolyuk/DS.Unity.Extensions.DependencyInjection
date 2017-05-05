using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUnity(this IServiceCollection services, Action<IUnityContainer> configurationAction = null)
        {
            return services.AddSingleton<IServiceProviderFactory<IUnityContainer>>(new UnityServiceProviderFactory(configurationAction));
        }
    }
}