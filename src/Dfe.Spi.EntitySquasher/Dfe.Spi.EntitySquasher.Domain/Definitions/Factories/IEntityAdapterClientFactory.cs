namespace Dfe.Spi.EntitySquasher.Domain.Definitions.Factories
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Describes the operations of the <see cref="IEntityAdapterClient" />
    /// factory.
    /// </summary>
    public interface IEntityAdapterClientFactory
    {
        /// <summary>
        /// Creates an instance of type <see cref="IEntityAdapterClient" />.
        /// </summary>
        /// <param name="entityAdapterName">
        /// The name of the entity adapter.
        /// </param>
        /// <param name="baseUrl">
        /// The base URL of the entity adapter.
        /// </param>
        /// <param name="headers">
        /// A <see cref="Dictionary{String, String}" /> of headers to supply
        /// with each call to the entity adapter.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IEntityAdapterClient" />.
        /// </returns>
        IEntityAdapterClient Create(
            string entityAdapterName,
            Uri baseUrl,
            Dictionary<string, string> headers);

        IEntityAdapterClient Create(string entityAdapterName);
    }
}