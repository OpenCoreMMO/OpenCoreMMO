namespace NeoServer.Domain.Common.Creatures.Structs;

public readonly ref struct MovementParams
{
    public MovementParams(Location.Structs.Location fromLocation, Location.Structs.Location toLocation, byte amount)
    {
        FromLocation = fromLocation;
        ToLocation = toLocation;
        Amount = amount;
    }

    public Location.Structs.Location FromLocation { get; }
    public Location.Structs.Location ToLocation { get; }
    public byte Amount { get; }
}