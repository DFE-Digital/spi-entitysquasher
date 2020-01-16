namespace Dfe.Spi.EntitySquasher.AcdfGen.Models
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
        /// Gets or sets a list of adapter names.
        /// </summary>
        [Option(
            "adapter-names",
            Required = true,
            HelpText = "A list of adapter names.")]
        public IEnumerable<string> AdapterNames
        {
            get;
            set;
        }

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