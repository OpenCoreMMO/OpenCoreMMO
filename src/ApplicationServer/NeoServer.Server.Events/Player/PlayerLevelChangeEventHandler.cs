using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Server.Events.Player;

public abstract class PlayerLevelChangeEventHandler
{
    private readonly IGameServer game;

    protected PlayerLevelChangeEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IPlayer player, SkillType skillType, int fromLevel, int toLevel)
    {
        if (Guard.IsNull(player)) return;

        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        SendLevelChangeMessage(skillType, connection, fromLevel, toLevel);

        connection.OutgoingPackets.Enqueue(new PlayerStatusPacket(player));

        connection.Send();
    }

    protected abstract void SendLevelChangeMessage(SkillType skillType, IConnection connection, int fromLevel,
        int toLevel);
}