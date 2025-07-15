namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface ISummon : IMonster
{
    ICreature Master { get; }
    void Dismiss();
}