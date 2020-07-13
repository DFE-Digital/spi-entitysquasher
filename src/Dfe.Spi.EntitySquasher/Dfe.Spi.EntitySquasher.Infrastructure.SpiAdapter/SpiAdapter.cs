using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Context.Definitions;
using Dfe.Spi.Common.Http;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.Models;
using Dfe.Spi.EntitySquasher.Domain.Adapters;
using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;
using Dfe.Spi.Models.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Dfe.Spi.EntitySquasher.Infrastructure.SpiAdapter
{
    public abstract class SpiAdapter : IDataAdapter
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };
        
        private readonly string _baseUrl;
        private readonly string _subscriptionKey;
        private readonly ISpiExecutionContextManager _executionContextManager;
        private readonly ILoggerWrapper _logger;

        protected SpiAdapter(
            string baseUrl,
            string subscriptionKey,
            ISpiExecutionContextManager executionContextManager,
            ILoggerWrapper logger)
        {
            _baseUrl = baseUrl;
            _subscriptionKey = subscriptionKey;
            _executionContextManager = executionContextManager;
            _logger = logger;
        }

        public abstract string SourceName { get; }

        protected async Task<DataAdapterResult<T>[]> GetEntitiesFromApi<T>(string[] identifiers, Dictionary<string, AggregateQuery> aggregateQueries, string[] fields, bool live, CancellationToken cancellationToken)
        {
            var bearerToken = _executionContextManager.SpiExecutionContext.IdentityToken;
            var entityType = GetPluralUrlEntityName<T>();

            if (string.IsNullOrEmpty(entityType))
            {
                throw new Exception($"Unsupported entity type {typeof(T)}");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl, UriKind.Absolute);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
                client.DefaultRequestHeaders.Add(SpiHeaderNames.InternalRequestIdHeaderName,
                    _executionContextManager.SpiExecutionContext.InternalRequestId.ToString());
                if (!string.IsNullOrEmpty(_executionContextManager.SpiExecutionContext.ExternalRequestId))
                {
                    client.DefaultRequestHeaders.Add(SpiHeaderNames.ExternalRequestIdHeaderName,
                        _executionContextManager.SpiExecutionContext.ExternalRequestId);
                }

                var body = JsonConvert.SerializeObject(
                    new SpiAdapterBatchRequest
                    {
                        Identifiers = identifiers,
                        AggregateQueries = aggregateQueries,
                        Fields = fields,
                        Live = live,
                    }, SerializerSettings);
                var response = await client.PostAsync(
                    entityType,
                    new StringContent(body, Encoding.UTF8, "application/json"),
                    cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    HttpErrorBody errorBody = null;
                    try
                    {
                        var errorJson = await response.Content.ReadAsStringAsync();
                        errorBody = JsonConvert.DeserializeObject<HttpErrorBody>(errorJson);
                    }
                    catch (Exception ex)
                    {
                        var fullUrl = new Uri(new Uri(_baseUrl, UriKind.Absolute), new Uri(entityType, UriKind.Relative));
                        _logger.Warning($"Error reading standard error response from {(int)response.StatusCode} response from {fullUrl}: {ex.Message}");
                    }
                    
                    throw new DataAdapterException($"Error calling {SourceName} adapter")
                    {
                        AdapterName = SourceName,
                        RequestedEntityName = entityType,
                        RequestedIds = identifiers,
                        RequestedFields = fields,
                        HttpStatusCode = response.StatusCode,
                        HttpErrorBody = errorBody,
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                var entities = JsonConvert.DeserializeObject<T[]>(json);
            
                var results = new DataAdapterResult<T>[identifiers.Length];
                for (var i = 0; i < identifiers.Length; i++)
                {
                    results[i] = new DataAdapterResult<T>
                    {
                        Identifier = identifiers[i],
                        Entity = entities[i],
                    };
                }

                return results;
            }
        }

        protected string GetPluralUrlEntityName<T>()
        {
            if (typeof(T) == typeof(LearningProvider))
            {
                return "learning-providers";
            }

            return null;
        }
    }
}