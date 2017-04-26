using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace DS.Unity.Extensions.DependencyInjection.Tests
{
    [TestFixture]
    public class UnityRegistrationTests
    {
        [Test]
        [TestCase(typeof(IServiceProvider), typeof(UnityServiceProvider))]
        [TestCase(typeof(IServiceScopeFactory), typeof(UnityServiceScopeFactory))]
        public void Populate_ShouldRegisterUnityServiceProvider(Type registeredType, Type expectedImplementationType)
        {
            // given
            var container = new UnityContainer();

            // when
            container.Populate(Enumerable.Empty<ServiceDescriptor>());

            // then
            var service = container.Resolve(registeredType);
            Assert.That(service, Is.AssignableFrom(expectedImplementationType));
        }

        [Test]
        [TestCase(ServiceLifetime.Transient, typeof(TransientLifetimeManager))]
        [TestCase(ServiceLifetime.Singleton, typeof(ContainerControlledLifetimeManager))]
        [TestCase(ServiceLifetime.Scoped, typeof(HierarchicalLifetimeManager))]
        public void Populate_ShouldRegisterWithTransientLifetime(ServiceLifetime lifetime, Type expectedUnityLifetime)
        {
            // given
            var container = new UnityContainer();
            var descriptor = new ServiceDescriptor(typeof(ISomeService), typeof(SomeService), lifetime);

            // when
            container.Populate(new[] { descriptor });

            // then
            var registration = container.Registrations.FirstOrDefault(p => p.RegisteredType == typeof(ISomeService));
            Assert.That(registration.LifetimeManagerType, Is.EqualTo(expectedUnityLifetime));
        }

        [Test]
        public void Populate_ShouldCorrectlyRegisterOptions()
        {
            // given
            var collection = new ServiceCollection();
            collection.AddOptions();

            TestOptions expectedOptions = null;

            collection.Configure<TestOptions>(
                options =>
                {
                    expectedOptions = options; 
                });

            var container = new UnityContainer();

            // when
            container.Populate(collection);

            // then
            var resolvedOptions = container.Resolve<IOptions<TestOptions>>().Value;
            Assert.That(resolvedOptions, Is.SameAs(expectedOptions));
        }

        public class SomeService : ISomeService
        {
        }

        public interface ISomeService
        {
        }

        public class TestOptions
        {
            public int TestSetting { get; set; }
        }
    }
}