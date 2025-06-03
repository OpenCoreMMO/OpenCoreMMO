using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Contracts;

public interface IFactory
{
    public event CreateItem OnItemCreated;
}