using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Server.Common.Contracts;
using Serilog;
using System.Linq;

namespace NeoServer.Server.Events.Creature;

public class CreatureChangedOutfitEventHandler
{
    private readonly IGameServer game;
    private readonly IMap map;
    private readonly ILogger logger;

    public CreatureChangedOutfitEventHandler(IMap map, IGameServer game, ILogger logger)
    {
        this.map = map;
        this.game = game;
        this.logger = logger;
    }

    public void Execute(ICreature creature, IOutfit outfit)
    {
        logger.Information("Processing outfit change for {CreatureName} (ID: {CreatureId}) to outfit {LookType}", 
            creature.Name, creature.CreatureId, outfit.LookType);

        // Get all spectators including the creature itself
        var spectators = map.GetPlayersAtPositionZone(creature.Location);
        logger.Information("Found {SpectatorCount} spectators at position {Position}", 
            spectators.Count(), creature.Location);

        var notifiedPlayers = 0;
        var playerNotified = false;

        foreach (var spectator in spectators)
        {
            // Skip if spectator cannot see the creature's location
            if (!spectator.CanSee(creature.Location))
            {
                logger.Debug("Spectator {SpectatorName} cannot see creature location", spectator.Name);
                continue;
            }

            // Get the connection for this spectator
            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection))
            {
                logger.Debug("No connection found for spectator {SpectatorName}", spectator.Name);
                continue;
            }

            // Send the outfit change packet
            connection.OutgoingPackets.Enqueue(new CreatureOutfitPacket(creature));
            connection.Send();
            
            notifiedPlayers++;
            logger.Debug("Sent outfit change notification to {SpectatorName}", spectator.Name);

            // Track if the player themselves was notified
            if (creature is IPlayer player && spectator.CreatureId == player.CreatureId)
            {
                playerNotified = true;
                logger.Information("Player {PlayerName} was notified of their own outfit change", player.Name);
            }
        }

        // IMPORTANTE: Se o próprio jogador não foi notificado através dos espectadores, enviar diretamente
        if (creature is IPlayer playerToNotify && !playerNotified)
        {
            if (game.CreatureManager.GetPlayerConnection(playerToNotify.CreatureId, out var playerConnection))
            {
                // Enviar atualização do outfit para o próprio jogador
                playerConnection.OutgoingPackets.Enqueue(new CreatureOutfitPacket(creature));
                playerConnection.Send();
                notifiedPlayers++;
                logger.Information("Sent direct outfit change notification to the player: {PlayerName}", playerToNotify.Name);
            }
        }

        logger.Information("Outfit change processed for {CreatureName}. Notified {NotifiedCount} players", 
            creature.Name, notifiedPlayers);
    }
}