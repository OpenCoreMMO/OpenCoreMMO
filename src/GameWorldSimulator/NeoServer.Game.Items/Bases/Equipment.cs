using System;
using System.Linq;
using System.Text;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Factories.AttributeFactory;
using NeoServer.Game.Items.Items.Attributes;

namespace NeoServer.Game.Items.Bases;

public abstract class Equipment : BaseItem, IEquipment
{
    protected Equipment(IItemType type, Location location) : base(type, location)
    {
        if (type.Attributes.SkillBonuses?.Any() ?? false) SkillBonus = new SkillBonus(this);
        Protection = ProtectionFactory.Create(this);
    }

    public IProtection Protection { get; private set; }
    public ISkillBonus SkillBonus { get; private set; }
    public IChargeable Chargeable { get; init; }

    protected abstract string PartialInspectionText { get; }
    public Func<ushort, IItemType> ItemTypeFinder { get; init; }
    public IPlayer PlayerDressing { get; set; }

    public event Action<IEquipment> OnDressed;
    public event Action<IEquipment> OnUndressed;

    public string InspectionText
    {
        get
        {
            var stringBuilder = new StringBuilder();
            var attributeStringBuilder = new StringBuilder();

            void AppendAttributes(string attributes)
            {
                if (!string.IsNullOrWhiteSpace(attributes)) attributeStringBuilder.Append($", {attributes}");
            }

            AppendAttributes(PartialInspectionText);
            AppendAttributes($"{SkillBonus}");
            AppendAttributes($"{Protection}");

            if (attributeStringBuilder.Length > 0) stringBuilder.Append($"({attributeStringBuilder.Remove(0, 2)})");

            if (Decay is not null) stringBuilder.Append($" that {Decay}");
            if (Chargeable is not null && Chargeable.ShowCharges) stringBuilder.Append($" that {Chargeable}");
            return stringBuilder.ToString();
        }
    }

    public string CloseInspectionText => InspectionText;

    #region Protection

    public bool Protect(ref CombatDamage damage)
    {
        if (NoCharges) return false;
        var @protected = Protection?.Protect(ref damage) ?? false;
        if (@protected) DecreaseCharges();
        return true;
    }

    #endregion

    public abstract bool CanBeDressed(IPlayer player);
    public byte[] Vocations => Metadata.Attributes.GetRequiredVocations();
    public ushort MinLevel => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.MinimumLevel);

    private void OnPlayerAttackedHandler(IThing enemy, ICombatActor victim, ref CombatDamage damage)
    {
        Protect(ref damage);
    }

    #region Charges

    public ushort Charges => Chargeable?.Charges ?? 0;
    public bool NoCharges => Chargeable?.NoCharges ?? false;
    public bool ShowCharges => Chargeable?.ShowCharges ?? false;

    public void DecreaseCharges()
    {
        Chargeable?.DecreaseCharges();
    }

    #endregion

    #region Skill Bonus

    public void AddSkillBonus(IPlayer player)
    {
        if (Expired) return;
        SkillBonus?.AddSkillBonus(player);
    }

    public void RemoveSkillBonus(IPlayer player)
    {
        SkillBonus?.RemoveSkillBonus(player);
    }

    #endregion

    #region Dressable

    public void DressedIn(IPlayer player)
    {
        if (Guard.AnyNull(player)) return;
        TransformOnEquip();

        player.OnAttacked += OnPlayerAttackedHandler;
        PlayerDressing = player;
        AddSkillBonus(player);
        StartDecay();
        OnDressed?.Invoke(this);
        player.OnDressedItem(this);
    }

    public void UndressFrom(IPlayer player)
    {
        if (Guard.AnyNull(player)) return;

        RemoveSkillBonus(player);

        TransformOnDequip();

        player.OnAttacked -= OnPlayerAttackedHandler;
        PlayerDressing = null;
        PauseDecay();
        OnUndressed?.Invoke(this);
        player.OnUndressedItem(this);
    }

    #endregion

    #region Decay

    public bool Expired => Decay?.Expired ?? false;

    public void StartDecay()
    {
        if (Guard.AnyNull(Metadata)) return;
        if (Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.StopDecaying, out var stopDecaying) &&
            stopDecaying == 1) return;
        Decay?.StartDecay();
    }

    public void PauseDecay()
    {
        if (Metadata is null) return;

        var hasStopDecaying =
            Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.StopDecaying, out var stopDecaying);
        if (!hasStopDecaying || stopDecaying == 0) return;

        Decay?.PauseDecay();
    }

    #endregion

    #region Transformable

    public void TransformOnEquip()
    {
        if (!Metadata.Attributes.HasAttribute(ItemAttribute.TransformEquipTo)) return;

        var before = Metadata;
        UpdateMetadata(TransformEquipItem);

        if (Metadata.Attributes.SkillBonuses is not null) SkillBonus ??= new SkillBonus(this);
        Decay ??= DecayableFactory.CreateIfItemIsDecayable(this);
        Protection ??= ProtectionFactory.Create(this);

        OnTransformed?.Invoke(before, Metadata);
    }

    public void TransformOnDequip()
    {
        if (!Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.TransformDequipTo, out _)) return;

        var before = Metadata;
        UpdateMetadata(TransformDequipItem);

        OnTransformed?.Invoke(before, Metadata);
    }

    public IItemType TransformEquipItem =>
        ItemTypeFinder?.Invoke(Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.TransformEquipTo));

    public IItemType TransformDequipItem =>
        ItemTypeFinder?.Invoke(Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.TransformDequipTo));

    public event TransformEquipment OnTransformed;

    #endregion
}