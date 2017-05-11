using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    internal class UnityServiceScope : IServiceScope
    {
        private readonly IUnityContainer _container;

        public UnityServiceScope(IUnityContainer container)
        {
            _container = container;
            ServiceProvider = new UnityServiceProvider(container);
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}