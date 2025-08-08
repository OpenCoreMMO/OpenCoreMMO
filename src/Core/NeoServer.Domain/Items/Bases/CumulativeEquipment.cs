using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Items.Bases;

//todo: code duplicated from cumulative class
public abstract class CumulativeEquipment : Equipment, ICumulative
{
    protected CumulativeEquipment(IItemType type, ocation location) : base(type, location)
    {
    }

    public event ItemReduce OnReduced;

    public new float Weight => CalculateWeight(Amount);

    public float CalculateWeight(byte amount)
    {
        return Metadata.Weight * amount;
    }

    public Span<byte> GetRaw()
    {
        Span<byte> cache = stackalloc byte[3];
        var idBytes = BitConverter.GetBytes(Metadata.ClientId);

        cache[0] = idBytes[0];
        cache[1] = idBytes[1];
        cache[2] = Amount;

        return cache.ToArray();
    }

    public ICumulative Clone(byte amount)
    {
        var clone = (ICumulative)MemberwiseClone();
        clone.SetAmount(amount);
        clone.ClearSubscribers();
        return clone;
    }

    public void ClearSubscribers()
    {
        OnReduced = null;
    }

    /// <summary>
    ///     Split item in two parts
    /// </summary>
    /// <param name="amount">Amount to be reduced</param>
    public ICumulative Split(byte amount)
    {
        if (amount == Amount)
        {
            ClearSubscribers();
            return this;
        }

        if (TryReduce(amount) is false) return null;
        return Clone(amount);
    }

    public byte AmountToComplete => (byte)(100 - Amount);

    public bool TryJoin(ref ICumulative item)
    {
        if (item?.Metadata?.ClientId is null) return false;
        if (item.Metadata.ClientId != Metadata.ClientId) return false;

        var totalAmount = Amount + item.Amount;

        if (totalAmount <= 100)
        {
            SetAmount((byte)totalAmount);
            item = null;
            return true;
        }

        item.SetAmount((byte)(totalAmount - Amount));

        return true;
    }

    /// <summary>
    ///     Reduce amount from item
    /// </summary>
    /// <param name="amount">Amount to be reduced</param>
    public void Reduce(byte amount = 1)
    {
        if (TryReduce(amount) is false) return;

        OnReduced?.Invoke(this, amount);
    }

    public void SetAmount(byte amount)
    {
        Attributes.SetAttribute(ItemAttribute.Count, amount);
    }

    public void Increase(byte amount)
    {
        var newAmount = (byte)(amount + Amount > 100 ? 100 : amount + Amount);
        SetAmount(newAmount);
    }

    private bool TryReduce(byte amount = 1)
    {
        if (amount == 0 || Amount == 0) return false;

        amount = (byte)(amount > 100 ? 100 : amount);

        var oldAmount = Amount;
        var newAmount = Amount - amount;
        SetAmount((byte)newAmount);

        if (oldAmount == Amount) return false;
        return true;
    }
}