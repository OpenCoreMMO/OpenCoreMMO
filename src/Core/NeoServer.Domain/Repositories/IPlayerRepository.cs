namespace NeoServer.Domain.Repositories;

public interface IPlayerRepository
{
    int GetIdByName(string name);
}