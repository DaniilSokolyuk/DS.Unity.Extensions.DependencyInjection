using System;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

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

                var originalSelectorPolicy = context.Policies.Get<IConstructorSelectorPolicy>(context.BuildKey, out IPolicyList selectorPolicyDestination);

                selectorPolicyDestination.Set<IConstructorSelectorPolicy>(
                    new DerivedTypeConstructorSelectorPolicy(GetUnityFromBuildContext(context), originalSelectorPolicy),
                    context.BuildKey);
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

                public DerivedTypeConstructorSelectorPolicy(IUnityContainer container, IConstructorSelectorPolicy originalSelectorPolicy)
                {
                    _originalConstructorSelectorPolicy = originalSelectorPolicy;
                    _container = container;
                }

                public SelectedConstructor SelectConstructor(IBuilderContext context, IPolicyList resolverPolicyDestination)
                {
                    SelectedConstructor originalConstructor = null;

                    try
                    {
                        originalConstructor = _originalConstructorSelectorPolicy.SelectConstructor(context, resolverPolicyDestination);

                        if (originalConstructor.Constructor.GetParameters().All(arg => _container.CanResolve(arg.ParameterType)))
                        {
                            return originalConstructor;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    var implementingType = context.BuildKey.Type;
                    var bestConstructor = implementingType.GetTypeInfo()
                        .DeclaredConstructors
                        .Select(ctor => new { Constructor = ctor, Parameters = ctor.GetParameters() })
                        .OrderByDescending(x => x.Parameters.Length)
                        .FirstOrDefault(
                            _ => _.Constructor.IsPublic
                                 && (originalConstructor == null || _.Constructor != originalConstructor.Constructor)
                                 && _.Parameters.All(arg => _container.CanResolve(arg.ParameterType)));

                    if (bestConstructor == null)
                    {
                        return originalConstructor;
                    }

                    var newSelectedConstructor = new SelectedConstructor(bestConstructor.Constructor);

                    foreach (var parameter in bestConstructor.Constructor.GetParameters())
                    {
                        newSelectedConstructor.AddParameterResolver(new NamedTypeDependencyResolverPolicy(parameter.ParameterType, null));
                    }

                    return newSelectedConstructor;
                }
            }
        }
    }
}