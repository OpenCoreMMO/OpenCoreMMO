using NeoServer.Domain.Common.Combat.Structs;

namespace NeoServer.Domain.Common.Creatures.Structs;

public struct FormulaValues
{
    public FormulaType FormulaType { get; set; }
    public double MinA { get; set; }
    public double MinB { get; set; }
    public double MaxA { get; set; }
    public double MaxB { get; set; }
}