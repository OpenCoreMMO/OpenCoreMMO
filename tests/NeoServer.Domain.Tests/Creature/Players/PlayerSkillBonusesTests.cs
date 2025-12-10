using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerSkillBonusesTests
{
    [Fact]
    public void AddSkillBonus_0_KeepBonusTheSame()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 0);

        sut.GetSkillBonus(SkillType.Axe).Should().Be(0);
    }

    [Fact]
    public void AddSkillBonus_Add10ButMissingSkill_CreateOneAndAdd()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Sword, 10);

        sut.GetSkillBonus(SkillType.Sword).Should().Be(10);
    }

    [Fact]
    public void AddSkillBonus_0_DoNotCallEvent()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        var called = false;
        sut.OnAddedSkillBonus += (_, _, _) => { called = true; };

        sut.AddSkillBonus(SkillType.Axe, 0);

        called.Should().BeFalse();
    }

    [Fact]
    public void AddSkillBonus_10_IncreaseBonusBy10()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 10);
        sut.GetSkillBonus(SkillType.Axe).Should().Be(10);

        sut.AddSkillBonus(SkillType.Axe, 5);
        sut.GetSkillBonus(SkillType.Axe).Should().Be(15);
    }

    [Fact]
    public void AddSkillBonus_10_CallEvent()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 10);
        sut.GetSkillBonus(SkillType.Axe).Should().Be(10);

        var eventEncreased = 0;
        IPlayer eventPlayer = null;
        sut.OnAddedSkillBonus += (player, _, increased) =>
        {
            eventPlayer = player;
            eventEncreased = increased;
        };

        sut.AddSkillBonus(SkillType.Axe, 5);
        eventEncreased.Should().Be(5);
        eventPlayer.Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void RemoveSkillBonus_0_KeepBonusTheSame()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        sut.RemoveSkillBonus(SkillType.Axe, 0);

        sut.GetSkillBonus(SkillType.Axe).Should().Be(0);
    }

    [Fact]
    public void RemoveSkillBonus_0_DoNotCallEvent()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        var called = false;
        sut.OnRemovedSkillBonus += (_, _, _) => { called = true; };

        sut.RemoveSkillBonus(SkillType.Axe, 0);

        called.Should().BeFalse();
    }

    [Fact]
    public void RemoveSkillBonus_50_DecreaseBonusBy50()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 100);

        sut.RemoveSkillBonus(SkillType.Axe, 50);
        sut.GetSkillBonus(SkillType.Axe).Should().Be(50);
    }

    [Fact]
    public void RemoveSkillBonus_5_CallEvent()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 100);

        var eventDecreased = 0;
        IPlayer eventPlayer = null;
        sut.OnRemovedSkillBonus += (player, _, decreased) =>
        {
            eventPlayer = player;
            eventDecreased = decreased;
        };

        sut.RemoveSkillBonus(SkillType.Axe, 5);
        eventDecreased.Should().Be(5);
        eventPlayer.Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void Skill_bonus_negative_should_remain_negative()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 10);

        sut.RemoveSkillBonus(SkillType.Axe, 20);
        sut.GetSkillBonus(SkillType.Axe).Should().Be(-10);
    }

    [Fact]
    public void Add_negative_skill_bonus_never_turn_skill_to_negative()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 10);

        sut.RemoveSkillBonus(SkillType.Axe, 20);
        sut.GetSkillBonus(SkillType.Axe).Should().Be(-10);
        sut.GetSkillLevel(SkillType.Axe).Should().Be(0);
    }
}