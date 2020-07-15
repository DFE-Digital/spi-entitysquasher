using CommandLine;

namespace Dfe.Spi.EntitySquasher.ProfileGenerator
{
    public class CommandLineOptions
    {
        [Option('n', "file-name", Required = true, HelpText = "Name of the file to export profiles to")]
        public string FileName { get; set; }
    }
}