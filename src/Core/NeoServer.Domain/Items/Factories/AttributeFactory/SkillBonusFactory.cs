using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Domain.Items.Factories.AttributeFactory;

public class SkillBonusFactory : IFactory
{
    public event CreateItem OnItemCreated;

    public ISkillBonus Create(IItemType itemType)
    {
        //if (itemType.Attributes.SkillBonuses is not { } skillBonuses) return null;
        //if (!skillBonuses.Any()) return null;

        //return new SkillBonus();
        return null;
    }
}