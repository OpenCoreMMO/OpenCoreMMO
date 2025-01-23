namespace NeoServer.Server.Commands.WaitingInLine;

internal record Wait
{
    public Wait(long timeout, int playerId)
    {
        Timeout = timeout;
        PlayerId = playerId;
    }

    public long Timeout;
    public int PlayerId;
}