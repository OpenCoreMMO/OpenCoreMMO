using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items;

namespace NeoServer.Domain.Tests.Common.Items;

public class CoinCalculatorTest
{
    public enum CoinType
    {
        Gold = 2148,
        Platinum = 2152,
        Crystal = 2160
    }

    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[] { 0, new (CoinType, byte)[] { } },
            new object[] { 1, new (CoinType, byte)[] { (CoinType.Gold, 1) } },
            new object[] { 99, new (CoinType, byte)[] { (CoinType.Gold, 99) } },
            new object[] { 100, new (CoinType, byte)[] { (CoinType.Platinum, 1) } },
            new object[] { 200, new (CoinType, byte)[] { (CoinType.Platinum, 2) } },
            new object[] { 199, new (CoinType, byte)[] { (CoinType.Platinum, 1), (CoinType.Gold, 99) } },
            new object[] { 9_999, new (CoinType, byte)[] { (CoinType.Platinum, 99), (CoinType.Gold, 99) } },
            new object[] { 10_000, new (CoinType, byte)[] { (CoinType.Crystal, 1) } },
            new object[] { 20_000, new (CoinType, byte)[] { (CoinType.Crystal, 2) } },
            new object[]
            {
                20_150, new (CoinType, byte)[] { (CoinType.Crystal, 2), (CoinType.Platinum, 1), (CoinType.Gold, 50) }
            },
            new object[] { 20_500, new (CoinType, byte)[] { (CoinType.Crystal, 2), (CoinType.Platinum, 5) } },
            new object[] { 1_000_000, new (CoinType, byte)[] { (CoinType.Crystal, 100) } },
            new object[] { 1_010_000, new (CoinType, byte)[] { (CoinType.Crystal, 100), (CoinType.Crystal, 1) } },
            new object[]
            {
                1_015_790,
                new (CoinType, byte)[]
                    { (CoinType.Crystal, 100), (CoinType.Crystal, 1), (CoinType.Platinum, 57), (CoinType.Gold, 90) }
            }
        };

    [Theory]
    [MemberData(nameof(Data))]
    public void Calculate(uint value, (CoinType, byte)[] expected)
    {
        var goldType = new ItemType();
        goldType.SetId((ushort)CoinType.Gold).Attributes.SetAttribute(ItemAttribute.Worth, 1);

        var platinumType = new ItemType();
        platinumType.SetId((ushort)CoinType.Platinum).Attributes.SetAttribute(ItemAttribute.Worth, 100);

        var crystalType = new ItemType();
        crystalType.SetId((ushort)CoinType.Crystal).Attributes.SetAttribute(ItemAttribute.Worth, 10_000);

        var coinTypes = new Dictionary<ushort, IItemType>
        {
            { (ushort)CoinType.Gold, goldType },
            { (ushort)CoinType.Platinum, platinumType },
            { (ushort)CoinType.Crystal, crystalType }
        };

        var result = CoinCalculator.Calculate(coinTypes, value);

        var expecteds = expected.Select(x => ((ushort)x.Item1, x.Item2));

        Assert.Equal(expecteds, result);
    }
}