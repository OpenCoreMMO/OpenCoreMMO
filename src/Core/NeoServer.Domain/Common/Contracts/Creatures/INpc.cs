using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public delegate string KeywordReplacement(string message, INpc npc, ISociableCreature to);

public delegate void CustomerLeft(ICreature creature);

public delegate IItem CreateItem(ushort typeId, Location.Structs.Location location,
    IDictionary<ItemAttribute, IConvertible> attributes,
    IDictionary<string, IConvertible> customAttributes, IEnumerable<IItem> children = null);

public delegate void PlayerCloseChannel(INpc npc, IPlayer player);

public interface INpc : ISociableCreature
{
    INpcType Metadata { get; }
    ISpawnPoint SpawnPoint { get; }
    KeywordReplacement ReplaceKeywords { get; set; }
    event PlayerCloseChannel OnPlayerCloseChannel;

    void Advertise();
    event CustomerLeft OnCustomerLeft;
    bool CanInteract(Location.Structs.Location location, int range = 4);
    void SetPlayerInteraction(IPlayer player, ushort topicId);
    void RemovePlayerInteraction(IPlayer player);
    bool IsInteractingWithPlayer(IPlayer player);
    bool IsInteractingWithAnyPlayer();
    bool IsPlayerInteractingOnTopic(IPlayer player, ushort topicId);
    void PlayerCloseChannel(IPlayer player);
}