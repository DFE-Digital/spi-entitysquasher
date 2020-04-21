namespace Dfe.Spi.EntitySquasher.AcdfGen.ConsoleApp
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Definitions.Processors;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Models;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Processors;
    using Dfe.Spi.EntitySquasher.AcdfGen.ConsoleApp.Definitions;
    using Dfe.Spi.EntitySquasher.AcdfGen.ConsoleApp.Models;
    using Dfe.Spi.EntitySquasher.AcdfGen.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.AcdfGen.Infrastructure.IO;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Main entry class for the console app.
    /// </summary>
    public class Program : IProgram
    {
        private readonly ILoggerWrapper loggerWrapper;
        private readonly IGenerateAlgorithmConfigurationDeclarationFileProcessor generateAlgorithmConfigurationDeclarationFileProcessor;

        /// <summary>
        /// Initialises a new instance of the <see cref="Program" /> class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        /// <param name="generateAlgorithmConfigurationDeclarationFileProcessor">
        /// An instance of type
        /// <see cref="IGenerateAlgorithmConfigurationDeclarationFileProcessor" />.
        /// </param>
        public Program(
            ILoggerWrapper loggerWrapper,
            IGenerateAlgorithmConfigurationDeclarationFileProcessor generateAlgorithmConfigurationDeclarationFileProcessor)
        {
            this.loggerWrapper = loggerWrapper;
            this.generateAlgorithmConfigurationDeclarationFileProcessor = generateAlgorithmConfigurationDeclarationFileProcessor;
        }

        /// <summary>
        /// Main entry method for the console app.
        /// </summary>
        /// <param name="args">
        /// The command line arguments.
        /// </param>
        /// <returns>
        /// An exit code for the application process.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static int Main(string[] args)
        {
            int toReturn = -1;

            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(x =>
                {
                    toReturn = InvokeRun(x);
                });

            return toReturn;
        }

        /// <inheritdoc />
        public int Run(Options options)
        {
            int toReturn = -1;

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            GenerateAlgorithmConfigurationDeclarationFileRequest generateAlgorithmConfigurationDeclarationFileRequest =
                new GenerateAlgorithmConfigurationDeclarationFileRequest()
                {
                    Filename = options.Filename,
                };

            GenerateAlgorithmConfigurationDeclarationFileResponse generateAlgorithmConfigurationDeclarationFileResponse = null;
            try
            {
                this.loggerWrapper.Debug(
                    $"Invoking " +
                    $"{nameof(IGenerateAlgorithmConfigurationDeclarationFileProcessor)} " +
                    $"with " +
                    $"{generateAlgorithmConfigurationDeclarationFileRequest}...");

                generateAlgorithmConfigurationDeclarationFileResponse =
                    this.generateAlgorithmConfigurationDeclarationFileProcessor.GenerateAlgorithmConfigurationDeclarationFile(
                        generateAlgorithmConfigurationDeclarationFileRequest);

                // If everything passes here, without exception, call it a
                // success.
                toReturn = 0;

                this.loggerWrapper.Info(
                    $"The " +
                    $"{nameof(IGenerateAlgorithmConfigurationDeclarationFileProcessor)} " +
                    $"completed with success.");
            }
            catch (Exception exception)
            {
                this.loggerWrapper.Error(
                    $"The " +
                    $"{nameof(IGenerateAlgorithmConfigurationDeclarationFileProcessor)} " +
                    $"threw an unhandled exception! This will be re-thrown " +
                    $"to the run-time.",
                    exception);

                throw;
            }

            this.loggerWrapper.Info($"Returning exit code: {toReturn}.");

            return toReturn;
        }

        [ExcludeFromCodeCoverage]
        private static int InvokeRun(Options options)
        {
            int toReturn = -1;

            using (ServiceProvider serviceProvider = CreateServiceProvider())
            {
                IProgram program = serviceProvider.GetService<IProgram>();

                toReturn = program.Run(options);
            }

            return toReturn;
        }

        [ExcludeFromCodeCoverage]
        private static ServiceProvider CreateServiceProvider()
        {
            ServiceProvider toReturn = new ServiceCollection()
                .AddScoped<IGeneratedAlgorithmConfigurationDeclarationFileRepository, GeneratedAlgorithmConfigurationDeclarationFileRepository>()
                .AddScoped<IGenerateAlgorithmConfigurationDeclarationFileProcessor, GenerateAlgorithmConfigurationDeclarationFileProcessor>()
                .AddScoped<ILoggerWrapper, LoggerWrapper>()
                .AddScoped<IProgram, Program>()
                .BuildServiceProvider();

            return toReturn;
        }
    }
}