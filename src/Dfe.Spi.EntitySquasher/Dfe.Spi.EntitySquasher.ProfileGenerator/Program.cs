using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Dfe.Spi.EntitySquasher.Domain.Profiles;
using Dfe.Spi.Models.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dfe.Spi.EntitySquasher.ProfileGenerator
{
    class Program
    {
        private static Logger _logger;

        static async Task Run(CommandLineOptions options, CancellationToken cancellationToken = default)
        {
            var entities = new[]
            {
                GetProfileForType(typeof(LearningProvider), new[] {"GIAS", "UKRLP"}),
                GetProfileForType(typeof(ManagementGroup), new[] {"GIAS"}),
                GetProfileForType(typeof(LearningProviderRates), new[] {"Rates"}),
                GetProfileForType(typeof(ManagementGroupRates), new[] {"Rates"}),
                GetProfileForType(typeof(Census), new[] {"IStore"}),
            };
            var profile = new Profile
            {
                Name = "default",
                Entities = entities,
            };

            await SaveAsync(profile, options.FileName, cancellationToken);
        }

        static EntityProfile GetProfileForType(Type type, string[] sources)
        {
            try
            {
                _logger.Info($"Building profile for {type.Name}");
                
                var properties = type.GetProperties()
                    .Where(p => !p.Name.Equals("_lineage", StringComparison.InvariantCultureIgnoreCase) &&
                                !p.Name.Equals("_aggregations", StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
                var fields = properties
                    .Select(p =>
                        new EntityFieldProfile
                        {
                            Name = p.Name,
                        })
                    .ToArray();
                _logger.Info($"{type.Name} has profile with {fields.Length} fields");
                
                return new EntityProfile
                {
                    Name = type.Name,
                    Sources = sources,
                    Fields = fields,
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error building profile for {type.FullName}: {ex.Message}", ex);
            }
        }

        static async Task SaveAsync(Profile profile, string fileName, CancellationToken cancellationToken)
        {
            var fileInfo = new FileInfo(fileName);
            _logger.Info($"Saving profile to {fileInfo.FullName}");

            var json = JsonConvert.SerializeObject(profile,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                });

            await using var stream = fileInfo.Open(FileMode.Create);
            await using var writer = new StreamWriter(stream);
            
            await writer.WriteAsync(json);
            await writer.FlushAsync();
            writer.Close();
        }


        static void Main(string[] args)
        {
            _logger = new Logger();

            CommandLineOptions options = null;
            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed((parsed) => options = parsed);
            if (options != null)
            {
                try
                {
                    Run(options).Wait();
                }
                catch (Exception ex)
                {
                    _logger.Error("Unexpected error producing file", ex);
                }
            }
        }
    }
}