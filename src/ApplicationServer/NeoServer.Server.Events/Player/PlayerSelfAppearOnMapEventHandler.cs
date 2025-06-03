using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.World;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Networking.Packets.Outgoing.Map;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Configurations;

namespace NeoServer.Server.Events.Player;

public class PlayerSelfAppearOnMapEventHandler : IEventHandler
{
    private readonly ClientConfiguration _clientConfiguration;
    private readonly IGameServer _game;
    private readonly IMap _map;
    private readonly World _world;

    public PlayerSelfAppearOnMapEventHandler(IMap map, IGameServer game, ClientConfiguration clientConfiguration,
        World world)
    {
        _map = map;
        _game = game;
        _clientConfiguration = clientConfiguration;
        _world = world;
    }

    public void Execute(IWalkableCreature creature)
    {
        if (creature.IsNull()) return;

        if (creature is not IPlayer player) return;

        if (!_game.CreatureManager.GetPlayerConnection(creature.CreatureId, out var connection)) return;

        SendPacketsToPlayer(player, connection);
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
        foreach (var condition in player.Conditions) icons |= (ushort)ConditionIconParser.Parse(condition.Key);

        connection.OutgoingPackets.Enqueue(new ConditionIconPacket(icons));
    }
}