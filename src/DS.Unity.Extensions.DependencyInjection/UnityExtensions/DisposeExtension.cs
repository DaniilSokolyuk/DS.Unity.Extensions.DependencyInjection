using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace DS.Unity.Extensions.DependencyInjection.UnityExtensions
{
    public class DisposeExtension : UnityContainerExtension, IDisposable
    {
        private DisposeStrategy strategy = new DisposeStrategy();

        protected override void Initialize()
        {
            Context.Strategies.Add(strategy, UnityBuildStage.TypeMapping);
        }

        public void Dispose()
        {
            strategy.Dispose();
            strategy = null;
        }

        private class DisposeStrategy : BuilderStrategy, IDisposable
        {
            private DisposableObjectsList disposables = new DisposableObjectsList();

            public override void PostBuildUp(IBuilderContext context)
            {
                if (context != null)
                {
                    IDisposable instance = context.Existing as IDisposable;
                    if (instance != null 
                        && !IsControlledByParrent(context) 
                        && !IsInheritedStrategy(context)
                        && !IsCurrentUnityUnityContainer(context, instance))
                    {
                        disposables.Add(instance);
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
                IPolicyList lifetimePolicySource;
                ILifetimePolicy activeLifetime = context.PersistentPolicies.Get<ILifetimePolicy>(context.BuildKey, out lifetimePolicySource);

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
                disposables.Dispose();
                disposables = null;
            }
        }

        public class DisposableObjectsList : IDisposable
        {
            private ConcurrentBag<WeakReference> items = new ConcurrentBag<WeakReference>();

            public void Add(IDisposable disposable)
            {
                items.Add(new WeakReference(disposable));
            }

            public void Dispose()
            {
                foreach (var item in items)
                {
                    object target = item.Target;
                    IDisposable disposable = target as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                items = new ConcurrentBag<WeakReference>();
            }
        }
    }
}