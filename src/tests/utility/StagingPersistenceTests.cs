using Bogus;
using Bogus.DataSets;
using legallead.jdbc.entities;
using legallead.jdbc.interfaces;
using legallead.reader.service.utility;
using legallead.records.search.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace legallead.reader.service.tests.utility
{
    public class StagingPersistenceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RepoCanAdd(bool isOk)
        {
            var exception = Record.Exception(() =>
            {
                var sut = GetPersistence();
                var mock = sut.MqRepo;
                var persistence = sut.Persistence;
                var payload = sut.KeyParameter;
                var response = new KeyValuePair<bool, string>(isOk, payload.Message);
                mock.Setup(m => m.Append(
                    It.Is<jdbc.SearchTargetTypes>(s => s == jdbc.SearchTargetTypes.Staging),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                    )).ReturnsAsync(response);
                persistence.Add(payload.Id, payload.Key, payload.Value);
                mock.Verify(m => m.Append(
                    It.Is<jdbc.SearchTargetTypes>(s => s == jdbc.SearchTargetTypes.Staging),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                    ));
            });
            Assert.Null(exception);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RepoCanAddBytes(bool isOk)
        {
            var exception = Record.Exception(() =>
            {
                var sut = GetPersistence();
                var mock = sut.MqRepo;
                var persistence = sut.Persistence;
                var payload = sut.KeyParameter;
                var response = new KeyValuePair<bool, string>(isOk, payload.Message);
                mock.Setup(m => m.Append(
                    It.Is<jdbc.SearchTargetTypes>(s => s == jdbc.SearchTargetTypes.Staging),
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<string>()
                    )).ReturnsAsync(response);
                persistence.Add(payload.Id, payload.Key, payload.ValueArray);
                mock.Verify(m => m.Append(
                    It.Is<jdbc.SearchTargetTypes>(s => s == jdbc.SearchTargetTypes.Staging),
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<string>()
                    ));
            });
            Assert.Null(exception);
        }
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void RepoCanFetch(bool isOk, bool isContent)
        {
            var exception = Record.Exception(() =>
            {
                var sut = GetPersistence();
                var mock = sut.MqRepo;
                var persistence = sut.Persistence;
                var payload = sut.KeyParameter;
                var response = sut.GetFetchResponse(isOk, isContent);
                mock.Setup(m => m.GetStaged(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                    )).ReturnsAsync(response);
                persistence.Fetch(payload.Id, payload.Key);
                mock.Verify(m => m.GetStaged(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                    ));
            });
            Assert.Null(exception);
        }
        private static MockStagingPersistence GetPersistence()
        {
            return new MockStagingPersistence();
        }

        private sealed class MockStagingPersistence
        {
            public MockStagingPersistence()
            {
                MqRepo = new Mock<IUserSearchRepository>();
                Persistence = new(MqRepo.Object);
            }
            public Mock<IUserSearchRepository> MqRepo { get; private set; }
            public StagingPersistence Persistence { get; private set; }
            public AddKeyParameter KeyParameter { get; private set; } = new();

            public KeyValuePair<bool, object> GetFetchResponse(bool isFailed = false, bool isContent = true)
            {
                var phrase = faker.Hacker.Phrase();
                var arr = Encoding.UTF8.GetBytes(phrase);
                var model = new SearchStagingDto
                {
                    Id = faker.Random.Guid().ToString(),
                    StagingType = faker.Lorem.Word(),
                    LineNbr = faker.Random.Int(1, 500),
                    LineData = arr,
                    LineText = phrase,
                    IsBinary = faker.Random.Bool(),
                    CreateDate = faker.Date.Recent()
                };
                object data = isContent ? model : new { model.Id };
                return new KeyValuePair<bool, object>(isFailed, data);
            }
        }

        private sealed class AddKeyParameter
        {
            public AddKeyParameter()
            {
                Id = faker.Random.Guid().ToString();
                Key = faker.Lorem.Word();
                Value = faker.Hacker.Phrase();
                ValueArray = Encoding.UTF8.GetBytes(Value);
                Message = faker.System.Random.Words(5);
            }
            public string Id { get; }
            public string Key { get; }
            public string Value { get; }
            public byte[] ValueArray { get; }
            public string Message { get; }
        }
        private static readonly Faker faker = new ();
    }
}
