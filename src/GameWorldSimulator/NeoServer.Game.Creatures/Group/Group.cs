using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures.Players;

namespace NeoServer.Game.Creatures.Group;

public class Group : IGroup
{
    public byte Id { get; set; }
    public string Name { get; set; }
    public bool Access { get; set; }
    public int MaxDepotItems { get; set; }
    public int MaxVipEntries { get; set; }
    public Dictionary<PlayerFlag, bool> Flags { get; set; } = new Dictionary<PlayerFlag, bool>();
}