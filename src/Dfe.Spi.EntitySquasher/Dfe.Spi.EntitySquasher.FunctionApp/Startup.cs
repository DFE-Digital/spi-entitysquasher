namespace Dfe.Spi.EntitySquasher.FunctionApp
{
    using System;
    using Dfe.Spi.Common.Logging.Definitions.Factories;
    using Dfe.Spi.Common.Logging.Factories;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Application.Factories;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.WebJobs.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Functions startup class.
    /// </summary>
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

            functionsHostBuilder
                .Services
                .AddScoped<ILogger>(this.CreateILogger)
                .AddScoped<ILoggerWrapperFactory, LoggerWrapperFactory>()
                .AddSingleton<IGetSquashedEntityProcessorFactory, GetSquashedEntityProcessorFactory>();
        }

        private ILogger CreateILogger(IServiceProvider serviceProvider)
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
