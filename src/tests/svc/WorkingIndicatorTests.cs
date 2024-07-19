using legallead.reader.service.services;
using Newtonsoft.Json;
using System.Reflection;

namespace legallead.reader.service.tests.svc
{
    public class WorkingIndicatorTests : IDisposable
    {
        private bool disposedValue;

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ServiceCanBeCreated(bool status)
        {
            var exception = Record.Exception(() =>
            {
                var sut = GetService();
                Assert.True(Directory.Exists(sut.SearchLocation));
                sut.Update(status);
            });
            Assert.Null(exception);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DeleteStatusFile();
                }
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WorkingIndicatorTests()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        private static WorkingIndicator GetService()
        {
            CreateStatusFile();
            return new();
        }

        private static void CreateStatusFile()
        {
            var exeName = new Uri(Assembly.GetExecutingAssembly().Location).AbsolutePath;
            var exePath = Path.GetDirectoryName(exeName) ?? string.Empty;
            var configPath = Path.Combine(exePath, "_sys");
            if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);
            var targetFile = Path.Combine(configPath, childFileName);
            if (File.Exists(targetFile)) return;
            lock (sync)
            {
                var model = new { isWorking = false, autoShutdown = false };
                var serialized = JsonConvert.SerializeObject(model, Formatting.Indented);
                File.WriteAllText(targetFile, serialized);
            }
        }
        private static void DeleteStatusFile()
        {
            var exeName = new Uri(Assembly.GetExecutingAssembly().Location).AbsolutePath;
            var exePath = Path.GetDirectoryName(exeName) ?? string.Empty;
            var configPath = Path.Combine(exePath, "_sys");
            if (!Directory.Exists(configPath)) return;
            var targetFile = Path.Combine(configPath, childFileName);
            if (!File.Exists(targetFile)) return;
            lock (sync)
            {
                File.Delete(targetFile);
            }
        }
        private const string childFileName = "process-state.json";
        private static readonly object sync = new();
    }
}
