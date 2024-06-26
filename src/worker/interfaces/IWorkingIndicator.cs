namespace legallead.reader.service.interfaces
{
    public interface IWorkingIndicator
    {
        string SearchLocation { get; }
        void Update(bool status);
    }
}
