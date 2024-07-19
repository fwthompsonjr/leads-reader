using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace legallead.reader.service.services
{
    internal class IndexReader : IIndexReader
    {
        public IndexReader()
        {
            lock (sync)
            {
                Collection = GetIndexes();
                Watch();
            }
        }
        public string SearchLocation => ConfigurationFolder;
        public void Rebuild()
        {
            var arg = new FileSystemEventArgs(WatcherChangeTypes.Changed, SearchLocation, null);
            OnChanged(new(), arg);
        }
        public IEnumerable<string> Indexes => Collection;
        private IEnumerable<string> Collection { get; set; }

        private void Watch()
        {
            if (string.IsNullOrWhiteSpace(ConfigurationFolder) ||
                !Directory.Exists(ConfigurationFolder)) return;
            FileSystemWatcher watcher = new()
            {
                Path = ConfigurationFolder,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.txt"
            };
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            lock (sync)
            {
                Collection = GetIndexes();
            }
        }

        private static IEnumerable<string> GetIndexes()
        {
            if (string.IsNullOrEmpty(ConfigurationFolder)) return Enumerable.Empty<string>();
            var di = new DirectoryInfo(ConfigurationFolder);
            var files = di.GetFiles("*.json.txt")?.ToList();
            if (files == null || files.Count == 0) return Enumerable.Empty<string>();
            var collection = new List<string>();
            files.ForEach(f =>
            {
                var content = Get<List<string>>(File.ReadAllText(f.FullName));
                if (content != null)
                {
                    var items = content.Distinct().ToList();
                    items.ForEach(i =>
                    {
                        var ismapped = Guid.TryParse(i, out var _);
                        if (ismapped && !collection.Contains(i, StringComparer.OrdinalIgnoreCase)) collection.Add(i);
                    });
                }
            });
            return collection;
        }
        private static readonly object sync = new();
        private static string ConfigurationFolder => _configurationFolder ??= GetConfigurationFolderName();

        private static string? _configurationFolder;
        [ExcludeFromCodeCoverage]
        private static string GetConfigurationFolderName()
        {
            if (_configurationFolder != null)
            {
                return _configurationFolder;
            }

            string? execPath = new Uri(Assembly.GetExecutingAssembly().Location).AbsolutePath;
            execPath = Path.GetDirectoryName(execPath);
            if (!Directory.Exists(execPath))
            {
                _configurationFolder = string.Empty;
                return string.Empty;
            }
            var configPath = Path.Combine(execPath, "_configuration");
            if (!Directory.Exists(configPath))
            {
                _configurationFolder = string.Empty;
                return string.Empty;
            }
            _configurationFolder = configPath;
            return configPath;
        }
        [ExcludeFromCodeCoverage]
        private static T? Get<T>(string json)
        {
            try
            {
                var converted = JsonConvert.DeserializeObject<T>(json);
                return converted;
            }
            catch
            {
                return default;
            }
        }
    }
}
