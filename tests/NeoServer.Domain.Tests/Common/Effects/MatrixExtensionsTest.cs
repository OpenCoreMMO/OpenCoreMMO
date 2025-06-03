using FluentAssertions;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Tests.Common.Effects;

public class MatrixExtensionsTest
{
    [Fact]
    public void Rotate_area()
    {
        //arrange
        byte[,] area =
        {
            { 1, 1, 1, 1, 1 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 0, 3, 0, 0 },
            { 0, 0, 0, 0, 0 }
        };

        byte[,] expected =
        {
            { 1, 0, 0, 0, 0 },
            { 1, 1, 1, 0, 0 },
            { 1, 1, 1, 3, 0 },
            { 1, 1, 1, 0, 0 },
            { 1, 0, 0, 0, 0 }
        };

        //act
        var result = area.Rotate();

        //assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Rotate_area_with_different_column_and_rows()
    {
        //arrange
        byte[,] area =
        {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 3, 0 }
        };

        byte[,] expected =
        {
            { 0, 0, 0, 0 },
            { 1, 1, 0, 0 },
            { 1, 1, 1, 3 },
            { 1, 1, 0, 0 }
        };

        //act
        var result = area.Rotate();

        //assert
        result.Should().BeEquivalentTo(expected);
    }
}