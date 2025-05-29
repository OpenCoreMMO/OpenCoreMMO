namespace NeoServer.Game.Common.Contracts.Creatures;

public interface IBankable
{
    IBank Bank { get; }
    ulong BankAmount { get; }
}


public interface IBank
{
    public void Credit(ulong amount);
    ulong Amount { get; }
    void Debit(ulong amount);
}