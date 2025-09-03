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

    [Fact]
    public void Create_When_Length_Is_5_And_Spread_Is_3_Should_Create_5_Rows_And_3_Columns()
    {
        var coordinates = SpreadEffect.Create(Direction.North, 5, 3);

        Assert.Equal(11, coordinates.Length);

        Assert.Contains(new Coordinate(0, -1, 0), coordinates);
        Assert.Contains(new Coordinate(0, -2, 0), coordinates);

        Assert.Contains(new Coordinate(-1, -3, 0), coordinates);
        Assert.Contains(new Coordinate(0, -3, 0), coordinates);
        Assert.Contains(new Coordinate(1, -3, 0), coordinates);

        Assert.Contains(new Coordinate(-1, -4, 0), coordinates);
        Assert.Contains(new Coordinate(0, -4, 0), coordinates);
        Assert.Contains(new Coordinate(1, -4, 0), coordinates);

        Assert.Contains(new Coordinate(-1, -5, 0), coordinates);
        Assert.Contains(new Coordinate(0, -5, 0), coordinates);
        Assert.Contains(new Coordinate(1, -5, 0), coordinates);
    }
    
      [Fact]
    public void Create_South_When_Length_Is_5_And_Spread_Is_3_Should_Match_Expected_Pattern()
    {
        var coordinates = SpreadEffect.Create(Direction.South, 5, 3);

        Assert.Equal(11, coordinates.Length);

        Assert.Contains(new Coordinate(0, 1, 0), coordinates);
        Assert.Contains(new Coordinate(0, 2, 0), coordinates);

        Assert.Contains(new Coordinate(-1, 3, 0), coordinates);
        Assert.Contains(new Coordinate(0, 3, 0), coordinates);
        Assert.Contains(new Coordinate(1, 3, 0), coordinates);

        Assert.Contains(new Coordinate(-1, 4, 0), coordinates);
        Assert.Contains(new Coordinate(0, 4, 0), coordinates);
        Assert.Contains(new Coordinate(1, 4, 0), coordinates);

        Assert.Contains(new Coordinate(-1, 5, 0), coordinates);
        Assert.Contains(new Coordinate(0, 5, 0), coordinates);
        Assert.Contains(new Coordinate(1, 5, 0), coordinates);
    }

    [Fact]
    public void Create_East_When_Length_Is_5_And_Spread_Is_3_Should_Match_Expected_Pattern()
    {
        var coordinates = SpreadEffect.Create(Direction.East, 5, 3);

        Assert.Equal(11, coordinates.Length);

        Assert.Contains(new Coordinate(1, 0, 0), coordinates);
        Assert.Contains(new Coordinate(2, 0, 0), coordinates);

        Assert.Contains(new Coordinate(3, -1, 0), coordinates);
        Assert.Contains(new Coordinate(3, 0, 0), coordinates);
        Assert.Contains(new Coordinate(3, 1, 0), coordinates);

        Assert.Contains(new Coordinate(4, -1, 0), coordinates);
        Assert.Contains(new Coordinate(4, 0, 0), coordinates);
        Assert.Contains(new Coordinate(4, 1, 0), coordinates);

        Assert.Contains(new Coordinate(5, -1, 0), coordinates);
        Assert.Contains(new Coordinate(5, 0, 0), coordinates);
        Assert.Contains(new Coordinate(5, 1, 0), coordinates);
    }

    [Fact]
    public void Create_West_When_Length_Is_5_And_Spread_Is_3_Should_Match_Expected_Pattern()
    {
        var coordinates = SpreadEffect.Create(Direction.West, 5, 3);

        Assert.Equal(11, coordinates.Length);

        Assert.Contains(new Coordinate(-1, 0, 0), coordinates);
        Assert.Contains(new Coordinate(-2, 0, 0), coordinates);

        Assert.Contains(new Coordinate(-3, 1, 0), coordinates);
        Assert.Contains(new Coordinate(-3, 0, 0), coordinates);
        Assert.Contains(new Coordinate(-3, -1, 0), coordinates);

        Assert.Contains(new Coordinate(-4, 1, 0), coordinates);
        Assert.Contains(new Coordinate(-4, 0, 0), coordinates);
        Assert.Contains(new Coordinate(-4, -1, 0), coordinates);

        Assert.Contains(new Coordinate(-5, 1, 0), coordinates);
        Assert.Contains(new Coordinate(-5, 0, 0), coordinates);
        Assert.Contains(new Coordinate(-5, -1, 0), coordinates);
    }

    [Fact]
    public void Create_All_Length_And_Spread_Combinations_From_0_To_8_Should_Not_Throw_Exception()
    {
        var directions = new[] { Direction.North, Direction.South, Direction.East, Direction.West };

        for (int length = 0; length <= 8; length++)
        {
            for (int spread = 0; spread <= 8; spread++)
            {
                foreach (var direction in directions)
                {
                    var exception = Record.Exception(() => SpreadEffect.Create(direction, length, spread));
                    Assert.Null(exception);
                }
            }
        }
    }
}