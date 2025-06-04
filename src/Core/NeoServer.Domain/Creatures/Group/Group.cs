using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Creatures.Group;

public class Group
{
    public byte Id { get; set; }
    public string Name { get; set; }
    public bool Access { get; set; }
    public int MaxDepotItems { get; set; }
    public int MaxVipEntries { get; set; }
    public Dictionary<PlayerFlag, bool> Flags { get; set; } = new();
    
    public bool FlagIsEnabled(PlayerFlag flag) => Flags.GetValueOrDefault(flag, false);
    public void EnableFlag(PlayerFlag flag) => Flags.AddOrUpdate(flag, true);
    public void DisableFlag(PlayerFlag flag) => Flags.AddOrUpdate(flag, false);
}