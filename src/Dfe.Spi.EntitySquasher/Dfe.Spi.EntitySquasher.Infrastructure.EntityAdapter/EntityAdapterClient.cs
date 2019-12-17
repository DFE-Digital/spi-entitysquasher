namespace Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Http.Client;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.Common.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Newtonsoft.Json;
    using RestSharp;
    using ModelsBase = Dfe.Spi.Models.ModelsBase;

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
                toReturn = restResponse.Data;

                this.loggerWrapper.Info(
                    $"Request executed with success: {restResponse}.");
            }
            else
            {
                this.loggerWrapper.Warning(
                    $"A non-successful status code was returned " +
                    $"({restResponse.StatusCode}).");

                // Deserialise the data as the standard error model.
                string content = restResponse.Content;

                this.loggerWrapper.Debug($"content = \"{content}\"");
                this.loggerWrapper.Debug(
                    $"Attempting to de-serialise the body (\"{content}\") " +
                    $"as a {nameof(HttpErrorBody)} instance...");

                // TODO: Cater for null/dodgy bodies.
                HttpErrorBody httpErrorBody =
                    JsonConvert.DeserializeObject<HttpErrorBody>(content);

                this.loggerWrapper.Warning(
                    $"{nameof(httpErrorBody)} = {httpErrorBody}");

                // Throw exception.
                throw new SpiWebServiceException(httpErrorBody);
            }

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