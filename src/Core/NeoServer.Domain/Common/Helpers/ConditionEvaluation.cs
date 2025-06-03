namespace NeoServer.Domain.Common.Helpers;

public class ConditionEvaluation
{
    public static bool And(params bool[] conditions)
    {
        foreach (var condition in conditions)
            if (condition is false)
                return false;
        return true;
    }
}