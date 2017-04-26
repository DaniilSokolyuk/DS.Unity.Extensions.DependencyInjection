using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Microsoft.Extensions.Options;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace DS.Unity.Extensions.DependencyInjection.Tests
{
    [TestFixture]
    public class UnityRegistrationTests
    {
        [Test]
        public void PopulateRegistersServiceProvider()
        {
            var container = new UnityContainer();
            container.Populate(Enumerable.Empty<ServiceDescriptor>());

            container.AssertRegistered<IServiceProvider>();
        }

        [Test]
        public void CorrectServiceProviderIsRegistered()
        {
            var container = new UnityContainer();
            container.Populate(Enumerable.Empty<ServiceDescriptor>());

            container.AssertImplementation<IServiceProvider, UnityServiceProvider>();
        }

        [Test]
        public void PopulateRegistersServiceScopeFactory()
        {
            var container = new UnityContainer();
            container.Populate(Enumerable.Empty<ServiceDescriptor>());

            container.AssertRegistered<IServiceScopeFactory>();
        }

        [Test]
        public void ServiceScopeFactoryIsRegistered()
        {
            var container = new UnityContainer();
            container.Populate(Enumerable.Empty<ServiceDescriptor>());

            container.AssertImplementation<IServiceScopeFactory, UnityServiceScopeFactory>();
        }

        [Test]
        public void CanRegisterTransientService()
        {
            var container = new UnityContainer();
            var descriptor = new ServiceDescriptor(typeof(IService), typeof(Service), ServiceLifetime.Transient);
            container.Populate(new ServiceDescriptor[] { descriptor });

            container.AssertLifetime<IService, TransientLifetimeManager>();
        }

        [Test]
        public void CanRegisterSingletonService()
        {
            var container = new UnityContainer();
            var descriptor = new ServiceDescriptor(typeof(IService), typeof(Service), ServiceLifetime.Singleton);
            container.Populate(new ServiceDescriptor[] { descriptor });

            container.AssertLifetime<IService, ContainerControlledLifetimeManager>();
        }

        [Test]
        public void CanRegisterScopedService()
        {
            var container = new UnityContainer();
            var descriptor = new ServiceDescriptor(typeof(IService), typeof(Service), ServiceLifetime.Scoped);
            container.Populate(new ServiceDescriptor[] { descriptor });

            container.AssertLifetime<IService, HierarchicalLifetimeManager>();
        }

        [Test]
        public void LastServiceReplacesPreviousServices()
        {
            var container = new UnityContainer();
            var collection = new ServiceCollection();
            collection.AddTransient<IFakeMultipleService, FakeOneMultipleService>();
            collection.AddTransient<IFakeMultipleService, FakeTwoMultipleService>();

            container.Populate(collection);

            container.AssertImplementation<IFakeMultipleService, FakeTwoMultipleService>();
        }

        [Test]
        public void ServiceCollectionConfigurationIsRetainedInRootContainer()
        {
            var collection = new ServiceCollection();
            collection.AddOptions();
            collection.Configure<TestOptions>(options =>
            {
                options.Value = 5;
            });

            var container = new UnityContainer();
            container.Populate(collection);

            var resolved = container.Resolve<IOptions<TestOptions>>();
            Assert.NotNull(resolved.Value);
            Assert.AreEqual(5, resolved.Value.Value);
        }

        public class Service : IService
        {
        }

        public interface IService
        {
        }

        public class TestOptions
        {
            public int Value { get; set; }
        }
    }
}