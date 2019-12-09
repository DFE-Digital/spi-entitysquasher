namespace Dfe.Spi.EntitySquasher.FunctionApp.SettingsProviders
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;

    /// <summary>
    /// Implements <see cref="IGetSquashedEntityProcessorSettingsProvider" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetSquashedEntityProcessorSettingsProvider
        : IGetSquashedEntityProcessorSettingsProvider
    {
        /// <inheritdoc />
        public string DefaultAlgorithm
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    nameof(this.DefaultAlgorithm));

                return toReturn;
            }
        }
    }
}