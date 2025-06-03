namespace NeoServer.Domain.Common.Contracts.Creatures.Players;

public interface IVip
{
    HashSet<uint> VipList { get; set; }
    void LoadVipList(IEnumerable<(uint, string)> vips);
    bool AddToVip(IPlayer player);
    void RemoveFromVip(uint playerId);
    bool HasInVipList(uint playerId);
    event AddToVipList OnAddedToVipList;
    event PlayerLoadVipList OnLoadedVipList;
}