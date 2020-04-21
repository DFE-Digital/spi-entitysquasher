using System.Threading;
using Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter.Models;
using Newtonsoft.Json.Linq;

namespace Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Context.Definitions;
    using Dfe.Spi.Common.Context.Models;
    using Dfe.Spi.Common.Extensions;
    using Dfe.Spi.Common.Http.Client;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.Common.Models;
    using Dfe.Spi.EntitySquasher.Domain;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models;
    using Dfe.Spi.Models.Entities;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClient" />.
    /// </summary>
    public class EntityAdapterClient : IEntityAdapterClient
    {
        private const string RelativeResourceUriFormat =
            "./{0}/{1}{2}";

        private const string RelativeResourceQueryStringFormat =
            "?fields={0}";

        private readonly ILoggerWrapper loggerWrapper;
        private readonly IRestClient restClient;
        private readonly ISpiExecutionContextManager spiExecutionContextManager;

        private readonly string entityAdapterName;
        private readonly JsonSerializerSettings jsonSerializerSettings;

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
        /// <param name="spiExecutionContextManager">
        /// An instance of type <see cref="ISpiExecutionContextManager" />.
        /// </param>
        /// <param name="entityAdapterName">
        /// The name of the entity adapter.
        /// </param>
        public EntityAdapterClient(
            ILoggerWrapper loggerWrapper,
            IRestClient restClient,
            ISpiExecutionContextManager spiExecutionContextManager,
            string entityAdapterName)
        {
            this.loggerWrapper = loggerWrapper;
            this.restClient = restClient;
            this.spiExecutionContextManager = spiExecutionContextManager;

            this.entityAdapterName = entityAdapterName;

            this.jsonSerializerSettings = JsonConvert.DefaultSettings();
        }

        /// <inheritdoc />
        public async Task<EntityBase> GetEntityAsync(
            string entityName,
            string id,
            IEnumerable<string> fields,
            AggregatesRequest aggregatesRequest)
        {
            EntityBase toReturn = null;

            Uri resourceUri = this.CreateRelativeResourceUri(
                entityName,
                id,
                fields);

            bool includeAggregatesRequest = aggregatesRequest != null;

            Method method =
                !includeAggregatesRequest ? Method.GET : Method.POST;

            RestRequest restRequest = new RestRequest(resourceUri, method);

            if (includeAggregatesRequest)
            {
                restRequest.AddHeader("Content-Type", "application/json");

                string body = JsonConvert.SerializeObject(
                    aggregatesRequest,
                    this.jsonSerializerSettings);

                Parameter parameter = new Parameter(
                    nameof(body),
                    body,
                    ParameterType.RequestBody);

                restRequest.AddParameter(parameter);
            }

            SpiExecutionContext spiExecutionContext =
                this.spiExecutionContextManager.SpiExecutionContext;

            restRequest.AppendContext(spiExecutionContext);

            restRequest.AddHeader("Ocp-Apim-Trace", "true");

            this.loggerWrapper.Debug($"Executing {restRequest}...");

            IRestResponse restResponse =
                await this.restClient.ExecuteTaskAsync(
                    restRequest)
                    .ConfigureAwait(false);

            string[] headersAndValues = restResponse.Request
                .Parameters
                .Where(x => x.Type == ParameterType.HttpHeader)
                .Select(x => $"{x.Name} = \"{x.Value}\"")
                .ToArray();

            string headersAndValuesDesc = string.Join(", ", headersAndValues);

            this.loggerWrapper.Debug(
                $"Response URI: {restResponse.ResponseUri}.");

            this.loggerWrapper.Debug(
                $"Request headers sent were: {headersAndValuesDesc}.");

            this.loggerWrapper.Debug(
                $"Response code: {restResponse.StatusCode}.");

            headersAndValues = restResponse.Headers
                .Select(x => $"{x.Name} = \"{x.Value}\"")
                .ToArray();

            headersAndValuesDesc = string.Join(", ", headersAndValues);

            this.loggerWrapper.Debug(
                $"Response headers are: {headersAndValuesDesc}.");

            if (restResponse.ErrorException != null)
            {
                this.loggerWrapper.Warning(
                    $"The {nameof(restResponse.ErrorException)} property " +
                    $"was not null.",
                    restResponse.ErrorException);
            }

            if (restResponse.IsSuccessful)
            {
                string content = restResponse.Content;

                Type type = this.GetActualUnboxingType(entityName);

                toReturn =
                    JsonConvert.DeserializeObject(content, type) as EntityBase;

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

                HttpErrorBody httpErrorBody = null;
                try
                {
                    httpErrorBody =
                        JsonConvert.DeserializeObject<HttpErrorBody>(content);

                    this.loggerWrapper.Warning(
                        $"{nameof(httpErrorBody)} = {httpErrorBody}");
                }
                catch (JsonException jsonException)
                {
                    this.loggerWrapper.Warning(
                        $"Could not de-serialise error body to an instance " +
                        $"of {nameof(HttpErrorBody)}.",
                        jsonException);
                }

                HttpStatusCode httpStatusCode = restResponse.StatusCode;

                // Throw exception.
                EntityAdapterErrorDetail entityAdapterErrorDetail =
                    new EntityAdapterErrorDetail()
                    {
                        AdapterName = this.entityAdapterName,
                        RequestedEntityName = entityName,
                        RequestedFields = fields,
                        RequestedId = id,
                        HttpStatusCode = httpStatusCode,
                        HttpErrorBody = httpErrorBody,
                    };
                throw new EntityAdapterException(
                    entityAdapterErrorDetail,
                    httpStatusCode,
                    httpErrorBody);
            }

            return toReturn;
        }

        public async Task<EntityBase[]> GetEntitiesAsync(
            string entityName, 
            string[] ids, 
            string[] fields, 
            AggregatesRequest aggregatesRequest,
            CancellationToken cancellationToken)
        {
            var request = new RestRequest(GetEntityNameForUri(entityName), Method.POST);
            request.AppendContext(this.spiExecutionContextManager.SpiExecutionContext);

            var batchRequest = JsonConvert.SerializeObject(
                new BatchGetEntitiesRequest
                {
                    Identifiers = ids,
                    Fields = fields,
                    AggregateQueries = aggregatesRequest.AggregateQueries,
                });
            
            request.AddParameter("", batchRequest, ParameterType.RequestBody);

            var response = await this.restClient.ExecuteTaskAsync(request, cancellationToken);
            if (!response.IsSuccessful)
            {
                HttpErrorBody httpErrorBody = null;
                try
                {
                    httpErrorBody =
                        JsonConvert.DeserializeObject<HttpErrorBody>(response.Content);

                    this.loggerWrapper.Warning(
                        $"{nameof(httpErrorBody)} = {httpErrorBody}");
                }
                catch (JsonException jsonException)
                {
                    this.loggerWrapper.Warning(
                        $"Could not de-serialise error body to an instance " +
                        $"of {nameof(HttpErrorBody)}.",
                        jsonException);
                }

                HttpStatusCode httpStatusCode = response.StatusCode;

                // Throw exception.
                EntityAdapterErrorDetail entityAdapterErrorDetail =
                    new EntityAdapterErrorDetail()
                    {
                        AdapterName = this.entityAdapterName,
                        RequestedEntityName = entityName,
                        RequestedFields = fields,
                        RequestedId = ids[0],
                        HttpStatusCode = httpStatusCode,
                        HttpErrorBody = httpErrorBody,
                    };
                throw new EntityAdapterException(
                    entityAdapterErrorDetail,
                    httpStatusCode,
                    httpErrorBody);
            }

            var deserializationType = GetActualUnboxingType(entityName);
            var results = JsonConvert.DeserializeObject(response.Content, deserializationType.MakeArrayType()) as EntityBase[];

            return results;
        }

        private Type GetActualUnboxingType(string entityName)
        {
            Type toReturn = null;

            // Use ModelsBase as a starting point...
            toReturn = typeof(EntityBase);

            string requiredConcreteType = toReturn.FullName;

            requiredConcreteType = requiredConcreteType.Replace(
                toReturn.Name,
                entityName);

            this.loggerWrapper.Debug(
                $"{nameof(requiredConcreteType)} = " +
                $"\"{requiredConcreteType}\"");

            Assembly assembly = toReturn.Assembly;

            // Get the actual type...
            toReturn = assembly.GetType(requiredConcreteType);

            this.loggerWrapper.Debug(
                $"Actual type to deserialise to: {toReturn.FullName}");

            return toReturn;
        }

        private Uri CreateRelativeResourceUri(
            string entityName,
            string id,
            IEnumerable<string> fields)
        {
            Uri toReturn = null;

            string pluraliser = null;

            // TODO: Need something configurable here. This is getting silly.
            // Don't forget to plural-ise.
            // Annoyingly, this depends on the type.
            if (entityName == nameof(Census))
            {
                pluraliser = "es";
            }
            else if (entityName == nameof(LearningProviderRates) || entityName == nameof(ManagementGroupRates))
            {
                // Do nothing - it's already correct.
            }
            else
            {
                pluraliser = "s";
            }

            entityName = entityName.PascalToKebabCase();

            entityName = $"{entityName}{pluraliser}";

            this.loggerWrapper.Debug(
                $"{nameof(entityName)} converted: \"{entityName}\".");

            string queryString = null;
            if (fields != null)
            {
                // The id can remain the same.
                // Build up the fields list, comma seperated.
                string fieldsList = string.Join(",", fields);

                queryString = string.Format(
                    CultureInfo.InvariantCulture,
                    RelativeResourceQueryStringFormat,
                    fieldsList);
            }

            string toReturnStr = string.Format(
                CultureInfo.InvariantCulture,
                RelativeResourceUriFormat,
                entityName,
                id,
                queryString);

            toReturn = new Uri(toReturnStr, UriKind.Relative);

            this.loggerWrapper.Info($"{nameof(toReturn)} = {toReturn}");

            return toReturn;
        }

        private string GetEntityNameForUri(string entityName)
        {
            string pluralEntityName;
            if (entityName == nameof(Census))
            {
                pluralEntityName = $"{entityName}es";
            }
            else if (entityName == nameof(LearningProviderRates) || entityName == nameof(ManagementGroupRates))
            {
                pluralEntityName = entityName;
            }
            else
            {
                pluralEntityName = $"{entityName}s";
            }

            return pluralEntityName.PascalToKebabCase();
        }
    }
}