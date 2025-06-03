namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface IBankable
{
    IBank Bank { get; }
    ulong BankAmount { get; }
}

public interface IBank
{
    ulong Amount { get; }
    public void Credit(ulong amount);
    void Debit(ulong amount);
}