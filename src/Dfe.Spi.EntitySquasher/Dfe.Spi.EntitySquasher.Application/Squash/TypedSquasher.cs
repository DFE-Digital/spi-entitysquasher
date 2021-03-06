using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.EntitySquasher.Domain.Adapters;
using Dfe.Spi.EntitySquasher.Domain.Profiles;
using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;
using Dfe.Spi.Models;
using Dfe.Spi.Models.Entities;

namespace Dfe.Spi.EntitySquasher.Application.Squash
{
    public interface ITypedSquasher<T> where T : class, new()
    {
        Task<SquashedEntityResult[]> SquashAsync(
            EntityReference[] entityReferences,
            AggregatesRequest aggregatesRequest,
            string[] fields,
            bool live,
            DateTime? pointInTime,
            Profile profile,
            CancellationToken cancellationToken);
    }

    public class TypedSquasher<T> : ITypedSquasher<T>
        where T : class, new()
    {
        private readonly IDataAdapter<T>[] _adapters;
        private readonly ILoggerWrapper _logger;
        private readonly PropertyInfo[] _typeProperties;

        public TypedSquasher(
            IDataAdapter<T>[] adapters,
            ILoggerWrapper logger)
        {
            _adapters = adapters;
            _logger = logger;

            _typeProperties = typeof(T)
                .GetProperties()
                .Where(p => !p.Name.Equals("_lineage", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
        }

        public async Task<SquashedEntityResult[]> SquashAsync(
            EntityReference[] entityReferences,
            AggregatesRequest aggregatesRequest,
            string[] fields,
            bool live,
            DateTime? pointInTime,
            Profile profile,
            CancellationToken cancellationToken)
        {
            var entityProfile = profile.Entities.SingleOrDefault(x => x.Name.Equals(typeof(T).Name, StringComparison.InvariantCultureIgnoreCase));
            if (entityProfile == null)
            {
                throw new ProfileMisconfiguredException(profile.Name, typeof(T));
            }

            _logger.Info($"Getting {entityReferences.Length} references from adapters");
            var adapterResults = await GetEntitiesFromAdapters(entityReferences, aggregatesRequest, fields, live, pointInTime, cancellationToken);

            _logger.Info($"Collating {adapterResults.Count} adapter results");
            var candidates = CollateAdapterResults(entityReferences, adapterResults);

            var squashed = Squash(candidates, aggregatesRequest, fields, entityProfile);
            return squashed;
        }

        private async Task<Dictionary<string, DataAdapterResult<T>[]>> GetEntitiesFromAdapters(
            EntityReference[] entityReferences,
            AggregatesRequest aggregatesRequest,
            string[] fields,
            bool live,
            DateTime? pointInTime,
            CancellationToken cancellationToken)
        {
            // Split request for adapters
            var adapterReferences =
                entityReferences
                    .SelectMany(er => er.AdapterRecordReferences)
                    .GroupBy(ar => ar.SourceSystemName.ToUpper())
                    .Select(grp => new
                    {
                        SourceName = grp.Key,
                        Identifiers = grp
                            .Where(ar => !string.IsNullOrEmpty(ar.SourceSystemId))
                            .Select(ar => ar.SourceSystemId)
                            .Distinct()
                            .ToArray(),
                    })
                    .ToArray();

            // Start calls to adapters
            var tasks = new Task<DataAdapterResult<T>[]>[adapterReferences.Length];
            for (var i = 0; i < adapterReferences.Length; i++)
            {
                var adapterName = adapterReferences[i].SourceName;
                var adapterIdentifiers = adapterReferences[i].Identifiers;
                var adapter = _adapters.SingleOrDefault(a => a.SourceName.Equals(adapterName, StringComparison.InvariantCultureIgnoreCase));
                if (adapter == null)
                {
                    tasks[i] = Task.FromException<DataAdapterResult<T>[]>(
                        new DataAdapterException($"Cannot find adapter for {adapterName}")
                        {
                            AdapterName = adapterName,
                            RequestedIds = adapterIdentifiers,
                            RequestedFields = fields,
                        });
                    continue;
                }

                tasks[i] = adapter.GetEntitiesAsync(adapterIdentifiers, aggregatesRequest?.AggregateQueries, fields, live, pointInTime, cancellationToken);
            }

            // Collate results from adapters
            var results = new Dictionary<string, DataAdapterResult<T>[]>();
            for (var i = 0; i < adapterReferences.Length; i++)
            {
                var adapterName = adapterReferences[i].SourceName;
                var task = tasks[i];

                DataAdapterResult<T>[] adapterResults;
                try
                {
                    adapterResults = await task;
                }
                catch (DataAdapterException ex)
                {
                    _logger.Warning($"{adapterName} adapter returned error status {(int) ex.HttpStatusCode}:\n{ex.HttpErrorBody}");
                    var adapterIdentifiers = adapterReferences[i].Identifiers;
                    adapterResults = adapterIdentifiers
                        .Select(id =>
                            new DataAdapterResult<T>
                            {
                                Identifier = id,
                                AdapterError = ex,
                            })
                        .ToArray();
                }
                catch (Exception ex)
                {
                    _logger.Warning($"{adapterName} adapter encountered an unexpected error: {ex.Message}");
                    var adapterIdentifiers = adapterReferences[i].Identifiers;
                    adapterResults = adapterIdentifiers
                        .Select(id =>
                            new DataAdapterResult<T>
                            {
                                Identifier = id,
                                AdapterError = new DataAdapterException("Unexpected error calling adapter", ex)
                                {
                                    AdapterName = adapterName,
                                    RequestedIds = adapterIdentifiers,
                                    RequestedFields = fields,
                                },
                            })
                        .ToArray();
                }

                results.Add(adapterName, adapterResults);
            }

            return results;
        }

        private EntityReferenceSourceData<T>[] CollateAdapterResults(EntityReference[] entityReferences,
            Dictionary<string, DataAdapterResult<T>[]> adapterResults)
        {
            var candidates = new EntityReferenceSourceData<T>[entityReferences.Length];

            for (var i = 0; i < entityReferences.Length; i++)
            {
                var entityReference = entityReferences[i];
                var sourceEntities = new List<SourceSystemEntity<T>>();

                foreach (var adapterRecordReference in entityReference.AdapterRecordReferences)
                {
                    var adapterResult = adapterResults[adapterRecordReference.SourceSystemName.ToUpper()]
                        .SingleOrDefault(r => r.Identifier.Equals(adapterRecordReference.SourceSystemId, StringComparison.InvariantCultureIgnoreCase));

                    sourceEntities.Add(new SourceSystemEntity<T>
                    {
                        SourceName = adapterRecordReference.SourceSystemName,
                        SourceId = adapterRecordReference.SourceSystemId,
                        Entity = adapterResult?.Entity,
                        AdapterError = adapterResult?.AdapterError,
                    });
                }

                candidates[i] = new EntityReferenceSourceData<T>
                {
                    EntityReference = entityReference,
                    SourceEntities = sourceEntities.ToArray(),
                };
            }

            return candidates;
        }

        private SquashedEntityResult[] Squash(EntityReferenceSourceData<T>[] candidates, AggregatesRequest aggregatesRequest, string[] fields,
            EntityProfile entityProfile)
        {
            var results = new SquashedEntityResult[candidates.Length];

            var requiredProperties = fields.Length == 0
                ? _typeProperties
                : _typeProperties
                    .Where(p => fields.Any(f => f.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase)))
                    .ToArray();
            _logger.Info($"Squashing {candidates.Length} entities with {requiredProperties.Length} properties");
            _logger.Debug("Required properties: " + requiredProperties.Select(x => x.Name).Aggregate((x, y) => $"{x}, {y}"));

            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                var nonErroredSources = candidate.SourceEntities.Where(e => e.AdapterError == null).ToArray();
                T entity = null;

                if (nonErroredSources.Length > 0)
                {
                    entity = new T();
                    var entityBase = entity as EntityBase;
                    var isLineageRequired = entity != null &&
                                            (fields.Length == 0 || fields.Any(x => x.Equals("_lineage", StringComparison.InvariantCultureIgnoreCase)));
                    if (isLineageRequired)
                    {
                        entityBase._Lineage = new Dictionary<string, LineageEntry>();
                    }

                    _logger.Debug($"Squashing {candidate.EntityReference} {(isLineageRequired ? "with" : "without")} lineage");

                    foreach (var property in requiredProperties)
                    {
                        var fieldProfile = entityProfile.Fields.SingleOrDefault(f => f.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));
                        var sources = fieldProfile?.Sources != null && fieldProfile.Sources.Length > 0
                            ? fieldProfile.Sources
                            : entityProfile.Sources;
                        var treatWhitespaceAsNull = fieldProfile?.TreatWhitespaceAsNull ?? false;

                        if (property.Name.Equals("_aggregations", StringComparison.InvariantCultureIgnoreCase))
                        {
                            SquashAggregationsForEntity(property, entity, nonErroredSources, sources, aggregatesRequest);
                        }
                        else
                        {
                            SquashPropertyForEntity(property, entity, entityBase, nonErroredSources, sources, treatWhitespaceAsNull, isLineageRequired);
                        }
                    }
                }

                results[i] = new SquashedEntityResult
                {
                    EntityReference = candidate.EntityReference,
                    SquashedEntity = entity,
                    EntityAdapterErrorDetails = candidate.SourceEntities
                        .Where(e => e.AdapterError != null)
                        .Select(e =>
                            new EntityAdapterErrorDetail
                            {
                                AdapterName = e.SourceName,
                                RequestedId = e.SourceId,
                                RequestedFields = fields,
                                RequestedEntityName = e.AdapterError.RequestedEntityName,
                                HttpStatusCode = e.AdapterError.HttpStatusCode,
                                HttpErrorBody = e.AdapterError.HttpErrorBody
                            })
                        .ToArray(),
                };
            }

            return results;
        }

