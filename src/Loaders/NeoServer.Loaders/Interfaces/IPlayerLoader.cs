using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Loaders.Interfaces;

public interface IPlayerLoader
{
    IPlayer Load(PlayerEntity playerEntity);
    bool IsApplicable(PlayerEntity player);
}