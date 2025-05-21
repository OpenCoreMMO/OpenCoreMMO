using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Creatures;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Combat;

public class SpellInvokedEventHandler(IGameServer game)
{
    public void Execute(ICreature creature, ISpell spell)
    {
        if (spell.Effect is EffectT.None) return;
        
        foreach (var spectator in game.Map.GetPlayersAtPositionZone(creature.Location))
        {
            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(creature.Location, spell.Effect));
            connection.Send();
        }
    }
}