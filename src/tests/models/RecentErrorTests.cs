using Bogus;
using legallead.reader.service.models;

namespace legallead.reader.service.tests.models
{
    public class RecentErrorTests
    {
        private static readonly Faker<RecentError> faker
            = new Faker<RecentError>()
            .RuleFor(x => x.CreateDate, y => y.Date.Recent())
            .RuleFor(x => x.Message, y => y.Hacker.Phrase());
        [Fact]
        public void ModelCanBeGenerated()
        {
            var exception = Record.Exception(() =>
            {
                _ = faker.Generate();
            });
            Assert.Null(exception);
        }

        [Fact]
        public void ModelCanSetCreateDate()
        {
            var exception = Record.Exception(() =>
            {
                var list = faker.Generate(2);
                list[0].CreateDate = list[1].CreateDate;
                Assert.Equal(list[0].CreateDate, list[1].CreateDate);
            });
            Assert.Null(exception);
        }

        [Fact]
        public void ModelCanSetMessage()
        {
            var exception = Record.Exception(() =>
            {
                var list = faker.Generate(2);
                list[0].Message = list[1].Message;
                Assert.Equal(list[0].Message, list[1].Message);
            });
            Assert.Null(exception);
        }
    }
}
