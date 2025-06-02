namespace NeoServer.Game.Common.Contracts.Creatures;

public interface ISummon
{
    ICreature Master { get; }
}