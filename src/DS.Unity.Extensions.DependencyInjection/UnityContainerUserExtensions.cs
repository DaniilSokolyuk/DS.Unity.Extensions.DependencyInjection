using System;
using System.Collections.Generic;
using System.Linq;
using DS.Unity.Extensions.DependencyInjection.UnityExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    public static class UnityContainerUserExtensions
    {
        public static void Populate(this IUnityContainer container, IEnumerable<ServiceDescriptor> descriptors)
        {
            container.AddExtensions();
            container.RegisterInstance(descriptors);
            container.RegisterType<IServiceProvider, UnityServiceProvider>();
            container.RegisterType<IServiceScopeFactory, UnityServiceScopeFactory>();
            container.RegisterType<IServiceScope, UnityServiceScope>();

            var aggregateTypes = new HashSet<Type>(
                descriptors
                    .GroupBy(serviceDescriptor => serviceDescriptor.ServiceType, serviceDescriptor => serviceDescriptor)
                    .Where(typeGrouping => typeGrouping.Count() > 1)
                    .Select(type => type.Key)
            );

            foreach (var serviceDescriptor in descriptors)
            {
                var isAggregateType = aggregateTypes.Contains(serviceDescriptor.ServiceType);

                if (serviceDescriptor.ImplementationType != null)
                {
                    container.RegisterImplementation(serviceDescriptor, isAggregateType);
                }
                else if (serviceDescriptor.ImplementationFactory != null)
                {
                    container.RegisterFactory(serviceDescriptor, isAggregateType);
                }
                else if (serviceDescriptor.ImplementationInstance != null)
                {
                    container.RegisterSingleton(serviceDescriptor, isAggregateType);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported registration type");
                }
            }
        }

        private static void RegisterImplementation(this IUnityContainer container, ServiceDescriptor serviceDescriptor, bool isAggregateType)
        {
            if (isAggregateType)
            {
                container.RegisterType(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType,
                    serviceDescriptor.ImplementationType.AssemblyQualifiedName,
                    serviceDescriptor.Lifetime.ToUnityLifetimeManager());
            }

            container.RegisterType(
                serviceDescriptor.ServiceType,
                serviceDescriptor.ImplementationType,
                serviceDescriptor.Lifetime.ToUnityLifetimeManager());
        }

        private static void RegisterFactory(this IUnityContainer container, ServiceDescriptor serviceDescriptor, bool isAggregateType)
        {
            if (isAggregateType)
            {
                container.RegisterType(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType.AssemblyQualifiedName,
                    serviceDescriptor.Lifetime.ToUnityLifetimeManager(),
                    new InjectionFactory(
                        unityContainer =>
                        {
                            var serviceProvider = unityContainer.Resolve<IServiceProvider>();
                            var instance = serviceDescriptor.ImplementationFactory(serviceProvider);
                            return instance;
                        }));
            }

            container.RegisterType(
                serviceDescriptor.ServiceType,
                serviceDescriptor.Lifetime.ToUnityLifetimeManager(),
                new InjectionFactory(
                    unityContainer =>
                    {
                        var serviceProvider = unityContainer.Resolve<IServiceProvider>();
                        var instance = serviceDescriptor.ImplementationFactory(serviceProvider);
                        return instance;
                    }));
        }

        private static void RegisterSingleton(this IUnityContainer container, ServiceDescriptor serviceDescriptor, bool isAggregateType)
        {
            if (isAggregateType)
            {
                var name = Guid.NewGuid().ToString();
                if (serviceDescriptor.ImplementationType != null)
                {
                    name = serviceDescriptor.ImplementationType.AssemblyQualifiedName;
                }
                else if (serviceDescriptor.ImplementationInstance != null)
                {
                    name = serviceDescriptor.ImplementationInstance.GetType().AssemblyQualifiedName;
                }

                container.RegisterInstance(
                    serviceDescriptor.ServiceType,
                    name,
                    serviceDescriptor.ImplementationInstance,
                    serviceDescriptor.Lifetime.ToUnityLifetimeManager());
            }

            container.RegisterInstance(
                serviceDescriptor.ServiceType,
                serviceDescriptor.ImplementationInstance,
                serviceDescriptor.Lifetime.ToUnityLifetimeManager());
        }

        public static IUnityContainer AddExtensions(this IUnityContainer container)
        {
            container.AddExtension(new EnumerableResolutionUnityExtension());
            container.AddExtension(new ConstructorSelectionUnityExtension());
            //container.AddExtension(new DisposeExtension());

            return container;
        }

        internal static bool CanResolve(this IUnityContainer container, Type type)
        {
            if (type.IsClass)
            {
                return true;
            }

            if (type.IsGenericType)
            {
                var gerericType = type.GetGenericTypeDefinition();
                if ((gerericType == typeof(IEnumerable<>)) ||
                    gerericType.IsClass ||
                    container.IsRegistered(gerericType))
                {
                    return true;
                }
            }

            return container.IsRegistered(type);
        }

        internal static T TryResolve<T>(this IUnityContainer container)
        {
            var result = TryResolve(container, typeof(T));

            if (result != null)
            {
                return (T)result;
            }

            return default(T);
        }

        internal static object TryResolve(this IUnityContainer container, Type typeToResolve)
        {
            try
            {
                return container.Resolve(typeToResolve);
            }
            catch
            {
                return null;
            }
        }

        internal static object TryResolve(this IUnityContainer container, Type typeToResolve, string name)
        {
            try
            {
                return container.Resolve(typeToResolve, name);
            }
            catch
            {
                return null;
            }
        }
    }
}