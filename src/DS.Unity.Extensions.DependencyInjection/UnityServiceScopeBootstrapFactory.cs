using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    public class UnityServiceScopeBootstrapFactory : IServiceScopeFactory
    {
        private readonly IUnityContainer _container;

        public UnityServiceScopeBootstrapFactory(IUnityContainer container)
        {
            _container = container;
        }

        public IServiceScope CreateScope()
        {
            return new UnityServiceScopeBootstrap(CreateChildContainer());
        }

        private IUnityContainer CreateChildContainer()
        {
            var child = _container.CreateChildContainer();
            child.AddExtension(new EnumerableExtension());

            return child;
        }
    }
}