using NeoServer.Domain.Chat;
using NeoServer.Domain.Chat.Rules;

namespace NeoServer.Domain.Tests.Chat;

public class UserChatTest
{
    [Theory]
    [InlineData(20, 0, 20)]
    [InlineData(350, 0, 350)]
    [InlineData(20, 3, 17)]
    [InlineData(5, 1, 4)]
    [InlineData(3, 3, 0)]
    [InlineData(2, 3, -1)]
    public void RemainingMutedSeconds_Returns_Seconds(ushort mutedForSeconds, int sleepSeconds, int expected)
    {
        var sup = new UserChat();

        sup.UpdateLastMessage(new MuteRule());
        sup.MutedForSeconds = mutedForSeconds;

        Thread.Sleep(sleepSeconds * 1000);
        Assert.Equal(expected, sup.RemainingMutedSeconds);
    }
}