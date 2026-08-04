using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public delegate string KeywordReplacement(string message, INpc npc, ISociableCreature to);

public delegate void CustomerLeft(ICreature creature);

public delegate IItem CreateItem(
    ushort typeId, Location.Structs.Location location,
    IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
    IDictionary<string, IConvertible> itemTypeCustomAttributes = null,
    IDictionary<ItemAttribute, IConvertible> itemAttributes = null,
    IDictionary<string, IConvertible> itemCustomAttributes = null,
    IEnumerable<IItem> children = null);

public delegate void PlayerCloseChannel(INpc npc, IPlayer player);

public interface INpc : ISociableCreature
{
    INpcType Metadata { get; }
    ISpawnPoint SpawnPoint { get; }
    KeywordReplacement ReplaceKeywords { get; set; }
    event PlayerCloseChannel OnPlayerCloseChannel;

    /// <summary>
    /// Allows the NPC to advertise its marketing messages to a list of sociable creatures (receivers), if applicable.
    /// </summary>
    /// <param name="receivers">The list of creatures that will receive the advertisement message.</param>
    /// <remarks>
    /// The method ensures that the NPC has marketing messages available and checks if the advertisement
    /// cooldown has expired before proceeding. It then selects a random marketing message and broadcasts
    /// it to the specified receivers, also resetting the cooldown timer for advertising.
    /// </remarks>
    public void Advertise(List<ICreature> receivers);
    bool CanInteract(Location.Structs.Location location, int range = 4);
    void SetPlayerInteraction(IPlayer player, ushort topicId);
    void RemovePlayerInteraction(IPlayer player);
    bool IsInteractingWithPlayer(IPlayer player);
    bool IsInteractingWithAnyPlayer();
    bool IsPlayerInteractingOnTopic(IPlayer player, ushort topicId);
    void PlayerCloseChannel(IPlayer player);
}