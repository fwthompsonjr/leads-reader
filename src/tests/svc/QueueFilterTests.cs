using Bogus.DataSets;
using Bogus;
using component;
using legallead.jdbc.entities;
using legallead.jdbc.interfaces;
using legallead.permissions.api.Model;
using legallead.reader.service.interfaces;
using legallead.reader.service.services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Reflection;

namespace legallead.reader.service.tests.svc
{
    public class QueueFilterTests
    {
        [Theory]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(0, false)]
        public void ServiceCanGetIsEnabled(int locationId, bool hasIndex)
        {
            var error = Record.Exception(() =>
            {
                var service = GetFilter(locationId, hasIndex, out var _);
                _ = service.IsEnabled;
            });
            Assert.Null(error);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(0, false)]
        [InlineData(0, true, 15, 10)]
        [InlineData(0, true, 0, 0)]
        [InlineData(0, true, 10, 0)]
        public void ServiceCanApply(int locationId, bool hasIndex, int rcount = 10, int expectedCount = 10)
        {
            var error = Record.Exception(() =>
            {
                var fkr = new Faker();
                var service = GetFilter(locationId, hasIndex, out var mqfilter);
                var indexes = mqfilter.Object.Indexes;
                var list = MockService.GetQueue(rcount);
                if (hasIndex)
                {
                    var id = 0;
                    while (id < expectedCount)
                    {
                        if (list.Count <= id || id > list.Count - 1) break;
                        list[id].UserId = fkr.PickRandom(indexes);
                        id++;
                    }
                }
                _ = service.Apply(list);
            });
            Assert.Null(error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(6)]
        [InlineData(12)]
        [InlineData(20)]
        public void NonFilterServiceCanApply(int rcount)
        {
            var error = Record.Exception(() =>
            {
                var service = new QueueNonFilter();
                var list = MockService.GetQueue(rcount);
                var filtered = service.Apply(list);
                Assert.True(service.IsEnabled);
                Assert.Equal(rcount, filtered.Count);
            });
            Assert.Null(error);
        }

        private static class MockService
        {
            public static List<SearchQueueDto> GetQueue(int count)
            {
                if (count == 0) return [];
                return faker.Generate(count);
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
        private static QueueFilter GetFilter(int locationId, bool hasIndexes, out Mock<IIndexReader> reader)
        {
            reader = new Mock<IIndexReader>();
            var location = locationId switch
            {
                1 => string.Empty,
                2 => "not a file path",
                _ => Location
            };
            var indexes = hasIndexes ? new List<string> {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            } : [];
            reader.SetupGet(m => m.SearchLocation).Returns(location);
            reader.SetupGet(m => m.Indexes).Returns(indexes);
            return new(reader.Object);
        }

        private static string? searchLocation;
        private static string Location => searchLocation ??= GetLocation();
        private static string GetLocation()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        }
    }
}
