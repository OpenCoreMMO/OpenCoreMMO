namespace NeoServer.Game.Common.Effects.Magical;

public static partial class AreaEffect
{
    [AreaEffect("AREA_WAVE3", true)] public static byte[,] Wave3 =
    {
        { 1, 1, 1 },
        { 1, 1, 1 },
        { 0, 1, 0 },
        { 0, 3, 0 }
    };
    
    [AreaEffect("AREA_SHORTWAVE3", true)] public static byte[,] ShortWave3 =
    {
        { 1, 1, 1 },
        { 1, 1, 1 },
        { 0, 3, 0 }
    };

    [AreaEffect("AREA_WAVE4", true)] public static byte[,] Wave4 =
    {
        { 1, 1, 1, 1, 1 },
        { 0, 1, 1, 1, 0 },
        { 0, 1, 1, 1, 0 },
        { 0, 0, 1, 0, 0 },
        { 0, 0, 3, 0, 0 }
    };

    [AreaEffect("AREA_WAVE6")] public static byte[,] Wave6 =
    {
        { 0, 0, 0 },
        { 1, 3, 1 },
        { 0, 0, 0 }
    };

    [AreaEffect("AREA_SQUAREWAVE5", true)] public static byte[,] SquareWave5 =
    {
        { 1, 1, 1 },
        { 1, 1, 1 },
        { 1, 1, 1 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 3, 0 }
    };

    [AreaEffect("AREA_SQUAREWAVE6", true)] public static byte[,] SquareWave6 =
    {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 0, 0, 1, 1, 1, 1, 1, 0, 0 },
        { 0, 0, 1, 1, 1, 1, 1, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 0, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 3, 0, 0, 0, 0 }
    };

    [AreaEffect("AREA_SQUAREWAVE7", true)] public static byte[,] SquareWave7 =
    {
        { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
        { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0 }
    };

    //Diagonal waves
    [AreaEffect("AREADIAGONAL_WAVE4", true)]
    public static byte[,] AreaDiagonalWave4 =
    {
        { 0, 0, 0, 0, 1, 0 },
        { 0, 0, 0, 1, 1, 0 },
        { 0, 0, 1, 1, 1, 0 },
        { 0, 1, 1, 1, 1, 0 },
        { 1, 1, 1, 1, 1, 0 },
        { 0, 0, 0, 0, 0, 3 }
    };

    [AreaEffect("AREADIAGONAL_SQUAREWAVE5", true)]
    public static byte[,] AreaDiagonalSquareWave5 =
    {
        { 1, 1, 1, 0, 0 },
        { 1, 1, 1, 0, 0 },
        { 1, 1, 1, 0, 0 },
        { 0, 0, 0, 1, 0 },
        { 0, 0, 0, 0, 3 }
    };

    [AreaEffect("AREADIAGONAL_WAVE6")] public static byte[,] AreaDiagonalWave6 =
    {
        { 0, 0, 1 },
        { 0, 3, 0 },
        { 1, 0, 0 }
    };

    //Beams
    [AreaEffect("AREA_BEAM1")] public static byte[,] Beam1 =
    {
        { 3 }
    };

    [AreaEffect("AREA_BEAM5", true)] public static byte[,] Beam5 =
    {
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 3 }
    };

    [AreaEffect("AREA_BEAM7", true)] public static byte[,] Beam7 =
    {
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 3 }
    };

    [AreaEffect("AREA_BEAM8", true)] public static byte[,] Beam8 =
    {
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 1 },
        { 3 }
    };

    //Diagonal Beams
    [AreaEffect("AREADIAGONAL_BEAM5", true)]
    public static byte[,] AreaDiagonalBeam5 =
    {
        { 1, 0, 0, 0, 0 },
        { 0, 1, 0, 0, 0 },
        { 0, 0, 1, 0, 0 },
        { 0, 0, 0, 1, 0 },
        { 0, 0, 0, 0, 3 }
    };

    [AreaEffect("AREADIAGONAL_BEAM7", true)]
    public static byte[,] AreaDiagonalBeam7 =
    {
        { 1, 0, 0, 0, 0, 0, 0 },
        { 0, 1, 0, 0, 0, 0, 0 },
        { 0, 0, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 1, 0, 0, 0 },
        { 0, 0, 0, 0, 1, 0, 0 },
        { 0, 0, 0, 0, 0, 1, 0 },
        { 0, 0, 0, 0, 0, 0, 3 }
    };

    //Circles
    [AreaEffect("AREA_CIRCLE2X2")] public static byte[,] Circle2X2 =
    {
        { 0, 1, 1, 1, 0 },
        { 1, 1, 1, 1, 1 },
        { 1, 1, 3, 1, 1 },
        { 1, 1, 1, 1, 1 },
        { 0, 1, 1, 1, 0 }
    };

    [AreaEffect("AREA_CIRCLE3X3")] public static byte[,] Circle3X3 =
    {
        { 0, 0, 1, 1, 1, 0, 0 },
        { 0, 1, 1, 1, 1, 1, 0 },
        { 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 3, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1 },
        { 0, 1, 1, 1, 1, 1, 0 },
        { 0, 0, 1, 1, 1, 0, 0 }
    };

    // Crosses
    [AreaEffect("AREA_CROSS1X1")] public static byte[,] Cross1X1 =
    {
        { 0, 1, 0 },
        { 1, 3, 1 },
        { 0, 1, 0 }
    };

    [AreaEffect("AREA_CIRCLE5X5")] public static byte[,] Circle5X5 =
    {
        { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 }
    };

    [AreaEffect("AREA_CIRCLE6X6")] public static byte[,] Circle6X6 =
    {
        { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 1, 1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1, 1 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 }
    };

    //Squares
    [AreaEffect("AREA_SQUARE1X1")] public static byte[,] Square1X1 =
    {
        { 1, 1, 1 },
        { 1, 3, 1 },
        { 1, 1, 1 }
    };

    //Walls
    [AreaEffect("AREA_WALLFIELD", true)] public static byte[,] WallField =
    {
        { 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0 },
        { 1, 1, 3, 1, 1 },
        { 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0 }
    };

    [AreaEffect("AREADIAGONAL_WALLFIELD", true)]
    public static byte[,] AreaDiagonalWallField =
    {
        { 0, 0, 0, 0, 1 },
        { 0, 0, 0, 1, 1 },
        { 0, 1, 3, 1, 0 },
        { 1, 1, 0, 0, 0 },
        { 1, 0, 0, 0, 0 }
    };
}