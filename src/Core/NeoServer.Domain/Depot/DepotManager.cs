namespace NeoServer.Domain.Depot;

public class DepotManager
{
    private readonly Dictionary<uint, Locker> _depotMap = new();

    public void Load(uint playerId, Locker locker)
    {
        _depotMap.TryAdd(playerId, locker);
    }

    public Locker Get(uint playerId)
    {
        _depotMap.TryGetValue(playerId, out var depot);
        return depot;
    }

    public bool Get(uint playerId, out Locker locker)
    {
        return _depotMap.TryGetValue(playerId, out locker);
    }

    public void Unload(uint playerId)
    {
        _depotMap.Remove(playerId);
    }
}