namespace Dfe.Spi.EntitySquasher.Application.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Domain;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models;
    using Dfe.Spi.Models;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EntityAdapterInvokerTests
    {
        private EntityAdapterInvoker entityAdapterInvoker;
        private LoggerWrapper loggerWrapper;
        private Mock<IEntityAdapterClientManager> mockEntityAdapterClientManager;

        [SetUp]
        public void Arrange()
        {
            this.mockEntityAdapterClientManager =
                new Mock<IEntityAdapterClientManager>();

            IEntityAdapterClientManager entityAdapterClientManager =
                mockEntityAdapterClientManager.Object;

            Mock<IEntityAdapterClientManagerFactory> mockEntityAdapterClientManagerFactory =
                new Mock<IEntityAdapterClientManagerFactory>();

            mockEntityAdapterClientManagerFactory
                .Setup(x => x.Create())
                .Returns(entityAdapterClientManager);

            IEntityAdapterClientManagerFactory entityAdapterClientManagerFactory =
                mockEntityAdapterClientManagerFactory.Object;

            this.loggerWrapper = new LoggerWrapper();

            this.entityAdapterInvoker = new EntityAdapterInvoker(
                entityAdapterClientManagerFactory,
                this.loggerWrapper);
        }

        [Test]
        public void InvokeEntityAdaptersAsync_PostWithoutEntityReference_ThrowsArgumentNullException()
        {
            // Arrange
            string algorithm = null;
            string entityName = null;
            string[] fields = null;
            EntityReference entityReference = null;
            CancellationToken cancellationToken = CancellationToken.None;

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.entityAdapterInvoker.InvokeEntityAdaptersAsync(
                        algorithm,
                        entityName,
                        fields,
                        entityReference,
                        cancellationToken);
                };

            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(asyncTestDelegate);
        }

        [Test]
        public async Task InvokeEntityAdaptersAsync_2AdaptersSucceed1Fails_OutputIsAsExpected()
        {
            // Arrange
            string algorithm = "some-algorithm";
            string entityName = "LearningProvider";
            string[] fields = new string[]
            {
                "SomeFieldOne",
                "SomeFieldTwo",
            };

            const string mockAdapter1Id = "working-adapter-#1";
            const string mockAdapter2Id = "failing-adapter-#2";
            const string mockAdapter3Id = "working-adapter-#3";

            string mockAdapter2RecordId = "2890d784-900f-4861-a034-30e645bd57b5";

            EntityAdapterErrorDetail expectedEntityAdapterErrorDetail =
                new EntityAdapterErrorDetail()
                {
                    AdapterName = mockAdapter2Id,
                    HttpErrorBody = new Common.Models.HttpErrorBody()
                    {
                        ErrorIdentifier = "FA-008",
                        Message = "Some ficticious error message to go here.",
                        StatusCode = HttpStatusCode.UnavailableForLegalReasons,
                    },
                    HttpStatusCode = HttpStatusCode.UnavailableForLegalReasons,
                    RequestedEntityName = entityName,
                    RequestedFields = fields,
                    RequestedId = mockAdapter2RecordId,
                };
            EntityAdapterErrorDetail actualEntityAdapterErrorDetail = null;

            IEntityAdapterClient adapter1 = this.CreateEntityAdapterClient(
                TimeSpan.Parse("00:00:01"));

            EntityAdapterException entityAdapterException =
                new EntityAdapterException(
                    expectedEntityAdapterErrorDetail,
                    expectedEntityAdapterErrorDetail.HttpStatusCode,
                    expectedEntityAdapterErrorDetail.HttpErrorBody);

            IEntityAdapterClient adapter2 = this.CreateEntityAdapterClient(
                TimeSpan.Parse("00:00:05"),
                entityAdapterException);
            IEntityAdapterClient adapter3 = this.CreateEntityAdapterClient(
                TimeSpan.Parse("00:00:03"));

            Dictionary<string, IEntityAdapterClient> adapters =
                new Dictionary<string, IEntityAdapterClient>()
                {
                    { mockAdapter1Id, adapter1 },
                    { mockAdapter2Id, adapter2 },
                    { mockAdapter3Id, adapter3 },
                };

            AdapterRecordReference[] adapterRecordReferences =
                new AdapterRecordReference[]
                {
                    new AdapterRecordReference()
                    {
                        Id = "123456",
                        Source = mockAdapter1Id,
                    },
                    new AdapterRecordReference()
                    {
                        Id = mockAdapter2RecordId,
                        Source = mockAdapter2Id,
                    },
                    new AdapterRecordReference()
                    {
                        Id = "abc",
                        Source = mockAdapter3Id,
                    },
                };

            EntityReference entityReference = new EntityReference()
            {
                AdapterRecordReferences = adapterRecordReferences,
            };

            CancellationToken cancellationToken = CancellationToken.None;

            Func<EntityAdapterClientKey, CancellationToken, IEntityAdapterClient> getAsyncCallback =
                (x, y) =>
                {
                    IEntityAdapterClient entityAdapterClient =
                        adapters[x.Name];

                    return entityAdapterClient;
                };

            this.mockEntityAdapterClientManager
                .Setup(x => x.GetAsync(It.IsAny<EntityAdapterClientKey>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getAsyncCallback);

            int expectedSuccessfulTasks = 2;
            int actualSuccessfulTasks;

            GetEntityAsyncResult unsuccessfulResult = null;

            // Act
            InvokeEntityAdaptersResult invokeEntityAdaptersResult =
                await this.entityAdapterInvoker.InvokeEntityAdaptersAsync(
                    algorithm,
                    entityName,
                    fields,
                    entityReference,
                    cancellationToken);

            // Assert
            // We should have 2 successful results, and one unsuccessful
            // result.
            // First, assert the successful ones:
            actualSuccessfulTasks = invokeEntityAdaptersResult
                .GetEntityAsyncResults
                .Count(x => x.ModelsBase != null);

            Assert.AreEqual(expectedSuccessfulTasks, actualSuccessfulTasks);

            // We should have one unsuccessful task.
            unsuccessfulResult = invokeEntityAdaptersResult
                .GetEntityAsyncResults
                .SingleOrDefault(x => x.EntityAdapterException != null);

            Assert.IsNotNull(unsuccessfulResult);

            // And the error detail should match up...
            actualEntityAdapterErrorDetail = unsuccessfulResult
                .EntityAdapterException
                .EntityAdapterErrorDetail;

            Assert.AreEqual(
                expectedEntityAdapterErrorDetail,
                actualEntityAdapterErrorDetail);

            string logOutput = this.loggerWrapper.ReturnLog();
        }

        [Test]
        public void InvokeEntityAdaptersAsync_OnlyAdapterFails_ThrowsAllAdaptersUnavailableException()
        {
            // Arrange
            string algorithm = "some-algorithm";
            string entityName = "LearningProvider";
            string[] fields = new string[]
            {
                "SomeFieldOne",
                "SomeFieldTwo",
            };

            const string mockAdapter2Id = "failing-adapter-#2";

            string mockAdapter2RecordId = "2890d784-900f-4861-a034-30e645bd57b5";

            EntityAdapterErrorDetail expectedEntityAdapterErrorDetail =
                new EntityAdapterErrorDetail()
                {
                    AdapterName = mockAdapter2Id,
                    HttpErrorBody = new Common.Models.HttpErrorBody()
                    {
                        ErrorIdentifier = "FA-008",
                        Message = "Some ficticious error message to go here.",
                        StatusCode = HttpStatusCode.UnavailableForLegalReasons,
                    },
                    HttpStatusCode = HttpStatusCode.UnavailableForLegalReasons,
                    RequestedEntityName = entityName,
                    RequestedFields = fields,
                    RequestedId = mockAdapter2RecordId,
                };

            EntityAdapterException entityAdapterException =
                new EntityAdapterException(
                    expectedEntityAdapterErrorDetail,
                    expectedEntityAdapterErrorDetail.HttpStatusCode,
                    expectedEntityAdapterErrorDetail.HttpErrorBody);

            IEntityAdapterClient adapter2 = this.CreateEntityAdapterClient(
                TimeSpan.Parse("00:00:02"),
                entityAdapterException);

            Dictionary<string, IEntityAdapterClient> adapters =
                new Dictionary<string, IEntityAdapterClient>()
                {
                    { mockAdapter2Id, adapter2 },
                };


            AdapterRecordReference[] adapterRecordReferences =
                new AdapterRecordReference[]
                {
                    new AdapterRecordReference()
                    {
                        Id = mockAdapter2RecordId,
                        Source = mockAdapter2Id,
                    },
                };

            EntityReference entityReference = new EntityReference()
            {
                AdapterRecordReferences = adapterRecordReferences,
            };

            CancellationToken cancellationToken = CancellationToken.None;

            Func<EntityAdapterClientKey, CancellationToken, IEntityAdapterClient> getAsyncCallback =
                (x, y) =>
                {
                    IEntityAdapterClient entityAdapterClient =
                        adapters[x.Name];

                    return entityAdapterClient;
                };

            this.mockEntityAdapterClientManager
                .Setup(x => x.GetAsync(It.IsAny<EntityAdapterClientKey>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getAsyncCallback);

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.entityAdapterInvoker.InvokeEntityAdaptersAsync(
                        algorithm,
                        entityName,
                        fields,
                        entityReference,
                        cancellationToken);
                };

            // Assert
            Assert.ThrowsAsync<AllAdaptersUnavailableException>(
                asyncTestDelegate);

            string logOutput = this.loggerWrapper.ReturnLog();
        }

        [Test]
        public void InvokeEntityAdaptersAsync_UnhandledExceptionOccursInTask_ExceptionsSurfaced()
        {
            // Arrange
            string algorithm = "some-algorithm";
            string entityName = "LearningProvider";
            string[] fields = new string[]
            {
                "SomeFieldOne",
                "SomeFieldTwo",
            };

            const string mockAdapter2Id = "failing-adapter-#2";

            string mockAdapter2RecordId = "2890d784-900f-4861-a034-30e645bd57b5";

            SystemException systemException =
                new SystemException("Some other, unknown exception!");

            IEntityAdapterClient adapter2 = this.CreateEntityAdapterClient(
                TimeSpan.Parse("00:00:02"),
                systemException);

            Dictionary<string, IEntityAdapterClient> adapters =
                new Dictionary<string, IEntityAdapterClient>()
                {
                    { mockAdapter2Id, adapter2 },
                };


            AdapterRecordReference[] adapterRecordReferences =
                new AdapterRecordReference[]
                {
                    new AdapterRecordReference()
                    {
                        Id = mockAdapter2RecordId,
                        Source = mockAdapter2Id,
                    },
                };

            EntityReference entityReference = new EntityReference()
            {
                AdapterRecordReferences = adapterRecordReferences,
            };

            CancellationToken cancellationToken = CancellationToken.None;

            Func<EntityAdapterClientKey, CancellationToken, IEntityAdapterClient> getAsyncCallback =
                (x, y) =>
                {
                    IEntityAdapterClient entityAdapterClient =
                        adapters[x.Name];

                    return entityAdapterClient;
                };

            this.mockEntityAdapterClientManager
                .Setup(x => x.GetAsync(It.IsAny<EntityAdapterClientKey>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getAsyncCallback);

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.entityAdapterInvoker.InvokeEntityAdaptersAsync(
                        algorithm,
                        entityName,
                        fields,
                        entityReference,
                        cancellationToken);
                };

            // Assert
            Assert.ThrowsAsync<AggregateException>(asyncTestDelegate);

            string logOutput = this.loggerWrapper.ReturnLog();
        }

        private IEntityAdapterClient CreateEntityAdapterClient(
            TimeSpan delay,
            Exception toThrow = null)
        {
            IEntityAdapterClient toReturn = null;

            Mock<IEntityAdapterClient> mockEntityAdapterClient =
                new Mock<IEntityAdapterClient>();

            Func<Task<Spi.Models.ModelsBase>> getEntityAsyncCallback =
                () =>
                {
                    Task<Spi.Models.ModelsBase> taskToReturn =
                        this.FakeTaskCreator(delay, toThrow);

                    return taskToReturn;
                };

            mockEntityAdapterClient
                .Setup(x => x.GetEntityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(getEntityAsyncCallback);

            toReturn = mockEntityAdapterClient.Object;

            return toReturn;
        }

        private async Task<Spi.Models.ModelsBase> FakeTaskCreator(
            TimeSpan delay,
            Exception toThrow)
        {
            Spi.Models.ModelsBase toReturn = null;

            await Task.Delay(delay);

            if (toThrow != null)
            {
                throw toThrow;
            }

            toReturn = new LearningProvider();

            return toReturn;
        }
    }
}