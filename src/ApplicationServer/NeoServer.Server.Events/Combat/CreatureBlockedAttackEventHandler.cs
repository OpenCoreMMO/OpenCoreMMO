using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Combat;

public class CreatureBlockedAttackEventHandler(IGameServer game)
{
    public void Execute(ICreature creature, BlockType blockType)
    {
        foreach (var spectator in game.Map.GetPlayersAtPositionZone(creature.Location))
        {
            var effect = blockType == BlockType.Armor ? EffectT.SparkYellow : EffectT.Puff;

            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(creature.Location, effect));
            connection.Send();
        }
    }
}