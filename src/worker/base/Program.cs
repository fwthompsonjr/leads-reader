using component;
using legallead.reader.service.utility;
using System.Diagnostics.CodeAnalysis;

namespace legallead.reader.service
{
    [ExcludeFromCodeCoverage]
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