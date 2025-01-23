using System;
using System.Collections.Generic;
using NeoServer.Data.Entities;
using NeoServer.Data.Enums;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Creatures.Players;

namespace NeoServer.Server.Commands.WaitingInLine;

public class WaitingQueueManager : IWaitingQueueManager
{
    private readonly ICreatureGameInstance _gameCreatureManager;
    private readonly IGroupStore _groupStore;
    private WaitListInfo info;
    public WaitingQueueManager(ICreatureGameInstance gameCreatureManager, IGroupStore groupStore)
    {
        _gameCreatureManager = gameCreatureManager;
        _groupStore = groupStore;
        info = new WaitListInfo();
    }
    
    public bool CanLogin(PlayerEntity player, out uint currentSlot)
    {
        currentSlot = 0;
        var group  = _groupStore.Get(player.Group);

        //todo: implement check if accountype is game master if true should be return true.
        if (group.FlagIsEnabled(PlayerFlag.CanAlwaysLogin))
        {
            return true;
        }

        CleanupList(info.PriorityWaitList);
        CleanupList(info.WaitList);

        var maxPlayers = player.World.MaxCapacity;
        var playersOnlines = _gameCreatureManager.CountOnlinePlayers();
        if (maxPlayers == 0 || (info.PriorityWaitList.Count == 0 && info.WaitList.Count == 0 && playersOnlines < maxPlayers))
        {
            return true;
        }

        var result = info.FindClient(player);
        if (result.Item2 is not null)
        {
            currentSlot = result.Item3;
            if ((playersOnlines + currentSlot) <= maxPlayers)
            {
                result.Item1.Remove(result.Item2);
                return true;
            }

            result.Item2.Value.Timeout = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + GetTime(currentSlot);
            return false;
        }

        currentSlot = (uint)info.PriorityWaitList.Count;
        if (player.Account.PremiumTimeEndAt.HasValue)
        {
            info.PriorityWaitList.AddLast(new Wait(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + GetTime(++currentSlot), player.Id));
        }
        else
        {
            currentSlot += (uint)info.WaitList.Count;
            info.WaitList.AddLast(new Wait(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + GetTime(++currentSlot), player.Id));
        }
        return false;
    }

    public long GetTime(uint slot)
    {
        if (slot < 5) return 20;
        else if (slot < 10) return 25;
        else if (slot < 20) return 35;
        else if (slot < 50) return 75;
        else return 135;
    }

    private void CleanupList(LinkedList<Wait> list)
    {
        long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var node = list.First;
        while (node != null)
        {
            if ((node.Value.Timeout - time) <= 0)
            {
                var temp = node;
                node = node.Next;
                list.Remove(temp);
            }
            else
            {
                node = node.Next;
            }
        }
    }
}

