namespace NeoServer.Domain.Common.Helpers;

public static class QueueExtensions
{
    public static bool IsEmpty<T>(this Queue<T> queue)
    {
        return queue.Count == 0;
    }
}