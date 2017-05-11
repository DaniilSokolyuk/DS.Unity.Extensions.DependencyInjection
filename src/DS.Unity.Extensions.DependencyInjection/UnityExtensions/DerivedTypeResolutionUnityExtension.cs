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
                    var originalConstructor = _originalConstructorSelectorPolicy.SelectConstructor(context, resolverPolicyDestination);

                    if (originalConstructor.Constructor.GetParameters().All(arg => _container.CanResolve(arg.ParameterType)))
                    {
                        return originalConstructor;
                    }

                    var newSelectedConstructor = FindNewCtor(originalConstructor);
                    if (newSelectedConstructor == null)
                    {
                        return originalConstructor;
                    }

                    foreach (var newParameterResolver in originalConstructor.GetParameterResolvers().Take(newSelectedConstructor.Constructor.GetParameters().Length))
                    {
                        newSelectedConstructor.AddParameterResolver(newParameterResolver);
                    }

                    return newSelectedConstructor;
                }

                private SelectedConstructor FindNewCtor(SelectedConstructor originalConstructor)
                {
                    var implementingType = originalConstructor.Constructor.DeclaringType;
                    var constructors = implementingType.GetTypeInfo()
                        .DeclaredConstructors
                        .Where(constructor => constructor.IsPublic && (constructor != originalConstructor.Constructor))
                        .ToArray();

                    if (constructors.Length == 0)
                    {
                        return null;
                    }

                    if (constructors.Length == 1)
                    {
                        return new SelectedConstructor(constructors[0]);
                    }

                    var newCtor = constructors
                        .OrderByDescending(c=>c.GetParameters().Length)
                        .FirstOrDefault(c => c.GetParameters().All(arg => _container.CanResolve(arg.ParameterType)));

                    if (newCtor == null)
                    {
                        return null;
                    }

                    return new SelectedConstructor(newCtor);
                }
            }
        }
    }
}