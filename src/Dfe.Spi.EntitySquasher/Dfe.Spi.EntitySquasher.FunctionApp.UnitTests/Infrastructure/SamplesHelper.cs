namespace Dfe.Spi.EntitySquasher.FunctionApp.UnitTests.Infrastructure
{
    using System;
    using System.IO;
    using System.Reflection;

    public static class SamplesHelper
    {
        private static Assembly assembly;
        private static string samplesNamespace;

        static SamplesHelper()
        {
            Type type = typeof(SamplesHelper);

            assembly = type.Assembly;

            // There's never a good way to get programatically, the root
            // namespace of a unit test project...
            samplesNamespace = type.FullName;
            samplesNamespace = samplesNamespace.Replace(
                nameof(Infrastructure),
                "Samples");
            samplesNamespace = samplesNamespace.Replace(
                type.Name,
                "{0}");
        }

        public static string GetSample(string name)
        {
            string toReturn = null;

            // Fully qualify it and...
            name = string.Format(samplesNamespace, name);

            Stream stream = assembly.GetManifestResourceStream(name);

            using (StreamReader streamReader = new StreamReader(stream))
            {
                toReturn = streamReader.ReadToEnd();
            }

            return toReturn;
        }
    }
}
