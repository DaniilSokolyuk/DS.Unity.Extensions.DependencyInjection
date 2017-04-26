using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    public class UnityServiceScopeFactory : IServiceScopeFactory
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

            return child;
        }
    }
}