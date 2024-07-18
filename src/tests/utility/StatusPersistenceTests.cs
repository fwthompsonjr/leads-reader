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
    public class StatusPersistenceTests
    {
        [Fact]
        public void RepoCanBeConstructed()
        {
            var sut = GetPersistence();
            Assert.NotNull(sut.LivePersistence);
        }

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
                    It.Is<jdbc.SearchTargetTypes>(s => s == jdbc.SearchTargetTypes.Status),
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()
                    )).ReturnsAsync(response);
                persistence.Status(payload.Id, payload.Message);
                mock.Verify(m => m.Append(
                    It.Is<jdbc.SearchTargetTypes>(s => s == jdbc.SearchTargetTypes.Status),
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()
                    ));
            });
            Assert.Null(exception);
        }

        private static MockStatusPersistence GetPersistence()
        {
            return new MockStatusPersistence();
        }

        private sealed class MockStatusPersistence
        {
            public MockStatusPersistence()
            {
                MqRepo = new Mock<IUserSearchRepository>();
                Persistence = new(MqRepo.Object);
            }
            public Mock<IUserSearchRepository> MqRepo { get; private set; }
            public StatusPersistence Persistence { get; private set; }
            public StatusPersistence LivePersistence { get; private set; } = new();
            public AddKeyParameter KeyParameter { get; private set; } = new();

        }

        private sealed class AddKeyParameter
        {
            public AddKeyParameter()
            {
                Id = faker.Random.Guid().ToString();
                Message = faker.System.Random.Words(5);
            }
            public string Id { get; }
            public string Message { get; }
        }
        private static readonly Faker faker = new();
    }
}