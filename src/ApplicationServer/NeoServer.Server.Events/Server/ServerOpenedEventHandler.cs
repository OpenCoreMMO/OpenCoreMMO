using System;
using NeoServer.Data.Interfaces;
using Serilog;

namespace NeoServer.Server.Events.Server;

public class ServerOpenedEventHandler(IPlayerRepository playerRepository, ILogger logger)
{
    public void Execute()
    {
        try
        {
            playerRepository.UpdateAllPlayersToOffline();
        }
        catch (Exception e)
        {
            logger.Error(e, "Error while updating all players to offline");
        }
    }
}