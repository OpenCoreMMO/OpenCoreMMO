using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Items.Cumulatives;

namespace NeoServer.Domain.Items.Items.UsableItems.Runes;

public class Rune : Cumulative, IHasCooldown, IUsableRequirement
{
    public Rune(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) : base(type,
        location, attributes)
    {
    }

    public Rune(IItemType type, Location location, byte amount) : base(type, location, amount)
    {
    }
    
    public string Name => Metadata.Name;
    public bool CheckFloor => Metadata.Attributes.GetAttribute<bool>(ItemAttribute.CheckFloor);
    public bool BlockWalls => Metadata.Attributes.GetAttribute<bool>(ItemAttribute.BlockWalls);
    public ushort ManaConsumption => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.ManaUse);
    public ushort SoulConsumption => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.SoulUse);
    public bool NeedDirection => Metadata.Attributes.GetAttribute<bool>(ItemAttribute.NeedDirection);

    public bool CasterNeedsTargetOrDirection =>
        Metadata.Attributes.GetAttribute<bool>(ItemAttribute.CasterNeedsTargetOrDirection);

    public bool NeedsTarget => Metadata.Attributes.GetAttribute<bool>(ItemAttribute.NeedTarget);

    public byte? Range => Metadata.Attributes.HasAttribute(ItemAttribute.Range)
        ? Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Range)
        : null;

    public bool SelfTarget => Metadata.Attributes.GetAttribute<bool>(ItemAttribute.SelfTarget);

    public bool IsAggressive => !Metadata.Attributes.HasAttribute(ItemAttribute.IsAggressive) ||
                                Metadata.Attributes.GetAttribute<bool>(ItemAttribute.IsAggressive);

    public bool BlockingCreature => Metadata.Attributes.HasAttribute(ItemAttribute.Blocking) &&
                                    !Metadata.Attributes.GetAttribute<bool>(ItemAttribute.Blocking);

    public bool BlockingSolid => Metadata.Attributes.HasAttribute(ItemAttribute.Blocking) &&
                                 Metadata.Attributes.GetAttribute<bool>(ItemAttribute.Blocking);

    public bool NeedWeapon => Metadata.Attributes.GetAttribute<bool>(ItemAttribute.NeedWeapon);
    public bool NeedLearn => Metadata.Attributes.GetAttribute<bool>(ItemAttribute.NeedLearn);
    public byte[] VocationIds { get; set; }
    public bool NeedsPremium => Metadata.Attributes.GetAttribute<bool>(ItemAttribute.NeedsPremium);

    public ISpell Spell => Metadata.Attributes.GetAttribute<ISpell>("spell");
    public ushort MinLevel => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.MinimumLevel);
    public ushort MinMagicLevel => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.MinimumMagicLevel);
    public bool Enabled => true;

    //Cooldown
    public Guid CooldownId
    {
        get
        {
            if (Metadata.Attributes.HasAttribute(ItemAttribute.CooldownId))
                return Metadata.Attributes.GetAttribute<Guid>(ItemAttribute.CooldownId);

            var id = Guid.NewGuid();
            Metadata.Attributes.SetAttribute(ItemAttribute.CooldownId, id);

            return id;
        }
    }

    public (int Id, uint Cooldown) PrimaryGroup =>
        (Metadata.Attributes.GetAttribute<int>(ItemAttribute.PrimaryGroup),
            Metadata.Attributes.GetAttribute<uint>(ItemAttribute.PrimaryGroupCooldown));

    public (int Id, uint Cooldown) SecondaryGroup =>
        (Metadata.Attributes.GetAttribute<int>(ItemAttribute.SecondaryGroup),
            Metadata.Attributes.GetAttribute<uint>(ItemAttribute.SecondaryGroupCooldown));

    public uint Cooldown => Metadata.Attributes.GetAttribute<uint>(ItemAttribute.CooldownTime);

    public static bool IsApplicable(IItemType type)
    {
        return type.Attributes.GetAttribute(ItemAttribute.Type)
            ?.Equals("rune", StringComparison.InvariantCultureIgnoreCase) ?? false;
    }

    public Result CanBeCastBy(ICombatActor caster, IThing target)
    {
        if (caster is IPlayer player)
        {
            if (player.Group.FlagIsEnabled(PlayerFlag.CannotUseSpells))
                return Result.Fail(InvalidOperation.CannotUseThisObject);

            if (!player.Group.FlagIsEnabled(PlayerFlag.HasNoExhaustion) && !player.CooldownHasExpired(this))
                return Result.Fail(InvalidOperation.Exhausted);
        }

        var result = CanBeUsedOn(target.Location);

        return result;
    }

    public void PostUse(bool reduce = true)
    {
        if (reduce) Reduce();
    }

    private Result CanBeUsedOn(Location target)
    {
        if (target.X == 0xFFFF)
        {
            if (NeedsTarget) return Result.Fail(InvalidOperation.CanOnlyUseOnCreatures);

            if (!SelfTarget) return Result.Fail(InvalidOperation.NotEnoughRoom);
        }

        return Result.Success;
    }
}