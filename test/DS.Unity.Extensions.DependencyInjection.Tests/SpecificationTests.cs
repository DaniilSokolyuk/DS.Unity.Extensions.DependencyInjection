using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Practices.Unity;

namespace DS.Unity.Extensions.DependencyInjection.Tests
{
    public class SpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var container = new UnityContainer();

            container.Populate(serviceCollection);

            return container.Resolve<IServiceProvider>();
        }
    }
}