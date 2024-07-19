using legallead.jdbc.interfaces;
using legallead.reader.service;
using legallead.reader.service.interfaces;
using legallead.reader.service.models;
using legallead.reader.service.services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace component
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
        "VSTHRD110:Observe result of async calls", Justification = "This is a fire and forget call. No observer needed.")]
    internal class SearchGenerationService : BaseTimedSvc<SearchGenerationService>, ISearchGenerationService
    {
        private const string ns = "legallead.reader.service";
        private const string clsname = "search.generation.service";

        private readonly IQueueFilter queueFilterSvc;
        private readonly IWorkingIndicator indicatorSvc;
        private readonly ISearchGenerationHelper helperSvc;

        public SearchGenerationService(
            ILoggingRepository? logger,
            ISearchQueueRepository? repo,
            IBgComponentRepository? component,
            IBackgroundServiceSettings? settings,
            IMainWindowService main,
            IQueueFilter filter,
            IWorkingIndicator indicator,
            ISearchGenerationHelper helper) : base(logger, repo, component, settings)
        {
            queueFilterSvc = filter;
            indicatorSvc = indicator;
            helperSvc = helper;
            if (!main.IsMainVisible)
            {
                Debug.WriteLine("Main window is hidden.");
            }
        }

        public void Search()
        {
            DoWork(null);
        }

        public void Report()
        {
            Task.Run(() =>
            {
                var statuses = action;
                var stmt = string.Join(Environment.NewLine, statuses);
                var message = string.Format(stmt, GetServiceHealth(), IsWorking, ErrorCollection.Count > 0, ErrorCollection.Count);
                _logger?.LogInformation(message, ns, clsname);
            });
        }

        private string GetServiceHealth()
        {
            if (_logger == null || _queueDb == null || DataService == null) return "Unhealthy";
            if (ErrorCollection.Count > 0) return "Degraded";
            return "Healthy";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
            Justification = "<Pending>")]
        protected override void DoWork(object? state)
        {
            if (IsWorking) { return; }
            if (DataService == null || _queueDb == null) return;
            try
            {
                lock (_lock)
                {
                    IsWorking = true;
                    indicatorSvc.Update(IsWorking);
                    var queue = helperSvc.GetQueueAsync().GetAwaiter().GetResult();
                    if (queue == null || queue.Count == 0) return;
                    queue = queueFilterSvc.Apply(queue);
                    if (queue.Count == 0) { return; }
                    helperSvc.Enqueue(queue);
                    using var hideprocess = GetWindowService();
                    var message = $"Found ( {queue.Count} ) records to process.";
                    DataService.Echo(message);
                    queue.ForEach(q => helperSvc.Generate(q));
                }
            }
            catch (Exception ex)
            {
                _ = _logger?.LogError(ex, ns, clsname).ConfigureAwait(false);
                helperSvc.AppendError(ex.Message);
            }
            finally
            {
                IsWorking = false;
                indicatorSvc.Update(IsWorking);
            }
        }

        private static T? TryConvert<T>(string? data)
        {
            try
            {
                if (string.IsNullOrEmpty(data)) return default;
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception)
            {
                return default;
            }
        }

        private static HiddenWindowService GetWindowService()
        {
            return new HiddenWindowService(OperationMode.Headless);
        }
        [ExcludeFromCodeCoverage(Justification = "Private member tested from public methods")]
        private static OperationSetting GetSetting()
        {
            var fallback = new OperationSetting();
            var content = legallead.reader.service.Properties.Resources.operation_mode;
            if (string.IsNullOrEmpty(content)) return fallback;
            var mapped = TryConvert<OperationSetting>(content) ?? fallback;
            return mapped;
        }

        private static OperationSetting? operationSetting;
        private static OperationSetting OperationMode => operationSetting ??= GetSetting();
        private static readonly List<RecentError> ErrorCollection = [];
        internal static readonly string[] action = ["sevice health: {0}", "is working: {1}", "has errors: {2}", "error count: {3}"];
    }
}