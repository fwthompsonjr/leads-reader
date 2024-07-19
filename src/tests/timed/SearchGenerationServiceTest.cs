using Bogus;
using component;
using legallead.jdbc.entities;
using legallead.jdbc.interfaces;
using legallead.reader.service.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace legallead.reader.service.tests.timed
{
    public class SearchGenerationServiceTest
    {
        [Fact]
        public void ServiceCanBeCreated()
        {
            var problems = Record.Exception(() =>
            {
                var testsetup = new MockService();
                var sut = testsetup.TestService;
                Assert.NotNull(sut);
            });
            Assert.Null(problems);
        }

        [Fact]
        public void ServiceCanReport()
        {
            var problems = Record.Exception(() =>
            {
                var testsetup = new MockService();
                var sut = testsetup.TestService;
                sut.Report();
            });
            Assert.Null(problems);
        }

        [Theory]
        [InlineData(5, 5)]
        [InlineData(1, 1)]
        [InlineData(10, 0)]
        [InlineData(0, 10)]
        [InlineData(10, 10)]
        [InlineData(10, 10, false)]
        [InlineData(2, 2, true, false)]
        public void ServiceCanSearch(int found, int filtered, bool canFind = true, bool canFilter = true)
        {
            var problems = Record.Exception(() =>
            {
                var testsetup = new MockService();
                var sut = testsetup.TestService;
                var provider = testsetup.Provider;
                var mqhelper = provider.GetService<Mock<ISearchGenerationHelper>>();
                var mqfilter = provider.GetService<Mock<IQueueFilter>>();
                var fkr = new Faker();
                var exception1 = fkr.System.Exception();
                var exception2 = fkr.System.Exception();
                Assert.NotNull(mqhelper);
                Assert.NotNull(mqfilter);

                var response = testsetup.GetQueue(found) ?? new();
                var filter = testsetup.GetQueue(filtered) ?? new();
                if (canFind)
                    mqhelper.Setup(m => m.GetQueueAsync()).ReturnsAsync(response);
                else
                    mqhelper.Setup(m => m.GetQueueAsync()).ThrowsAsync(exception1);


                if (canFilter)
                    mqfilter.Setup(m => m.Apply(It.IsAny<List<SearchQueueDto>>())).Returns(filter);
                else
                    mqfilter.Setup(m => m.Apply(It.IsAny<List<SearchQueueDto>>())).Throws(exception2);

                mqhelper.Setup(m => m.Generate(It.IsAny<SearchQueueDto>())).Verifiable();

                sut.Search();

            });
            Assert.Null(problems);
        }

        private sealed class MockService
        {
            public MockService()
            {
                ServiceMock = new MqSvc();
                var provider = ServiceMock.Provider;
                var logger = provider.GetRequiredService<ILoggingRepository>();
                var search = provider.GetRequiredService<ISearchQueueRepository>();
                var bck = provider.GetRequiredService<IBgComponentRepository>();
                var settings = provider.GetRequiredService<IBackgroundServiceSettings>();
                var mn = provider.GetRequiredService<IMainWindowService>();
                var q = provider.GetRequiredService<IQueueFilter>();
                var indc = provider.GetRequiredService<IWorkingIndicator>();
                var helper = provider.GetRequiredService<ISearchGenerationHelper>();
                TestService = new SearchGenerationService(logger, search, bck, settings, mn, q, indc, helper);
            }
            public MqSvc ServiceMock { get; }
            public SearchGenerationService TestService { get; }
            public IServiceProvider Provider => ServiceMock.Provider;

            public List<SearchQueueDto>? GetQueue(int count)
            {
                if (count < 0) return null;
                return faker.Generate(count);
            }
            private readonly static Faker<SearchQueueDto> faker = new Faker<SearchQueueDto>()
                .RuleFor(x => x.Id, y => y.Random.Guid().ToString());

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
