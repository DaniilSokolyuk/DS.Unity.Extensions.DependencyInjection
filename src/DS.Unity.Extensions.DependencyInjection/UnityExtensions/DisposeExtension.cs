using System;
using System.Collections.Concurrent;
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

        protected override void Initialize()
        {
            Context.Strategies.Add(_strategy, UnityBuildStage.TypeMapping);
        }

        public void Dispose()
        {
            _strategy.Dispose();
            _strategy = null;
        }

        private class DisposeStrategy : BuilderStrategy, IDisposable
        {
            private List<IDisposable> _disposables = new List<IDisposable>();

            public override void PostBuildUp(IBuilderContext context)
            {
                ILifetimePolicy activeLifetime = context.PersistentPolicies.Get<ILifetimePolicy>(context.BuildKey, out IPolicyList lifetimePolicySource);

                IDisposable instance = context.Existing as IDisposable;

                if (instance != null 
                    && !(activeLifetime is IDisposable && context.Lifetime.Contains(activeLifetime)) // all IDisposable in lifitimemanager dipose when unitycontainer dispose
                    && !IsControlledByParrent(context)
                    && !IsInheritedStrategy(context)
                    && !IsCurrentUnityUnityContainer(context, instance))
                {
                    lock (_disposables)
                    {
                        _disposables.Add(instance);
                    }
                }

                base.PostBuildUp(context);
            }

            private bool IsCurrentUnityUnityContainer(IBuilderContext context, IDisposable instance)
            {
                var lifetime = context.Policies.Get<ILifetimePolicy>(NamedTypeBuildKey.Make<IUnityContainer>());
                return ReferenceEquals(lifetime.GetValue() as IDisposable, instance);
            }

            private bool IsControlledByParrent(IBuilderContext context)
            {
                ILifetimePolicy activeLifetime = context.PersistentPolicies.Get<ILifetimePolicy>(context.BuildKey, out IPolicyList lifetimePolicySource);

                return activeLifetime is ContainerControlledLifetimeManager
                       &&
                       !ReferenceEquals(lifetimePolicySource, context.PersistentPolicies);
            }

            private bool IsInheritedStrategy(IBuilderContext builderContext)
            {
                // unity container puts the parent container strategies before child strategies when it builds the chain
                IBuilderStrategy lastStrategy = builderContext.Strategies.LastOrDefault(s => s is DisposeStrategy);

                return !ReferenceEquals(this, lastStrategy);
            }

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
        }
    }
}