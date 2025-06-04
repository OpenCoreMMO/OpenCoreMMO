using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Items.Items.Attributes;

public class Chargeable : IChargeable
{
    public Chargeable(ushort charges, bool showCharges)
    {
        Charges = charges;
        ShowCharges = showCharges;
    }

    public ushort Charges { get; private set; }
    public bool ShowCharges { get; }
    public bool NoCharges => Charges == 0;

    public void DecreaseCharges()
    {
        Charges -= (ushort)(Charges == 0 ? 0 : 1);
    }

    public override string ToString()
    {
        return $"has {(Charges > 0 ? Charges : "no")} charge{(Charges == 1 ? string.Empty : "s")} left";
    }
}