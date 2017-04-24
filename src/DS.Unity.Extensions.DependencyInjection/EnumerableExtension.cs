using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace DS.Unity.Extensions.DependencyInjection
{
    public class EnumerableExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Context.Strategies.AddNew<EnumerableResolutionStrategy>(UnityBuildStage.TypeMapping);
        }
    }
}