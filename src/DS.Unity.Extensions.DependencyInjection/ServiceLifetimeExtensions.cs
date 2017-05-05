using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    internal static class ServiceLifetimeExtensions
    {
        internal static LifetimeManager ToUnityLifetimeManager(this ServiceLifetime lifecycle)
        {
            switch (lifecycle)
            {
                case ServiceLifetime.Transient:
                    return new TransientLifetimeManager();
                case ServiceLifetime.Singleton:
                    return new ContainerControlledLifetimeManager();
                case ServiceLifetime.Scoped:
                    return new HierarchicalLifetimeManager();
            }

            return new TransientLifetimeManager();
        }
    }
}