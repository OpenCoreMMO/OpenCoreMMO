namespace NeoServer.Domain.Items.Items.Attributes;

public class ChargeCounter
    (ushort amount, bool showAmount)
{
    public ushort Amount { get; private set; } = amount;
    public bool ShowAmount { get; } = showAmount;
    public bool IsEmpty => Amount == 0;

    public void DecreaseAmount() => Amount -= (ushort)(Amount == 0 ? 0 : 1);

    public override string ToString() => $"has {(Amount > 0 ? Amount : "no")} charge{(Amount == 1 ? string.Empty : "s")} left";
}