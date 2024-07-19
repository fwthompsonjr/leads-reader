using legallead.permissions.api.Model;
using legallead.reader.service.interfaces;
using legallead.records.search.Classes;
using legallead.records.search.Models;

namespace legallead.reader.service.services
{
    internal class WebInteractiveWrapper : IWebInteractiveWrapper
    {
        public WebInteractive? GetInterative(UserSearchRequest bo)
        {
            return WebMapper.MapFrom<UserSearchRequest, WebInteractive>(bo);
        }
        public WebFetchResult? Fetch(WebInteractive web)
        {
            return web.Fetch();
        }
    }
}
