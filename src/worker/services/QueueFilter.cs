using legallead.jdbc.entities;

namespace legallead.reader.service.services
{
    internal class QueueFilter(IIndexReader indexReader) : IQueueFilter
    {
        private readonly IIndexReader _indexReader = indexReader;

        public bool IsEnabled
        {
            get
            {
                var searchPath = _indexReader.SearchLocation;
                if (string.IsNullOrEmpty(searchPath) ||
                    !Directory.Exists(searchPath)) return false;
                _indexReader.Rebuild();
                var searches = _indexReader.Indexes.Count();
                return searches > 0;
            }
        }

        public List<SearchQueueDto> Apply(List<SearchQueueDto> source)
        {
            var response = new List<SearchQueueDto>();
            if (!IsEnabled || source.Count == 0) return response;
            var lookup = _indexReader.Indexes.ToList();
            var filtered = source.FindAll(s =>
            {
                var userId = s.UserId;
                if (userId == null) return false;
                return lookup.Exists(x => x.Equals(userId, StringComparison.OrdinalIgnoreCase));
            });
            response.AddRange(filtered);
            return response;
        }
    }
}
