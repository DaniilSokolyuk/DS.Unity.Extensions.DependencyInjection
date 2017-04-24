using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection
{
    public static class UnityContainerUserExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T TryResolve<T>(this IUnityContainer container)
        {
            var result = TryResolve(container, typeof(T));

            if (result != null)
            {
                return (T)result;
            }

            return default(T);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static object TryResolve(this IUnityContainer container, Type typeToResolve)
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
        public static object TryResolve(this IUnityContainer container, Type typeToResolve, string name)
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