namespace NeoServer.Domain.Common;

public readonly ref struct MinMax
{
    public MinMax(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public MinMax(decimal min, decimal max)
    {
        Min = (int)Math.Round(min);
        Max = (int)Math.Round(max);
    }

    public MinMax(double min, double max)
    {
        Min = (int)Math.Round(min);
        Max = (int)Math.Round(max);
    }

    public static MinMax Zero => new(0, 0);

    public int Min { get; }
    public int Max { get; }
}