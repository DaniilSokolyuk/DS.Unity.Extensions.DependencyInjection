using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Microsoft.Practices.Unity;
using Xunit;

namespace DS.Unity.Extensions.DependencyInjection.SpecificationTests
{
    public class SpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var container = new UnityContainer();

            container.Populate(serviceCollection);

            return new UnityServiceProvider(container);
        }

        [Fact]
        public void DisposingScopeDisposesService()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IFakeSingletonService, FakeService>();
            services.AddScoped<IFakeScopedService, FakeService>();
            services.AddTransient<IFakeService, FakeService>();
            IServiceProvider serviceProvider = this.CreateServiceProvider((IServiceCollection)services);
            FakeService fakeService = Assert.IsType<FakeService>((object)serviceProvider.GetService<IFakeService>());
            FakeService service1;
            FakeService service2;
            FakeService service3;
            FakeService service4;
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                service1 = (FakeService)scope.ServiceProvider.GetService<IFakeScopedService>();
                service2 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                service3 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                service4 = (FakeService)scope.ServiceProvider.GetService<IFakeSingletonService>();
                Assert.False(service1.Disposed);
                Assert.False(service2.Disposed);
                Assert.False(service3.Disposed);
                Assert.False(service4.Disposed);
            }
            Assert.True(service1.Disposed);
            Assert.True(service2.Disposed);
            Assert.True(service3.Disposed);
            Assert.False(service4.Disposed);
            IDisposable disposable = serviceProvider as IDisposable;
            if (disposable == null)
                return;
            disposable.Dispose();
            Assert.True(service4.Disposed);
            Assert.True(fakeService.Disposed);
        }
    }
}