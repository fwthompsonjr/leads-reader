using component;
using legallead.jdbc.helpers;
using legallead.jdbc.implementations;
using legallead.jdbc.interfaces;
using legallead.logging;
using legallead.logging.helpers;
using legallead.logging.implementations;
using legallead.logging.interfaces;
using legallead.reader.service.interfaces;
using legallead.reader.service.models;
using legallead.reader.service.services;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace legallead.reader.service.utility
{
    public static class ServiceExtensions
    {
        public static void Initialize(this IServiceCollection services, IConfiguration? configuration = null)
        {
            string environ = GetConfigOrDefault(configuration, "DataEnvironment", "Test");
            var setting = new BackgroundServiceSettings
            {
                Enabled = configuration?.GetValue<bool>("BackgroundServices:Enabled") ?? true,
                Delay = configuration?.GetValue<int>("BackgroundServices:Delay") ?? 15,
                Interval = configuration?.GetValue<int>("BackgroundServices:Interval") ?? 2
            };
            services.AddSingleton<IBackgroundServiceSettings>(x => setting);
            services.AddSingleton<IDapperCommand, DapperExecutor>();
            services.AddSingleton(x =>
            {
                var command = x.GetRequiredService<IDapperCommand>();
                return new DataContext(command, null, environ, "app");
            });
            services.AddSingleton<IUserSearchRepository, UserSearchRepository>(x =>
            {
                var context = x.GetRequiredService<DataContext>();
                return new UserSearchRepository(context);
            });
            services.AddSingleton<ISearchQueueRepository, SearchQueueRepository>(x =>
            {
                var context = x.GetRequiredService<DataContext>();
                var service = x.GetRequiredService<IUserSearchRepository>();
                return new SearchQueueRepository(context, service);
            });
            services.AddSingleton<IBgComponentRepository, BgComponentRepository>(x =>
            {
                var context = x.GetRequiredService<DataContext>();
                return new BgComponentRepository(context);
            });
            services.AddSingleton<ISearchStatusRepository, SearchStatusRepository>(x =>
            {
                var context = x.GetRequiredService<DataContext>();
                return new SearchStatusRepository(context);
            });
            services.AddSingleton<IExcelGenerator, ExcelGenerator>();
            // logging
            services.AddSingleton<LoggingDbServiceProvider>();
            services.AddSingleton<ILoggingDbCommand, LoggingDbExecutor>();
            services.AddSingleton<ILoggingDbContext>(s =>
            {
                var command = s.GetRequiredService<ILoggingDbCommand>();
                return new LoggingDbContext(command, environ, "error");
            });
            // logging content repository
            services.AddSingleton<ILogContentRepository>(s =>
            {
                var context = s.GetRequiredService<ILoggingDbContext>();
                return new LogContentRepository(context);
            });
            // logging configuration
            services.AddSingleton(p =>
            {
                var logprovider = p.GetRequiredService<LoggingDbServiceProvider>().Provider;
                return logprovider.GetRequiredService<ILogConfiguration>();
            });
            // logging service
            services.AddSingleton<ILoggingService>(p =>
            {
                var guid = Guid.NewGuid();
                var repo = p.GetRequiredService<ILogContentRepository>();
                var cfg = p.GetRequiredService<ILogConfiguration>();
                return new LoggingService(guid, repo, cfg);
            });
            // logging repository
            services.AddSingleton<ILoggingRepository>(p =>
            {
                var lg = p.GetRequiredService<ILoggingService>();
                return new LoggingRepository(lg);
            });
            services.AddSingleton(s =>
            {
                var fallback = new OperationSetting();
                var content = Properties.Resources.operation_mode;
                if (string.IsNullOrEmpty(content)) return fallback;
                var mapped = TryJsConvert<OperationSetting>(content) ?? fallback;
                return mapped;
            });
            services.AddSingleton<IMainWindowService>(s =>
            {
                var ops = s.GetRequiredService<OperationSetting>();
                if (ops.IsServer) return new MainWindowVisibleService();
                var config = s.GetService<IConfiguration>();
                return new MainWindowService(config);
            });
            services.AddSingleton<IIndexReader, IndexReader>();
            services.AddSingleton<IQueueFilter>(s =>
            {
                var reader = s.GetRequiredService<IIndexReader>();
                var filter = new QueueFilter(reader);
                var mapped = s.GetRequiredService<OperationSetting>();
                if (mapped == null || !mapped.IsServer) return filter;
                return new QueueNonFilter();
            });
            services.AddSingleton(GetIndicator);

            services.AddSingleton<ISearchGenerationHelper>(x =>
            {
                var excel = x.GetRequiredService<IExcelGenerator>();
                var search = x.GetRequiredService<IUserSearchRepository>();
                var queue = x.GetRequiredService<ISearchQueueRepository>();
                var sts = x.GetRequiredService<ISearchStatusRepository>();
                var logger = x.GetService<ILoggingRepository>();
                return new SearchGenerationHelper(excel, search, queue, sts, logger);
            });

            services.AddSingleton(s =>
            {
                var logger = s.GetRequiredService<ILoggingRepository>();
                var search = s.GetRequiredService<ISearchQueueRepository>();
                var component = s.GetRequiredService<IBgComponentRepository>();
                var settings = s.GetRequiredService<IBackgroundServiceSettings>();
                var mn = s.GetRequiredService<IMainWindowService>();
                var qu = s.GetRequiredService<IQueueFilter>();
                var indc = s.GetRequiredService<IWorkingIndicator>();
                var helper = s.GetRequiredService<ISearchGenerationHelper>();
                return new SearchGenerationService(logger, search, component, settings, mn, qu, indc, helper);
            });
            services.AddSingleton<ISearchGenerationService>(p => p.GetRequiredService<SearchGenerationService>());
            services.AddSingleton(s => { return s; });
        }

        [ExcludeFromCodeCoverage]
        internal static string GetConfigOrDefault(IConfiguration? configuration, string key, string backup)
        {
            try
            {
                if (configuration == null) return backup;
                return configuration.GetValue<string>(key) ?? backup;
            }
            catch (Exception)
            {
                return backup;
            }
        }
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
        private static IWorkingIndicator GetIndicator(IServiceProvider s)
        {
            var ops = s.GetRequiredService<OperationSetting>();
            if (ops.IsServer) return new WorkingServiceIndicator();
            return new WorkingIndicator();
        }
    }
}
