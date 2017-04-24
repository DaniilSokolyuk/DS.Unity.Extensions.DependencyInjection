using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    public static class UnityBootstrapper
    {
        public static void Populate(this IUnityContainer container, IServiceCollection services)
        {
            container.AddExtension(new EnumerableExtension());

            container.RegisterInstance(services);
            container.RegisterType<IServiceProvider, UnityServiceProvider>();
            container.RegisterType<IServiceScopeFactory, UnityServiceScopeBootstrapFactory>();

            var aggregateTypes = GetAggregateTypes(services);

            var registerInstance = RegisterInstance();

            foreach (var serviceDescriptor in services)
            {
                //System.Diagnostics.Debugger.Break(); 
                RegisterType(container, serviceDescriptor, aggregateTypes, registerInstance);
            }
        }

        private static MethodInfo RegisterInstance()
        {
            var miRegisterInstanceOpen =
                typeof(UnityContainerExtensions).
                    GetMethods(BindingFlags.Static | BindingFlags.Public).
                    Single(mi => (mi.Name == "RegisterInstance") && mi.IsGenericMethod && (mi.GetParameters().Length == 4));
            return miRegisterInstanceOpen;
        }

        private static HashSet<Type> GetAggregateTypes(IServiceCollection services)
        {
            var aggregateTypes = new HashSet<Type>(
                services
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

        private static void RegisterType(
            IUnityContainer _container,
            ServiceDescriptor serviceDescriptor,
            ICollection<Type> aggregateTypes,
            MethodInfo miRegisterInstanceOpen)
        {
            var lifetimeManager = GetLifetimeManager(serviceDescriptor.Lifetime);
            var isAggregateType = aggregateTypes.Contains(serviceDescriptor.ServiceType);

            if (serviceDescriptor.ImplementationType != null)
            {
                RegisterImplementation(_container, serviceDescriptor, isAggregateType, lifetimeManager);
            }
            else if (serviceDescriptor.ImplementationFactory != null)
            {
                RegisterFactory(_container, serviceDescriptor, isAggregateType, lifetimeManager);
            }
            else if (serviceDescriptor.ImplementationInstance != null)
            {
                RegisterSingleton(_container, serviceDescriptor, miRegisterInstanceOpen, isAggregateType, lifetimeManager);
            }
            else
            {
                throw new InvalidOperationException("Unsupported registration type");
            }
        }

        private static void RegisterImplementation(
            IUnityContainer _container,
            ServiceDescriptor serviceDescriptor,
            bool isAggregateType,
            LifetimeManager lifetimeManager)
        {
            if (isAggregateType)
            {
                _container.RegisterType(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType,
                    serviceDescriptor.ImplementationType.AssemblyQualifiedName,
                    lifetimeManager);
            }
            else
            {
                _container.RegisterType(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType,
                    lifetimeManager);
            }
        }

        private static void RegisterFactory(
            IUnityContainer _container,
            ServiceDescriptor serviceDescriptor,
            bool isAggregateType,
            LifetimeManager lifetimeManager)
        {
            if (isAggregateType)
            {
                _container.RegisterType(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType.AssemblyQualifiedName,
                    lifetimeManager,
                    new InjectionFactory(
                        container =>
                        {
                            var serviceProvider = container.Resolve<IServiceProvider>();
                            var instance = serviceDescriptor.ImplementationFactory(serviceProvider);
                            return instance;
                        }));
            }
            else
            {
                _container.RegisterType(
                    serviceDescriptor.ServiceType,
                    lifetimeManager,
                    new InjectionFactory(
                        container =>
                        {
                            var serviceProvider = container.Resolve<IServiceProvider>();
                            var instance = serviceDescriptor.ImplementationFactory(serviceProvider);
                            return instance;
                        }));
            }
        }

        private static void RegisterSingleton(
            IUnityContainer _container,
            ServiceDescriptor serviceDescriptor,
            MethodInfo miRegisterInstanceOpen,
            bool isAggregateType,
            LifetimeManager lifetimeManager)
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

                miRegisterInstanceOpen.
                    MakeGenericMethod(serviceDescriptor.ServiceType).
                    Invoke(
                        null,
                        new[]
                        {
                            _container, implementationType.AssemblyQualifiedName,
                            serviceDescriptor.ImplementationInstance, lifetimeManager
                        });
            }
            else
            {
                _container.RegisterInstance(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationInstance,
                    lifetimeManager);
            }
        }
    }
}