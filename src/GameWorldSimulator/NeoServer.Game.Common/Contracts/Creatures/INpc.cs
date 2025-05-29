using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Common.Contracts.Creatures;

public delegate string KeywordReplacement(string message, INpc npc, ISociableCreature to);

public delegate void CustomerLeft(ICreature creature);

public delegate IItem CreateItem(ushort typeId, Location.Structs.Location location,
    IDictionary<ItemAttribute, IConvertible> attributes, IEnumerable<IItem> children = null);

public delegate void PlayerCloseChannel(INpc npc, IPlayer player);

public interface INpc : ISociableCreature
{
    event PlayerCloseChannel OnPlayerCloseChannel;

    INpcType Metadata { get; }
    ISpawnPoint SpawnPoint { get; }
    KeywordReplacement ReplaceKeywords { get; set; }

    void Advertise();
    event CustomerLeft OnCustomerLeft;
    bool CanInteract(NeoServer.Game.Common.Location.Structs.Location location, int range = 4);
    void SetPlayerInteraction(IPlayer player, ushort topicId);
    void RemovePlayerInteraction(IPlayer player);
    bool IsInteractingWithPlayer(IPlayer player);
    bool IsInteractingWithAnyPlayer();
    bool IsPlayerInteractingOnTopic(IPlayer player, ushort topicId);
    void PlayerCloseChannel(IPlayer player);
}