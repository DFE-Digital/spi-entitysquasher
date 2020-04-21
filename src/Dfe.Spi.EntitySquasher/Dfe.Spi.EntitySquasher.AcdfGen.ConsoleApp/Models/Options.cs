namespace Dfe.Spi.EntitySquasher.AcdfGen.ConsoleApp.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// The options class, as used by the <see cref="CommandLine" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Options
    {
        /// <summary>
        /// Gets or sets the destination filename for the generated file.
        /// </summary>
        [Option(
            "filename",
            Required = true,
            HelpText = "The destination filename for the generated file.")]
        public string Filename
        {
            get;
            set;
        }
    }
}