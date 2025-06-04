using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerUpdatedSkillPointsEventHandler
{
    private readonly IGameServer game;

    public PlayerUpdatedSkillPointsEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IPlayer player, SkillType skill)
    {
        if (Guard.AnyNull(player)) return;
        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new PlayerSkillsPacket(player));
        connection.Send();
    }

    public void Execute(IPlayer player, SkillType skill, sbyte increased)
    {
        if (game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection))
        {
            connection.OutgoingPackets.Enqueue(new PlayerSkillsPacket(player));
            connection.Send();
        }
    }
}