using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NeoServer.Server.Configurations;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum DatabaseType
{
    INMEMORY,
    POSTGRESQL,
    SQLITE
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public record ServerConfiguration(int Version, string OTBM, string OTB, string Data, string ServerName, string ServerIp,
    string Extensions, SaveConfiguration Save);

public record LogConfiguration(string MinimumLevel);

public record SaveConfiguration(uint Players);

public record DatabaseConfiguration(Dictionary<DatabaseType, string> Connections, DatabaseType Active);