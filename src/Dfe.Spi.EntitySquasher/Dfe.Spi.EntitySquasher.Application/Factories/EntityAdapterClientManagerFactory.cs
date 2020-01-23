﻿namespace Dfe.Spi.EntitySquasher.Application.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClientManagerFactory" />.
    /// </summary>
    public class EntityAdapterClientManagerFactory
        : IEntityAdapterClientManagerFactory
    {
        private readonly IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager;
        private readonly IEntityAdapterClientCache entityAdapterClientCache;
        private readonly IEntityAdapterClientFactory entityAdapterClientFactory;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterClientManagerFactory" /> class.
        /// </summary>
        /// <param name="algorithmConfigurationDeclarationFileManagerFactory">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileManagerFactory" />.
        /// </param>
        /// <param name="entityAdapterClientCache">
        /// An instance of type <see cref="IEntityAdapterClientCache" />.
        /// </param>
        /// <param name="entityAdapterClientFactory">
        /// An instance of type <see cref="IEntityAdapterClientFactory" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public EntityAdapterClientManagerFactory(
            IAlgorithmConfigurationDeclarationFileManagerFactory algorithmConfigurationDeclarationFileManagerFactory,
            IEntityAdapterClientCache entityAdapterClientCache,
            IEntityAdapterClientFactory entityAdapterClientFactory,
            ILoggerWrapper loggerWrapper)
        {
            if (algorithmConfigurationDeclarationFileManagerFactory == null)
            {
                throw new ArgumentNullException(
                    nameof(algorithmConfigurationDeclarationFileManagerFactory));
            }

            this.entityAdapterClientCache = entityAdapterClientCache;
            this.loggerWrapper = loggerWrapper;
            this.entityAdapterClientFactory = entityAdapterClientFactory;

            this.algorithmConfigurationDeclarationFileManager =
                algorithmConfigurationDeclarationFileManagerFactory.Create();
        }

        /// <inheritdoc />
        public IEntityAdapterClientManager Create()
        {
            EntityAdapterClientManager toReturn =
                new EntityAdapterClientManager(
                    this.entityAdapterClientCache,
                    this.loggerWrapper,
                    this.InitialiseCacheItemAsync);

            return toReturn;
        }

        /// <inheritdoc />
        public async Task<IEntityAdapterClient> InitialiseCacheItemAsync(
            EntityAdapterClientKey entityAdapterClientKey,
            CancellationToken cancellationToken)
        {
            IEntityAdapterClient toReturn = null;

            if (entityAdapterClientKey == null)
            {
                throw new ArgumentNullException(
                    nameof(entityAdapterClientKey));
            }

            string algorithm = entityAdapterClientKey.Algorithm;
            string name = entityAdapterClientKey.Name;

            // 1) Pull back the ACDF from the manager and;
            this.loggerWrapper.Debug(
                $"Pulling back " +
                $"{nameof(AlgorithmConfigurationDeclarationFile)} for " +
                $"algorithm \"{algorithm}\" from the " +
                $"{nameof(IAlgorithmConfigurationDeclarationFileManager)}...");

            // algorithmConfigurationDeclarationFile will always get populated
            // here, or throw an exception back up (FileNotFound).
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                await this.algorithmConfigurationDeclarationFileManager.GetAsync(
                    algorithm,
                    cancellationToken)
                    .ConfigureAwait(false);

            // 2) Pull back the base URL of the right
            //    adapter using the name and;
            this.loggerWrapper.Info(
                $"Pulled back " +
                $"{algorithmConfigurationDeclarationFile}. " +
                $"Searching for {nameof(EntityAdapter)} with " +
                $"{nameof(name)} \"{name}\"...");

            EntityAdapter entityAdapter =
                algorithmConfigurationDeclarationFile.EntityAdapters
                    .SingleOrDefault(x => x.Name == name);

            // 2) Create the EntityAdapterClient via the
            //    factory and return it.
            if (entityAdapter != null)
            {
                this.loggerWrapper.Info(
                    $"Found matching {nameof(EntityAdapter)}: " +
                    $"{entityAdapter}.");

                Uri baseUrl = entityAdapter.BaseUrl;

                this.loggerWrapper.Debug($"{nameof(baseUrl)} = {baseUrl}");

                Dictionary<string, string> headers = entityAdapter.Headers;

                this.loggerWrapper.Debug(
                    $"{nameof(headers)} = {headers.Count} item(s)");

                toReturn = this.entityAdapterClientFactory.Create(
                    name,
                    baseUrl,
                    headers);

                this.loggerWrapper.Info(
                    $"Created {nameof(IEntityAdapterClient)} with " +
                    $"{nameof(baseUrl)} = {baseUrl}.");
            }
            else
            {
                throw new InvalidAlgorithmConfigurationDeclarationFileException(
                    $"Found {algorithmConfigurationDeclarationFile} for " +
                    $"{nameof(algorithm)} = \"{algorithm}\", but could not " +
                    $"find {nameof(EntityAdapter)} with {nameof(name)} = " +
                    $"\"{name}\"!");
            }

            return toReturn;
        }
    }
}