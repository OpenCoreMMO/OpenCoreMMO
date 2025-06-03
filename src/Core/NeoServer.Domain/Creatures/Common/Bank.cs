using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Common;

public class Bank(ulong amount) : IBank
{
    public ulong Amount { get; private set; } = amount;

    public void Credit(ulong amount)
    {
        Amount += amount;
    }

    public void Debit(ulong amount)
    {
        Amount -= Math.Min(amount, Amount);
    }
}