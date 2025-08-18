namespace NeoServer.Domain.Common;

public enum ReloadType : byte
{
    None,
    All,
    Chat,
    Config,
    Events,
    Core,
    Items,
    Modules,
    Monsters,
    Mounts,
    Npcs,
    Raids,
    Scripts,
    Groups
}