using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    public class UnityServiceProvider : IServiceProvider, ISupportRequiredService
    {
        private readonly IUnityContainer _container;

        public UnityServiceProvider(IUnityContainer container)
        {
            _container = container;
        }

        public object GetService(Type serviceType)
        {
            return _container.TryResolve(serviceType);
        }

        public object GetRequiredService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
    }
}