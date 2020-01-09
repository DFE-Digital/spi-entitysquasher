namespace Dfe.Spi.EntitySquasher.Application.UnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.UnitTesting.Infrastructure;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
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
        public async Task Unnamed()
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

            IEntityAdapterClient adapter1 = this.CreateEntityAdapterClient();
            IEntityAdapterClient adapter2 = this.CreateEntityAdapterClient();
            IEntityAdapterClient adapter3 = this.CreateEntityAdapterClient();

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
                        Id = "2890d784-900f-4861-a034-30e645bd57b5",
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

            // Act
            InvokeEntityAdaptersResult invokeEntityAdaptersResult =
                await this.entityAdapterInvoker.InvokeEntityAdapters(
                    algorithm,
                    entityName,
                    fields,
                    entityReference);

            // Assert

        }

        private IEntityAdapterClient CreateEntityAdapterClient()
        {
            IEntityAdapterClient toReturn = null;

            Mock<IEntityAdapterClient> mockEntityAdapterClient =
                new Mock<IEntityAdapterClient>();

            toReturn = mockEntityAdapterClient.Object;

            return toReturn;
        }
    }
}