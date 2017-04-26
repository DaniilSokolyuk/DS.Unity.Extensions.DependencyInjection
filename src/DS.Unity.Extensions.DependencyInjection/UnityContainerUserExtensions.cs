using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    public static class UnityContainerUserExtensions
    {
        public static void Populate(this IUnityContainer container, IEnumerable<ServiceDescriptor> descriptors)
        {
            container.AddExtension(new EnumerableResolutionUnityExtension());

            container.RegisterInstance(descriptors);
            container.RegisterType<IServiceProvider, UnityServiceProvider>();
            container.RegisterType<IServiceScopeFactory, UnityServiceScopeFactory>();

            var aggregateTypes = UnityBootstrapHelper.GetAggregateTypes(descriptors);

            var registerInstance = UnityBootstrapHelper.RegisterInstance();

            foreach (var serviceDescriptor in descriptors)
            {
                //System.Diagnostics.Debugger.Break(); 
                UnityBootstrapHelper.RegisterType(container, serviceDescriptor, aggregateTypes, registerInstance);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        internal static T TryResolve<T>(this IUnityContainer container)
        {
            var result = TryResolve(container, typeof(T));

            if (result != null)
            {
                return (T)result;
            }

            return default(T);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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