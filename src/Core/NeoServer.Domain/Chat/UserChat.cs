using NeoServer.Domain.Chat.Rules;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Chat;

public class UserChat
{
    private const byte MAX_MUTED_TIMES = 36;
    private long firstMessageBeforeMuted;
    private ushort lastMutedForSeconds;
    private byte mutedTimes; //count how many times user get muted.
    public long LastMessage { get; private set; }
    public ushort MutedForSeconds { get; set; }
    public ushort MessagesCount { get; private set; }

    public int RemainingMutedSeconds => MutedForSeconds == 0
        ? 0
        : (int)Math.Round(
            TimeSpan.FromTicks(LastMessage + TimeSpan.TicksPerSecond * MutedForSeconds - DateTime.UtcNow.Ticks)
                .TotalSeconds, MidpointRounding.AwayFromZero);

    public IPlayer Player { get; init; }
    public bool Removed { get; private set; }
    public bool IsMuted => RemainingMutedSeconds > 0;

    public void UpdateLastMessage(MuteRule rule)
    {
        var secondsSinceFirstMessage =
            (ushort)TimeSpan.FromTicks(DateTime.UtcNow.Ticks - firstMessageBeforeMuted).Seconds;

        if (secondsSinceFirstMessage > 0 && secondsSinceFirstMessage > rule.TimeToBlock) MessagesCount = 0;

        if (!IsMuted)
        {
            MessagesCount += 1;
            LastMessage = DateTime.UtcNow.Ticks;
            if (MessagesCount == 1)
            {
                firstMessageBeforeMuted = LastMessage;
                MutedForSeconds = 0;
            }
        }
        else
        {
            MessagesCount = 0;
        }

        if (MessagesCount >= rule.MessagesCount && secondsSinceFirstMessage <= rule.TimeToBlock)
        {
            MutedForSeconds = (ushort)rule.Formula(lastMutedForSeconds, mutedTimes);

            lastMutedForSeconds = MutedForSeconds;
            mutedTimes += (byte)(mutedTimes < MAX_MUTED_TIMES ? 1 : 0);
        }
    }

    public void MarkAsRemoved()
    {
        Removed = true;
    }

    public void MarkAsAdded()
    {
        Removed = false;
    }
}