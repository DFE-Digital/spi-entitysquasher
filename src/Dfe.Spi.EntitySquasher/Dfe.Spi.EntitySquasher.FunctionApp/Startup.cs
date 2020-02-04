namespace Dfe.Spi.EntitySquasher.FunctionApp
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.Common.Context.Definitions;
    using Dfe.Spi.Common.Http.Server;
    using Dfe.Spi.Common.Http.Server.Definitions;
    using Dfe.Spi.Common.Logging;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application;
    using Dfe.Spi.EntitySquasher.Application.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Factories;
    using Dfe.Spi.EntitySquasher.Application.Processors;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.FunctionApp.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Infrastructure.AzureStorage;
    using Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter.Factories;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.WebJobs.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Functions startup class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        private const string SystemErrorIdentifier = "ESQ";

        /// <inheritdoc />
        public override void Configure(
            IFunctionsHostBuilder functionsHostBuilder)
        {
            if (functionsHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(functionsHostBuilder));
            }

            // camelCase, if you please.
            JsonConvert.DefaultSettings =
                () => new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

            IServiceCollection serviceCollection =
                functionsHostBuilder.Services;

            AddLogging(serviceCollection);
            AddSettingsProviders(serviceCollection);
            AddFactories(serviceCollection);
            AddCaches(serviceCollection);

            HttpErrorBodyResultProvider httpErrorBodyResultProvider =
                new HttpErrorBodyResultProvider(
                    SystemErrorIdentifier,
                    HttpErrorMessages.ResourceManager);

            HttpSpiExecutionContextManager httpSpiExecutionContextManager =
                new HttpSpiExecutionContextManager();

            serviceCollection
                .AddSingleton<ISpiExecutionContextManager>(httpSpiExecutionContextManager)
                .AddSingleton<IHttpSpiExecutionContextManager>(httpSpiExecutionContextManager)
                .AddSingleton<IHttpErrorBodyResultProvider>(httpErrorBodyResultProvider)
                .AddScoped<IResultSquasher, ResultSquasher>()
                .AddScoped<IEntityAdapterInvoker, EntityAdapterInvoker>()
                .AddScoped<IGetSquashedEntityProcessor, GetSquashedEntityProcessor>()
                .AddScoped<IAlgorithmConfigurationDeclarationFileStorageAdapter, AlgorithmConfigurationDeclarationFileStorageAdapter>();
        }

        private static void AddLogging(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddScoped<ILogger>(CreateILogger)
                .AddScoped<ILoggerWrapper, LoggerWrapper>();
        }

        private static void AddSettingsProviders(
            IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IGetSquashedEntityProcessorSettingsProvider, GetSquashedEntityProcessorSettingsProvider>()
                .AddSingleton<IAlgorithmConfigurationDeclarationFileStorageAdapterSettingsProvider, AlgorithmConfigurationDeclarationFileStorageAdapterSettingsProvider>();
        }

        private static void AddFactories(
            IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddScoped<IAlgorithmConfigurationDeclarationFileCacheManagerFactory, AlgorithmConfigurationDeclarationFileManagerFactory>()
                .AddScoped<IEntityAdapterClientCacheManagerFactory, EntityAdapterClientManagerFactory>()
                .AddScoped<IEntityAdapterClientFactory, EntityAdapterClientFactory>();
        }

        private static void AddCaches(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IAlgorithmConfigurationDeclarationFileCache, AlgorithmConfigurationDeclarationFileCache>()
                .AddSingleton<IEntityAdapterClientCache, EntityAdapterClientCache>();
        }

        private static ILogger CreateILogger(IServiceProvider serviceProvider)
        {
            ILogger toReturn = null;

            ILoggerFactory loggerFactory =
                serviceProvider.GetService<ILoggerFactory>();

            string categoryName = LogCategories.CreateFunctionUserCategory(
                nameof(Dfe.Spi.EntitySquasher));

            toReturn = loggerFactory.CreateLogger(categoryName);

            return toReturn;
        }
    }
}
