namespace Dfe.Spi.EntitySquasher.FunctionApp.SettingsProviders
{
    using System;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;

    /// <summary>
    /// Implements <see cref="IGetSquashedEntityProcessorSettingsProvider" />.
    /// </summary>
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