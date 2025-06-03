using NeoServer.Data.Entities;

namespace NeoServer.Server.Commands.WaitingInLine;

public interface IWaitingQueueManager
{
    bool CanLogin(PlayerEntity player, out uint currentSlot);
    long GetTime(uint slot);
}