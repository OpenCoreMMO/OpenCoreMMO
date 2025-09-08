using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Spells;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Services;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;
using Xunit;
using PathFinder = NeoServer.Domain.World.Map.PathFinder;

namespace NeoServer.Server.Tests.Commands;

public class PlayerSayCommandTest
{
    [Fact]
    public void Execute_When_Player_Send_Message_To_Another_Player_Should_Call_Player_Method_Once()
    {
        //arrange
        var player = new Mock<IPlayer>();
        var connection = new Mock<IConnection>();
        var network = new Mock<IReadOnlyNetworkMessage>();
        var scriptManager = ScriptManagerTestBuilder.Build();
        var logger = new Mock<ILogger>();
        var spellListManager = new SpellListManager();

        var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);
        var mapTool = new MapTool(map, new PathFinder(map));

        var spellService = new SpellService(new SpellCastValidation(mapTool), new Mock<IEventAggregator>().Object, map);

        var playerSayPacket = new Mock<PlayerSayPacket>(network.Object);
        playerSayPacket.SetupGet(x => x.TalkType).Returns(SpeechType.PrivateRedTo);
        playerSayPacket.SetupGet(x => x.Receiver).Returns("receiver");
        playerSayPacket.SetupGet(x => x.Message).Returns("hello");

        var chatChannelStore = new ChatChannelStore();

        var receiverMock = new Mock<IPlayer>();
        var receiver = receiverMock.Object;

        var game = new Mock<IGameServer>();
        game.Setup(x => x.CreatureManager.TryGetPlayer("receiver", out receiver)).Returns(true);

        var sut = new PlayerSayCommand(game.Object, chatChannelStore, scriptManager, spellService, spellListManager, yellConfiguration: new YellConfiguration());

        //act
        sut.Execute(player.Object, connection.Object, playerSayPacket.Object);

        //assert
        player.Verify(x => x.SendMessageTo(receiver, SpeechType.PrivateRedTo, "hello"), Times.Once());
    }
}