using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface IGroup
{
    byte Id { get; set; }
    string Name { get; set; }
    bool Access { get; set; }
    int MaxDepotItems { get; set; }
    int MaxVipEntries { get; set; }
    Dictionary<PlayerFlag, bool> Flags { get; }

    bool FlagIsEnabled(PlayerFlag flag)
    {
        return Flags.TryGetValue(flag, out var value) ? value : false;
    }

    void EnableFlag(PlayerFlag flag)
    {
        Flags.AddOrUpdate(flag, true);
    }

    void DisableFlag(PlayerFlag flag)
    {
        Flags.AddOrUpdate(flag, false);
    }
}