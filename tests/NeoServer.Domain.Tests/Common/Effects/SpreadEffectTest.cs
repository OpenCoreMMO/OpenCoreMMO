using NeoServer.Domain.Common.Effects.Magical;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Tests.Common.Effects;

public class SpreadEffectTest
{
    [Fact]
    public void Create_When_Length_Is_3_And_Spread_Is_1_Should_Create_3_Rows_And_1_Column()
    {
        var coordinates = SpreadEffect.Create(Direction.North, 3, 1);

        Assert.Equal(3, coordinates.Length);

        Assert.Contains(new Coordinate(0, -1, 0), coordinates);
        Assert.Contains(new Coordinate(0, -2, 0), coordinates);
        Assert.Contains(new Coordinate(0, -3, 0), coordinates);
    }

    [Fact]
    public void Create_When_Length_Is_5_And_Spread_Is_1_Should_Create_5_Rows_And_1_Column()
    {
        var coordinates = SpreadEffect.Create(Direction.North, 5, 1);

        Assert.Equal(5, coordinates.Length);

        Assert.Contains(new Coordinate(0, -1, 0), coordinates);
        Assert.Contains(new Coordinate(0, -2, 0), coordinates);
        Assert.Contains(new Coordinate(0, -3, 0), coordinates);
        Assert.Contains(new Coordinate(0, -4, 0), coordinates);
        Assert.Contains(new Coordinate(0, -5, 0), coordinates);
    }

    [Fact]
    public void Create_When_Length_Is_8_And_Spread_Is_3_Should_Create_8_Rows_And_5_Columns()
    {
        var coordinates = SpreadEffect.Create(Direction.North, 8, 3);

        Assert.Equal(26, coordinates.Length);

        Assert.Contains(new Coordinate(0, -1, 0), coordinates);
        Assert.Contains(new Coordinate(0, -2, 0), coordinates);

        Assert.Contains(new Coordinate(0, -3, 0), coordinates);
        Assert.Contains(new Coordinate(-1, -3, 0), coordinates);
        Assert.Contains(new Coordinate(1, -3, 0), coordinates);

        Assert.Contains(new Coordinate(0, -4, 0), coordinates);
        Assert.Contains(new Coordinate(-1, -4, 0), coordinates);
        Assert.Contains(new Coordinate(1, -4, 0), coordinates);

        Assert.Contains(new Coordinate(0, -5, 0), coordinates);
        Assert.Contains(new Coordinate(-1, -5, 0), coordinates);
        Assert.Contains(new Coordinate(1, -5, 0), coordinates);

        Assert.Contains(new Coordinate(-2, -6, 0), coordinates);
        Assert.Contains(new Coordinate(-1, -6, 0), coordinates);
        Assert.Contains(new Coordinate(0, -6, 0), coordinates);
        Assert.Contains(new Coordinate(1, -6, 0), coordinates);
        Assert.Contains(new Coordinate(2, -6, 0), coordinates);

        Assert.Contains(new Coordinate(-2, -7, 0), coordinates);
        Assert.Contains(new Coordinate(-1, -7, 0), coordinates);
        Assert.Contains(new Coordinate(0, -7, 0), coordinates);
        Assert.Contains(new Coordinate(1, -7, 0), coordinates);
        Assert.Contains(new Coordinate(2, -7, 0), coordinates);

        Assert.Contains(new Coordinate(-2, -8, 0), coordinates);
        Assert.Contains(new Coordinate(-1, -8, 0), coordinates);
        Assert.Contains(new Coordinate(0, -8, 0), coordinates);
        Assert.Contains(new Coordinate(1, -8, 0), coordinates);
        Assert.Contains(new Coordinate(2, -8, 0), coordinates);
    }
}