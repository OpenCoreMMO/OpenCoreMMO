using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures.Player;

public class PlayerEnemyList
{
    private Dictionary<uint, long> PlayersAttackedList { get; } = new();
    
    
    public void Remove(uint creatureId) => PlayersAttackedList.Remove(creatureId);

    public void AddEnemy(IPlayer player) => AddPlayerToEnemyList(player.CreatureId);

    public void AddPlayerToEnemyList(uint creatureId)
    {
        if (HasEnemy(creatureId)) return;
        PlayersAttackedList.Add(creatureId, DateTime.Now.Ticks);   
    }
    public bool HasEnemy(uint creatureId) => PlayersAttackedList.ContainsKey(creatureId);

    public long GetAttackTime(uint creatureId)
    {
        PlayersAttackedList.TryGetValue(creatureId, out var time);
        return time;
    }
    public void Clear() => PlayersAttackedList.Clear();
}