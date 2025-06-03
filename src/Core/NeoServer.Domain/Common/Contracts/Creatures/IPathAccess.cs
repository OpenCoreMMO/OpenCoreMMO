namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface IPathAccess
{
    PathFinder FindPathToDestination { get; }
    CanGoToDirection CanGoToDirection { get; }
}