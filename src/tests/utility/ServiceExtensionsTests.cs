using component;
using legallead.jdbc.helpers;
using legallead.jdbc.interfaces;
using legallead.logging;
using legallead.logging.interfaces;
using legallead.reader.service.interfaces;
using legallead.reader.service.models;
using legallead.reader.service.utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace legallead.reader.service.tests.utility
{
    public class ServiceExtensionsTests
    {

        [Theory]
        [InlineData(typeof(IBackgroundServiceSettings))]
        [InlineData(typeof(IDapperCommand))]
        [InlineData(typeof(DataContext))]
        [InlineData(typeof(IUserSearchRepository))]
        [InlineData(typeof(ISearchQueueRepository))]
        [InlineData(typeof(IBgComponentRepository))]
        [InlineData(typeof(ISearchStatusRepository))]
        [InlineData(typeof(IExcelGenerator))]
        [InlineData(typeof(LoggingDbServiceProvider))]
        [InlineData(typeof(ILoggingDbCommand))]
        [InlineData(typeof(ILoggingDbContext))]
        [InlineData(typeof(ILogContentRepository))]
        [InlineData(typeof(ILogConfiguration))]
        [InlineData(typeof(ILoggingService))]
        [InlineData(typeof(OperationSetting))]
        [InlineData(typeof(IIndexReader))]
        [InlineData(typeof(IQueueFilter))]
        [InlineData(typeof(IWorkingIndicator))]
        [InlineData(typeof(ISearchGenerationHelper))]
        [InlineData(typeof(SearchGenerationService))]
        [InlineData(typeof(ISearchGenerationService))]
        public void ProviderCanBuildType(Type type)
        {
            var exception = Record.Exception(() =>
            {
                _ = Provider.GetRequiredService(type);
            });
            Assert.Null(exception);
        }

        private static IServiceCollection Collection => services ??= GetServices();
        private static ServiceProvider Provider => provider ??= GetProvider();

        private static IServiceCollection? services;
        private static ServiceProvider? provider;
        private static ServiceCollection GetServices()
        {
            var collection = new ServiceCollection();
            collection.Initialize();
            return collection;
        }
        private static ServiceProvider GetProvider()
        {
            return Collection.BuildServiceProvider();
        }
    }
}
