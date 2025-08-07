using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Common.Helpers;

public static class MatrixExtensions
{
    public static byte[,] Rotate(this byte[,] matrix, Direction direction)
    {
        if (IsCircularArea(matrix))
            return matrix;

        return direction switch
        {
            Direction.West => matrix,
            Direction.East => Rotate180(matrix),
            Direction.North => Rotate270(matrix),
            Direction.South => Rotate90(matrix),
            Direction.NorthWest => matrix,
            Direction.NorthEast => Mirror(matrix),
            Direction.SouthWest => Flip(matrix),
            Direction.SouthEast => Mirror(Flip(matrix)),
            _ => matrix
        };
    }

    private static byte[,] Mirror(byte[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var result = new byte[rows, cols];

        for (var y = 0; y < rows; y++)
        for (var x = 0; x < cols; x++)
            result[y, x] = matrix[y, cols - 1 - x];

        return result;
    }

    private static byte[,] Flip(byte[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var result = new byte[rows, cols];

        for (var y = 0; y < rows; y++)
        for (var x = 0; x < cols; x++)
            result[y, x] = matrix[rows - 1 - y, x];

        return result;
    }


    public static byte[,] Rotate90(byte[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var result = new byte[cols, rows];

        for (var i = 0; i < rows; ++i)
        for (var j = 0; j < cols; ++j)
            result[j, rows - i - 1] = matrix[i, j];

        return result;
    }

    public static byte[,] Rotate180(byte[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var result = new byte[rows, cols];

        for (var i = 0; i < rows; ++i)
        for (var j = 0; j < cols; ++j)
            result[rows - i - 1, cols - j - 1] = matrix[i, j];

        return result;
    }

    public static byte[,] Rotate270(byte[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var result = new byte[cols, rows];

        for (var i = 0; i < rows; ++i)
        for (var j = 0; j < cols; ++j)
            result[cols - j - 1, i] = matrix[i, j];

        return result;
    }

    // Rotação diagonal simplificada: espelha a matriz
    public static byte[,] Rotate45(byte[,] matrix, Direction diagonal)
    {
        var rot = diagonal switch
        {
            Direction.NorthEast => Rotate90(matrix),
            Direction.SouthEast => Rotate180(Rotate90(matrix)),
            Direction.SouthWest => Rotate180(Rotate270(matrix)),
            Direction.NorthWest => Rotate270(matrix),
            _ => matrix
        };

        return rot;
    }

    public static bool IsCircularArea(byte[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        // Deve ser quadrada e com dimensões ímpares
        if (rows != cols || rows % 2 == 0)
            return false;

        var center = rows / 2;

        if (matrix[center, center] != 3)
            return false;

        // Verifica simetria vertical e horizontal
        for (var y = 0; y < rows; y++)
        for (var x = 0; x < cols; x++)
        {
            if (matrix[y, x] != matrix[rows - 1 - y, x])
                return false;

            if (matrix[y, x] != matrix[y, cols - 1 - x])
                return false;
        }

        return true;
    }

    public static byte[,] Rotate(this byte[,] area)
    {
        var rows = area.GetLength(0);
        var columns = area.GetLength(1);

        var max = Math.Max(rows, columns);

        var rotatedArea = new byte[max, max];

        var dstOffset = 0;
        var srcOffset = 0;

        for (var i = 0; i < rows; i++)
        {
            Buffer.BlockCopy(area, srcOffset, rotatedArea, dstOffset, columns);
            dstOffset += rotatedArea.GetLength(1);
            srcOffset += columns;
        }

        var rotations = rotatedArea.GetLength(0);

        for (var i = 0; i < rotations / 2; i += 1)
        for (var j = i; j < rotations - i - 1; j += 1)
        {
            int temp = rotatedArea[i, j];
            rotatedArea[i, j] = rotatedArea[j, rotations - 1 - i];
            rotatedArea[j, rotations - 1 - i] = rotatedArea[rotations - 1 - i, rotations - 1 - j];
            rotatedArea[rotations - 1 - i, rotations - 1 - j] = rotatedArea[rotations - 1 - j, i];
            rotatedArea[rotations - 1 - j, i] = (byte)temp;
        }

        return rotatedArea;
    }
}