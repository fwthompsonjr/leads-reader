using legallead.jdbc.entities;

namespace legallead.reader.service.services
{
    internal class QueueNonFilter : IQueueFilter
    {
        public bool IsEnabled => true;

        public List<SearchQueueDto> Apply(List<SearchQueueDto> source)
        {
            return source;
        }
    }
}