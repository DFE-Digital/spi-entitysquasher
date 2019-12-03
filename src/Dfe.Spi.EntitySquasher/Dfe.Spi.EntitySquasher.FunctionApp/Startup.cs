namespace Dfe.Spi.EntitySquasher.FunctionApp
{
    using System;
    using Dfe.Spi.Common.Logging.Definitions.Factories;
    using Dfe.Spi.Common.Logging.Factories;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Application.Factories;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;

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
                .AddSingleton<ILoggerWrapperFactory, LoggerWrapperFactory>()
                .AddSingleton<IGetSquashedEntityProcessorFactory, GetSquashedEntityProcessorFactory>();
        }
    }
}
