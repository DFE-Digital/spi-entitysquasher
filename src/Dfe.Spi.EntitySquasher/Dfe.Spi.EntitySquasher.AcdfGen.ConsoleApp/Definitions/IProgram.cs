namespace Dfe.Spi.EntitySquasher.AcdfGen.ConsoleApp.Definitions
{
    using Dfe.Spi.EntitySquasher.AcdfGen.ConsoleApp.Models;

    /// <summary>
    /// Describes the operations of the main entry point class.
    /// </summary>
    public interface IProgram
    {
        /// <summary>
        /// The main, non/static entry method. Where dependency injection
        /// begins.
        /// </summary>
        /// <param name="options">
        /// An instance of <see cref="Options" />.
        /// </param>
        /// <returns>
        /// An exit code for the application process.
        /// </returns>
        int Run(Options options);
    }
}