namespace NeoServer.Domain.Common.Helpers;

public static class GameAssemblyCache
{
    public static Type[] Cache { get; private set; }

    public static void Load()
    {
        Cache = AppDomain.CurrentDomain
            .GetAssemblies()
            .AsParallel()
            .Where(assembly => !assembly.IsDynamic &&
                               !assembly.FullName.StartsWith("System.") &&
                               !assembly.FullName.StartsWith("Microsoft.") &&
                               !assembly.FullName.StartsWith("Windows.") &&
                               !assembly.FullName.StartsWith("mscorlib,") &&
                               !assembly.FullName.StartsWith("netstandard,") &&
                               !assembly.FullName.StartsWith("Serilog,") &&
                               !assembly.FullName.StartsWith("Autofac,") &&
                               !assembly.FullName.StartsWith("netstandard,"))
            .SelectMany(x => x.GetTypes())
            .ToArray();
    }
}