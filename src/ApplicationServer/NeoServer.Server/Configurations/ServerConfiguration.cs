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
public record ServerConfiguration(
    int Version,
    string OTBM,
    string OTB,
    string Data,
    string ServerName,
    string ServerIp,
    string Extensions,
    int ServerLoginPort,
    int ServerGamePort,
    SaveConfiguration Save,
    string DataLuaJit)
{
}

public record LogConfiguration(string MinimumLevel);

public record SaveConfiguration(uint Players);

public record DatabaseConfiguration(Dictionary<DatabaseType, string> Connections, DatabaseType Active);

[SuppressMessage("ReSharper", "InconsistentNaming")]
public record GrayLogConfiguration(
    string HostnameOrAddress,
    int Port,
    string HostnameOverride,
    string Facility)
{
}

public record ClientConfiguration(ClientConfiguration.OtcV8Configuration OtcV8)
{
    public record OtcV8Configuration(
        bool GameExtendedOpcode,
        bool GameEnvironmentEffect,
        bool GameExtendedClientPing,
        bool GameItemTooltip);
}