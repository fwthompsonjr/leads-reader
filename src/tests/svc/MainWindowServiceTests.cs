using legallead.reader.service.services;
using Microsoft.Extensions.Configuration;

namespace legallead.reader.service.tests.svc
{
    public class MainWindowServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void ServiceCanBeCreated(int id)
        {
            var error = Record.Exception(() =>
            {
                var configuration = GetConfiguration(id);
                var sut = new MainWindowService(configuration);
                _ = sut.IsMainVisible;
            });
            Assert.Null(error);
        }
        private static IConfiguration? GetConfiguration(int typeIndex)
        {
            if (typeIndex == 0) return null;
            var showWindow = typeIndex switch
            {
                1 => "true",
                2 => "false",
                3 => "null",
                _ => "true"
            };
            var cfg =
                new List<KeyValuePair<string, string?>> {
                new("ShowAppWindow", showWindow) };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(cfg)
                .Build();
        }
    }
}
