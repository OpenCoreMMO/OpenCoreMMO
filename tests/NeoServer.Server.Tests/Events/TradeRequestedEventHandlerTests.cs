using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NeoServer.Domain.SafeTrade.Request;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Networking.Packets.Messages;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Trade;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Configurations;
using NeoServer.Server.Events.Player.Trade;
using Xunit;

namespace NeoServer.Server.Tests.Events;

public class TradeRequestedEventHandlerTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void Buyer_receives_trade_window_when_seller_requests_trade()
    {
        var (handler, sellerConnection, buyerConnection, tradeRequest) = CreateHandler();

        handler.Execute(tradeRequest);

        var buyerTradePacket = buyerConnection.Object.OutgoingPackets.OfType<TradeRequestPacket>().Should()
            .ContainSingle().Subject;
        ReadOpcode(buyerTradePacket).Should().Be((byte)GameOutgoingPacketType.AcknowlegdeTradeRequest);

        var sellerTradePacket = sellerConnection.Object.OutgoingPackets.OfType<TradeRequestPacket>().Should()
            .ContainSingle().Subject;
        ReadOpcode(sellerTradePacket).Should().Be((byte)GameOutgoingPacketType.TradeRequest);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Buyer_receives_message_with_seller_name_when_trade_is_requested()
    {
        var (handler, _, buyerConnection, tradeRequest) = CreateHandler();

        handler.Execute(tradeRequest);

        var textPacket = buyerConnection.Object.OutgoingPackets.OfType<TextMessagePacket>().Should().ContainSingle()
            .Subject;
        ReadText(textPacket).Should().Contain("Seller wants to trade with you.");
    }

    [Fact]
    [Trait("Category", "ErrorCondition")]
    public void TradeRequestedEventHandler_does_not_throw_when_buyer_connection_is_missing()
    {
        var seller = PlayerTestDataBuilder.Build(1, "Seller");
        var buyer = PlayerTestDataBuilder.Build(2, "Buyer");
        var item = ItemTestDataBuilder.CreateWeaponItem(1);
        var tradeRequest = new TradeRequest(seller, buyer, [item]);

        var sellerConnection = CreateConnection();
        var creatureManager = new Mock<IGameCreatureManager>();
        IConnection sellerConnectionObject = sellerConnection.Object;
        IConnection missingConnection = null;
        creatureManager.Setup(manager => manager.GetPlayerConnection(seller.CreatureId, out sellerConnectionObject))
            .Returns(true);
        creatureManager.Setup(manager => manager.GetPlayerConnection(buyer.CreatureId, out missingConnection))
            .Returns(false);

        var gameServer = new Mock<IGameServer>();
        gameServer.SetupGet(server => server.CreatureManager).Returns(creatureManager.Object);

        var handler = new TradeRequestedEventHandler(gameServer.Object, CreateClientConfiguration());

        var act = () => handler.Execute(tradeRequest);

        act.Should().NotThrow();
        sellerConnection.Object.OutgoingPackets.OfType<TradeRequestPacket>().Should().ContainSingle();
        sellerConnection.Verify(connection => connection.Send(), Times.Once);
    }

    private static (TradeRequestedEventHandler Handler, Mock<IConnection> SellerConnection,
        Mock<IConnection> BuyerConnection, TradeRequest TradeRequest) CreateHandler()
    {
        var seller = PlayerTestDataBuilder.Build(1, "Seller");
        var buyer = PlayerTestDataBuilder.Build(2, "Buyer");
        var item = ItemTestDataBuilder.CreateWeaponItem(1);
        var tradeRequest = new TradeRequest(seller, buyer, [item]);

        var sellerConnection = CreateConnection();
        var buyerConnection = CreateConnection();

        var creatureManager = new Mock<IGameCreatureManager>();
        IConnection sellerConnectionObject = sellerConnection.Object;
        IConnection buyerConnectionObject = buyerConnection.Object;
        creatureManager.Setup(manager => manager.GetPlayerConnection(seller.CreatureId, out sellerConnectionObject))
            .Returns(true);
        creatureManager.Setup(manager => manager.GetPlayerConnection(buyer.CreatureId, out buyerConnectionObject))
            .Returns(true);

        var gameServer = new Mock<IGameServer>();
        gameServer.SetupGet(server => server.CreatureManager).Returns(creatureManager.Object);

        var handler = new TradeRequestedEventHandler(gameServer.Object, CreateClientConfiguration());
        return (handler, sellerConnection, buyerConnection, tradeRequest);
    }

    private static Mock<IConnection> CreateConnection()
    {
        var connection = new Mock<IConnection>();
        connection.SetupGet(c => c.OutgoingPackets).Returns(new Queue<IOutgoingPacket>());
        connection.SetupGet(c => c.OtcV8Version).Returns((ushort)0);
        return connection;
    }

    private static ClientConfiguration CreateClientConfiguration()
    {
        return new ClientConfiguration(new ClientConfiguration.OtcV8Configuration(false, false, false, false));
    }

    private static byte ReadOpcode(IOutgoingPacket packet)
    {
        var message = new NetworkMessage();
        packet.WriteToMessage(message);
        return message.Buffer[0];
    }

    private static string ReadText(IOutgoingPacket packet)
    {
        var message = new NetworkMessage();
        packet.WriteToMessage(message);
        return Encoding.Latin1.GetString(message.Buffer, 0, message.Length);
    }
}
