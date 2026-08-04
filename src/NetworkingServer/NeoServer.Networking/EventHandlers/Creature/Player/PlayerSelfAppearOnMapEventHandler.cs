using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Networking.Packets.Outgoing.Map;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Configurations;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerSelfAppearOnMapEventHandler : INetworkingEventHandler<PlayerLoggedInEvent>
{
    private readonly ClientConfiguration _clientConfiguration;
    private readonly IGameServer _game;
    private readonly IMap _map;
    private readonly Domain.World.World _world;

    public PlayerSelfAppearOnMapEventHandler(IMap map, IGameServer game, ClientConfiguration clientConfiguration,
        Domain.World.World world)
    {
        _map = map;
        _game = game;
        _clientConfiguration = clientConfiguration;
        _world = world;
    }

    public void Handle(PlayerLoggedInEvent @event)
    {
        if (@event.Player.IsNull()) return;

        if (!_game.CreatureManager.GetPlayerConnection(@event.Player.CreatureId, out var connection)) return;

        SendPacketsToPlayer(@event.Player, connection);
        connection.Send();
    }

    private void SendPacketsToPlayer(IPlayer player, IConnection connection)
    {
        connection.OutgoingPackets.Enqueue(new SelfAppearPacket(player));
        connection.OutgoingPackets.Enqueue(new MapDescriptionPacket(player, _map));
        connection.OutgoingPackets.Enqueue(new MagicEffectPacket(player.Location, EffectT.BubbleBlue));
        connection.OutgoingPackets.Enqueue(new PlayerInventoryPacket(player.Inventory)
        {
            ShowItemDescription = connection.OtcV8Version > 0 && _clientConfiguration.OtcV8.GameItemTooltip
        });

        connection.OutgoingPackets.Enqueue(new PlayerStatusPacket(player));
        connection.OutgoingPackets.Enqueue(new PlayerSkillsPacket(player));

        connection.OutgoingPackets.Enqueue(new WorldLightPacket(_world.WorldLight.LightLevel));

        connection.OutgoingPackets.Enqueue(new CreatureLightPacket(player));

        ushort icons = 0;
        foreach (var condition in player.GetConditions())
        {
            icons |= (ushort)ConditionIconParser.Parse(condition.Type);
        }

        connection.OutgoingPackets.Enqueue(new ConditionIconPacket(icons));
    }
}