namespace NeoServer.Domain.Common.Helpers;

public static class MatrixExtensions
{
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