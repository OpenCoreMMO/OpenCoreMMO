using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Parsers;

namespace NeoServer.Domain.World.Algorithms.AStar;

internal class Node(ushort x, ushort y)
{
    public int F { get; set; }
    public ushort X { get; } = x;
    public ushort Y { get; } = y;
    public Node Parent { get; set; }
    public int Heuristic { get; init; }
    public byte ExtraCost { get; init; }
    public int Weight => F + Heuristic;

    public bool IsOpen { get; private set; } = true;

    public void Close()
    {
        IsOpen = false;
    }

    public void Open()
    {
        IsOpen = true;
    }

    public int GetMapWalkCost(Location neighborPos)
    {
        return (Math.Abs(X - neighborPos.X) + Math.Abs(Y - neighborPos.Y) - 1) * 25 + 10;
    }

    public static int GetTileWalkCost(ICreature creature, IDynamicTile tile)
    {
        var cost = 0;

        if (tile.GetTopVisibleCreature(creature) != null) cost += 10 * 4;

        if (tile.MagicField != null && creature is IMonster monster && tile.MagicField.DamageType != DamageType.None)
        {
            if (!monster.IsImmune(tile.MagicField.DamageType) &&
                !monster.HasCondition(tile.MagicField.DamageType.ToCondition()))
            {
                cost += 10 * 18;
            }
        }

        return cost;
    }
}