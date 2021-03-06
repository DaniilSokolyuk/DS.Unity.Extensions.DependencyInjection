using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using Microsoft.Practices.Unity.Utility;

namespace DS.Unity.Extensions.DependencyInjection.UnityExtensions
{
    internal class EnumerableResolutionUnityExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Context.Strategies.AddNew<EnumerableResolutionStrategy>(UnityBuildStage.TypeMapping);
        }

        /// <summary>
        ///     This strategy implements the logic that will return all instances
        ///     when an <see cref="IEnumerable{T}" /> parameter is detected.
        /// </summary>
        /// <remarks>
        ///     Nicked from
        ///     https://piotr-wlodek-code-gallery.googlecode.com/svn-history/r40/trunk/Unity.Extensions/Unity.Extensions/EnumerableResolutionStrategy.cs
        /// </remarks>
        internal class EnumerableResolutionStrategy : BuilderStrategy
        {
            private static readonly MethodInfo GenericResolveEnumerableMethod =
                typeof(EnumerableResolutionStrategy).GetMethod(
                    nameof(ResolveEnumerable),
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

            private static readonly MethodInfo GenericResolveLazyEnumerableMethod =
                typeof(EnumerableResolutionStrategy).GetMethod(
                    nameof(ResolveLazyEnumerable),
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

            private static Type GetTypeToBuild(Type type)
            {
                return type.GetGenericArguments()[0];
            }

            private static bool IsResolvingIEnumerable(Type type)
            {
                return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            }

            private static bool IsResolvingLazy(Type type)
            {
                return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Lazy<>));
            }

            private static object ResolveLazyEnumerable<T>(IBuilderContext context)
            {
                var container = context.NewBuildUp<IUnityContainer>();

                var typeToBuild = typeof(T);
                var typeWrapper = typeof(Lazy<T>);

                return ResolveAll(container, typeToBuild, typeWrapper).OfType<Lazy<T>>().ToList();
            }

            private static object ResolveEnumerable<T>(IBuilderContext context)
            {
                var container = context.NewBuildUp<IUnityContainer>();

                var typeToBuild = typeof(T);

                return ResolveAll(container, typeToBuild, typeToBuild).OfType<T>().ToList();
            }

            private static IEnumerable<object> ResolveAll(IUnityContainer container, Type type, Type typeWrapper)
            {
                var names = GetRegisteredNames(container, type);

                if (type.IsGenericType)
                {
                    names = names.Concat(GetRegisteredNames(container, type.GetGenericTypeDefinition()));
                }

                return names
                    .GroupBy(t => t.MappedToType)
                    .Select(t => t.Last())
                    .Select(t => t.Name)
                    .Select(name => container.TryResolve(typeWrapper, name))
                    .Where(x => x != null);
            }

            private static IEnumerable<ContainerRegistration> GetRegisteredNames(IUnityContainer container, Type type)
            {
                return container.Registrations.Where(t => t.RegisteredType == type);
            }

            /// <summary>
            ///     Do the PreBuildUp stage of construction. This is where the actual work is performed.
            /// </summary>
            /// <param name="context">Current build context.</param>
            public override void PreBuildUp(IBuilderContext context)
            {
                Guard.ArgumentNotNull(context, "context");

                if (!IsResolvingIEnumerable(context.BuildKey.Type))
                {
                    return;
                }

                MethodInfo resolverMethod;
                var typeToBuild = GetTypeToBuild(context.BuildKey.Type);

                if (IsResolvingLazy(typeToBuild))
                {
                    typeToBuild = GetTypeToBuild(typeToBuild);
                    resolverMethod = GenericResolveLazyEnumerableMethod.MakeGenericMethod(typeToBuild);
                }
                else
                {
                    resolverMethod = GenericResolveEnumerableMethod.MakeGenericMethod(typeToBuild);
                }

                var resolver = (Resolver)Delegate.CreateDelegate(typeof(Resolver), resolverMethod);
                context.Existing = resolver(context);
                context.BuildComplete = true;
            }

            private delegate object Resolver(IBuilderContext context);
        }
    }
}