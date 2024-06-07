using component;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace legallead.reader.service.tests.timed
{
    public class WorkerTests
    {
        [Fact]
        public void ServiceCanBeCreated()
        {
            var problems = Record.Exception(() =>
            {
                var provider = new MqSvc().Provider;
                var logger = provider.GetRequiredService<ILogger<Worker>>();
                var search = provider.GetRequiredService<ISearchGenerationService>();
                var sut = new Worker(logger, search);
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
