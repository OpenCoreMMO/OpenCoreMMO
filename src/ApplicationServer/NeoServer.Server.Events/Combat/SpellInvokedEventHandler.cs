using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Combat;

public class SpellInvokedEventHandler(IGameServer game)
{
    public void Execute(ICreature creature, ISpell spell)
    {
        if (spell.Effect is EffectT.None) return;

        if (spell.Effect == 0) return;

        foreach (var spectator in game.Map.GetPlayersAtPositionZone(creature.Location))
        {
            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;


            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(creature.Location, spell.Effect));
            connection.Send();
        }
    }
}