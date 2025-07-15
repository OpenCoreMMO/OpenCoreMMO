using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Item;

public static class CoinCalculator
{
    public static IEnumerable<(ushort, byte)> Calculate(IDictionary<ushort, IItemType> coinTypes, ulong value)
    {
        if (coinTypes is null) return null;
        return Calculate(value,
            coinTypes.ToDictionary(x => x.Key,
                x => x.Value?.Attributes?.GetAttribute<uint>(ItemTypeAttribute.Worth) ?? 0));
    }

    private static IEnumerable<(ushort, byte)> Calculate(ulong value, IDictionary<ushort, uint> coinTypes,
        List<(ushort, byte)> coins = null)
    {
        var coinType = coinTypes.Aggregate((l, r) => l.Value > r.Value ? l : r);

        coins ??= new List<(ushort, byte)>(10);

        if (value == 0) return Array.Empty<(ushort, byte)>();

        var (coinId, worth) = coinType;

        var money = (long)(value / worth);

        while (money > 0)
        {
            coins.Add((coinId, (byte)Math.Min(100, money)));
            money = money - 100;
        }

        var mod = (uint)(value % worth);
        if (mod == 0) return coins;

        coinTypes.Remove(coinId);

        return Calculate(mod, coinTypes, coins);
    }
}