        private void SquashPropertyForEntity(
            PropertyInfo property,
            T entity,
            EntityBase entityBase,
            SourceSystemEntity<T>[] nonErroredSources,
            string[] sources,
            bool treatWhitespaceAsNull,
            bool isLineageRequired)
        {
            var lineageEntries = sources
                .SelectMany((source) =>
                {
                    var sourceEntities = nonErroredSources
                        .Where(x => x.SourceName.Equals(source, StringComparison.InvariantCultureIgnoreCase))
                        .ToArray();
                    if (!sourceEntities.Any())
                    {
                        return new[]
                        {
                            new LineageEntry
                            {
                                AdapterName = source,
                                ReadDate = DateTime.Now,
                            }
                        };
                    }

                    return sourceEntities
                        .Select(e => new LineageEntry
                        {
                            AdapterName = e.SourceName,
                            EntityId = e.SourceId,
                            Value = e.Entity == null ? null : property.GetValue(e.Entity),
                            ReadDate = DateTime.Now,
                        })
                        .ToArray();
                })
                .ToArray();
            var primaryCandidate = lineageEntries
                .FirstOrDefault(le => le.Value != null && !(treatWhitespaceAsNull && le.Value == string.Empty)) 
                                   ?? lineageEntries.First();
            property.SetValue(entity, primaryCandidate.Value);

            if (isLineageRequired)
            {
                primaryCandidate.Alternatives = lineageEntries.Where(le => le != primaryCandidate).ToArray();
                entityBase._Lineage.Add(property.Name, primaryCandidate);
            }
        }

