using component;
using legallead.jdbc.interfaces;
using legallead.logging.interfaces;
using legallead.permissions.api.Model;
using legallead.reader.service.interfaces;
using legallead.reader.service.models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace legallead.reader.service.tests
{
    internal static class MockUtility
    {
        public static void Setup(this IServiceCollection services, IConfiguration? configuration = null)
        {
            var mDapperCommand = new Mock<IDapperCommand>();
            var mLoggingRepository = new Mock<ILoggingRepository>();
            var mUserSearchRepository = new Mock<IUserSearchRepository>();
            var mSearchQueueRepository = new Mock<ISearchQueueRepository>();
            var mBgComponentRepository = new Mock<IBgComponentRepository>();
            var mExcelGenerator = new Mock<IExcelGenerator>();
            var mLogContentRepository = new Mock<ILogContentRepository>();
            var mLogConfiguration = new Mock<ILogConfiguration>();
            var mLoggingService = new Mock<ILoggingService>();
            var mMainWindowService = new Mock<IMainWindowService>();
            var mIndexReader = new Mock<IIndexReader>();
            var mQueueFilter = new Mock<IQueueFilter>();
            var mWorkLogger = new Mock<ILogger<Worker>>();
            var mWorkingSvc = new Mock<IWorkingIndicator>();
            var mSearchGenerationService = new Mock<ISearchGenerationService>();
            var mSearchHelper = new Mock<ISearchGenerationHelper>();
            configuration ??= GetConfiguration();
            var setting = new BackgroundServiceSettings
            {
                Enabled = configuration.GetValue<bool>("BackgroundServices:Enabled"),
                Delay = configuration.GetValue<int>("BackgroundServices:Delay"),
                Interval = configuration.GetValue<int>("BackgroundServices:Interval")
            };
            services.AddSingleton<IBackgroundServiceSettings>(x => setting);

            services.AddSingleton(m => configuration);
            services.AddSingleton(m => mLoggingRepository);
            services.AddSingleton(m => mDapperCommand);
            services.AddSingleton(m => mUserSearchRepository);
            services.AddSingleton(m => mSearchQueueRepository);
            services.AddSingleton(m => mBgComponentRepository);
            services.AddSingleton(m => mExcelGenerator);
            services.AddSingleton(m => mLogContentRepository);
            services.AddSingleton(m => mLogConfiguration);
            services.AddSingleton(m => mLoggingService);
            services.AddSingleton(m => mMainWindowService);
            services.AddSingleton(m => mIndexReader);
            services.AddSingleton(m => mQueueFilter);
            services.AddSingleton(m => mWorkLogger);
            services.AddSingleton(m => mSearchGenerationService);
            services.AddSingleton(m => mSearchHelper);

            services.AddSingleton(m => mLoggingRepository.Object);
            services.AddSingleton(m => mDapperCommand.Object);
            services.AddSingleton(m => mUserSearchRepository.Object);
            services.AddSingleton(m => mSearchQueueRepository.Object);
            services.AddSingleton(m => mBgComponentRepository.Object);
            services.AddSingleton(m => mExcelGenerator.Object);
            services.AddSingleton(m => mLogContentRepository.Object);
            services.AddSingleton(m => mLogConfiguration.Object);
            services.AddSingleton(m => mLoggingService.Object);
            services.AddSingleton(m => mMainWindowService.Object);
            services.AddSingleton(m => mIndexReader.Object);
            services.AddSingleton(m => mQueueFilter.Object);
            services.AddSingleton(m => mWorkLogger.Object);
            services.AddSingleton(m => mSearchGenerationService.Object);
            services.AddSingleton(m => mWorkingSvc.Object);
            services.AddSingleton(m => mSearchHelper.Object);

        }
        public static UserSearchRequest? GetRequest(string county)
        {
            const string single = "'";
            var doubleQt = '"'.ToString();
            if (SampleRequests.Count == 0) { return null; }
            var request = SampleRequests.Find(x =>
                x.Target.Equals(county, StringComparison.OrdinalIgnoreCase));
            if (request == null) { return null; }
            var content = string.Join(Environment.NewLine, request.Payload);
            content = content.Replace(single, doubleQt);
            return TryJsConvert<UserSearchRequest>(content);
        }

        public static IConfiguration GetConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"DataEnvironment", "Test"},
                {"ShowAppWindow", "false"},
                {"Logging::LogLevel::Default", "Information"},
                {"Logging::LogLevel::Microsoft.Hosting.Lifetime", "Information"},
                {"BackgroundServices::Delay", "120" },
                {"BackgroundServices::Interval", "120" },
            };
            var dictionary = new List<KeyValuePair<string, string?>>();
            var keys = inMemorySettings.Keys.ToList();
            keys.ForEach(k =>
            {
                var item = inMemorySettings[k];
                var kvp = new KeyValuePair<string, string?>(k, item);
                dictionary.Add(kvp);
            });
            return new ConfigurationBuilder()
                .AddInMemoryCollection(dictionary)
                .Build();
        }

        private static List<MockRequest>? sampleRequests;
        private static List<MockRequest> SampleRequests => sampleRequests ??= GetSampleRequests();
        private static string RequestSample => requestSample ??= GetRequestSample();
        private static string? requestSample;
        private static string GetRequestSample()
        {
            if (!string.IsNullOrEmpty(requestSample)) return requestSample;
            var sample = Properties.Resources.requestsample_json;
            return sample;
        }
        private static List<MockRequest> GetSampleRequests()
        {
            var json = RequestSample;
            if (string.IsNullOrWhiteSpace(json)) return [];
            return TryJsConvert<List<MockRequest>>(json) ?? [];
        }

        private static T? TryJsConvert<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
