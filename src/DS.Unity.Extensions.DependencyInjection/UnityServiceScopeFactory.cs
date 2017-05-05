using DS.Unity.Extensions.DependencyInjection.UnityExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    internal class UnityServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IUnityContainer _container;

        public UnityServiceScopeFactory(IUnityContainer container)
        {
            _container = container;
        }

        public IServiceScope CreateScope()
        {
            return new UnityServiceScope(CreateChildContainer());
        }

        private IUnityContainer CreateChildContainer()
        {
            var child = _container.CreateChildContainer();
            child.AddExtension(new EnumerableResolutionUnityExtension());
            child.AddExtension(new DerivedTypeResolutionUnityExtension());

            return child;
        }
    }
}