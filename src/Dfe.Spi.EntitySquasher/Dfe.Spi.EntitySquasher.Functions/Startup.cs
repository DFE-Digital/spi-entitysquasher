using System.IO;
using Dfe.Spi.Common.Context.Definitions;
using Dfe.Spi.Common.Http.Server;
using Dfe.Spi.Common.Http.Server.Definitions;
using Dfe.Spi.Common.Logging;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.EntitySquasher.Application.Squash;
using Dfe.Spi.EntitySquasher.Domain.Adapters;
using Dfe.Spi.EntitySquasher.Domain.Configuration;
using Dfe.Spi.EntitySquasher.Domain.Profiles;
using Dfe.Spi.EntitySquasher.Infrastructure.AzureStorage.Profiles;
using Dfe.Spi.EntitySquasher.Infrastructure.SpiAdapter;
using Dfe.Spi.Models.Entities;
using Dfe.Spi.Registry.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Dfe.Spi.Registry.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var rawConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables(prefix: "SPI_")
                .Build();

            Configure(builder, rawConfiguration);
        }

        public void Configure(IFunctionsHostBuilder builder, IConfigurationRoot rawConfiguration)
        {
            
            
            var services = builder.Services;

            AddConfiguration(services, rawConfiguration);
            AddLogging(services);
            AddAdapters(services);
            AddSquashing(services);
        }

        private void AddConfiguration(IServiceCollection services, IConfigurationRoot rawConfiguration)
        {
            services.AddSingleton(rawConfiguration);

            var configuration = new EntitySquasherConfiguration();
            rawConfiguration.Bind(configuration);
            services.AddSingleton(configuration);
        }

        private void AddLogging(IServiceCollection services)
        {
            services.AddLogging();
            services.AddScoped<ILogger>(provider =>
                provider.GetService<ILoggerFactory>().CreateLogger(LogCategories.CreateFunctionUserCategory("EntitySquasher")));

            services.AddScoped<IHttpSpiExecutionContextManager, HttpSpiExecutionContextManager>();
            services.AddScoped<ISpiExecutionContextManager>((provider) =>
                (ISpiExecutionContextManager) provider.GetService(typeof(IHttpSpiExecutionContextManager)));
            services.AddScoped<ILoggerWrapper, LoggerWrapper>();
        }

        private void AddAdapters(IServiceCollection services)
        {
            services
                .AddScoped<IDataAdapter<LearningProvider>, GiasDataAdapter>()
                .AddScoped<IDataAdapter<ManagementGroup>, GiasDataAdapter>()
                .AddScoped<IDataAdapter<LearningProvider>, UkrlpDataAdapter>();
        }

        private void AddSquashing(IServiceCollection services)
        {
            services
                .AddScoped<IProfileRepository, BlobProfileRepository>()
                .AddScoped(typeof(ITypedSquasher<>), typeof(TypedSquasher<>))
                .AddScoped<ISquashManager, SquashManager>();
        }
    }
}