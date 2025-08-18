using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.Items.Types;

public delegate void ItemReduce(ICumulative item, byte amount);

public interface ICumulative : IItem
{
    byte AmountToComplete { get; }
    event ItemReduce OnReduced;

    bool TryJoin(ref ICumulative item);
    float CalculateWeight(byte amount);
    ICumulative Clone(byte amount);
    ICumulative Split(byte amount);
    void ClearSubscribers();
    void SetAmount(byte count);

    public static bool IsApplicable(IItemType type)
    {
        return type.Flags.Contains(ItemFlag.Stackable);
    }

    /// <summary>
    ///     Reduce amount from item
    /// </summary>
    /// <param name="amount">Amount to be reduced</param>
    void Reduce(byte amount = 1);
}