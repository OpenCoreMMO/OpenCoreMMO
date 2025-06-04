namespace NeoServer.Domain.Common.Helpers;

public class GameRandom : Random
{
    private static GameRandom _instance;

    public static GameRandom Random => _instance ??= new GameRandom();

    public double Next(double mu = 0, double sigma = 1)
    {
        var u1 = NextDouble();
        var u2 = NextDouble();

        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                            Math.Sin(2.0 * Math.PI * u2);

        var randNormal = mu + sigma * randStdNormal;

        return randNormal;
    }

    public T Next<T>(T[] values)
    {
        var randomValue = Next(0, maxValue: values.Length);
        return values[randomValue];
    }

    /// <summary>
    ///     Random value in a interval using gaussian
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public double NextInRange(double min, double max)
    {
        var diff = max - min;
        var gaussian = Next(0.5f, 0.25f);

        double increment;
        if (gaussian < 0.0)
            increment = diff / 2;
        else if (gaussian > 1.0)
            increment = (diff + 1) / 2;
        else
            increment = Math.Round(gaussian * diff);
        return min + increment;
    }
}