using legallead.jdbc.entities;
using legallead.jdbc.interfaces;
using legallead.permissions.api.Model;
using legallead.records.search.Classes;
using legallead.records.search.Models;
using OfficeOpenXml;

namespace legallead.reader.service
{
    public interface ISearchGenerationHelper
    {
        void Enqueue(List<SearchQueueDto> collection);
        void Generate(SearchQueueDto dto);
        WebFetchResult? Fetch(WebInteractive web);
        ExcelPackage? GetAddresses(WebFetchResult fetchResult);
        bool SerializeResult(string uniqueId, ExcelPackage package, ISearchQueueRepository repo);
        Task<List<SearchQueueDto>> GetQueueAsync();
        void PostStatus(string uniqueId, UserSearchRequest request);
        void PostStatus(string uniqueId, int messageId, int statusId);
        void AppendError(string message);
    }
}
