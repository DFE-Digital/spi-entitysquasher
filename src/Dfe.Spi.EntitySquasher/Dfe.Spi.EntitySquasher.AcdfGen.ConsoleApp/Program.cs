namespace Dfe.Spi.EntitySquasher.AcdfGen
{
    using CommandLine;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Definitions.Processors;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Models;
    using Dfe.Spi.EntitySquasher.AcdfGen.Definitions;
    using Dfe.Spi.EntitySquasher.AcdfGen.Models;
    using StructureMap;

    /// <summary>
    /// Main entry class for the console app.
    /// </summary>
    public class Program : IProgram
    {
        private readonly IGenerateAlgorithmConfigurationDeclarationFileProcessor generateAlgorithmConfigurationDeclarationFileProcessor;

        /// <summary>
        /// Initialises a new instance of the <see cref="Program" /> class.
        /// </summary>
        /// <param name="generateAlgorithmConfigurationDeclarationFileProcessor">
        /// An instance of type
        /// <see cref="IGenerateAlgorithmConfigurationDeclarationFileProcessor" />.
        /// </param>
        public Program(IGenerateAlgorithmConfigurationDeclarationFileProcessor generateAlgorithmConfigurationDeclarationFileProcessor)
        {
            this.generateAlgorithmConfigurationDeclarationFileProcessor =
                generateAlgorithmConfigurationDeclarationFileProcessor;
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

            GenerateAlgorithmConfigurationDeclarationFileRequest generateAlgorithmConfigurationDeclarationFileRequest =
                new GenerateAlgorithmConfigurationDeclarationFileRequest()
                {
                    // Nothing, yet.
                };

            GenerateAlgorithmConfigurationDeclarationFileResponse generateAlgorithmConfigurationDeclarationFileResponse =
                this.generateAlgorithmConfigurationDeclarationFileProcessor.GenerateAlgorithmConfigurationDeclarationFile(
                    generateAlgorithmConfigurationDeclarationFileRequest);

            // If everything passes here, without exception, call it a
            // success.
            toReturn = 0;

            return toReturn;
        }

        private static int InvokeRun(Options options)
        {
            int toReturn = -1;

            Registry registry = new Registry();
            using (Container container = new Container(registry))
            {
                IProgram program = container.GetInstance<IProgram>();

                toReturn = program.Run(options);
            }

            return toReturn;
        }
    }
}