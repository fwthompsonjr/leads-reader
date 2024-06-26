namespace legallead.reader.service
{
    public interface IIndexReader
    {
        string SearchLocation { get; }
        IEnumerable<string> Indexes { get; }

        void Rebuild();
    }
}
