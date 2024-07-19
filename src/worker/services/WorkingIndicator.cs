using legallead.reader.service.interfaces;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace legallead.reader.service.services
{
    internal class WorkingIndicator : IWorkingIndicator
    {
        private readonly IndicationModel model = new();
        public WorkingIndicator()
        {
            if (string.IsNullOrEmpty(StatusFolder)) return;
            if (!string.IsNullOrEmpty(StatusFileName)) return;
            Update(false);
        }
        public string SearchLocation => StatusFolder;

        public void Update(bool status)
        {
            if (string.IsNullOrEmpty(StatusFolder)) return;
            lock (sync)
            {
                model.IsWorking = status;
                model.ShutdownOnComplete = GetShutdownProperty();
                Save();
            }
            if (!model.ShutdownOnComplete) { return; }
            Environment.Exit(0);
        }

        private void Save()
        {

            var serialized = JsonConvert.SerializeObject(model, Formatting.Indented);
            var targetFile = Path.Combine(StatusFolder, childFileName);
            lock (sync)
            {
                if (File.Exists(targetFile)) { File.Delete(targetFile); }
                File.WriteAllText(targetFile, serialized);
                _statusFile = targetFile;
            }
        }

        private static bool GetShutdownProperty()
        {
            if (string.IsNullOrEmpty(StatusFolder) || string.IsNullOrEmpty(StatusFileName)) return false;
            lock (sync)
            {
                var text = File.ReadAllText(StatusFileName);
                var current = JsonConvert.DeserializeObject<IndicationModel>(text);
                return current?.ShutdownOnComplete ?? false;
            }
        }

        private static readonly object sync = new();
        private static string? _statusFolder;
        private static string? _statusFile;

        private const string childFileName = "process-state.json";
        private static string StatusFolder => _statusFolder ??= GetStatusFolderName();
        private static string StatusFileName => _statusFile ??= GetStatusFile();


        [ExcludeFromCodeCoverage(Justification = "Private member tested from public accessor")]
        private static string GetStatusFolderName()
        {
            if (_statusFolder != null)
            {
                return _statusFolder;
            }

            string? execPath = new Uri(Assembly.GetExecutingAssembly().Location).AbsolutePath;
            execPath = Path.GetDirectoryName(execPath);
            if (!Directory.Exists(execPath))
            {
                _statusFolder = string.Empty;
                return string.Empty;
            }
            var configPath = Path.Combine(execPath, "_sys");
            if (!Directory.Exists(configPath))
            {
                _statusFolder = string.Empty;
                return string.Empty;
            }
            _statusFolder = configPath;
            return configPath;
        }

        [ExcludeFromCodeCoverage(Justification = "Private member tested from public accessor")]
        private static string GetStatusFile()
        {
            var parent = StatusFolder;
            if (!Directory.Exists(parent)) return string.Empty;
            var child = Path.Combine(parent, childFileName);
            if (!File.Exists(child)) return string.Empty;
            return child;
        }

        private sealed class IndicationModel
        {
            [JsonProperty("isWorking")]
            public bool IsWorking { get; set; }
            [JsonProperty("autoShutdown")]
            public bool ShutdownOnComplete { get; set; }
        }
    }
}
/*

        
        
*/