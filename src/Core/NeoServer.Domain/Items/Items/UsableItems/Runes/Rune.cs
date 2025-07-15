using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Items.Items.Cumulatives;

namespace NeoServer.Domain.Items.Items.UsableItems.Runes;

public class Rune : Cumulative, IHasCooldown, IUsableRequirement
{
    public Rune(IItemType type, Location location, IDictionary<ItemTypeAttribute, IConvertible> attributes) : base(type,
        location, attributes)
    {
    }

    public Rune(IItemType type, Location location, byte amount) : base(type, location, amount)
    {
    }

    public bool CheckFloor => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.CheckFloor);
    public bool BlockWalls => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.BlockWalls);
    public ushort ManaConsumption => Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.ManaUse);
    public ushort SoulConsumption => Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.SoulUse);
    public bool NeedDirection => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.NeedDirection);

    public bool CasterNeedsTargetOrDirection =>
        Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.CasterNeedsTargetOrDirection);

    public bool NeedsTarget => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.NeedTarget);

    public bool SelfTarget => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.SelfTarget);

    public bool IsAggressive => !Metadata.Attributes.HasAttribute(ItemTypeAttribute.IsAggressive) ||
                                Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.IsAggressive);

    public bool BlockingCreature => Metadata.Attributes.HasAttribute(ItemTypeAttribute.Blocking) &&
                                    !Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.Blocking);

    public bool BlockingSolid => Metadata.Attributes.HasAttribute(ItemTypeAttribute.Blocking) &&
                                 Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.Blocking);

    public bool NeedWeapon => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.NeedWeapon);
    public bool NeedLearn => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.NeedLearn);
    public byte[] VocationIds { get; set; }
    public bool NeedsPremium => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.NeedsPremium);

    public ISpell Spell => Metadata.Attributes.GetCustomAttribute<ISpell>("spell");
    public bool Enabled => true;

    //Cooldown
    public Guid CooldownId
    {
        get
        {
            if (Metadata.Attributes.HasAttribute(ItemTypeAttribute.CooldownId))
                return Metadata.Attributes.GetAttribute<Guid>(ItemTypeAttribute.CooldownId);

            var id = Guid.NewGuid();
            Metadata.Attributes.SetAttribute(ItemTypeAttribute.CooldownId, id);

            return id;
        }
    }

    public (int Id, uint Cooldown) PrimaryGroup =>
        (Metadata.Attributes.GetAttribute<int>(ItemTypeAttribute.PrimaryGroup),
            Metadata.Attributes.GetAttribute<uint>(ItemTypeAttribute.PrimaryGroupCooldown));

    public (int Id, uint Cooldown) SecondaryGroup =>
        (Metadata.Attributes.GetAttribute<int>(ItemTypeAttribute.SecondaryGroup),
            Metadata.Attributes.GetAttribute<uint>(ItemTypeAttribute.SecondaryGroupCooldown));

    public uint Cooldown => Metadata.Attributes.GetAttribute<uint>(ItemTypeAttribute.CooldownTime);

    public ushort MinLevel => Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.MinimumLevel);
    public ushort MinMagicLevel => Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.MinimumMagicLevel);

    public static bool IsApplicable(IItemType type)
    {
        return type.Attributes.GetAttribute(ItemTypeAttribute.Type)
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