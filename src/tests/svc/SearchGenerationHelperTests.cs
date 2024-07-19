using Bogus;
using component;
using legallead.jdbc.entities;
using legallead.jdbc.interfaces;
using legallead.models.Search;
using legallead.permissions.api.Model;
using legallead.reader.service.interfaces;
using legallead.records.search.Classes;
using legallead.records.search.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace legallead.reader.service.tests.svc
{
    public class SearchGenerationHelperTests
    {

        [Fact]
        public void ServiceCanBeCreated()
        {
            var problems = Record.Exception(() =>
            {
                var testsetup = new MockService();
                _ = testsetup.GetQueue(2);
                Assert.NotNull(testsetup.MqGenerator);
                Assert.NotNull(testsetup.MqSearch);
                Assert.NotNull(testsetup.MqQueue);
                Assert.NotNull(testsetup.MqStatus);
                Assert.NotNull(testsetup.MqWrapper);
            });
            Assert.Null(problems);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void ServiceCanEnqueue(int count)
        {
            var problems = Record.Exception(() =>
            {
                var testsetup = new MockService();
                var sut = testsetup.TestService;
                var collection = testsetup.GetQueue(count) ?? [];
                var svc = testsetup.MqStatus;
                svc.Setup(m => m.Begin(It.IsAny<WorkBeginningBo>())).Returns(true);
                sut.Enqueue(collection);
                if (count != 0) svc.Verify(m => m.Begin(It.IsAny<WorkBeginningBo>()));
            });
            Assert.Null(problems);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ServiceCanGetQueueAsync(int count)
        {
            var problems = await Record.ExceptionAsync(async () =>
            {
                var testsetup = new MockService();
                var sut = testsetup.TestService;
                var collection = testsetup.GetQueue(count) ?? [];
                var svc = testsetup.MqQueue;
                svc.Setup(m => m.GetQueue()).ReturnsAsync(collection);
                await sut.GetQueueAsync();
                svc.Verify(m => m.GetQueue());
            });
            Assert.Null(problems);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(5, false)]
        [InlineData(12, true, 30)]
        public void ServiceCanCompleteGeneration(int count, bool hasParameter = true, int webIndex = -1)
        {
            var problems = Record.Exception(() =>
            {
                var testsetup = new MockService();
                var sut = testsetup.TestService;
                var collection = MockPersonAddressProvider.GetPeople(count);
                var dtos = testsetup.GetQueue(1);
                if (dtos == null) return;
                var dto = dtos[0];
                var bo = JsonConvert.DeserializeObject<UserSearchRequest>(dto.Payload ?? string.Empty) ?? new();
                var parameter = hasParameter ? WebMapper.MapFrom<UserSearchRequest, SearchRequest>(bo) ?? new() : null;
                if (webIndex >= 0 && parameter != null) parameter.WebId = webIndex;
                sut.GenerationComplete("", parameter, collection);
            });
            Assert.Null(problems);
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(7, 0)]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(0, 2)]
        [InlineData(0, 3)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 0)]
        [InlineData(2, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 0)]
        [InlineData(3, 1)]
        [InlineData(3, 2)]
        [InlineData(4, 0)]
        [InlineData(4, 1)]
        [InlineData(4, 2)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 2)]
        [InlineData(6, 0)]
        [InlineData(6, 1)]
        [InlineData(6, 2)]
        public void ServiceCanPostStatus(int messagId, int statusId)
        {
            var id = Guid.NewGuid().ToString();
            var problems = Record.Exception(() =>
            {
                var testsetup = new MockService();
                var sut = testsetup.TestService;
                sut.PostStatus(id, messagId, statusId);
            });
            Assert.Null(problems);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        public void ServiceCanGenerate(int conditionId, int records = 10)
        {
            var problems = Record.Exception(() =>
            {
                var error = new Faker().System.Exception();
                var testsetup = new MockService();
                var sut = testsetup.TestService;
                var collection = testsetup.GetQueue(1) ?? [];
                Assert.Single(collection);
                var dto = collection[0];
                var bo = JsonConvert.DeserializeObject<UserSearchRequest>(dto.Payload ?? string.Empty) ?? new();
                var interaction = conditionId == 6 ? null : WebMapper.MapFrom<UserSearchRequest, WebInteractive>(bo) ?? new();
                var parameter = WebMapper.MapFrom<UserSearchRequest, SearchRequest>(bo) ?? new();
                var people = MockPersonAddressProvider.GetPeople(records);
                var websiteId = conditionId == 7 ? 0 : parameter.WebId;
                var webresult = conditionId == 5 ? null : new WebFetchResult
                {
                    WebsiteId = websiteId,
                    PeopleList = people,
                    Result = MockService.GetXmlFileName()
                };
                var serialized = conditionId != 3;
                var package = conditionId == 2 ? null : new ExcelPackage();
                var wrapper = testsetup.MqWrapper;
                var generator = testsetup.MqGenerator;
                var status = testsetup.MqStatus;
                if (conditionId == 9)
                {
                    wrapper.Setup(m => m.Fetch(It.IsAny<WebInteractive>())).Throws(error);
                }
                else
                {
                    wrapper.Setup(m => m.Fetch(It.IsAny<WebInteractive>())).Returns(webresult);
                }
                wrapper.Setup(m => m.GetInterative(It.IsAny<UserSearchRequest>())).Returns(interaction);
                generator.Setup(m => m.GetAddresses(It.IsAny<WebFetchResult>(), It.IsAny<ILoggingRepository>())).Returns(package);
                if (conditionId == 1)
                {
                    status.Setup(s => s.Update(It.IsAny<WorkStatusBo>())).Throws(error);
                }
                if (conditionId == 8)
                {
                    generator.Setup(m => m.SerializeResult(
                        It.IsAny<string>(),
                        It.IsAny<ExcelPackage>(),
                        It.IsAny<ISearchQueueRepository>(),
                        It.IsAny<ILoggingRepository>())).Throws(error);
                }
                else
                {
                    generator.Setup(m => m.SerializeResult(
                        It.IsAny<string>(),
                        It.IsAny<ExcelPackage>(),
                        It.IsAny<ISearchQueueRepository>(),
                        It.IsAny<ILoggingRepository>())).Returns(serialized);
                }
                if (conditionId == 0) dto.Payload = string.Empty;
                if (conditionId == 4)
                {
                    bo.County.Name = "not-mapped";
                    dto.Payload = JsonConvert.SerializeObject(bo);
                }
                sut.Generate(dto);
            });
            Assert.Null(problems);
        }

        private sealed class MockService
        {
            public MockService()
            {
                ServiceMock = new MqSvc();
                var provider = ServiceMock.Provider;
                var excel = provider.GetRequiredService<IExcelGenerator>();
                var repo = provider.GetRequiredService<IUserSearchRepository>();
                var queue = provider.GetRequiredService<ISearchQueueRepository>();
                var sts = provider.GetRequiredService<ISearchStatusRepository>();
                var logger = provider.GetRequiredService<ILoggingRepository>();
                var wrapper = provider.GetRequiredService<IWebInteractiveWrapper>();
                TestService = new SearchGenerationHelper(excel, repo, queue, sts, logger, wrapper);
            }
            public MqSvc ServiceMock { get; }
            public SearchGenerationHelper TestService { get; }
            public IServiceProvider Provider => ServiceMock.Provider;

            public Mock<IExcelGenerator> MqGenerator => Provider.GetRequiredService<Mock<IExcelGenerator>>();
            public Mock<IUserSearchRepository> MqSearch => Provider.GetRequiredService<Mock<IUserSearchRepository>>();
            public Mock<ISearchQueueRepository> MqQueue => Provider.GetRequiredService<Mock<ISearchQueueRepository>>();
            public Mock<ISearchStatusRepository> MqStatus => Provider.GetRequiredService<Mock<ISearchStatusRepository>>();
            public Mock<IWebInteractiveWrapper> MqWrapper => Provider.GetRequiredService<Mock<IWebInteractiveWrapper>>();
            public List<SearchQueueDto>? GetQueue(int count)
            {
                if (count < 0) return null;
                return faker.Generate(count);
            }
            public static string GetXmlFileName()
            {
                var y = new Faker();
                var path = y.System.FilePath();
                var shortName = string.Concat(Path.GetFileNameWithoutExtension(path), ".xml");
                return Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, shortName);
            }
            private static string GetPayload()
            {
                var weekend = new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Saturday };
                var fk = new Faker();
                var startDate = fk.Date.Recent(180);
                while (weekend.Contains(startDate.DayOfWeek)) { startDate = startDate.AddDays(1); }
                var endingDate = startDate.AddDays(fk.Random.Int(0, 1));
                while (weekend.Contains(endingDate.DayOfWeek)) { endingDate = endingDate.AddDays(-1); }

                var model = new UserSearchRequest
                {
                    County = new UserSearchCounty { Name = fk.PickRandom(Counties).ToUpper(), Value = fk.Random.Int(1000, 5000) },
                    State = "TX",
                    Details = [],
                    StartDate = new DateTimeOffset(startDate).ToUnixTimeMilliseconds(),
                    EndDate = new DateTimeOffset(endingDate).ToUnixTimeMilliseconds()
                };
                return JsonConvert.SerializeObject(model, Formatting.Indented);
            }
            private readonly static Faker<SearchQueueDto> faker =
                new Faker<SearchQueueDto>()
                .RuleFor(x => x.Payload, y => GetPayload())
                .RuleFor(x => x.Id, y => y.Random.Guid().ToString());

            private static readonly List<string> Counties = "Collin,Denton,Harris,Tarrant".Split(',').ToList();

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
