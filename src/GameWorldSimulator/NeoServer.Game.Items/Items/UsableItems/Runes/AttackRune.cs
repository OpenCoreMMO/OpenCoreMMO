using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Effects.Magical;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Game.Items.Items.UsableItems.Runes;

public class AttackRune : Rune, IAttackRune
{
    public AttackRune(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) :
        base(type, location, attributes)
    {
    }

    public Func<string, byte[,]> GetAreaTypeFunc { get; init; }
    public override ushort Duration => 2;
    public bool HasNoInjureEffect => Metadata.Attributes.HasAttribute("hasnoinjureEffect");
    public string Area => Metadata.Attributes.GetAttribute(ItemAttribute.Area);
    public virtual ShootType ShootType => Metadata.ShootType;
    public virtual DamageType DamageType => Metadata.DamageType;
    public virtual EffectT Effect => Metadata.EffectT;

    public virtual bool NeedTarget => Metadata.Attributes.GetAttribute<bool>(ItemAttribute.NeedTarget);
    public virtual MagicGroup[] Groups { get; }

    public virtual bool Use(ICreature usedBy, ICreature creature, out CombatAttackResult combatAttackResult)
    {
        if (NeedTarget == false) return AttackArea(usedBy, creature.Tile, out combatAttackResult);

        combatAttackResult = CombatAttackResult.None;

        if (creature is not ICombatActor enemy) return false;
        if (usedBy is not IPlayer player) return false;

        var minMaxDamage = Formula(player, player.Level, player.GetSkillLevel(SkillType.Magic));
        var damage = (ushort)GameRandom.Random.Next(minMaxDamage.Min, maxValue: minMaxDamage.Max);

        if (enemy.TakeDamage(player, new CombatDamage(damage, DamageType, HasNoInjureEffect)))
        {
            combatAttackResult.ShootType = ShootType;
            combatAttackResult.DamageType = DamageType;
            combatAttackResult.EffectT = Effect;

            Reduce();
            return true;
        }

        return false;
    }

    public virtual bool Use(ICreature usedBy, ITile tile, out CombatAttackResult combatAttackResult)
    {
        return AttackArea(usedBy, tile, out combatAttackResult);
    }

    private bool AttackArea(ICreature usedBy, ITile tile, out CombatAttackResult combatAttackResult)
    {
        combatAttackResult = CombatAttackResult.None;

        if (NeedTarget)
        {
            if (tile is IDynamicTile { HasCreature: true } t)
                return Use(usedBy, t.TopCreatureOnStack, out combatAttackResult);
            return false;
        }

        if (usedBy is not IPlayer player) return false;

        var minMaxDamage = Formula(player, player.Level, player.GetSkillLevel(SkillType.Magic));
        var damage = (ushort)GameRandom.Random.Next(minMaxDamage.Min, maxValue: minMaxDamage.Max);

        combatAttackResult.DamageType = DamageType;

        var template = GetAreaTypeFunc?.Invoke(Area);
        combatAttackResult.SetArea(AreaEffect.Create(tile.Location, template));

        combatAttackResult.EffectT = Effect;

        player.PropagateAttack(combatAttackResult.Area, new CombatDamage(damage, DamageType, HasNoInjureEffect));

        Reduce();
        return true;
    }

    public new static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.AttackRune;
    }
}