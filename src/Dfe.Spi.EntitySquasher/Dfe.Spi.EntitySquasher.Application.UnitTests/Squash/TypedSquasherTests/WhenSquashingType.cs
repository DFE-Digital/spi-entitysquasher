using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.Models;
using Dfe.Spi.EntitySquasher.Application.Squash;
using Dfe.Spi.EntitySquasher.Domain.Adapters;
using Dfe.Spi.EntitySquasher.Domain.Profiles;
using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;
using Moq;
using NUnit.Framework;

namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Squash.TypedSquasherTests
{
    public class WhenSquashingType
    {
        private Random _random;
        public Mock<IDataAdapter<TestEntity>> _adapter1Mock;
        public Mock<IDataAdapter<TestEntity>> _adapter2Mock;
        private Mock<ILoggerWrapper> _loggerMock;
        private TypedSquasher<TestEntity> _squasher;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _random = new Random();

            _adapter1Mock = new Mock<IDataAdapter<TestEntity>>();
            _adapter1Mock.Setup(a => a.SourceName)
                .Returns("Source1");
            SetupAdapterMock(_adapter1Mock);

            _adapter2Mock = new Mock<IDataAdapter<TestEntity>>();
            _adapter2Mock.Setup(a => a.SourceName)
                .Returns("Source2");
            SetupAdapterMock(_adapter2Mock);

            _loggerMock = new Mock<ILoggerWrapper>();

            _squasher = new TypedSquasher<TestEntity>(
                new[] {_adapter1Mock.Object, _adapter2Mock.Object},
                _loggerMock.Object);
        }

        [Test]
        public async Task ThenItShouldThrowProfileMisconfiguredIfProfileDoesNotContainEntityType()
        {
            var requestParameters = GetRequestParameters();
            requestParameters.Profile.Entities[0].Name = "AnotherEntity";

            var actual = Assert.ThrowsAsync<ProfileMisconfiguredException>(async () =>
                await _squasher.SquashAsync(
                    requestParameters.EntityReferences,
                    requestParameters.AggregatesRequest,
                    requestParameters.Fields,
                    requestParameters.Live,
                    requestParameters.PointInTime,
                    requestParameters.Profile,
                    _cancellationToken));
            Assert.AreEqual(requestParameters.Profile.Name, actual.ProfileName);
            Assert.AreEqual(typeof(TestEntity), actual.EntityType);
        }

        [Test]
        public async Task ThenItShouldRequestDataFromAdaptersInBatch()
        {
            var requestParameters = GetRequestParameters();
            requestParameters.EntityReferences = new[]
            {
                GetEntityReference(true, true),
                GetEntityReference(true, false),
                GetEntityReference(false, true),
            };

            await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            _adapter1Mock.Verify(a => a.GetEntitiesAsync(
                    It.Is<string[]>(ids =>
                        ids.Length == 2 &&
                        ids[0] == requestParameters.EntityReferences[0].AdapterRecordReferences[0].Id &&
                        ids[1] == requestParameters.EntityReferences[1].AdapterRecordReferences[0].Id),
                    requestParameters.AggregatesRequest == null ? null : requestParameters.AggregatesRequest.AggregateQueries,
                    requestParameters.Fields,
                    requestParameters.Live,
                    requestParameters.PointInTime,
                    _cancellationToken),
                Times.Once);
            _adapter2Mock.Verify(a => a.GetEntitiesAsync(
                    It.Is<string[]>(ids =>
                        ids.Length == 2 &&
                        ids[0] == requestParameters.EntityReferences[0].AdapterRecordReferences[1].Id &&
                        ids[1] == requestParameters.EntityReferences[2].AdapterRecordReferences[0].Id),
                    requestParameters.AggregatesRequest == null ? null : requestParameters.AggregatesRequest.AggregateQueries,
                    requestParameters.Fields,
                    requestParameters.Live,
                    requestParameters.PointInTime,
                    _cancellationToken),
                Times.Once);
        }

        [TestCase("Source1", "Source2")]
        [TestCase("Source2", "Source1")]
        public async Task ThenItShouldReturnDataBasedOnProfileLevelSourcesWhenNoneOnFields(string primarySource, string secondarySource)
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            requestParameters.Profile.Entities[0].Sources = new[] {primarySource, secondarySource};
            var source1Entity = MakeTestEntity("Source1");
            var source2Entity = MakeTestEntity("Source2");
            SetupAdapterMock(_adapter1Mock, new[] {source1Entity});
            SetupAdapterMock(_adapter2Mock, new[] {source2Entity});

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Length);
            Assert.AreSame(actual[0].EntityReference, requestParameters.EntityReferences[0]);
            Assert.IsNotNull(actual[0].SquashedEntity);
            Assert.IsEmpty(actual[0].EntityAdapterErrorDetails);

            var actualTestEntity = actual[0].SquashedEntity as TestEntity;
            var expectedTestEntity = primarySource == "Source1" ? source1Entity : source2Entity;
            Assert.IsNotNull(actualTestEntity,
                $"Expected actual[0].SquashedEntity to be instance of {nameof(TestEntity)} but was {actual[0].SquashedEntity.GetType().Name}");
            Assert.AreEqual(expectedTestEntity.Name, actualTestEntity.Name);
            Assert.AreEqual(expectedTestEntity.Address, actualTestEntity.Address);
            Assert.AreEqual(expectedTestEntity.NumberOfPupils, actualTestEntity.NumberOfPupils);
            Assert.AreEqual(expectedTestEntity.IsOpen, actualTestEntity.IsOpen);
            Assert.AreEqual(expectedTestEntity.OpenDate, actualTestEntity.OpenDate);
            Assert.AreEqual(expectedTestEntity.ContactNumbers, actualTestEntity.ContactNumbers);
        }

        [TestCase("Source1", "Source2")]
        [TestCase("Source2", "Source1")]
        public async Task ThenItShouldReturnDataBasedOnFieldLevelSourceWhenAvailableOtherwiseProfileLevel(string primarySource, string secondarySource)
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            requestParameters.Profile.Entities[0].Sources = new[] {primarySource, secondarySource};
            requestParameters.Profile.Entities[0].Fields.Single(x => x.Name == "Name").Sources = new[] {"Source2", "Source1"};
            requestParameters.Profile.Entities[0].Fields.Single(x => x.Name == "Address").Sources = new[] {"Source1", "Source2"};
            var source1Entity = MakeTestEntity("Source1");
            var source2Entity = MakeTestEntity("Source2");
            SetupAdapterMock(_adapter1Mock, new[] {source1Entity});
            SetupAdapterMock(_adapter2Mock, new[] {source2Entity});

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Length);
            Assert.AreSame(actual[0].EntityReference, requestParameters.EntityReferences[0]);
            Assert.IsNotNull(actual[0].SquashedEntity);
            Assert.IsEmpty(actual[0].EntityAdapterErrorDetails);

            var actualTestEntity = actual[0].SquashedEntity as TestEntity;
            var expectedProfileTestEntity = primarySource == "Source1" ? source1Entity : source2Entity;
            Assert.IsNotNull(actualTestEntity,
                $"Expected actual[0].SquashedEntity to be instance of {nameof(TestEntity)} but was {actual[0].SquashedEntity.GetType().Name}");
            Assert.AreEqual(source2Entity.Name, actualTestEntity.Name);
            Assert.AreEqual(source1Entity.Address, actualTestEntity.Address);
            Assert.AreEqual(expectedProfileTestEntity.NumberOfPupils, actualTestEntity.NumberOfPupils);
            Assert.AreEqual(expectedProfileTestEntity.IsOpen, actualTestEntity.IsOpen);
            Assert.AreEqual(expectedProfileTestEntity.OpenDate, actualTestEntity.OpenDate);
            Assert.AreEqual(expectedProfileTestEntity.ContactNumbers, actualTestEntity.ContactNumbers);
        }

        [Test]
        public async Task ThenItShouldUseValueFromSecondarySourceIfPrimarySourceIsNull()
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            var source1Entity = MakeTestEntity("Source1");
            source1Entity.Address = null;
            var source2Entity = MakeTestEntity("Source2");
            SetupAdapterMock(_adapter1Mock, new[] {source1Entity});
            SetupAdapterMock(_adapter2Mock, new[] {source2Entity});

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            var actualTestEntity = (TestEntity) actual[0].SquashedEntity;
            Assert.AreEqual(source2Entity.Address, actualTestEntity.Address);
        }

        [Test]
        public async Task ThenItShouldUseBlankValueFromPrimarySourceIfTreatWhitespaceAsNullIsFalse()
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            requestParameters.Profile.Entities[0].Fields.Single(x => x.Name == "Address").TreatWhitespaceAsNull = false;
            var source1Entity = MakeTestEntity("Source1");
            source1Entity.Address = string.Empty;
            var source2Entity = MakeTestEntity("Source2");
            SetupAdapterMock(_adapter1Mock, new[] {source1Entity});
            SetupAdapterMock(_adapter2Mock, new[] {source2Entity});

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            var actualTestEntity = (TestEntity) actual[0].SquashedEntity;
            Assert.AreEqual(source1Entity.Address, actualTestEntity.Address);
        }

        [Test]
        public async Task ThenItShouldUseValueFromSecondarySourceIfPrimarySourceIsBlankAndTreatWhitespaceAsNullIsTrue()
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            requestParameters.Profile.Entities[0].Fields.Single(x => x.Name == "Address").TreatWhitespaceAsNull = true;
            var source1Entity = MakeTestEntity("Source1");
            source1Entity.Address = string.Empty;
            var source2Entity = MakeTestEntity("Source2");
            SetupAdapterMock(_adapter1Mock, new[] {source1Entity});
            SetupAdapterMock(_adapter2Mock, new[] {source2Entity});

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            var actualTestEntity = (TestEntity) actual[0].SquashedEntity;
            Assert.AreEqual(source2Entity.Address, actualTestEntity.Address);
        }

        [Test]
        public async Task ThenItShouldUsePrimarySourceIfAllSourcesNull()
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            requestParameters.Profile.Entities[0].Fields.Single(x => x.Name == "Address").TreatWhitespaceAsNull = true;
            var source1Entity = MakeTestEntity("Source1");
            source1Entity.Address = string.Empty;
            var source2Entity = MakeTestEntity("Source2");
            source2Entity.Address = null;
            SetupAdapterMock(_adapter1Mock, new[] {source1Entity});
            SetupAdapterMock(_adapter2Mock, new[] {source2Entity});

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            var actualTestEntity = (TestEntity) actual[0].SquashedEntity;
            Assert.AreEqual(source1Entity.Address, actualTestEntity.Address);
        }

        [Test]
        public async Task ThenItShouldOnlySetFieldsInSelectionList()
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            requestParameters.Fields = new[] {"Name", "NumberOfPupils"};
            var source1Entity = MakeTestEntity("Source1");
            var source2Entity = MakeTestEntity("Source2");
            SetupAdapterMock(_adapter1Mock, new[] {source1Entity});
            SetupAdapterMock(_adapter2Mock, new[] {source2Entity});

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            var actualTestEntity = (TestEntity) actual[0].SquashedEntity;
            Assert.AreEqual(source1Entity.Name, actualTestEntity.Name);
            Assert.AreEqual(default(string), actualTestEntity.Address);
            Assert.AreEqual(source1Entity.NumberOfPupils, actualTestEntity.NumberOfPupils);
            Assert.AreEqual(default(bool), actualTestEntity.IsOpen);
            Assert.AreEqual(default(DateTime?), actualTestEntity.OpenDate);
            Assert.AreEqual(default(string[]), actualTestEntity.ContactNumbers);
        }

        [Test]
        public async Task ThenItShouldBuildLineageIfNoFieldsSpecified()
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            requestParameters.Profile.Entities[0].Fields.Single(x => x.Name == "Name").Sources = new[] {"Source2", "Source1"};
            requestParameters.Profile.Entities[0].Fields.Single(x => x.Name == "IsOpen").Sources = new[] {"Source2", "Source1"};
            var source1Entity = MakeTestEntity("Source1");
            var source2Entity = MakeTestEntity("Source2");
            SetupAdapterMock(_adapter1Mock, new[] {source1Entity});
            SetupAdapterMock(_adapter2Mock, new[] {source2Entity});

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            var actualTestEntity = (TestEntity) actual[0].SquashedEntity;
            Assert.IsNotNull(actualTestEntity._Lineage);
            Assert.AreEqual(6, actualTestEntity._Lineage.Count);
            AssertLineage(actualTestEntity, "Name", source2Entity, source1Entity);
            AssertLineage(actualTestEntity, "Address", source1Entity, source2Entity);
            AssertLineage(actualTestEntity, "NumberOfPupils", source1Entity, source2Entity);
            AssertLineage(actualTestEntity, "IsOpen", source2Entity, source1Entity);
            AssertLineage(actualTestEntity, "OpenDate", source1Entity, source2Entity);
            AssertLineage(actualTestEntity, "ContactNumbers", source1Entity, source2Entity);
        }

        [Test]
        public async Task ThenItShouldBuildLineageIfSpecifiedInGivenFieldList()
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            requestParameters.Fields = new[] {"Name", "Address", "_Lineage"};
            requestParameters.Profile.Entities[0].Fields.Single(x => x.Name == "Name").Sources = new[] {"Source2", "Source1"};
            requestParameters.Profile.Entities[0].Fields.Single(x => x.Name == "IsOpen").Sources = new[] {"Source2", "Source1"};
            var source1Entity = MakeTestEntity("Source1");
            var source2Entity = MakeTestEntity("Source2");
            SetupAdapterMock(_adapter1Mock, new[] {source1Entity});
            SetupAdapterMock(_adapter2Mock, new[] {source2Entity});

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            var actualTestEntity = (TestEntity) actual[0].SquashedEntity;
            Assert.IsNotNull(actualTestEntity._Lineage);
            Assert.AreEqual(2, actualTestEntity._Lineage.Count);
            AssertLineage(actualTestEntity, "Name", source2Entity, source1Entity);
            AssertLineage(actualTestEntity, "Address", source1Entity, source2Entity);
        }

        [Test]
        public async Task ThenItShouldNotBuildLineageIfNotSpecifiedInGivenFieldList()
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            requestParameters.Fields = new[] {"Name", "Address"};
            var source1Entity = MakeTestEntity("Source1");
            var source2Entity = MakeTestEntity("Source2");
            SetupAdapterMock(_adapter1Mock, new[] {source1Entity});
            SetupAdapterMock(_adapter2Mock, new[] {source2Entity});

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            var actualTestEntity = (TestEntity) actual[0].SquashedEntity;
            Assert.IsNull(actualTestEntity._Lineage);
        }

        [Test]
        public async Task ThenItShouldReturnErrorsFromAdaptersIfThrown()
        {
            var adapterException = new DataAdapterException("testing")
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                HttpErrorBody = new HttpErrorBody(),
            };
            var requestParameters = GetRequestParameters(forceBothSources: true);
            _adapter1Mock.Setup(a => a.GetEntitiesAsync(
                    It.IsAny<string[]>(),
                    It.IsAny<Dictionary<string, AggregateQuery>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(adapterException);

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Length);
            Assert.AreSame(actual[0].EntityReference, requestParameters.EntityReferences[0]);
            Assert.IsNotNull(actual[0].EntityAdapterErrorDetails);
            Assert.AreEqual(1, actual[0].EntityAdapterErrorDetails.Length);
            Assert.AreEqual("Source1", actual[0].EntityAdapterErrorDetails[0].AdapterName);
            Assert.AreEqual(requestParameters.EntityReferences[0].AdapterRecordReferences[0].Id, actual[0].EntityAdapterErrorDetails[0].RequestedId);
            Assert.AreEqual(adapterException.HttpStatusCode, actual[0].EntityAdapterErrorDetails[0].HttpStatusCode);
            Assert.AreEqual(adapterException.HttpErrorBody, actual[0].EntityAdapterErrorDetails[0].HttpErrorBody);
        }

        [Test]
        public async Task ThenItShouldHandleUnexpectedErrorsFromAdapters()
        {
            var requestParameters = GetRequestParameters(forceBothSources: true);
            _adapter1Mock.Setup(a => a.GetEntitiesAsync(
                    It.IsAny<string[]>(),
                    It.IsAny<Dictionary<string, AggregateQuery>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("testing"));

            var actual = await _squasher.SquashAsync(
                requestParameters.EntityReferences,
                requestParameters.AggregatesRequest,
                requestParameters.Fields,
                requestParameters.Live,
                requestParameters.PointInTime,
                requestParameters.Profile,
                _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Length);
            Assert.AreSame(actual[0].EntityReference, requestParameters.EntityReferences[0]);
            Assert.IsNotNull(actual[0].EntityAdapterErrorDetails);
            Assert.AreEqual(1, actual[0].EntityAdapterErrorDetails.Length);
            Assert.AreEqual("Source1", actual[0].EntityAdapterErrorDetails[0].AdapterName);
            Assert.AreEqual(requestParameters.EntityReferences[0].AdapterRecordReferences[0].Id, actual[0].EntityAdapterErrorDetails[0].RequestedId);
        }


        private void SetupAdapterMock(Mock<IDataAdapter<TestEntity>> adapterMock, TestEntity[] entities = null)
        {
            adapterMock.Setup(a => a.GetEntitiesAsync(
                    It.IsAny<string[]>(),
                    It.IsAny<Dictionary<string, AggregateQuery>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((string[] identifiers, Dictionary<string, AggregateQuery> aggregateQueries, string[] fields, bool live, DateTime? pointInTime,
                    CancellationToken cancellationToken) =>
                {
                    var results = new DataAdapterResult<TestEntity>[identifiers.Length];

                    for (var i = 0; i < identifiers.Length; i++)
                    {
                        var entity = entities != null && i < entities.Length
                            ? entities[i]
                            : MakeTestEntity();
                        results[i] = new DataAdapterResult<TestEntity>
                        {
                            Identifier = identifiers[i],
                            Entity = entity,
                        };
                    }

                    return results;
                });
        }

        private RequestParameters GetRequestParameters(int numberOfEntities = 1, bool forceBothSources = false)
        {
            var entityReferences = new EntityReference[numberOfEntities];
            for (var i = 0; i < entityReferences.Length; i++)
            {
                var sourceSelection = forceBothSources ? 3 : _random.Next(1, 3); // 1=Source1, 2=Source2, 3=Both
                entityReferences[i] = GetEntityReference(sourceSelection == 1 || sourceSelection == 3, sourceSelection == 2 || sourceSelection == 3);
            }

            AggregatesRequest aggregatesRequest = null;

            var fields = new string[0];

            var live = _random.Next(0, 100) >= 50;

            var daysAgo = _random.Next(0, 100);
            var pointInTime = daysAgo < 10 ? null : (DateTime?)DateTime.Today.AddDays(-daysAgo);

            var profile = new Profile
            {
                Name = $"ProfileName-{Guid.NewGuid().ToString()}",
                Entities = new[]
                {
                    new EntityProfile
                    {
                        Name = nameof(TestEntity),
                        Sources = new[] {"Source1", "Source2"},
                        Fields = new[]
                        {
                            new EntityFieldProfile {Name = "Name"},
                            new EntityFieldProfile {Name = "Address"},
                            new EntityFieldProfile {Name = "NumberOfPupils"},
                            new EntityFieldProfile {Name = "IsOpen"},
                            new EntityFieldProfile {Name = "OpenDate"},
                            new EntityFieldProfile {Name = "ContactNumbers"},
                        }
                    },
                }
            };

            return new RequestParameters
            {
                EntityReferences = entityReferences,
                AggregatesRequest = aggregatesRequest,
                Fields = fields,
                Live = live,
                PointInTime = pointInTime,
                Profile = profile,
            };
        }

        private EntityReference GetEntityReference(bool includeSource1, bool includeSource2)
        {
            if (includeSource1 && !includeSource2)
            {
                return new EntityReference
                {
                    AdapterRecordReferences = new[]
                    {
                        new AdapterRecordReference {Source = "Source1", Id = _random.Next(1000, 9999).ToString()},
                    }
                };
            }

            if (!includeSource1 && includeSource2)
            {
                return new EntityReference
                {
                    AdapterRecordReferences = new[]
                    {
                        new AdapterRecordReference {Source = "Source2", Id = _random.Next(1000, 9999).ToString()},
                    }
                };
            }

            return new EntityReference
            {
                AdapterRecordReferences = new[]
                {
                    new AdapterRecordReference {Source = "Source1", Id = _random.Next(1000, 9999).ToString()},
                    new AdapterRecordReference {Source = "Source2", Id = _random.Next(1000, 9999).ToString()},
                }
            };
        }

        private TestEntity MakeTestEntity(string sourceName = null)
        {
            var prefix = string.IsNullOrEmpty(sourceName) ? string.Empty : $"{sourceName}-";
            return new TestEntity
            {
                Name = $"{prefix}Name-{Guid.NewGuid().ToString()}",
                Address = $"{prefix}Address-{Guid.NewGuid().ToString()}",
                IsOpen = _random.Next(0, 100) >= 50,
                OpenDate = DateTime.Today.AddDays(_random.Next(180, 365) * -1),
                NumberOfPupils = _random.Next(10, 300),
                ContactNumbers = new[]
                {
                    $"{prefix}ContactNumbers0-{Guid.NewGuid().ToString()}",
                    $"{prefix}ContactNumbers1-{Guid.NewGuid().ToString()}",
                },
            };
        }

        private void AssertLineage(TestEntity squashedEntity, string field, TestEntity primaryEntity, TestEntity secondaryEntity)
        {
            var property = typeof(TestEntity).GetProperty(field);
            var primaryValue = property.GetValue(primaryEntity);
            var secondaryValue = property.GetValue(secondaryEntity);
            var lineageEntry = squashedEntity._Lineage[field];

            Assert.AreEqual(primaryValue, lineageEntry.Value);
            Assert.AreEqual(secondaryValue, lineageEntry.Alternatives.First().Value);
        }

        private class RequestParameters
        {
            public EntityReference[] EntityReferences { get; set; }
            public AggregatesRequest AggregatesRequest { get; set; }
            public string[] Fields { get; set; }
            public bool Live { get; set; }
            public DateTime? PointInTime { get; set; }
            public Profile Profile { get; set; }
        }
    }
}