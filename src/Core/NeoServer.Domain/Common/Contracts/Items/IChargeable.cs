namespace NeoServer.Domain.Common.Contracts.Items;

public interface IChargeable
{
    ushort Charges { get; }
    bool NoCharges => Charges == 0;
    bool ShowCharges { get; }
    void DecreaseCharges();
}