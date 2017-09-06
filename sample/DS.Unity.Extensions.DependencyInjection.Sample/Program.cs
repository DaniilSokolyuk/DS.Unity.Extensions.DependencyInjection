using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace DS.Unity.Extensions.DependencyInjection.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services => services.AddUnity())
                .UseStartup<Startup>()
                .Build();
    }
}