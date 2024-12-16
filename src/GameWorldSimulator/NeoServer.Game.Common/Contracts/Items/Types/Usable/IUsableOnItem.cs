using System;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World.Tiles;

namespace NeoServer.Game.Common.Contracts.Items.Types.Usable;

public delegate void UseOnTile(ICreature usedBy, IDynamicTile tile, IUsableOnTile item);

public interface IUsableOnItem : IUsableOn
{
    public static Func<IItem, ICreature, IItem, bool> UseFunction { get; set; }

    public bool Use(ICreature usedBy, IItem onItem)
    {
        return UseFunction?.Invoke(this, usedBy, onItem) ?? false;
    }

    bool CanUseOn(IItem onItem);
    bool CanUseOn(ushort[] items, IItem onItem);
}

public interface IUsableOnTile : IUsableOn
{
    static event UseOnTile OnUsedOnTile;

    /// <summary>
    ///     Usable by creatures on items (ground, weapon, stairs..)
    /// </summary>
    /// <param name="usedBy">player whose item is being used</param>
    /// <param name="tile">tile which will receive action</param>
    public bool Use(ICreature usedBy, ITile tile);
}

public interface IUsableAttackOnTile : IUsableOn
{
    /// <summary>
    ///     Usable by creatures on items (ground, weapon, stairs..)
    /// </summary>
    /// <param name="usedBy">player whose item is being used</param>
    /// <param name="tile">tile which will receive action</param>
    /// <param name="combat"></param>
    public bool Use(ICreature usedBy, ITile tile, out CombatAttackResult combat);
}