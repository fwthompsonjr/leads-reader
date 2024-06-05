namespace legallead.reader.component.models
{
    public class BackgroundServiceSettings : IBackgroundServiceSettings
    {
        public bool Enabled { get; set; } = true;
        public int Delay { get; set; } = 45;
        public int Interval { get; set; } = 3;
    }
}
