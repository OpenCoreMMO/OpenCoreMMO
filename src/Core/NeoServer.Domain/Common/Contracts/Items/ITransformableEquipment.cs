namespace NeoServer.Domain.Common.Contracts.Items;

public delegate void TransformEquipment(IItemType before, IItemType now);

public interface ITransformableEquipment
{
    IItemType TransformEquipItem { get; }
    IItemType TransformDequipItem { get; }
    void TransformOnEquip();
    void TransformOnDequip();
    event TransformEquipment OnTransformed;
}