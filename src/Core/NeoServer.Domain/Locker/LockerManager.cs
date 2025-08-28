namespace NeoServer.Domain.Locker;

public class LockerManager
{
    private readonly HashSet<uint> _depotLoaded = [];
    private readonly Dictionary<uint, Locker> _lockerMap = new();
    private readonly HashSet<uint> _mailInboxLoaded = [];

    public void Load(uint playerId, Locker locker)
    {
        _lockerMap.TryAdd(playerId, locker);
    }

    public Locker Get(uint playerId)
    {
        _lockerMap.TryGetValue(playerId, out var depot);
        return depot;
    }

    public bool Get(uint playerId, out Locker locker)
    {
        return _lockerMap.TryGetValue(playerId, out locker);
    }

    public void Unload(uint playerId)
    {
        _lockerMap.Remove(playerId);
        _depotLoaded.Remove(playerId);
        _mailInboxLoaded.Remove(playerId);
    }

    public void SetDepotAsLoaded(uint playerId)
    {
        _depotLoaded.Add(playerId);
    }

    public bool IsDepotLoaded(uint playerId)
    {
        return _depotLoaded.Contains(playerId);
    }

    public void SetMailboxAsLoaded(uint playerId)
    {
        _mailInboxLoaded.Add(playerId);
    }

    public bool IsMailboxLoaded(uint playerId)
    {
        return _mailInboxLoaded.Contains(playerId);
    }
}