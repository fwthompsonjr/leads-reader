using Bogus;
using legallead.logging.interfaces;
using legallead.reader.service.services;
using Moq;

namespace legallead.reader.service.tests.svc
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3236:Caller information arguments should not be provided explicitly", Justification = "<Pending>")]
    public class LoggingRepositoryTests
    {
        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ServiceCanLogCriticalAsync(bool hasLogger, bool hasError)
        {
            var service = GetRepository(hasLogger, out var logger);
            var exception = new Faker().System.Exception();
            var error = await Record.ExceptionAsync(async () =>
            {
                if (hasError)
                {
                    logger.Setup(m => m.LogCritical(
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<string>())).ThrowsAsync(exception);
                }
                await service.LogCritical("abcdefg");
            });
            Assert.Null(error);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ServiceCanLogDebugAsync(bool hasLogger, bool hasError)
        {
            var service = GetRepository(hasLogger, out var logger);
            var exception = new Faker().System.Exception();
            var error = await Record.ExceptionAsync(async () =>
            {
                if (hasError)
                {
                    logger.Setup(m => m.LogDebug(
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<string>())).ThrowsAsync(exception);
                }
                await service.LogDebug("abcdefg");
            });
            Assert.Null(error);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ServiceCanLogErrorAsync(bool hasLogger, bool hasError)
        {
            var service = GetRepository(hasLogger, out var logger);
            var exception = new Faker().System.Exception();
            var payload = new Faker().System.Exception();
            var error = await Record.ExceptionAsync(async () =>
            {
                if (hasError)
                {
                    logger.Setup(m => m.LogError(
                        It.IsAny<Exception>(),
                        It.IsAny<int>(),
                        It.IsAny<string>())).ThrowsAsync(exception);
                }
                await service.LogError(payload);
            });
            Assert.Null(error);
        }


        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ServiceCanLogErrorMessageAsync(bool hasLogger, bool hasError)
        {
            var service = GetRepository(hasLogger, out var logger);
            var exception = new Faker().System.Exception();
            var payload = new Faker().System.Exception();
            var error = await Record.ExceptionAsync(async () =>
            {
                if (hasError)
                {
                    logger.Setup(m => m.LogError(
                        It.IsAny<Exception>(),
                        It.IsAny<int>(),
                        It.IsAny<string>())).ThrowsAsync(exception);
                }
                await service.LogError(payload, "1", "2");
            });
            Assert.Null(error);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ServiceCanLogInformationAsync(bool hasLogger, bool hasError)
        {
            var service = GetRepository(hasLogger, out var logger);
            var exception = new Faker().System.Exception();
            var payload = new Faker().System.Exception().Message;
            var error = await Record.ExceptionAsync(async () =>
            {
                if (hasError)
                {
                    logger.Setup(m => m.LogInformation(
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<string>())).ThrowsAsync(exception);
                }
                await service.LogInformation(payload);
            });
            Assert.Null(error);
        }


        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ServiceCanLogInformationMessageAsync(bool hasLogger, bool hasError)
        {
            var service = GetRepository(hasLogger, out var logger);
            var exception = new Faker().System.Exception();
            var payload = new Faker().System.Exception().Message;
            var error = await Record.ExceptionAsync(async () =>
            {
                if (hasError)
                {
                    logger.Setup(m => m.LogInformation(
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<string>())).ThrowsAsync(exception);
                }
                await service.LogInformation(payload, "1", "2");
            });
            Assert.Null(error);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ServiceCanLogVerboseAsync(bool hasLogger, bool hasError)
        {
            var service = GetRepository(hasLogger, out var logger);
            var exception = new Faker().System.Exception();
            var payload = new Faker().System.Exception().Message;
            var error = await Record.ExceptionAsync(async () =>
            {
                if (hasError)
                {
                    logger.Setup(m => m.LogVerbose(
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<string>())).ThrowsAsync(exception);
                }
                await service.LogVerbose(payload);
            });
            Assert.Null(error);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task ServiceCanLogWarningAsync(bool hasLogger, bool hasError)
        {
            var service = GetRepository(hasLogger, out var logger);
            var exception = new Faker().System.Exception();
            var payload = new Faker().System.Exception().Message;
            var error = await Record.ExceptionAsync(async () =>
            {
                if (hasError)
                {
                    logger.Setup(m => m.LogWarning(
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<string>())).ThrowsAsync(exception);
                }
                await service.LogWarning(payload);
            });
            Assert.Null(error);
        }

        private static LoggingRepository GetRepository(bool hasLogger, out Mock<ILoggingService> svc)
        {
            svc = new Mock<ILoggingService>();
            return hasLogger ? new LoggingRepository(svc.Object) : new LoggingRepository(null);
        }
    }
}
