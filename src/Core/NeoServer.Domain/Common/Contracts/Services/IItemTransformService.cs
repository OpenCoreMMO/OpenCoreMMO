using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IItemTransformService
{
    Result<IItem> Transform(IPlayer by, IItem fromItem, ushort toItem);
    Result<IItem> Transform(IItem fromItem, ushort toItem);
}