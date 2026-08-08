using System.Reflection;

namespace API.Template.Core.Extensions
{
    public static class AppAssemblyScanner
    {
        /// <summary>
        /// Discovers all assemblies matching the given pattern
        /// </summary>
        public static IEnumerable<Assembly> DiscoverAssemblies(string pattern)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var assemblyNames = Directory
                .GetFiles(baseDirectory, $"{pattern}*.dll")
                .Select(Path.GetFileNameWithoutExtension)
                .Distinct();

            var loadedAssemblies = new List<Assembly>();

            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    loadedAssemblies.Add(Assembly.Load(assemblyName));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load: {assemblyName}: {ex.Message}");
                }
            }

            return loadedAssemblies;
        }
    }
}