using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    public static class UnityBootstrapHelper
    {
        public static void RegisterType(
            IUnityContainer _container,
            ServiceDescriptor serviceDescriptor,
            ICollection<Type> aggregateTypes,
            MethodInfo miRegisterInstanceOpen)
        {
            var isAggregateType = aggregateTypes.Contains(serviceDescriptor.ServiceType);

            if (serviceDescriptor.ImplementationType != null)
            {
                RegisterImplementation(_container, serviceDescriptor, isAggregateType);
            }
            else if (serviceDescriptor.ImplementationFactory != null)
            {
                RegisterFactory(_container, serviceDescriptor, isAggregateType);
            }
            else if (serviceDescriptor.ImplementationInstance != null)
            {
                RegisterSingleton(_container, serviceDescriptor, miRegisterInstanceOpen, isAggregateType);
            }
            else
            {
                throw new InvalidOperationException("Unsupported registration type");
            }
        }

        public static MethodInfo RegisterInstance()
        {
            var miRegisterInstanceOpen =
                typeof(UnityContainerExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Single(mi => (mi.Name == "RegisterInstance") && mi.IsGenericMethod && (mi.GetParameters().Length == 4));
            return miRegisterInstanceOpen;
        }

        public static HashSet<Type> GetAggregateTypes(IEnumerable<ServiceDescriptor> descriptors)
        {
            var aggregateTypes = new HashSet<Type>(
                descriptors
                    .GroupBy(serviceDescriptor => serviceDescriptor.ServiceType, serviceDescriptor => serviceDescriptor)
                    .Where(typeGrouping => typeGrouping.Count() > 1)
                    .Select(type => type.Key)
            );
            return aggregateTypes;
        }

        private static LifetimeManager GetLifetimeManager(ServiceLifetime lifecycle)
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

        private static void RegisterImplementation(
            IUnityContainer _container,
            ServiceDescriptor serviceDescriptor,
            bool isAggregateType)
        {
            if (isAggregateType)
            {
                _container.RegisterType(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType,
                    serviceDescriptor.ImplementationType.AssemblyQualifiedName,
                    GetLifetimeManager(serviceDescriptor.Lifetime));
            }

            _container.RegisterType(
                serviceDescriptor.ServiceType,
                serviceDescriptor.ImplementationType,
                GetLifetimeManager(serviceDescriptor.Lifetime));
        }

        private static void RegisterFactory(
            IUnityContainer _container,
            ServiceDescriptor serviceDescriptor,
            bool isAggregateType)
        {
            if (isAggregateType)
            {
                _container.RegisterType(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType.AssemblyQualifiedName,
                    GetLifetimeManager(serviceDescriptor.Lifetime),
                    new InjectionFactory(
                        container =>
                        {
                            var serviceProvider = container.Resolve<IServiceProvider>();
                            var instance = serviceDescriptor.ImplementationFactory(serviceProvider);
                            return instance;
                        }));
            }

            _container.RegisterType(
                serviceDescriptor.ServiceType,
                GetLifetimeManager(serviceDescriptor.Lifetime),
                new InjectionFactory(
                    container =>
                    {
                        var serviceProvider = container.Resolve<IServiceProvider>();
                        var instance = serviceDescriptor.ImplementationFactory(serviceProvider);
                        return instance;
                    }));
        }

        private static void RegisterSingleton(
            IUnityContainer _container,
            ServiceDescriptor serviceDescriptor,
            MethodInfo miRegisterInstanceOpen,
            bool isAggregateType)
        {
            if (isAggregateType)
            {
                //todo: ImplementationType иногда not defined
                var implementationType = typeof(string);
                if (serviceDescriptor.ImplementationType != null)
                {
                    implementationType = serviceDescriptor.ImplementationType;
                }
                else if (serviceDescriptor.ImplementationInstance != null)
                {
                    implementationType = serviceDescriptor.ImplementationInstance.GetType();
                }

                miRegisterInstanceOpen.MakeGenericMethod(serviceDescriptor.ServiceType)
                    .Invoke(
                        null,
                        new[]
                        {
                            _container,
                            implementationType.AssemblyQualifiedName,
                            serviceDescriptor.ImplementationInstance,
                            GetLifetimeManager(serviceDescriptor.Lifetime)
                        });
            }

            _container.RegisterInstance(
                serviceDescriptor.ServiceType,
                serviceDescriptor.ImplementationInstance,
                GetLifetimeManager(serviceDescriptor.Lifetime));
        }
    }
}