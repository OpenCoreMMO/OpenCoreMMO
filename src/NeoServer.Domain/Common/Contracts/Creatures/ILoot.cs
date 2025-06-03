namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface ILoot
{
    ILootItem[] Items { get; }
    HashSet<ICreature> Owners { get; }
}