namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface ISummon
{
    ICreature Master { get; }
}