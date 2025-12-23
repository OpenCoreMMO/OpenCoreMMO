using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerStatsTests
{
    [Theory]
    [InlineData(100, 50)]
    [InlineData(1, 1)]
    [InlineData(0, 0)]
    public void HasEnoughMana_ReturnsTrue(ushort mana, ushort required)
    {
        var sut = PlayerTestDataBuilder.Build(mana: mana);
        var result = sut.HasEnoughMana(required);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(100, 150)]
    [InlineData(0, 1)]
    public void HasEnoughMana_ReturnsFalse(ushort mana, ushort required)
    {
        var sut = PlayerTestDataBuilder.Build(mana: mana);
        var result = sut.HasEnoughMana(required);
        result.Should().BeFalse();
    }


    [Theory]
    [InlineData(100, 50)]
    [InlineData(1, 1)]
    [InlineData(0, 0)]
    public void HasEnoughLevel_ReturnsTrue(ushort level, ushort required)
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            { SkillType.Level, new Skill(SkillType.Level, level) }
        });
        var result = sut.HasEnoughLevel(required);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(100, 150)]
    [InlineData(0, 1)]
    public void HasEnoughLevel_ReturnsFalse(ushort level, ushort required)
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            { SkillType.Level, new Skill(SkillType.Level, level) }
        });
        var result = sut.HasEnoughLevel(required);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(100, 200, 100)]
    [InlineData(1, 1, 0)]
    [InlineData(0, 1, 1)]
    public void ConsumeMana_ChangeManaPoints(ushort consume, ushort mana, ushort expectedMana)
    {
        var sut = PlayerTestDataBuilder.Build(mana: mana);

        sut.DecreaseMana(consume);

        sut.Mana.Should().Be(expectedMana);
    }

    [Fact]
    public void ConsumeMana_MoreThanAvailable_DontChange()
    {
        var sut = PlayerTestDataBuilder.Build(mana: 200);

        sut.DecreaseMana(300);
        sut.Mana.Should().Be(200);
    }
}