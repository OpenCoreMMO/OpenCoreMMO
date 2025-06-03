using System.Threading;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Chat.Rules;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Chats.Tests;

public class ChatChannelTest
{
    [Fact]
    public void WriteMessage_Sets_User_To_Muted_when_Sending_Multiple_Times()
    {
        var sut = new ChatChannel(1, "channel 1")
        {
            MuteRule = new MuteRule
            {
                WaitTime = 5,
                TimeMultiplier = 2,
                MessagesCount = 5,
                TimeToBlock = 10
            }
        };

        var player = PlayerTestDataBuilder.Build();
        sut.AddUser(player);
        sut.WriteMessage(player, "teste", out var cancelMessage);
        sut.WriteMessage(player, "teste", out cancelMessage);
        sut.WriteMessage(player, "teste", out cancelMessage);
        sut.WriteMessage(player, "teste", out cancelMessage);

        Assert.False(sut.PlayerIsMuted(player, out cancelMessage));

        sut.WriteMessage(player, "teste", out cancelMessage);

        Assert.True(sut.PlayerIsMuted(player, out cancelMessage));

        Thread.Sleep(2000);
        Assert.True(sut.PlayerIsMuted(player, out cancelMessage));
        Thread.Sleep(5000);
        Assert.False(sut.PlayerIsMuted(player, out cancelMessage));
    }
}