namespace Dfe.Spi.EntitySquasher.AcdfGen
{
    using System.Diagnostics.CodeAnalysis;
    using StructureMap.Graph;

    /// <summary>
    /// Custom, host-specific implementation of
    /// <see cref="StructureMap.Registry" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Registry : StructureMap.Registry
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Registry" /> class.
        /// </summary>
        public Registry()
        {
            this.Scan(DoScan);
        }

        private static void DoScan(IAssemblyScanner assemblyScanner)
        {
            // Always create concrete instances based on usual DI naming
            // convention
            // i.e. Search for class name "Concrete" when "IConcrete" is
            //      requested.
            assemblyScanner.WithDefaultConventions();

            // Scan all assemblies, including the one executing.
            assemblyScanner
                .AssembliesAndExecutablesFromApplicationBaseDirectory();
        }
    }
}