using legallead.permissions.api.Model;
using legallead.records.search.Classes;
using legallead.records.search.Models;

namespace legallead.reader.service.interfaces
{
    public interface IWebInteractiveWrapper
    {
        WebInteractive? GetInterative(UserSearchRequest bo);
        WebFetchResult? Fetch(WebInteractive web);
    }
}
