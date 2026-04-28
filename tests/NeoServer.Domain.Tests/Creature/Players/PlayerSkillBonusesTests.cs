using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerSkillBonusesTests
{
    [Fact]
    public void AddSkillBonus_0_KeepBonusTheSame()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 0);

        sut.GetSkillBonus(SkillType.Axe).Should().Be(0);
    }

    [Fact]
    public void AddSkillBonus_Add10ButMissingSkill_CreateOneAndAdd()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Sword, 10);

        sut.GetSkillBonus(SkillType.Sword).Should().Be(10);
    }

    [Fact]
    public void AddSkillBonus_0_DoNotCallEvent()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new(SkillType.Axe, 10)
        });

        var captured = new List<PlayerAddedSkillBonusEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<PlayerAddedSkillBonusEvent>(e => captured.Add(e));

        sut.AddSkillBonus(SkillType.Axe, 0);

        captured.Should().BeEmpty();
    }

    [Fact]
    public void AddSkillBonus_10_IncreaseBonusBy10()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new(SkillType.Axe, 10)
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
            [SkillType.Axe] = new(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 10);
        sut.GetSkillBonus(SkillType.Axe).Should().Be(10);

        var captured = new List<PlayerAddedSkillBonusEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<PlayerAddedSkillBonusEvent>(e => captured.Add(e));

        sut.AddSkillBonus(SkillType.Axe, 5);

        captured.Should().HaveCount(1);
        captured[0].Increase.Should().Be(5);
        captured[0].Player.Should().BeEquivalentTo(sut);
        captured[0].Type.Should().Be(SkillType.Axe);
    }

    [Fact]
    public void RemoveSkillBonus_0_KeepBonusTheSame()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new(SkillType.Axe, 10)
        });

        sut.RemoveSkillBonus(SkillType.Axe, 0);

        sut.GetSkillBonus(SkillType.Axe).Should().Be(0);
    }

    [Fact]
    public void RemoveSkillBonus_0_DoNotCallEvent()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new(SkillType.Axe, 10)
        });

        var captured = new List<PlayerRemovedSkillBonusEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<PlayerRemovedSkillBonusEvent>(e => captured.Add(e));

        sut.RemoveSkillBonus(SkillType.Axe, 0);

        captured.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSkillBonus_50_DecreaseBonusBy50()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new(SkillType.Axe, 10)
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
            [SkillType.Axe] = new(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 100);

        var captured = new List<PlayerRemovedSkillBonusEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<PlayerRemovedSkillBonusEvent>(e => captured.Add(e));

        sut.RemoveSkillBonus(SkillType.Axe, 5);

        captured.Should().HaveCount(1);
        captured[0].Decrease.Should().Be(5);
        captured[0].Player.Should().BeEquivalentTo(sut);
        captured[0].Type.Should().Be(SkillType.Axe);
    }

    [Fact]
    public void Skill_bonus_negative_should_remain_negative()
    {
        var sut = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new(SkillType.Axe, 10)
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
            [SkillType.Axe] = new(SkillType.Axe, 10)
        });

        sut.AddSkillBonus(SkillType.Axe, 10);

        sut.RemoveSkillBonus(SkillType.Axe, 20);
        sut.GetSkillBonus(SkillType.Axe).Should().Be(-10);
        sut.GetSkillLevel(SkillType.Axe).Should().Be(0);
    }
}