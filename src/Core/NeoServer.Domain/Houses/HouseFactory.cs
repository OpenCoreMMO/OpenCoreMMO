namespace NeoServer.Domain.Houses;

public class HouseFactory : IHouseFactory
{
    public House Create(uint id, string name, ushort townId, uint rent, uint ownerGuid, string ownerName, int ownerAccountId, DateTime? paidUntil, byte payRentWarnings)
    {
        return new House
        {
            Id = id,
            Name = name,
            TownId = townId,
            Rent = rent,
            OwnerGuid = ownerGuid,
            OwnerName = ownerGuid != 0 ? ownerName : string.Empty,
            OwnerAccountId = ownerAccountId,
            PaidUntil = paidUntil,
            PayRentWarnings = payRentWarnings
        };
    }
}
