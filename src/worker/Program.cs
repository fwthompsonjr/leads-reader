using component;
using legallead.reader.component.utility;

namespace legallead.reader.component
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args).UseSystemd();
            builder.ConfigureServices(services =>
            {
                services.Initialize();
                services.AddWindowsService();
                services.AddHostedService<Worker>();
                services.AddHostedService(s => s.GetRequiredService<SearchGenerationService>());
            });

            var host = builder.Build();
            host.Run();
        }
    }
}