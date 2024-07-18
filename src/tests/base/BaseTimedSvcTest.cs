namespace legallead.reader.service.tests.@base
{
    public class BaseTimedSvcTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ServiceCanStartAsync(bool hasConfiguration)
        {
            var exception = await Record.ExceptionAsync(async () =>
            {
                using var sut = new MockTimedService(hasConfiguration);
                _ = sut.IsWorking;
                _ = await sut.StartAndStopAsync();
            });
            Assert.Null(exception);
        }

        [Fact]
        public void ServiceCanVerifyTimer()
        {
            var exception = Record.Exception(() =>
            {
                using var sut = new MockTimedService();
                var verified = sut.VerifyTimer();
                Assert.True(verified);
            });
            Assert.Null(exception);
        }

        [Fact]
        public void ServiceCanVerifyHealth()
        {
            var exception = Record.Exception(() =>
            {
                using var sut = new MockTimedService();
                var verified = sut.VerifyHealth();
                Assert.True(verified);
            });
            Assert.Null(exception);
        }

        [Fact]
        public void ServiceCanVerifyEcho()
        {
            var exception = Record.Exception(() =>
            {
                using var sut = new MockTimedService();
                var verified = sut.VerifyEcho();
                Assert.True(verified);
            });
            Assert.Null(exception);
        }
    }
}
