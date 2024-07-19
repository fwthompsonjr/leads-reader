using Bogus;
using legallead.jdbc.interfaces;
using legallead.reader.service.services;
using legallead.records.search.Models;
using Moq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace legallead.reader.service.tests.svc
{
    public class ExcelGeneratorTests
    {
        [Fact]
        public void BuilderCanContruct()
        {
            var builder = new Builder();
            Assert.NotEmpty(builder.Payload.PeopleList);
        }

        [Fact]
        public void BuilderGetAddresses()
        {
            var builder = new Builder();
            Assert.True(builder.GetAddress());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BuilderCanGetResult(bool hadError)
        {
            var exception = Record.Exception(() =>
            {
                var builder = new Builder();
                _ = builder.GetResult(hadError);
            });
            Assert.Null(exception);
        }

        private sealed class Builder
        {
            private static readonly List<int> Indexes = [1, 10, 20, 30];
            private readonly Mock<ILoggingRepository> _logger = new();
            private readonly Mock<ISearchQueueRepository> _repo = new();
            private static readonly Faker<WebFetchResult> wfaker
                = new Faker<WebFetchResult>()
                .RuleFor(x => x.WebsiteId, y => y.PickRandom(Indexes))
                .RuleFor(x => x.Result, y =>
                {
                    var path = y.System.FilePath();
                    var shortName = string.Concat(Path.GetFileNameWithoutExtension(path), ".xml");
                    return Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, shortName);
                });
            public Builder()
            {
                Payload = wfaker.Generate();
                Payload.PeopleList = MockPersonAddressProvider.GetPeople(10);
            }
            public WebFetchResult Payload { get; set; }

            public bool GetAddress()
            {
                try
                {
                    var generator = new ExcelGenerator();
                    generator.GetAddresses(Payload, _logger.Object);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public bool GetResult(bool hasError = false)
            {
                try
                {
                    if (hasError)
                    {
                        var issue = new Faker().System.Exception();
                        _repo.Setup(m => m.Content(
                            It.IsAny<string>(),
                            It.IsAny<byte[]>())).ThrowsAsync(issue);
                    }
                    else
                    {
                        var kvp = new KeyValuePair<bool, string>(true, string.Empty);
                        _repo.Setup(m => m.Content(
                            It.IsAny<string>(),
                            It.IsAny<byte[]>())).ReturnsAsync(kvp);
                    }
                    var generator = new ExcelGenerator();
                    var addresses = generator.GetAddresses(Payload, _logger.Object);
                    if (addresses == null) { return false; }
                    return generator.SerializeResult(
                        "",
                        addresses,
                        _repo.Object,
                        _logger.Object);
                }
                catch
                {
                    return false;
                }
            }

        }
    }
}
