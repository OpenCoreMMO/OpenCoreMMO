using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Item;
using static NeoServer.Game.Common.Contracts.World.ISpawn;

namespace NeoServer.Game.Common.Contracts.Creatures;

public delegate string KeywordReplacement(string message, INpc npc, ISociableCreature to);

public delegate void Answer(INpc from, ICreature to, IDialog dialog, string message, SpeechType type);

public delegate void DialogAction(INpc from, ICreature to, IDialog dialog, string action,
    Dictionary<string, string> lastKeywords);

public delegate void CustomerLeft(ICreature creature);

public delegate IItem CreateItem(ushort typeId, Location.Structs.Location location,
    IDictionary<ItemAttribute, IConvertible> attributes, IEnumerable<IItem> children = null);

public interface INpc : ISociableCreature
{
    INpcType Metadata { get; }
    ISpawnPoint SpawnPoint { get; }
    KeywordReplacement ReplaceKeywords { get; set; }

    event Answer OnAnswer;

    void Advertise();
    void BackInDialog(ISociableCreature creature, byte count);
    void ForgetCustomer(ISociableCreature sociableCreature);
    Dictionary<string, string> GetPlayerStoredValues(ISociableCreature sociableCreature);
    void StopTalkingToCustomer(IPlayer player);
    event CustomerLeft OnCustomerLeft;
    bool CanInteract(NeoServer.Game.Common.Location.Structs.Location location, int range = 4);
    void SetPlayerInteraction(IPlayer player, ushort topicId);
    void RemovePlayerInteraction(IPlayer player);
    bool IsInteractingWithPlayer(IPlayer player);
    bool IsPlayerInteractingOnTopic(IPlayer player, ushort topicId);
}