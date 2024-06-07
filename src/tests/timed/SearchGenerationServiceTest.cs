using component;
using legallead.jdbc.interfaces;
using legallead.reader.service.services;
using Microsoft.Extensions.DependencyInjection;

namespace legallead.reader.service.tests.timed
{
    public class SearchGenerationServiceTest
    {
        [Fact]
        public void ServiceCanBeCreated()
        {
            var problems = Record.Exception(() =>
            {
                var provider = new MqSvc().Provider;
                var logger = provider.GetRequiredService<ILoggingRepository>();
                var search = provider.GetRequiredService<ISearchQueueRepository>();
                var bck = provider.GetRequiredService<IBgComponentRepository>();
                var settings = provider.GetRequiredService<IBackgroundServiceSettings>();
                var exl = provider.GetRequiredService<IExcelGenerator>();
                var mn = provider.GetRequiredService<IMainWindowService>();
                var q = provider.GetRequiredService<IQueueFilter>();
                var sut = new SearchGenerationService(logger, search, bck, settings, exl, mn, q);
                Assert.NotNull(sut);
            });
            Assert.Null(problems);
        }
        private sealed class MqSvc
        {
            private readonly IServiceProvider serviceProvider;
            public MqSvc()
            {
                var services = new ServiceCollection();
                services.Setup();
                serviceProvider = services.BuildServiceProvider();
            }
            public IServiceProvider Provider => serviceProvider;
        }
    }
}
