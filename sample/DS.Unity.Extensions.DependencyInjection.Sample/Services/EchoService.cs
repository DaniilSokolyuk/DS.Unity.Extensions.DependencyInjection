namespace DS.Unity.Extensions.DependencyInjection.Sample.Services
{
    public class EchoService : IEchoService
    {
        public string Echo(string message)
        {
            return message;
        }
    }
}
