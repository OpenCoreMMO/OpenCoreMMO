using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures.Players;

namespace NeoServer.Game.Creatures.Group;

public class Group : IGroup
{
    public byte Id { get; set; }
    public string Name { get; set; }
    public int Access { get; set; }
    public int MaxDepotItems { get; set; }
    public int Maxvipentries { get; set; }
    public Dictionary<PlayerFlag, int> Flags { get; set; }
}