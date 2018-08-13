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
    }
}