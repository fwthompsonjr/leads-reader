using legallead.jdbc.entities;
using legallead.jdbc.interfaces;
using legallead.permissions.api.Model;
using legallead.reader.component;
using legallead.records.search.Classes;
using legallead.records.search.Models;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace component
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
        "VSTHRD110:Observe result of async calls", Justification = "This is a fire and forget call. No observer needed.")]
    internal class SearchGenerationService : BaseTimedSvc<SearchGenerationService>, ISearchGenerationService
    {
        private const string ns = "legallead.reader.component";
        private const string clsname = "search.generation.service";

        private readonly IExcelGenerator generator;
        public SearchGenerationService(
            ILoggingRepository? logger,
            ISearchQueueRepository? repo,
            IBgComponentRepository? component,
            IBackgroundServiceSettings? settings,
            IExcelGenerator excel) : base(logger, repo, component, settings)
        {
            generator = excel;
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
                    var queue = GetQueue().Result;
                    if (queue.Count == 0) { return; }
                    var message = $"Found ( {queue.Count} ) records to process.";
                    DataService.Echo(message);
                    queue.ForEach(Generate);
                }
            }
            catch (Exception ex)
            {
                _ = _logger?.LogError(ex, ns, clsname).ConfigureAwait(false);
                AppendError(ex.Message);
            }
            finally
            {
                IsWorking = false;
            }
        }

        protected void Generate(SearchQueueDto dto)
        {
            try
            {
                if (DataService == null || _queueDb == null) return;
                var uniqueId = dto.Id;
                var payload = dto.Payload;
                _ = _queueDb.Start(dto).ConfigureAwait(false);
                PostStatus(uniqueId, 0, 0);
                var bo = TryConvert<UserSearchRequest>(payload);
                if (bo == null)
                {
                    PostStatus(uniqueId, 0, 2);
                    _ = _queueDb.Complete(uniqueId);
                    return;
                }
                PostStatus(uniqueId, bo);
                PostStatus(uniqueId, 0, 1);
                PostStatus(uniqueId, 1, 0);
                var interaction = WebMapper.MapFrom<UserSearchRequest, WebInteractive>(bo);
                if (interaction == null)
                {
                    PostStatus(uniqueId, 1, 2);
                    _ = _queueDb.Complete(uniqueId);
                    return;
                }
                interaction.UniqueId = uniqueId;
                PostStatus(uniqueId, 1, 1);
                PostStatus(uniqueId, 2, 0);
                var response = Fetch(interaction);
                if (response == null)
                {
                    PostStatus(uniqueId, 2, 2);
                    _ = _queueDb.Complete(uniqueId);
                    return;
                }
                PostStatus(uniqueId, 2, 1);
                PostStatus(uniqueId, 3, 0);
                if (response.WebsiteId == 0) response.WebsiteId = 1;
                var addresses = GetAddresses(response);
                if (addresses == null)
                {
                    PostStatus(uniqueId, 3, 2);
                    _ = _queueDb.Complete(uniqueId);
                    return;
                }
                PostStatus(uniqueId, 3, 1);
                PostStatus(uniqueId, 4, 0);
                var serialized = SerializeResult(uniqueId, addresses, _queueDb);
                if (!serialized)
                {
                    PostStatus(uniqueId, 4, 2);
                    _ = _queueDb.Complete(uniqueId);
                    return;
                }
                PostStatus(uniqueId, 4, 1);
                _ = _queueDb.Complete(uniqueId);
            }
            catch (Exception ex)
            {
                _ = _logger?.LogError(ex, ns, clsname).ConfigureAwait(false);
                AppendError(ex.Message);
            }
        }

        private WebFetchResult? Fetch(WebInteractive web)
        {
            try
            {
                return web.Fetch();
            }
            catch (Exception ex)
            {
                _ = _logger?.LogError(ex).ConfigureAwait(false);
                AppendError(ex.Message);
                return null;
            }
        }

        private ExcelPackage? GetAddresses(WebFetchResult fetchResult)
        {
            if (_logger == null) return null;
            return generator.GetAddresses(fetchResult, _logger);
        }

        private bool SerializeResult(string uniqueId, ExcelPackage package, ISearchQueueRepository repo)
        {
            if (_logger == null) return false;
            return generator.SerializeResult(uniqueId, package, repo, _logger);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods",
            Justification = "<Pending>")]
        private async Task<List<SearchQueueDto>> GetQueue()
        {
            if (_queueDb == null) return [];
            var items = await _queueDb.GetQueue();
            return items;
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


        private void PostStatus(string uniqueId, UserSearchRequest request)
        {
            if (_queueDb == null) return;
            var starting = DateTimeOffset.FromUnixTimeMilliseconds(request.StartDate).DateTime;
            var ending = DateTimeOffset.FromUnixTimeMilliseconds(request.EndDate).DateTime;
            var message = $"{_typeName}: search begin State: {request.State}, County: {request.County.Name}, Start: {starting:d}, Ending: {ending:d}";
            _ = _queueDb.Status(uniqueId, message).ConfigureAwait(false);
        }
        private void PostStatus(string uniqueId, int messageId, int statusId)
        {
            if (_queueDb == null) return;
            var indexes = new[] { 0, 1, 2 };
            var statuses = sourceArray.ToList();
            var messageState = indexes.Contains(statusId) ? statuses[statusId] : "-";
            var messages = new[]
            {
                $"{_typeName}: parameter evaluation: {messageState}",
                $"{_typeName}: parameter conversion to search request: {messageState}",
                $"{_typeName}: search request processing: {messageState}",
                $"{_typeName}: excel content conversion: {messageState}",
                $"{_typeName}: excel content serialization: {messageState}",
            };
            if (messageId < 0 || messageId > messages.Length - 1) { return; }
            var state = indexes.Contains(statusId) ? statuses[statusId] : "status";
            var message = string.Format(messages[messageId], state);
            _ = _queueDb.Status(uniqueId, message).ConfigureAwait(false);
        }

        private class RecentError
        {
            public DateTime CreateDate { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        private static void AppendError(string message)
        {
            var addition = new RecentError { Message = message, CreateDate = DateTime.UtcNow };
            ErrorCollection.Add(addition);
            var tolerance = DateTime.UtcNow.AddMinutes(-10);
            ErrorCollection.RemoveAll(x => x.CreateDate < tolerance);
        }


        private static readonly List<RecentError> ErrorCollection = [];
        internal static readonly string[] action = ["sevice health: {0}", "is working: {1}", "has errors: {2}", "error count: {3}"];
        internal static readonly string[] sourceArray = ["begin", "complete", "failed"];
    }
}
