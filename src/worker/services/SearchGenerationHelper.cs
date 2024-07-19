using legallead.jdbc;
using legallead.jdbc.entities;
using legallead.jdbc.interfaces;
using legallead.models.Search;
using legallead.permissions.api.Model;
using legallead.reader.service;
using legallead.reader.service.interfaces;
using legallead.reader.service.models;
using legallead.reader.service.services;
using legallead.records.search.Classes;
using legallead.records.search.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace component
{
    internal class SearchGenerationHelper(
        IExcelGenerator excel,
        IUserSearchRepository searchRepository,
        ISearchQueueRepository queueRepository,
        ISearchStatusRepository statusRepository,
        ILoggingRepository? logger,
        IWebInteractiveWrapper? wrapper = null) : ISearchGenerationHelper
    {
        private const string ns = "legallead.reader.service";
        private const string clsname = "search.generation.service";
        private const string _typeName = "search.processor";

        private readonly IExcelGenerator generator = excel;
        private readonly IUserSearchRepository repoSvc = searchRepository;
        private readonly ISearchQueueRepository? _queueDb = queueRepository;
        private readonly ISearchStatusRepository statusSvc = statusRepository;
        private readonly ILoggingRepository? _logger = logger;
        private readonly IWebInteractiveWrapper _wrapper = wrapper ?? new WebInteractiveWrapper();
        public void Enqueue(List<SearchQueueDto> collection)
        {
            if (collection.Count == 0) return;
            var bo = new WorkBeginningBo
            {
                WorkIndexes = collection.Select(x => new WorkIndexBo { Id = x.Id }).Distinct().ToList()
            };
            _ = statusSvc.Begin(bo);
        }

        public void Generate(SearchQueueDto dto)
        {
            if (_queueDb == null) return;
            var uniqueId = dto.Id;
            try
            {
                var payload = dto.Payload;
                _ = _queueDb.Start(dto).ConfigureAwait(false);
                PostStatus(uniqueId, MessageIndexes.BeginProcess, StatusIndexes.Begin);
                var bo = TryConvert<UserSearchRequest>(payload);
                if (bo == null)
                {
                    PostStatus(uniqueId, MessageIndexes.BeginProcess, StatusIndexes.Failed);
                    _ = _queueDb.Complete(uniqueId);
                    return;
                }
                PostStatus(uniqueId, bo);
                PostStatus(uniqueId, MessageIndexes.BeginProcess, StatusIndexes.Complete);
                PostStatus(uniqueId, MessageIndexes.ParameterEvaluation, StatusIndexes.Begin);
                var interaction = _wrapper.GetInterative(bo);
                var parameter = WebMapper.MapFrom<UserSearchRequest, SearchRequest>(bo);
                if (interaction == null)
                {
                    PostStatus(uniqueId, MessageIndexes.ParameterEvaluation, StatusIndexes.Failed);
                    _ = _queueDb.Complete(uniqueId);
                    return;
                }

                interaction.UniqueId = uniqueId;
                PostStatus(uniqueId, MessageIndexes.ParameterEvaluation, StatusIndexes.Complete);
                PostStatus(uniqueId, MessageIndexes.ParameterConversion, StatusIndexes.Complete);
                PostStatus(uniqueId, MessageIndexes.RequestProcessing, StatusIndexes.Begin);
                var response = Fetch(interaction);
                if (response == null)
                {
                    PostStatus(uniqueId, MessageIndexes.RequestProcessing, StatusIndexes.Failed);
                    _ = _queueDb.Complete(uniqueId);
                    return;
                }
                PostStatus(uniqueId, MessageIndexes.RequestProcessing, StatusIndexes.Complete);
                PostStatus(uniqueId, MessageIndexes.TranslateRecords, StatusIndexes.Begin);
                if (response.WebsiteId == 0) response.WebsiteId = parameter?.WebId ?? 1;

                var addresses = GetAddresses(response);
                if (addresses == null)
                {
                    PostStatus(uniqueId, MessageIndexes.TranslateRecords, StatusIndexes.Failed);
                    _ = _queueDb.Complete(uniqueId);
                    return;
                }
                PostStatus(uniqueId, MessageIndexes.TranslateRecords, StatusIndexes.Complete);
                PostStatus(uniqueId, MessageIndexes.SerializeRecords, StatusIndexes.Begin);
                var serialized = SerializeResult(uniqueId, addresses, _queueDb);
                if (!serialized)
                {
                    PostStatus(uniqueId, MessageIndexes.SerializeRecords, StatusIndexes.Failed);
                    _ = _queueDb.Complete(uniqueId);
                    return;
                }
                PostStatus(uniqueId, MessageIndexes.SerializeRecords, StatusIndexes.Complete);
                UpdateStatus(uniqueId, MessageIndexes.CompleteProcess, StatusIndexes.Begin);
                GenerationComplete(uniqueId, parameter, response.PeopleList);
                UpdateStatus(uniqueId, MessageIndexes.CompleteProcess, StatusIndexes.Complete);
            }
            catch (Exception ex)
            {
                _ = _logger?.LogError(ex, ns, clsname).ConfigureAwait(false);
                AppendError(ex.Message);
                UpdateStatus(uniqueId, MessageIndexes.CompleteProcess, StatusIndexes.Failed);
            }
        }

        public WebFetchResult? Fetch(WebInteractive web)
        {
            try
            {
                return _wrapper.Fetch(web);
            }
            catch (Exception ex)
            {
                _ = _logger?.LogError(ex).ConfigureAwait(false);
                AppendError(ex.Message);
                return null;
            }
        }

        public ExcelPackage? GetAddresses(WebFetchResult fetchResult)
        {
            if (_logger == null) return null;
            return generator.GetAddresses(fetchResult, _logger);
        }

        public bool SerializeResult(string uniqueId, ExcelPackage package, ISearchQueueRepository repo)
        {
            if (_logger == null) return false;
            return generator.SerializeResult(uniqueId, package, repo, _logger);
        }

        public async Task<List<SearchQueueDto>> GetQueueAsync()
        {
            if (_queueDb == null) return [];
            var items = await _queueDb.GetQueue();
            return items;
        }

        public void PostStatus(string uniqueId, UserSearchRequest request)
        {
            if (_queueDb == null) return;
            var starting = DateTimeOffset.FromUnixTimeMilliseconds(request.StartDate).DateTime;
            var ending = DateTimeOffset.FromUnixTimeMilliseconds(request.EndDate).DateTime;
            var message = $"{_typeName}: search begin State: {request.State}, County: {request.County.Name}, Start: {starting:d}, Ending: {ending:d}";
            _ = _queueDb.Status(uniqueId, message).ConfigureAwait(false);
            UpdateStatus(uniqueId);
        }
        public void PostStatus(string uniqueId, int messageId, int statusId)
        {
            if (_queueDb == null) return;
            var indexes = new[] { 0, 1, 2 };
            var statuses = sourceArray.ToList();
            var messageState = indexes.Contains(statusId) ? statuses[statusId] : "-";
            var messages = new[]
            {
                $"{_typeName}: process beginning: {messageState}", // 0
                $"{_typeName}: parameter evaluation: {messageState}", // 1
                $"{_typeName}: parameter conversion to search request: {messageState}", // 2
                $"{_typeName}: search request processing: {messageState}", // 3
                $"{_typeName}: excel content conversion: {messageState}", // 4
                $"{_typeName}: excel content serialization: {messageState}", // 5
                $"{_typeName}: process complete: {messageState}", // 6
            };
            if (messageId < 0 || messageId > messages.Length - 1) { return; }
            var state = indexes.Contains(statusId) ? statuses[statusId] : "status";
            var message = string.Format(messages[messageId], state);
            _ = _queueDb.Status(uniqueId, message).ConfigureAwait(false);
            if (indexes.Contains(statusId)) UpdateStatus(uniqueId, messageId, statusId);

        }

        public void AppendError(string message)
        {
            lock (_sync)
            {
                var addition = new RecentError { Message = message, CreateDate = DateTime.UtcNow };
                ErrorCollection.Add(addition);
                var tolerance = DateTime.UtcNow.AddMinutes(-10);
                ErrorCollection.RemoveAll(x => x.CreateDate < tolerance);
            }
        }


        public void GenerationComplete(string uniqueId, SearchRequest? parameter, List<PersonAddress> list)
        {
            var rcount = list.Count;
            if (parameter == null || rcount == 0 || parameter.WebId != 30)
            {
                _ = _queueDb?.Complete(uniqueId);
                return;
            }
            var js = JsonConvert.SerializeObject(list);
            _ = repoSvc.Append(SearchTargetTypes.Staging, uniqueId, js, "data-output-person-addres");
            _ = repoSvc.Append(SearchTargetTypes.Staging, uniqueId, rcount, "data-output-row-count");
            _ = _queueDb?.Complete(uniqueId);
            _ = repoSvc.UpdateRowCount(uniqueId, rcount);
        }

        private void UpdateStatus(string id, int messageId = 0, int statusId = 0)
        {
            try
            {
                var bo = new WorkStatusBo { Id = id, MessageId = messageId, StatusId = statusId };
                statusSvc.Update(bo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        [ExcludeFromCodeCoverage]
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
        internal static readonly string[] sourceArray = ["begin", "complete", "failed"];
        private static readonly List<RecentError> ErrorCollection = [];
        private static readonly object _sync = new();

        private static class MessageIndexes
        {
            public const int BeginProcess = 0;
            public const int ParameterEvaluation = 1;
            public const int ParameterConversion = 2;
            public const int RequestProcessing = 3;
            public const int TranslateRecords = 4;
            public const int SerializeRecords = 5;
            public const int CompleteProcess = 6;
        }
        private static class StatusIndexes
        {
            public const int Begin = 0;
            public const int Complete = 1;
            public const int Failed = 2;
        }
    }
}
