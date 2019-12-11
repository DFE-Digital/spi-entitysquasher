namespace Dfe.Spi.EntitySquasher.FunctionApp
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.Common.Logging;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.AzureStorage;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.FunctionApp.SettingsProviders;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.WebJobs.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Functions startup class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        /// <inheritdoc />
        public override void Configure(
            IFunctionsHostBuilder functionsHostBuilder)
        {
            if (functionsHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(functionsHostBuilder));
            }

            IServiceCollection serviceCollection =
                functionsHostBuilder.Services;

            AddLogging(serviceCollection);

            AddSettingsProviders(serviceCollection);

            serviceCollection
                .AddScoped<IGetSquashedEntityProcessor, GetSquashedEntityProcessor>()
                .AddScoped<IAlgorithmConfigurationDeclarationFileStorageAdapter, AlgorithmConfigurationDeclarationFileStorageAdapter>()
                .AddScoped<IAlgorithmConfigurationDeclarationFileManager, AlgorithmConfigurationDeclarationFileManager>()
                .AddSingleton<IAlgorithmConfigurationDeclarationFileCache, AlgorithmConfigurationDeclarationFileCache>();
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
