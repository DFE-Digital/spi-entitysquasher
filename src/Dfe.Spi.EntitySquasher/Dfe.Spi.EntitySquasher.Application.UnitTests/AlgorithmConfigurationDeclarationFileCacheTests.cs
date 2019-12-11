namespace Dfe.Spi.EntitySquasher.Application.UnitTests
{
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;
    using NUnit.Framework;

    [TestFixture]
    public class AlgorithmConfigurationDeclarationFileCacheTests
    {
        private AlgorithmConfigurationDeclarationFileCache algorithmConfigurationDeclarationFileCache;

        [SetUp]
        public void Arrange()
        {
            this.algorithmConfigurationDeclarationFileCache =
                new AlgorithmConfigurationDeclarationFileCache();
        }

        [Test]
        public void StoreAcdfAndGetAcdf_ReturnsOriginallyStoredAcdf()
        {
            // Arrange
            AlgorithmConfigurationDeclarationFile expectedAlgorithmConfigurationDeclarationFile =
                new AlgorithmConfigurationDeclarationFile()
                {
                    // Doesn't need anything - can be empty.
                };

            AlgorithmConfigurationDeclarationFile actualAlgorithmConfigurationDeclarationFile = null;
            string algorithm = "test";

            // Act
            // 1) Store the ACDF...
            this.algorithmConfigurationDeclarationFileCache.AddAlgorithmConfigurationDeclarationFile(
                algorithm,
                expectedAlgorithmConfigurationDeclarationFile);

            // 2) Then get what we put in...
            actualAlgorithmConfigurationDeclarationFile = this.algorithmConfigurationDeclarationFileCache.GetAlgorithmConfigurationDeclarationFile(
                algorithm);

            // Assert
            Assert.AreEqual(
                expectedAlgorithmConfigurationDeclarationFile,
                actualAlgorithmConfigurationDeclarationFile);
        }
    }
}