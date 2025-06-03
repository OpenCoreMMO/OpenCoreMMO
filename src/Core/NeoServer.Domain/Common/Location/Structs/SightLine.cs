namespace NeoServer.Domain.Common.Location.Structs;

[Flags]
public enum SightLine
{
    NoCheck = 0,
    CheckSightLine = 1 << 0,
    FloorCheck = 1 << 1,
    CheckSightLineAndFloor = CheckSightLine | FloorCheck
}