        private void SquashAggregationsForEntity(
            PropertyInfo property,
            T entity,
            SourceSystemEntity<T>[] nonErroredSources,
            string[] sources,
            AggregatesRequest aggregatesRequest)
        {
            var aggregations = new List<Aggregation>();

            foreach (var aggregationName in aggregatesRequest.AggregateQueries.Keys)
            {
                var candidateAggregations = nonErroredSources
                    .Where(c => sources.Any(s => s.Equals(c.SourceName, StringComparison.InvariantCultureIgnoreCase)))
                    .SelectMany(c => (Aggregation[]) property.GetValue(c.Entity))
                    .Where(x => x != null && x.Name.Equals(aggregationName, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();

                // Currently only supports count. Though this should work for others such as sum
                var value = candidateAggregations.Sum(x => x.Value);

                aggregations.Add(new Aggregation
                {
                    Name = aggregationName,
                    Value = value,
                });
            }

            property.SetValue(entity, aggregations.ToArray());
        }

        private SourceSystemEntity<T>[] OrderCandidates(SourceSystemEntity<T>[] candidateSourceEntities, string[] sources)
        {
            var ordered = new List<SourceSystemEntity<T>>();

            foreach (var source in sources)
            {
                var sourceEntities = candidateSourceEntities
                    .Where(e => e.SourceName.Equals(source, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
                ordered.AddRange(sourceEntities);
            }

            return ordered.ToArray();
        }
    }
}