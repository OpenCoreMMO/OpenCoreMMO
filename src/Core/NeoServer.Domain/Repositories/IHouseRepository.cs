using NeoServer.Domain.Houses;

namespace NeoServer.Domain.Repositories;

public interface IHouseRepository
{
    void Save(House house);
}
