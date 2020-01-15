namespace Dfe.Spi.EntitySquasher.AcdfGen
{
    using CommandLine;
    using Dfe.Spi.EntitySquasher.AcdfGen.Definitions;
    using Dfe.Spi.EntitySquasher.AcdfGen.Models;
    using StructureMap;

    /// <summary>
    /// Main entry class for the console app.
    /// </summary>
    public class Program : IProgram
    {
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
            throw new System.NotImplementedException();
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