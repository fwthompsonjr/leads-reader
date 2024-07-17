using legallead.jdbc.interfaces;
using legallead.records.search;
using legallead.records.search.Classes;
using legallead.records.search.Models;
using OfficeOpenXml;

namespace legallead.reader.service.services
{
    internal class ExcelGenerator : IExcelGenerator
    {
        public ExcelPackage? GetAddresses(WebFetchResult fetchResult, ILoggingRepository logging)
        {
            try
            {
                string extXml = CommonKeyIndexes.ExtensionXml;
                string extFile = CommonKeyIndexes.ExtensionXlsx;
                string tmpFileName = fetchResult.Result.Replace(extXml, extFile);
                if (fetchResult.WebsiteId == 0) { fetchResult.WebsiteId = 1; }
                ExcelWriter writer = new();
                return writer.ConvertToPersonTable(
                addressList: fetchResult.PeopleList,
                worksheetName: "Addresses",
                saveFile: false,
                outputFileName: tmpFileName,
                websiteId: fetchResult.WebsiteId);
            }
            catch (Exception ex)
            {
                _ = logging.LogError(ex).ConfigureAwait(false);
                return null;
            }
        }

        public bool SerializeResult(string uniqueId, ExcelPackage package, ISearchQueueRepository repo, ILoggingRepository logging)
        {
            try
            {
                using var ms = new MemoryStream();
                package.SaveAs(ms);
                var content = ms.ToArray();
                _ = repo.Content(uniqueId, content).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _ = logging.LogError(ex).ConfigureAwait(false);
                return false;
            }
        }
    }
}
