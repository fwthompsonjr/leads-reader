namespace legallead.reader.component
{
    public interface IBackgroundServiceSettings
    {
        bool Enabled { get; set; }
        int Delay { get; set; }
        int Interval { get; set; }
    }
}
