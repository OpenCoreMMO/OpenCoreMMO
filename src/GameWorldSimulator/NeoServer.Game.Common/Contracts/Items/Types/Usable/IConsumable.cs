using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Common.Contracts.Items.Types.Usable;

public delegate void Use(ICreature usedBy, ICreature creature, IItem item);

public interface IConsumable : IConsumableRequirement, IUsableOnCreature
{
    public string Sentence => Metadata.Attributes.GetAttribute(ItemAttribute.Sentence);
    public static event Use OnUsed;
}