namespace legallead.reader.service.models
{
    internal class OperationSetting
    {
        public string Mode { get; set; } = string.Empty;
        public bool Headless { get; set; } = true;
        public bool IsServer
        {
            get
            {
                if (string.IsNullOrEmpty(Mode)) { return false; }
                return Mode.Equals("service", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
