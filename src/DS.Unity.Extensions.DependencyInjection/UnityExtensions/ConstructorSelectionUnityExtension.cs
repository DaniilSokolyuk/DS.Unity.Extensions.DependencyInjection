using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using Microsoft.Practices.Unity.Utility;

namespace DS.Unity.Extensions.DependencyInjection.UnityExtensions
{
    internal class ConstructorSelectionUnityExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Context.Strategies.Add(new CustomBuilderStrategy(), UnityBuildStage.PreCreation);
        }

        internal sealed class CustomBuilderStrategy : BuilderStrategy
        {
            public override void PreBuildUp(IBuilderContext context)
            {
                if (context.Existing != null)
                {
                    return;
                }

                var originalSelectorPolicy = context.Policies.Get<IConstructorSelectorPolicy>(context.BuildKey, out var selectorPolicyDestination);

                if (originalSelectorPolicy.GetType() == typeof(DefaultUnityConstructorSelectorPolicy))
                {
                    selectorPolicyDestination.Set<IConstructorSelectorPolicy>(
                        new DerivedTypeConstructorSelectorPolicy(GetUnityFromBuildContext(context), originalSelectorPolicy),
                        context.BuildKey);
                }
            }

            private IUnityContainer GetUnityFromBuildContext(IBuilderContext context)
            {
                var lifetime = context.Policies.Get<ILifetimePolicy>(NamedTypeBuildKey.Make<IUnityContainer>());
                return lifetime.GetValue() as IUnityContainer;
            }

            private class DerivedTypeConstructorSelectorPolicy : IConstructorSelectorPolicy
            {
                private readonly IUnityContainer _container;
                private readonly IConstructorSelectorPolicy _originalConstructorSelectorPolicy;

                public DerivedTypeConstructorSelectorPolicy(IUnityContainer container,
                    IConstructorSelectorPolicy originalSelectorPolicy)
                {
                    _originalConstructorSelectorPolicy = originalSelectorPolicy;
                    _container = container;
                }

                public SelectedConstructor SelectConstructor(IBuilderContext context, IPolicyList resolverPolicyDestination)
                {
                    var type = context.BuildKey.Type;
                    var ctor = FindInjectionConstructor(type) ?? FindLongestConstructor(type);
                    return ctor != null ? CreateSelectedConstructor(ctor) : null;
                }

                private SelectedConstructor CreateSelectedConstructor(ConstructorInfo ctor)
                {
                    var selectedConstructor = new SelectedConstructor(ctor);
                    foreach (var parameter in ctor.GetParameters())
                    {
                        selectedConstructor.AddParameterResolver(CreateResolver(parameter));
                    }

                    return selectedConstructor;
                }

                private IDependencyResolverPolicy CreateResolver(ParameterInfo parameter)
                {
                    var list = parameter.GetCustomAttributes(false).OfType<DependencyResolutionAttribute>().ToList();
                    return list.Count > 0
                        ? list[0].CreateResolver(parameter.ParameterType)
                        : new NamedTypeDependencyResolverPolicy(parameter.ParameterType, null);
                }

                private ConstructorInfo FindInjectionConstructor(Type typeToConstruct)
                {
                    var array = new ReflectionHelper(typeToConstruct).InstanceConstructors.Where(ctor => ctor.IsDefined(typeof(InjectionConstructorAttribute), true)).ToArray();
                    switch (array.Length)
                    {
                        case 0:
                            return null;
                        case 1:
                            return array[0];
                        default:
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "MultipleInjectionConstructors {0}", typeToConstruct.GetTypeInfo().Name));
                    }
                }

                private ConstructorInfo FindLongestConstructor(Type typeToConstruct)
                {
                    return new ReflectionHelper(typeToConstruct).InstanceConstructors
                        .Select(ctor => new {Constructor = ctor, Parameters = ctor.GetParameters()})
                        .OrderByDescending(x => x.Parameters.Length)
                        .FirstOrDefault(_ => _.Parameters.All(arg => _container.CanResolve(arg)))
                        ?.Constructor;
                }
            }
        }
    }
}