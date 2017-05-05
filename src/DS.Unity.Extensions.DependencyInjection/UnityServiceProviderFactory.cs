using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    internal class UnityServiceProviderFactory : IServiceProviderFactory<IUnityContainer>
    {
        private readonly Action<IUnityContainer> _configurationAction;

        public UnityServiceProviderFactory(Action<IUnityContainer> configurationAction = null)
        {
            _configurationAction = configurationAction ?? (container => { });
        }

        public IUnityContainer CreateBuilder(IServiceCollection serviceCollection)
        {
            var builder = new UnityContainer();

            builder.Populate(serviceCollection);

            _configurationAction(builder);

            return builder;
        }

        public IServiceProvider CreateServiceProvider(IUnityContainer unityContainer)
        {
            return new UnityServiceProvider(unityContainer);
        }
    }
}