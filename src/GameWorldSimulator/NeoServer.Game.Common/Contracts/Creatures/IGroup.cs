using System.Collections.Generic;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Helpers;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface IGroup
{
    byte Id { get; set; }
    string Name { get; set; }
    int Access { get; set; }
    int MaxDepotItems { get; set; }
    int MaxVipEntries { get; set; }
    Dictionary<PlayerFlag, bool> Flags { get; }

    bool FlagIsEnabled(PlayerFlag flag) => Flags.TryGetValue(flag, out var value) ? value : false;

    void EnableFlag(PlayerFlag flag)
        => Flags.AddOrUpdate(flag, true);

    void DisableFlag(PlayerFlag flag)
        => Flags.AddOrUpdate(flag, false);
}