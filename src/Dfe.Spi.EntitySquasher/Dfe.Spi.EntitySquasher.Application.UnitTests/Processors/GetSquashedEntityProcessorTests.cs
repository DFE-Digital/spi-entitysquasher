using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.WellKnownIdentifiers;
using Dfe.Spi.EntitySquasher.Application.Definitions;
using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
using Dfe.Spi.EntitySquasher.Application.Models;
using Dfe.Spi.EntitySquasher.Application.Models.Processors;
using Dfe.Spi.EntitySquasher.Application.Models.Result;
using Dfe.Spi.EntitySquasher.Application.Processors;
using Dfe.Spi.EntitySquasher.Domain;
using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;
using Dfe.Spi.EntitySquasher.Domain.Models;
using Dfe.Spi.Models.Entities;
using Moq;
using NUnit.Framework;
using RestSharp.Extensions;

namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Processors
{
    public class GetSquashedEntityProcessorTests
    {
        private Random randomNumberGenerator;
        private Mock<IEntityAdapterInvoker> entityAdapterInvokerMock;
        private Mock<IGetSquashedEntityProcessorSettingsProvider> getSquashedEntityProcessorSettingsProviderMock;
        private Mock<ILoggerWrapper> loggerWrapperMock;
        private Mock<IResultSquasher> resultSquasherMock;
        private GetSquashedEntityProcessor processor;
        private CancellationToken cancellationToken;

        [SetUp]
        public void Arrange()
        {
            randomNumberGenerator = new Random();

            entityAdapterInvokerMock = new Mock<IEntityAdapterInvoker>();
            entityAdapterInvokerMock.Setup(x =>
                    x.GetResultsFromAdaptersAsync(
                        It.IsAny<string>(),
                        It.IsAny<EntityReference[]>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AggregatesRequest>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((string en, EntityReference[] er, string[] f, AggregatesRequest ar, bool live, CancellationToken ct) =>
                {
                    var adapterReferences = er.SelectMany(x => x.AdapterRecordReferences);
                    return adapterReferences
                        .ToDictionary(
                            x => x,
                            x => new GetEntityAsyncResult
                            {
                                AdapterRecordReference = x,
                                EntityAdapterException = new EntityAdapterException(new EntityAdapterErrorDetail
                                {
                                    AdapterName = x.Source,
                                    RequestedFields = f,
                                    RequestedId = x.Id,
                                    HttpStatusCode = HttpStatusCode.NotFound,
                                    RequestedEntityName = en,
                                }, HttpStatusCode.NotFound, null),
                            });
                });

            getSquashedEntityProcessorSettingsProviderMock = new Mock<IGetSquashedEntityProcessorSettingsProvider>();

            loggerWrapperMock = new Mock<ILoggerWrapper>();

            resultSquasherMock = new Mock<IResultSquasher>();

            processor = new GetSquashedEntityProcessor(
                entityAdapterInvokerMock.Object,
                getSquashedEntityProcessorSettingsProviderMock.Object,
                loggerWrapperMock.Object,
                resultSquasherMock.Object);

            cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task CallingGetSquashedEntityShouldGetDataFromAdapters()
        {
            var entityReferences = new[] {GetEntityReferenceForGiasAndUkrlp(), GetEntityReferenceForGiasAndUkrlp()};
            var request = GetRequest(entityReferences);

            await processor.GetSquashedEntityAsync(request, cancellationToken);

            entityAdapterInvokerMock.Verify(x =>
                    x.GetResultsFromAdaptersAsync(request.EntityName, entityReferences, request.Fields, request.AggregatesRequest, request.Live, cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task CallingGetSquashedEntityShouldReturnResultPerRequestedEntityReference()
        {
            var entityReferences = new[] {GetEntityReferenceForGiasAndUkrlp(), GetEntityReferenceForGiasAndUkrlp()};
            var request = GetRequest(entityReferences);

            var actual = await processor.GetSquashedEntityAsync(request, cancellationToken);

            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.SquashedEntityResults);
            Assert.AreEqual(entityReferences.Length, actual.SquashedEntityResults.Count());
            Assert.AreSame(entityReferences[0], actual.SquashedEntityResults.ElementAt(0).EntityReference);
            Assert.AreSame(entityReferences[1], actual.SquashedEntityResults.ElementAt(1).EntityReference);
        }

        [Test]
        public async Task CallingGetSquashedEntityShouldSquashResultsFromAdapters()
        {
            var entityReferences = new[] {GetEntityReferenceForGiasAndUkrlp(), GetEntityReferenceForGiasAndUkrlp()};
            var entityResults = GetEntityResultsForAdapterReferences(entityReferences);
            var request = GetRequest(entityReferences);
            SetupEntityAdapterInvokerToReturnResults(entityReferences, entityResults);

            await processor.GetSquashedEntityAsync(request, cancellationToken);

            resultSquasherMock.Verify(s =>
                    s.SquashAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<GetEntityAsyncResult>>(),
                        It.IsAny<AggregatesRequest>(),
                        It.IsAny<CancellationToken>()),
                Times.Exactly(entityReferences.Length));
            resultSquasherMock.Verify(s =>
                    s.SquashAsync(
                        request.Algorithm,
                        request.EntityName,
                        It.Is<IEnumerable<GetEntityAsyncResult>>((toSquash) =>
                            toSquash.Count() == entityReferences[0].AdapterRecordReferences.Count() &&
                            toSquash.Any(x => x.EntityBase == entityResults[0]) &&
                            toSquash.Any(x => x.EntityBase == entityResults[1])),
                        request.AggregatesRequest,
                        cancellationToken),
                Times.Once);
            resultSquasherMock.Verify(s =>
                    s.SquashAsync(
                        request.Algorithm,
                        request.EntityName,
                        It.Is<IEnumerable<GetEntityAsyncResult>>((toSquash) =>
                            toSquash.Count() == entityReferences[1].AdapterRecordReferences.Count() &&
                            toSquash.Any(x => x.EntityBase == entityResults[2]) &&
                            toSquash.Any(x => x.EntityBase == entityResults[3])),
                        request.AggregatesRequest,
                        cancellationToken),
                Times.Once);
        }


        private EntityReference GetEntityReferenceForGiasAndUkrlp(long? urn = null, long? ukprn = null)
        {
            return new EntityReference
            {
                AdapterRecordReferences = new[]
                {
                    new AdapterRecordReference
                    {
                        Source = SourceSystemNames.GetInformationAboutSchools,
                        Id = (urn ?? randomNumberGenerator.Next(100001, 999999)).ToString()
                    },
                    new AdapterRecordReference
                    {
                        Source = SourceSystemNames.UkRegisterOfLearningProviders,
                        Id = (urn ?? randomNumberGenerator.Next(10000001, 99999999)).ToString()
                    },
                },
            };
        }

        private GetSquashedEntityRequest GetRequest(EntityReference[] entityReferences)
        {
            return new GetSquashedEntityRequest
            {
                Algorithm = "algo1",
                Fields = new[]
                {
                    "name",
                    "urn",
                    "ukprn",
                },
                EntityName = "LearningProvider",
                EntityReferences = entityReferences,
            };
        }

        private EntityBase[] GetEntityResultsForAdapterReferences(EntityReference[] entityReferences)
        {
            var adapterRecordReferences = entityReferences
                .SelectMany(x => x.AdapterRecordReferences)
                .ToArray();

            return GetEntityResultsForAdapterReferences(adapterRecordReferences);
        }

        private EntityBase[] GetEntityResultsForAdapterReferences(AdapterRecordReference[] adapterRecordReferences)
        {
            var entities = new EntityBase[adapterRecordReferences.Length];

            for (var i = 0; i < adapterRecordReferences.Length; i++)
            {
                entities[i] = new LearningProvider
                {
                    Urn = adapterRecordReferences[i].Source == SourceSystemNames.GetInformationAboutSchools
                        ? (long?) long.Parse(adapterRecordReferences[i].Id)
                        : null,
                    Ukprn = adapterRecordReferences[i].Source == SourceSystemNames.UkRegisterOfLearningProviders
                        ? (long?) long.Parse(adapterRecordReferences[i].Id)
                        : null,
                };
            }

            return entities;
        }

        private void SetupEntityAdapterInvokerToReturnResults(EntityReference[] entityReferences, EntityBase[] entityResults)
        {
            var adapterRecordReferences = entityReferences
                .SelectMany(x => x.AdapterRecordReferences)
                .ToArray();

            SetupEntityAdapterInvokerToReturnResults(adapterRecordReferences, entityResults);
        }

        private void SetupEntityAdapterInvokerToReturnResults(AdapterRecordReference[] adapterRecordReferences, EntityBase[] entityResults)
        {
            var results = new Dictionary<AdapterRecordReference, GetEntityAsyncResult>();
            for (var i = 0; i < adapterRecordReferences.Length; i++)
            {
                results.Add(adapterRecordReferences[i], new GetEntityAsyncResult
                {
                    AdapterRecordReference = adapterRecordReferences[i],
                    EntityBase = entityResults[i],
                });
            }

            entityAdapterInvokerMock.Setup(x =>
                    x.GetResultsFromAdaptersAsync(
                        It.IsAny<string>(),
                        It.IsAny<EntityReference[]>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AggregatesRequest>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(results);
        }
    }
}