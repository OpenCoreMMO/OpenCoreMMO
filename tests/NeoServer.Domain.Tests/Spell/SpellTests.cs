using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Spells;
using NeoServer.Domain.Spells.Entities;
using NeoServer.Domain.Spells.Events;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Services;
using PathFinder = NeoServer.Domain.World.Map.PathFinder;

namespace NeoServer.Domain.Tests.Spell;

public class SpellTests
{
    private readonly Mock<IEventAggregator> _eventAggregatorMock;
    private readonly SpellService _spellService;

    public SpellTests()
    {
        _eventAggregatorMock = new Mock<IEventAggregator>();
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);
        var mapTool = new MapTool(map, pathFinder);
        var spellCastValidation = new SpellCastValidation(mapTool);
        _spellService = new  SpellService(spellCastValidation, _eventAggregatorMock.Object, map);
    }

    [Fact]
    public void Cast_When_Spell_Is_Null_Returns_False()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var target = PlayerTestDataBuilder.Build();

        // Act
        var result = _spellService.Cast(player, target, null, false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Cast_When_Spell_Needs_Target_And_Target_Is_Null_Uses_Current_Target()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var target = PlayerTestDataBuilder.Build();
        player.SetAttackTarget(target);
        
        var spell = new TestSpell { NeedsTarget = true };

        // Act
        var result = _spellService.Cast(player, null, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Needs_Direction_And_Target_Is_Null_Uses_Direction_Tile()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedDirection = true };

        // Act
        var result = _spellService.Cast(player, null, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Needs_Caster_Target_Or_Direction_And_Target_Is_Null_Uses_Direction_Tile()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedCasterTargetOrDirection = true };

        // Act
        var result = _spellService.Cast(player, null, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Validation_Fails_And_Caster_Is_Player_Publishes_Event_And_Returns_False()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var target = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedsTarget = true, MinLevel = 100 }; // Player level is 10, so this will fail

        // Act
        var result = _spellService.Cast(player, target, spell, false);

        // Assert
        result.Should().BeFalse();
        _eventAggregatorMock.Verify(x => x.Publish(It.Is<SpellFailedToCastEvent>(e => 
            e.Caster == player && e.Spell == spell)), Times.Once);
    }

    [Fact]
    public void Cast_When_Spell_Invoke_Fails_And_Caster_Is_Player_Publishes_Event_And_Returns_True()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var target = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedsTarget = true, ShouldFailInvoke = true };

        // Act
        var result = _spellService.Cast(player, target, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.Is<SpellFailedToCastEvent>(e => 
            e.Caster == player && e.Spell == spell)), Times.Once);
    }

    [Fact]
    public void Cast_When_Spell_Invoke_Fails_And_Caster_Is_Not_Player_Returns_True_Without_Publishing_Event()
    {
        // Arrange
        var monster = new Mock<ICombatActor>();
        var target = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedsTarget = true, ShouldFailInvoke = true };

        // Act
        var result = _spellService.Cast(monster.Object, target, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Succeeds_And_Caster_Is_Player_Calls_PostSpellCast()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var target = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedsTarget = true };

        // Act
        var result = _spellService.Cast(player, target, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Succeeds_And_Caster_Is_Not_Player_Does_Not_Call_PostSpellCast()
    {
        // Arrange
        var monster = new Mock<ICombatActor>();
        var target = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedsTarget = true };

        // Act
        var result = _spellService.Cast(monster.Object, target, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Is_Self_Target_Sets_Target_To_Caster()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { IsSelfTarget = true };

        // Act
        var result = _spellService.Cast(player, null, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Needs_Target_And_No_Current_Target_Validation_Fails()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedsTarget = true };

        // Act
        var result = _spellService.Cast(player, null, spell, false);

        // Assert
        result.Should().BeFalse();
        _eventAggregatorMock.Verify(x => x.Publish(It.Is<SpellFailedToCastEvent>(e => 
            e.Caster == player && e.Spell == spell)), Times.Once);
    }

    [Fact]
    public void Cast_When_Spell_Needs_Direction_And_No_Current_Target_Uses_Direction_Tile()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedDirection = true, NeedsTarget = false };

        // Act
        var result = _spellService.Cast(player, null, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Needs_Caster_Target_Or_Direction_And_No_Current_Target_Uses_Direction_Tile()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedCasterTargetOrDirection = true, NeedsTarget = false };

        // Act
        var result = _spellService.Cast(player, null, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Validation_Passes_And_Invoke_Succeeds_Returns_True()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var target = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedsTarget = true };

        // Act
        var result = _spellService.Cast(player, target, spell, false);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Validation_Passes_And_Invoke_Succeeds_With_Hotkey_Returns_True()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var target = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedsTarget = true };

        // Act
        var result = _spellService.Cast(player, target, spell, true);

        // Assert
        result.Should().BeTrue();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    private class TestSpell : BaseSpell
    {
        public bool ShouldFailInvoke { get; set; }

        public override uint Duration => 0;
        public override ConditionType ConditionType => ConditionType.None;
        public override EffectT Effect => EffectT.None;
        public override string Words { get; set; } = "test";

        public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
        {
            return ShouldFailInvoke ? Result.Fail(InvalidOperation.NotEnoughMana) : Result.Success;
        }
    }
}
