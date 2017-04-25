using System.Linq;
using Microsoft.Practices.Unity;
using NUnit;
using NUnit.Framework;

namespace DS.Unity.Extensions.DependencyInjection.Tests
{
    internal static class Assertions
    {
        public static void AssertRegistered<TService>(this IUnityContainer context)
        {
            Assert.True(context.IsRegistered<TService>());
        }

        public static void AssertNotRegistered<TService>(this IUnityContainer context)
        {
            Assert.False(context.IsRegistered<TService>());
        }

        public static void AssertImplementation<TService, TImplementation>(this IUnityContainer context)
        {
            var service = context.Resolve<TService>();
            Assert.IsAssignableFrom<TImplementation>(service);
        }

        //public static void AssertSharing<TComponent>(this IUnityContainer context, InstanceSharing sharing)
        //{
        //    var cr = context.RegistrationFor<TComponent>();
        //    Assert.AreEqual(sharing, cr.Sharing);
        //}

        public static void AssertLifetime<TComponent, TLifetime>(this IUnityContainer context)
        {
            var cr = context.RegistrationFor<TComponent>();
            Assert.AreEqual(typeof(TLifetime), cr.LifetimeManagerType);
        }

        //public static void AssertOwnership<TComponent>(this IUnityContainer context, InstanceOwnership ownership)
        //{
        //    var cr = context.RegistrationFor<TComponent>();
        //    Assert.AreEqual(ownership, cr.Ownership);
        //}

        public static ContainerRegistration RegistrationFor<TComponent>(this IUnityContainer context)
        {
            var r = context.Registrations.FirstOrDefault(p => p.RegisteredType == typeof(TComponent));
            Assert.NotNull(r);
            return r;
        }
    }
}