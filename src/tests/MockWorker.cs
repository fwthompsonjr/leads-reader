using component;
using Microsoft.Extensions.Logging;
using Moq;

namespace legallead.reader.service.tests
{
    internal class MockWorker : Worker
    {
        private static readonly Mock<ILogger<Worker>> _mockLogger = new();
        private static readonly Mock<ISearchGenerationService> _mockGenerationService = new();
        public MockWorker() : base(_mockLogger.Object, _mockGenerationService.Object, 50)
        {
            _mockLogger.Setup(m => m.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            _mockGenerationService.Setup(x => x.Report()).Verifiable();
        }
        public bool CanExecute()
        {
            try
            {
                using CancellationTokenSource source = new();
                CancellationToken token = source.Token;
                source.CancelAfter(TimeSpan.FromMilliseconds(300));
                for (int i = 0; i < 5; i++)
                {
                    ExecuteAsync(token).GetAwaiter().GetResult();
                    Thread.Sleep(100);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
