namespace NeoServer.Server.Commands.WaitingInLine;

internal record Wait
{
    public int PlayerId;

    public long Timeout;

    public Wait(long timeout, int playerId)
    {
        Timeout = timeout;
        PlayerId = playerId;
    }
}