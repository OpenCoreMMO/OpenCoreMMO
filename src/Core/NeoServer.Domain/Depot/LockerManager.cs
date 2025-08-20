namespace NeoServer.Domain.Depot;

public class LockerManager
{
    private readonly Dictionary<uint, Locker> _lockerMap = new();
    private readonly Dictionary<uint, bool> _depotLoaded = new();

    public void Load(uint playerId, Locker locker) => _lockerMap.TryAdd(playerId, locker);

    public Locker Get(uint playerId)
    {
        _lockerMap.TryGetValue(playerId, out var depot);
        return depot;
    }

    public bool Get(uint playerId, out Locker locker) => _lockerMap.TryGetValue(playerId, out locker);

    public void Unload(uint playerId)
    {
        _lockerMap.Remove(playerId);
        _depotLoaded.Remove(playerId);
    }

    public void SetDepotAsLoaded(uint playerId) => _depotLoaded[playerId] = true;
    public bool IsDepotLoaded(uint playerId) => _depotLoaded.TryGetValue(playerId, out var isLoaded) && isLoaded;
}