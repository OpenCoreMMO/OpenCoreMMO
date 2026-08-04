using FluentAssertions;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.E2E.Tests.Client.Protocol;
using NeoServer.E2E.Tests.Harness;
using NeoServer.E2E.Tests.Login;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Standalone.IoC;

namespace NeoServer.E2E.Tests.Movement;

[Collection(E2ECollection.NAME)]
[Trait("Category", "E2E")]
public sealed class ItemStackMergeTests(E2ECollectionFixture fixture)
{
    private const ushort PowerBoltServerId = 2547;
    private const byte ExpectedDestinationAmount = 100;

    private static readonly Location SourcePileLocation = new(1002, 1017, 7);
    private static readonly Location DestinationPileLocation = new(1002, 1016, 7);

    [SkipOnGitHubActionsFact]
    public async Task Moving_30_power_bolts_to_a_pile_of_100_power_bolts_sends_correct_packets_data()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = timeout.Token;

        await using var loggedInClient = await E2ELoginHelper.LoginAsync(fixture.Host, cancellationToken);

        var sourceAmount = AssertMapPreconditions(fixture.Host.Services.Resolve<IGameServer>());

        var itemTypeStore = fixture.Host.Services.Resolve<IItemTypeStore>();
        var powerBoltClientId = GetPowerBoltClientId(itemTypeStore);

        await loggedInClient.DrainPacketsAsync(TimeSpan.FromSeconds(2), cancellationToken);

        var itemThrowPacket = GamePacketBuilder.BuildItemThrow(
            SourcePileLocation,
            powerBoltClientId,
            fromStackPosition: 1,
            DestinationPileLocation,
            sourceAmount);

        await loggedInClient.SendGamePacketAsync(itemThrowPacket, cancellationToken);

        var tilePackets = await ReceiveTileItemPacketsAsync(loggedInClient, count: 3, cancellationToken);

        var removePacket = tilePackets[0];
        var updatePacket = tilePackets[1];
        var addPacket = tilePackets[2];

        removePacket.Opcode.Should().Be(GameOutgoingPacketType.RemoveAtStackPos);
        (removePacket.Location == SourcePileLocation).Should().BeTrue();
        removePacket.StackPosition.Should().Be(1);

        updatePacket.Opcode.Should().Be(GameOutgoingPacketType.TransformThing);
        (updatePacket.Location == DestinationPileLocation).Should().BeTrue();
        updatePacket.StackPosition.Should().Be(1);

        addPacket.Opcode.Should().Be(GameOutgoingPacketType.AddAtStackPos);
        (addPacket.Location == DestinationPileLocation).Should().BeTrue();
        addPacket.StackPosition.Should().Be(1);
    }

    private static byte AssertMapPreconditions(IGameServer game)
    {
        var sourceTile = game.Map[SourcePileLocation];
        var destinationTile = game.Map[DestinationPileLocation];

        sourceTile.Should().NotBeNull();
        destinationTile.Should().NotBeNull();

        var sourceItem = sourceTile.TopDownItemOnStack;
        var destinationItem = destinationTile.TopDownItemOnStack;

        sourceItem.Should().NotBeNull();
        destinationItem.Should().NotBeNull();
        sourceItem.Amount.Should().BeGreaterThan(0);
        destinationItem.Amount.Should().Be(ExpectedDestinationAmount);

        return sourceItem.Amount;
    }

    private static ushort GetPowerBoltClientId(IItemTypeStore itemTypeStore)
    {
        if (itemTypeStore.TryGetValue(PowerBoltServerId, out var itemType) && itemType is not null)
        {
            return itemType.ClientId;
        }

        foreach (var candidate in itemTypeStore.All)
        {
            if (string.Equals(candidate.Name, "power bolt", StringComparison.OrdinalIgnoreCase))
            {
                return candidate.ClientId;
            }
        }

        throw new InvalidOperationException("Power bolt item type was not loaded.");
    }

    private static async Task<IReadOnlyList<TileItemPacket>> ReceiveTileItemPacketsAsync(
        E2ELoggedInClient loggedInClient,
        int count,
        CancellationToken cancellationToken)
    {
        var packets = new List<TileItemPacket>(count);

        while (packets.Count < count)
        {
            var payload = await loggedInClient.ReceivePacketAsync(cancellationToken);

            foreach (var packet in ServerPacketParser.ParseTileItemPackets(payload))
            {
                if (!IsExpectedMovePacket(packet))
                {
                    continue;
                }

                packets.Add(packet);

                if (packets.Count >= count)
                {
                    break;
                }
            }
        }

        return packets;
    }

    private static bool IsExpectedMovePacket(TileItemPacket packet)
    {
        if (packet.Opcode == GameOutgoingPacketType.RemoveAtStackPos)
        {
            return packet.Location == SourcePileLocation;
        }

        if (packet.Opcode is GameOutgoingPacketType.TransformThing or GameOutgoingPacketType.AddAtStackPos)
        {
            return packet.Location == DestinationPileLocation;
        }

        return false;
    }
}
