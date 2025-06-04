using System.Reflection;
using System.Runtime.Loader;

namespace NeoServer.Web.API.Helpers;

public static class AssemblyHelper
{
    #region constants

    private const string ASSEMBLY_TYPE = "*.dll";

    #endregion

    #region pivate members

    private static List<Assembly> _assemblies;

    #endregion

    #region public methods implementations

    public static List<Assembly> GetAllAssemblies()
    {
        if (_assemblies != null)
            return _assemblies;

        _assemblies = new List<Assembly>();

        foreach (var assemblyPath in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, ASSEMBLY_TYPE,
                     SearchOption.TopDirectoryOnly))
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);

            if (_assemblies.Find(a => a == assembly) != null)
                continue;

            _assemblies.Add(assembly);
        }

        if (_assemblies.Count == 0) return _assemblies;

        var name = Assembly.GetExecutingAssembly().GetName().Name;
        if (name == null) return _assemblies;

        var assemblyName = name.Split('.').FirstOrDefault();
        _assemblies = _assemblies
            .Where(c => assemblyName != null && c.FullName != null && c.FullName.Contains(assemblyName)).ToList();

        return _assemblies;
    }

    #endregion
}