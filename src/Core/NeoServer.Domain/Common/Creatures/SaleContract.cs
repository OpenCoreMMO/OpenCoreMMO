namespace NeoServer.Domain.Common.Creatures;

public readonly ref struct SaleContract
{
    public SaleContract(ushort typeId, byte amount, uint possibleAmountOnInventory, uint possibleAmountOnBackpack)
    {
        TypeId = typeId;
        Amount = amount;
        PossibleAmountOnInventory = possibleAmountOnInventory;
        PossibleAmountOnBackpack = possibleAmountOnBackpack;
    }

    public ushort TypeId { get; }
    public byte Amount { get; }
    public uint PossibleAmountOnInventory { get; }
    public uint PossibleAmountOnBackpack { get; }
}