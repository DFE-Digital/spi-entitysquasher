namespace Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.Models;
    using RestSharp;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClient" />.
    /// </summary>
    public class EntityAdapterClient : IEntityAdapterClient
    {
        private const string RelativeResourceUriFormat =
            "./{0}/{1}?fields={2}";

        private readonly ILoggerWrapper loggerWrapper;
        private readonly IRestClient restClient;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterClient" /> class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        /// <param name="restClient">
        /// An instance of type <see cref="IRestClient" />.
        /// </param>
        public EntityAdapterClient(
            ILoggerWrapper loggerWrapper,
            IRestClient restClient)
        {
            this.loggerWrapper = loggerWrapper;
            this.restClient = restClient;
        }

        /// <inheritdoc />
        public async Task<ModelsBase> GetEntityAsync(
            string entityName,
            string id,
            IEnumerable<string> fields)
        {
            ModelsBase toReturn = null;

            Uri resourceUri = this.CreateRelativeResourceUri(
                entityName,
                id,
                fields);

            RestRequest restRequest = new RestRequest(resourceUri, Method.GET);

            this.loggerWrapper.Debug($"Executing {restRequest}...");

            IRestResponse<ModelsBase> restResponse =
                await this.restClient.ExecuteTaskAsync<ModelsBase>(
                    restRequest)
                    .ConfigureAwait(false);

            if (restResponse.IsSuccessful)
            {
                this.loggerWrapper.Info(
                    $"Request executed with success: {restResponse}.");
            }
            else
            {
                // TODO: Wrap up errors in the response object.
            }

            toReturn = restResponse.Data;

            return toReturn;
        }

        private Uri CreateRelativeResourceUri(
            string entityName,
            string id,
            IEnumerable<string> fields)
        {
            Uri toReturn = null;

            entityName = entityName.PascalToKebabCase();

            this.loggerWrapper.Debug(
                $"{nameof(entityName)} converted: \"{entityName}\".");

            // The id can remain the same.
            // Build up the fields list, comma seperated.
            string fieldsList = string.Join(",", fields);

            string toReturnStr = string.Format(
                CultureInfo.InvariantCulture,
                RelativeResourceUriFormat,
                entityName,
                id,
                fieldsList);

            toReturn = new Uri(toReturnStr, UriKind.Relative);

            this.loggerWrapper.Info($"{nameof(toReturn)} = {toReturn}");

            return toReturn;
        }
    }
}