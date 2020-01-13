namespace Dfe.Spi.EntitySquasher.Application
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Thrown upon finding problems with a particular
    /// <see cref="AlgorithmConfigurationDeclarationFile" />.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "This is internally used, and will not be serialised.")]
    public class InvalidAlgorithmConfigurationDeclarationFileException
        : Exception
    {
        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="InvalidAlgorithmConfigurationDeclarationFileException" />
        /// class.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public InvalidAlgorithmConfigurationDeclarationFileException(string message)
            : base(message)
        {
            // Nothing.
        }
    }
}