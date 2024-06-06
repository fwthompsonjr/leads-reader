namespace legallead.reader.service
{
    public interface IBackgroundServiceSettings
    {
        bool Enabled { get; set; }
        int Delay { get; set; }
        int Interval { get; set; }
    }
}
