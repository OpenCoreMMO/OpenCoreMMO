using Moq;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Services;

public class PlayerChannelServiceTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void JoinChannels_joins_open_channels_player_is_not_in()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var channel = new ChatChannel(1, "World Chat") { Opened = true };
        var chatChannelStore = new Mock<IChatChannelStore>();
        chatChannelStore.Setup(x => x.All).Returns([channel]);

        var sut = new PlayerChannelService(chatChannelStore.Object);

        // Act
        sut.JoinChannels(player);

        // Assert
        channel.HasUser(player).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void JoinChannels_does_not_send_error_when_player_already_in_channel()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var channel = new ChatChannel(1, "World Chat") { Opened = true };
        channel.AddUser(player);

        var chatChannelStore = new Mock<IChatChannelStore>();
        chatChannelStore.Setup(x => x.All).Returns([channel]);

        var sut = new PlayerChannelService(chatChannelStore.Object);

        using var monitor = player.Channels.Monitor();

        // Act
        sut.JoinChannels(player);

        // Assert - should not raise OnJoinedChannel since player is already in
        monitor.Should().NotRaise(nameof(player.Channels.OnJoinedChannel));
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void JoinChannels_joins_only_channels_player_is_not_already_in()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var joinedChannel = new ChatChannel(1, "World Chat") { Opened = true };
        var unjoinedChannel = new ChatChannel(2, "Game Chat") { Opened = true };
        joinedChannel.AddUser(player);

        var chatChannelStore = new Mock<IChatChannelStore>();
        chatChannelStore.Setup(x => x.All).Returns([joinedChannel, unjoinedChannel]);

        var sut = new PlayerChannelService(chatChannelStore.Object);

        // Act
        sut.JoinChannels(player);

        // Assert
        joinedChannel.HasUser(player).Should().BeTrue();
        unjoinedChannel.HasUser(player).Should().BeTrue();
    }
}
