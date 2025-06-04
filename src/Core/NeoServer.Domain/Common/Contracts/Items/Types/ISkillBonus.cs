using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Items.Types;

public interface ISkillBonus
{
    void AddSkillBonus(IPlayer player);
    void RemoveSkillBonus(IPlayer player);
}