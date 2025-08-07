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
        logger.Information("Creature {CreatureName} (ID: {CreatureId}) changed outfit to LookType: {LookType}", 
            creature.Name, creature.CreatureId, outfit.LookType);
        
        var spectators = map.GetPlayersAtPositionZone(creature.Location);
        logger.Information("Found {SpectatorCount} spectators at position {Position}", 
            spectators.Count(), creature.Location);
        
        foreach (var spectator in spectators)
        {
            logger.Debug("Processing spectator {SpectatorName} at {SpectatorLocation}", 
                spectator.Name, spectator.Location);
            
            if (!creature.CanSee(spectator.Location)) 
            {
                logger.Debug("Creature cannot see spectator {SpectatorName}", spectator.Name);
                continue;
            }

            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) 
            {
                logger.Warning("No connection found for spectator {SpectatorName}", spectator.Name);
                continue;
            }
            
            logger.Debug("Sending outfit packet to spectator {SpectatorName}", spectator.Name);
            connection.OutgoingPackets.Enqueue(new CreatureOutfitPacket(creature));
            connection.Send();
        }
        
        logger.Information("Finished processing outfit change for {CreatureName}", creature.Name);
    }
}