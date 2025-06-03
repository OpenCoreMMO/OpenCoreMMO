using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.Items.Types;

public interface IFood : IItem
{
    public ushort Duration => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.Duration);
}