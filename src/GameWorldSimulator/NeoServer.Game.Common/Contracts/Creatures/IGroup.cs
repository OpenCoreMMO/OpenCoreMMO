using System.Collections.Generic;
using NeoServer.Game.Common.Creatures.Players;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface IGroup
{
    byte Id { get; set; }
    string Name { get; set; }
    int Access { get; set; }
    int MaxDepotItems { get; set; }
    int Maxvipentries { get; set; }
    Dictionary<PlayerFlag, int> Flags { get; set; }
}