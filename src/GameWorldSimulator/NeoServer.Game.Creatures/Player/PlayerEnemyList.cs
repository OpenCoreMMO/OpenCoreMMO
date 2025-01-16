using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures.Player;

public class PlayerEnemyList
{
    private HashSet<uint> PlayersAttackedList { get; } = new();
    public void Remove(uint creatureId) => PlayersAttackedList.Remove(creatureId);
    public bool Any() => PlayersAttackedList.Count > 0;
    public void AddEnemy(IPlayer player) => AddPlayerToEnemyList(player.CreatureId);

    public void AddPlayerToEnemyList(uint creatureId)
    {
        if (HasEnemy(creatureId)) return;
        PlayersAttackedList.Add(creatureId);
    }

    public bool HasEnemy(uint creatureId) => PlayersAttackedList.Contains(creatureId);
    public bool HasEnemy(IPlayer creature) => PlayersAttackedList.Contains(creature.CreatureId);
    public void Clear() => PlayersAttackedList.Clear();
}