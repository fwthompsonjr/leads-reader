using legallead.jdbc.entities;

namespace legallead.reader.service
{
    internal interface IQueueFilter
    {
        bool IsEnabled { get; }

        List<SearchQueueDto> Apply(List<SearchQueueDto> source);
    }
}
