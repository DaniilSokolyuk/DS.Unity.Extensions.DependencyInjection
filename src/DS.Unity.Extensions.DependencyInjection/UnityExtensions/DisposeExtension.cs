using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace DS.Unity.Extensions.DependencyInjection.UnityExtensions
{
    public class DisposeExtension : UnityContainerExtension, IDisposable
    {
        private DisposeStrategy _strategy = new DisposeStrategy();

        public void Dispose()
        {
            _strategy.Dispose();
            _strategy = null;
        }

        protected override void Initialize()
        {
            Context.Strategies.Add(_strategy, UnityBuildStage.TypeMapping);
        }

        private class DisposeStrategy : BuilderStrategy, IDisposable
        {
            private readonly List<IDisposable> _disposables = new List<IDisposable>();

            public void Dispose()
            {
                lock (_disposables)
                {
                    foreach (var item in _disposables)
                    {
                        item.Dispose();
                    }

                    _disposables.Clear();
                }
            }

            public override void PostBuildUp(IBuilderContext context)
            {
                var activeLifetime = context.PersistentPolicies.Get<ILifetimePolicy>(context.BuildKey, out IPolicyList lifetimePolicySource);

                var instance = context.Existing as IDisposable;

                if (instance != null
                    && !IsIDisposableInLifetimeContainer()
                    && !IsControlledByParrent()
                    && !IsInheritedStrategy()
                    && !IsCurrentUnityUnityContainer())
                {
                    lock (_disposables)
                    {
                        _disposables.Add(instance);
                    }
                }

                base.PostBuildUp(context);

                bool IsIDisposableInLifetimeContainer()
                {
                    // all IDisposable in lifitimemanager dipose when unitycontainer dispose
                    return activeLifetime is IDisposable && context.Lifetime != null && context.Lifetime.Contains(activeLifetime);
                }

                bool IsCurrentUnityUnityContainer()
                {
                    var lifetime = context.Policies.Get<ILifetimePolicy>(NamedTypeBuildKey.Make<IUnityContainer>());
                    return ReferenceEquals(lifetime.GetValue() as IDisposable, instance);
                }

                bool IsControlledByParrent()
                {
                    return activeLifetime is ContainerControlledLifetimeManager
                           &&
                           !ReferenceEquals(lifetimePolicySource, context.PersistentPolicies);
                }

                bool IsInheritedStrategy()
                {
                    // unity container puts the parent container strategies before child strategies when it builds the chain
                    var lastStrategy = context.Strategies.LastOrDefault(s => s is DisposeStrategy);
                    return !ReferenceEquals(this, lastStrategy);
                }
            }
        }
    }
}