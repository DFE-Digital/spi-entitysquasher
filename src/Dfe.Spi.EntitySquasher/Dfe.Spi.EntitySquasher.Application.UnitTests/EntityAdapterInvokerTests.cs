﻿namespace Dfe.Spi.EntitySquasher.Application.UnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
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

            this.loggerWrapper = new LoggerWrapper();

            this.entityAdapterInvoker = new EntityAdapterInvoker(
                entityAdapterClientManager,
                this.loggerWrapper);
        }

        [Test]
        public void InvokeEntityAdapters_PostWithoutEntityReference_ThrowsArgumentNullException()
        {
            // Arrange
            string algorithm = null;
            string entityName = null;
            string[] fields = null;
            EntityReference entityReference = null;

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.entityAdapterInvoker.InvokeEntityAdapters(
                        algorithm,
                        entityName,
                        fields,
                        entityReference);
                };

            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(asyncTestDelegate);
        }

        [Test]
        public async Task InvokeEntityAdapters_2AdaptersSucceed1Fails_OutputIsAsExpected()
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
            IEntityAdapterClient adapter2 = this.CreateEntityAdapterClient(
                TimeSpan.Parse("00:00:05"),
                expectedEntityAdapterErrorDetail);
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

            Func<EntityAdapterClientKey, IEntityAdapterClient> getAsyncCallback =
                entityAdapterClientKey =>
                {
                    IEntityAdapterClient entityAdapterClient =
                        adapters[entityAdapterClientKey.Name];

                    return entityAdapterClient;
                };

            this.mockEntityAdapterClientManager
                .Setup(x => x.GetAsync(It.IsAny<EntityAdapterClientKey>()))
                .ReturnsAsync(getAsyncCallback);

            int expectedSuccessfulTasks = 2;
            int actualSuccessfulTasks;

            GetEntityAsyncResult unsuccessfulResult = null;

            // Act
            InvokeEntityAdaptersResult invokeEntityAdaptersResult =
                await this.entityAdapterInvoker.InvokeEntityAdapters(
                    algorithm,
                    entityName,
                    fields,
                    entityReference);

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
        }

        [Test]
        public void InvokeEntityAdapters_OnlyAdapterFails_ThrowsAllAdaptersUnavailableException()
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

            IEntityAdapterClient adapter2 = this.CreateEntityAdapterClient(
                TimeSpan.Parse("00:00:02"),
                expectedEntityAdapterErrorDetail);

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

            Func<EntityAdapterClientKey, IEntityAdapterClient> getAsyncCallback =
                entityAdapterClientKey =>
                {
                    IEntityAdapterClient entityAdapterClient =
                        adapters[entityAdapterClientKey.Name];

                    return entityAdapterClient;
                };

            this.mockEntityAdapterClientManager
                .Setup(x => x.GetAsync(It.IsAny<EntityAdapterClientKey>()))
                .ReturnsAsync(getAsyncCallback);

            AsyncTestDelegate asyncTestDelegate =
                async () =>
                {
                    // Act
                    await this.entityAdapterInvoker.InvokeEntityAdapters(
                        algorithm,
                        entityName,
                        fields,
                        entityReference);
                };

            // Assert
            Assert.ThrowsAsync<AllAdaptersUnavailableException>(
                asyncTestDelegate);
        }


        private IEntityAdapterClient CreateEntityAdapterClient(
            TimeSpan delay,
            EntityAdapterErrorDetail entityAdapterErrorDetail = null)
        {
            IEntityAdapterClient toReturn = null;

            Mock<IEntityAdapterClient> mockEntityAdapterClient =
                new Mock<IEntityAdapterClient>();

            Func<Task<Spi.Models.ModelsBase>> getEntityAsyncCallback =
                () =>
                {
                    Task<Spi.Models.ModelsBase> taskToReturn =
                        this.FakeTaskCreator(delay, entityAdapterErrorDetail);

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
            EntityAdapterErrorDetail entityAdapterErrorDetail)
        {
            Spi.Models.ModelsBase toReturn = null;

            await Task.Delay(delay);

            if (entityAdapterErrorDetail != null)
            {
                throw new EntityAdapterException(
                    entityAdapterErrorDetail,
                    entityAdapterErrorDetail.HttpStatusCode,
                    entityAdapterErrorDetail.HttpErrorBody);
            }

            toReturn = new LearningProvider();

            return toReturn;
        }
    }
}