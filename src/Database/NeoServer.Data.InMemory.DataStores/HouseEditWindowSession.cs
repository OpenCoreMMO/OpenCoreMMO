namespace NeoServer.Data.InMemory.DataStores;

/// <summary>
///     In-memory house access-list edit session (window id, house, list).
/// </summary>
public record HouseEditWindowSession(uint WindowTextId, uint HouseId, uint ListId);
