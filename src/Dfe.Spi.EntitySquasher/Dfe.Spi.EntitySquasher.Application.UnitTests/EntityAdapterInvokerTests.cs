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
                "someFieldOne",
                "someFieldTwo",
            };
            AdapterRecordReference[] adapterRecordReferences =
                new AdapterRecordReference[]
                {
                    new AdapterRecordReference()
                    {
                        Id = "1",
                        Source = "adapter-one",
                    },
                    new AdapterRecordReference()
                    {
                        Id = "2",
                        Source = "adapter-two",
                    },
                    new AdapterRecordReference()
                    {
                        Id = "3",
                        Source = "adapter-three",
                    },
                };

            EntityReference entityReference = new EntityReference()
            {
                AdapterRecordReferences = adapterRecordReferences,
            };

            Mock<IEntityAdapterClient> mockEntityAdapterClient =
                new Mock<IEntityAdapterClient>();

            IEntityAdapterClient entityAdapterClient =
                mockEntityAdapterClient.Object;

            this.mockEntityAdapterClientManager
                .Setup(x => x.GetAsync(It.IsAny<EntityAdapterClientKey>()))
                .ReturnsAsync(entityAdapterClient);

            // Act
            InvokeEntityAdaptersResult invokeEntityAdaptersResult =
                await this.entityAdapterInvoker.InvokeEntityAdapters(
                    algorithm,
                    entityName,
                    fields,
                    entityReference);

            // Assert

        }
    }
}