using System.Text;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Parsers;

namespace NeoServer.Domain.Items.Items.Attributes;

public sealed class SkillBonus : ISkillBonus
{
    private readonly IItem _item;

    public SkillBonus(IItem item)
    {
        _item = item;
    }

    private Dictionary<SkillType, sbyte> SkillBonuses => _item.Metadata.Attributes.SkillBonuses;

    public void AddSkillBonus(IPlayer player)
    {
        if (Guard.AnyNull(SkillBonuses, player)) return;
        foreach (var (skillType, bonus) in SkillBonuses) player.AddSkillBonus(skillType, bonus);
    }

    public void RemoveSkillBonus(IPlayer player)
    {
        if (Guard.AnyNull(SkillBonuses, player)) return;
        foreach (var (skillType, bonus) in SkillBonuses) player.RemoveSkillBonus(skillType, bonus);
    }

    public override string ToString()
    {
        if (Guard.AnyNullOrEmpty(SkillBonuses)) return string.Empty;

        var stringBuilder = new StringBuilder();
        foreach (var (skillType, value) in SkillBonuses)
        {
            if (value == 0) continue;
            stringBuilder.Append($"{SkillTypeParser.Parse(skillType).ToLower()} +{value}, ");
        }

        if (stringBuilder.Length < 2) return string.Empty;

        stringBuilder.Remove(stringBuilder.Length - 2, 2);
        return stringBuilder.ToString();
    }
}