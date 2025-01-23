using System.Collections.Generic;
using NeoServer.Data.Entities;

namespace NeoServer.Server.Commands.WaitingInLine;

internal class WaitListInfo
{
    public LinkedList<Wait> PriorityWaitList = new LinkedList<Wait>();
    public LinkedList<Wait> WaitList = new LinkedList<Wait>();

    public (LinkedList<Wait>, LinkedListNode<Wait>, uint) FindClient(PlayerEntity player)
    {
        uint slot = 1;
        var node = PriorityWaitList.First;
        while (node != null)
        {
            if (node.Value.PlayerId == player.Id)
            {
                return (PriorityWaitList, node, slot);
            }
            node = node.Next;
            slot++;
        }

        node = WaitList.First;
        while (node != null)
        {
            if (node.Value.PlayerId == player.Id)
            {
                return (WaitList, node, slot);
            }
            node = node.Next;
            slot++;
        }
        return (WaitList, null, slot);
    }
}