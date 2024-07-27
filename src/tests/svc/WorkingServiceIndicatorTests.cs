using legallead.reader.service.services;

namespace legallead.reader.service.tests.svc
{
    public class WorkingServiceIndicatorTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ServiceCanBeCreated(bool status)
        {
            var exception = Record.Exception(() =>
            {
                var sut = GetService();
                Assert.True(string.IsNullOrEmpty(sut.SearchLocation));
                sut.Update(status);
            });
            Assert.Null(exception);
        }


        private static WorkingServiceIndicator GetService()
        {
            return new();
        }
    }
}