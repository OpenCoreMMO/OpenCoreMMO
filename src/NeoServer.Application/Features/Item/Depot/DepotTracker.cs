using NeoServer.Game.Common.Contracts.Items.Types.Containers;

namespace NeoServer.Application.Features.Item.Depot;

public class DepotTracker
{
    private readonly IDictionary<uint, IDepot> _depotMap = new Dictionary<uint, IDepot>();

    public void Load(uint playerId, IDepot depot)
    {
        _depotMap.TryAdd(playerId, depot);
    }

    public IDepot Get(uint playerId)
    {
        _depotMap.TryGetValue(playerId, out var depot);
        return depot;
    }

    public bool Get(uint playerId, out IDepot depot)
    {
        return _depotMap.TryGetValue(playerId, out depot);
    }

    public void Unload(uint playerId)
    {
        _depotMap.Remove(playerId);
    }
}