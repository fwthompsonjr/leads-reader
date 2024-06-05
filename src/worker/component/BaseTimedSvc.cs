using legallead.jdbc.interfaces;
using legallead.reader.component;
using Newtonsoft.Json;
using System.Diagnostics;

namespace component
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
        "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
    internal abstract class BaseTimedSvc<T> : IHostedService, IDisposable where T : class
    {
        protected readonly ILoggingRepository? _logger;
        protected readonly ISearchQueueRepository? _queueDb;
        protected readonly Svcs? DataService;
        protected readonly IBgComponentRepository? _componentDb;
        protected readonly string _componentName;
        protected readonly string _typeName;
        private Timer? _timer = null;
        private bool disposedValue;
        protected readonly object _lock = new();
        protected virtual int DelayedStartInSeconds { get; set; }
        protected virtual int IntervalInMinutes { get; set; }
        protected virtual bool IsServiceEnabled { get; set; }

        public bool IsWorking { get; protected set; }

        protected BaseTimedSvc(
            ILoggingRepository? logger,
            ISearchQueueRepository? repo,
            IBgComponentRepository? component,
            IBackgroundServiceSettings? settings)
        {
            _logger = logger;
            _queueDb = repo;
            _componentDb = component;

            _componentName = typeof(T).Namespace ?? "namespace.undefined";
            _typeName = typeof(T).Name;

            IsServiceEnabled = settings?.Enabled ?? false;
            DelayedStartInSeconds = settings?.Delay ?? 15;
            IntervalInMinutes = settings?.Interval ?? 3;
            DataService = new Svcs(logger, component, _componentName, _typeName);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!IsServiceEnabled) return Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
            var message = $"{typeof(T).Name} : {DateTime.Now:s} : Timed Process is starting";
            Console.WriteLine(message);
            _timer = new Timer(OnTimer, null, TimeSpan.FromSeconds(DelayedStartInSeconds), TimeSpan.FromMinutes(IntervalInMinutes));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var message = $"{typeof(T).Name} : {DateTime.Now:s} : Timed Process is stopping";
            Console.WriteLine(message);

            _timer?.Change(Timeout.Infinite, 0);
            StopDrivers();
            return Task.CompletedTask;
        }

        protected abstract void DoWork(object? state);


        private void OnTimer(object? state)
        {
            if (DataService == null) return;
            var isActive = DataService.GetStatus().Result;
            DataService.ReportState(isActive);
            DataService.ReportHealth(GetHealth());
            if (isActive) DoWork(state);
        }


        private static void StopDrivers()
        {
            var processes = new List<string> {
                "geckodriver",
                "chromedriver",
                "IEDriverServer" };
            processes.ForEach(Kill);
        }

        private static void Kill(string processName)
        {
            var enumerable = Process.GetProcessesByName(processName);
            if (enumerable == null || !enumerable.Any()) return;
            foreach (var process in enumerable)
            {
                process.Kill();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _timer?.Dispose();
                    StopDrivers();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private string GetHealth()
        {
            if (_logger == null || _componentDb == null || _queueDb == null)
                return "Unhealthly";
            if (_timer == null) return "Degraded";
            return "Healthy";
        }

        protected class Svcs
        {
            private readonly ILoggingRepository? _logging;
            private readonly IBgComponentRepository? _componentDb;
            private readonly string _componentName;
            private readonly string _typeName;

            public Svcs(
                ILoggingRepository? logging,
                IBgComponentRepository? component,
                string componentName,
                string typeName)
            {
                _logging = logging;
                _componentDb = component;
                _componentName = componentName;
                _typeName = typeName;
            }

            private const string ns = "legallead.reader.component";

            /// <summary>
            /// This method writes an information record to the log.
            /// </summary>
            /// <param name="message"></param>
            internal void Echo(string message)
            {
                if (_logging == null) return;
                try
                {
                    var log = $"{_componentName}:{_typeName} -- {message}";
                    _logging.ClassContext = _componentName;
                    _ = _logging.LogInformation(log).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _ = _logging.LogError(ex);
                }
            }
            /// <summary>
            ///	There is a table in the repository that stores the on/off state
            /// of each service. This method queries the database
            /// to determine if the service is active.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style",
                "VSTHRD200:Use \"Async\" suffix for async methods",
                Justification = "Not willing to following convention for this method.")]
            public async Task<bool> GetStatus()
            {
                if (_logging == null || _componentDb == null) return false;
                var message = string.Format("GetStatus: {0}, {1}", _componentName, _typeName);
                try
                {
                    await _logging.LogDebug(message);
                    var response = await _componentDb.GetStatus(_componentName, _typeName);
                    return response;
                }
                catch (Exception ex)
                {
                    await _logging.LogError(ex);
                    return true;
                }
            }

            /// <summary>
            /// This method writes a record to the log.
            /// Confirming that this instance is either active or inactive.
            /// </summary>
            /// <param name="isActive"></param>
            internal void ReportState(bool isActive)
            {
                if (_logging == null || _componentDb == null) return;
                try
                {
                    var log = new
                    {
                        Environment.MachineName,
                        ComponentType = _componentName,
                        ProcessName = _typeName,
                        IsActive = isActive,
                        DateCreated = DateTime.UtcNow
                    };
                    var serial = JsonConvert.SerializeObject(log);
                    _ = _logging.LogInformation(serial).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _ = _logging.LogError(ex);
                }
            }

            /// <summary>
            /// This method writes a record to the component status.
            /// Confirming that this instance is healthy, unhealthy or degraded.
            /// </summary>
            /// <param name="componentName"></param>
            /// <param name="typeName"></param>
            /// <param name="health"></param>
            internal void ReportHealth(string health)
            {
                if (_logging == null || _componentDb == null) return;
                try
                {
                    _ = _componentDb.ReportHealth(_componentName, _typeName, health).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _ = _logging.LogError(ex);
                }
            }
        }

    }
}
