using component;
using legallead.jdbc.interfaces;
using legallead.reader.service.models;
using Moq;
using System.Diagnostics;

namespace legallead.reader.service.tests
{
    internal class MockTimedService : BaseTimedSvc<MockTimedService>
    {

        private static readonly Mock<ILoggingRepository> _mock_logger = new();
        private static readonly Mock<ISearchQueueRepository> _mock_queueDb = new();
        private static readonly Mock<IBgComponentRepository> _mock_componentDb = new();
        public MockTimedService(
            bool hasSetting = true,
            bool isEnabled = true,
            int delay = 30,
            int interval = 2) :
            base(
                _mock_logger.Object,
                _mock_queueDb.Object,
                _mock_componentDb.Object,
                GetSetting(hasSetting, isEnabled, delay, interval))
        { }

        public async Task<bool> StartAndStopAsync()
        {
            try
            {
                var token = CancellationToken.None;
                await StartAsync(token);
                await StopAsync(token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool VerifyTimer()
        {
            try
            {
                OnTimer(null);
                return true;
            }
            catch { return false; }
        }
        public bool VerifyHealth()
        {
            try
            {
                var health = GetHealth();
                return !string.IsNullOrWhiteSpace(health);
            }
            catch { return false; }
        }
        public bool VerifyEcho()
        {
            try
            {
                const string message = "12345 6789 0";
                if (DataService == null) return false;
                DataService.Echo(message);
                return true;
            }
            catch { return false; }
        }

        protected override void DoWork(object? state)
        {
            Debug.WriteLine("Component is doing work.");
        }

        private static BackgroundServiceSettings? GetSetting(
            bool hasSetting = true,
            bool isEnabled = true,
            int delay = 30,
            int interval = 2)
        {
            if (!hasSetting) return null;
            return new BackgroundServiceSettings
            {
                Enabled = isEnabled,
                Delay = delay,
                Interval = interval
            };
        }
    }
}
