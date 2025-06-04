namespace NeoServer.Domain.Depot;

public class DepotManager
{
    private readonly IDictionary<uint, Depot> _depotMap = new Dictionary<uint, Depot>();

    public void Load(uint playerId, Depot depot)
    {
        _depotMap.TryAdd(playerId, depot);
    }

    public Depot Get(uint playerId)
    {
        _depotMap.TryGetValue(playerId, out var depot);
        return depot;
    }

    public bool Get(uint playerId, out Depot depot)
    {
        return _depotMap.TryGetValue(playerId, out depot);
    }

    public void Unload(uint playerId)
    {
        _depotMap.Remove(playerId);
    }
}