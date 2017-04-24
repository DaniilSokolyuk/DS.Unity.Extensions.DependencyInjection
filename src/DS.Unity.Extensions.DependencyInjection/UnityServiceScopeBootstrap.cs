using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    public class UnityServiceScopeBootstrap : IServiceScope
    {
        private readonly IUnityContainer _container;

        public UnityServiceScopeBootstrap(IUnityContainer container)
        {
            _container = container;
            ServiceProvider = _container.Resolve<IServiceProvider>();
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}