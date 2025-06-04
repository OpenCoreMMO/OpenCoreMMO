using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Helpers;

public class RandomIdGenerator
{
    private static readonly Random Random = new();
    private static readonly object Lock = new();

    public static uint Generate()
    {
        lock (Lock)
        {
            return (uint)Random.Next(0, int.MaxValue);
        }
    }

    public static ushort Generate(ushort maxValue)
    {
        lock (Lock)
        {
            return (ushort)Random.Next(0, maxValue);
        }
    }
}

public class RandomCreatureIdGenerator
{
    //private static Random _random = new Random();
    private static readonly object Lock = new();
    private static uint _lastPlayerId = 0x10000000;
    private static uint _lastMonsterId = 0x40000000;
    private static uint _lastNpcId = 0x80000000;

    public static uint Generate(ICreature creature)
    {
        lock (Lock)
        {
            if (creature is IPlayer)
                return _lastPlayerId++;
            if (creature is IMonster)
                return _lastMonsterId++;
            if (creature is INpc)
                return _lastNpcId++;
        }

        return 0;
    }
}