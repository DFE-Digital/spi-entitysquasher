namespace Dfe.Spi.EntitySquasher.AcdfGen.Models
{
    using System.Collections.Generic;
    using CommandLine;

    /// <summary>
    /// The options class, as used by the <see cref="CommandLine" />.
    /// </summary>
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
    }
}