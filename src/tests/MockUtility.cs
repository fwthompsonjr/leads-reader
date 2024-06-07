using legallead.jdbc.interfaces;
using legallead.logging.interfaces;
using legallead.reader.service.models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

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
            var setting = new BackgroundServiceSettings
            {
                Enabled = configuration?.GetValue<bool>("BackgroundServices:Enabled") ?? true,
                Delay = configuration?.GetValue<int>("BackgroundServices:Delay") ?? 45,
                Interval = configuration?.GetValue<int>("BackgroundServices:Interval") ?? 10
            };
            services.AddSingleton<IBackgroundServiceSettings>(x => setting);
            configuration ??= GetConfiguration();
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
    }
}
