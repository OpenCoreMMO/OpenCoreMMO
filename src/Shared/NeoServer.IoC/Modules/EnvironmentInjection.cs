using dotenv.net;
using Microsoft.Extensions.DependencyInjection;

namespace NeoServer.Shared.IoC.Modules;

public static class EnvironmentInjection
{
    public static IServiceCollection AddEnvironments(this IServiceCollection builder)
    {
        var currentPath = AppContext.BaseDirectory;
        var dockerEnvironment = Environment.GetEnvironmentVariable("DOCKER");

        string envPath = "";

        if (!string.IsNullOrEmpty(dockerEnvironment) && bool.Parse(dockerEnvironment))
            envPath = Path.Combine(currentPath, "data/.env");
        else
            envPath = Path.Combine(currentPath, "..\\..\\..\\..\\..\\data\\.env");

        DotEnv.Load(new DotEnvOptions(envFilePaths: new List<string>() { envPath }, ignoreExceptions: false));

        return builder;
    }
}