namespace legallead.reader.service
{
    internal interface IIndexReader
    {
        string SearchLocation { get; }
        IEnumerable<string> Indexes { get; }
    }
}
