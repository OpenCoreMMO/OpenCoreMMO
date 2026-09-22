namespace NeoServer.Domain.Houses;

/// <summary>Creates a House aggregate from its stored representation.</summary>
public interface IHouseFactory
{
    House Create(uint id, string name, ushort townId, uint rent, uint ownerGuid, string ownerName, int ownerAccountId, DateTime? paidUntil, byte payRentWarnings);
}
