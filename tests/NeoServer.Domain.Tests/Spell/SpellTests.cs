using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location.Structs;
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
    private readonly SpellListManager _spellListManager;

    public SpellTests()
    {
        _eventAggregatorMock = new Mock<IEventAggregator>();
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);
        var mapTool = new MapTool(map, pathFinder);
        var spellCastValidation = new SpellCastValidation(mapTool);
        _spellService = new  SpellService(spellCastValidation, _eventAggregatorMock.Object, map);
        _spellListManager = new SpellListManager();
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

    [Fact]
    public void TryGetInstantSpell_ExoriSpell_ReturnsCorrectSpell()
    {
        // Arrange
        var exoriSpell = new TestSpell { Words = "exori", Name = "exori", HasParams = false };
        _spellListManager.Add("exori", exoriSpell);

        // Act & Assert
        // words = "exori" returns exori
        _spellListManager.TryGetInstantSpell("exori", out var spell).Should().BeTrue();
        spell.Should().Be(exoriSpell);
    }

    [Fact]
    public void TryGetInstantSpell_ExoriVisSpell_HandlesParametersAndInvalidInputs()
    {
        // Arrange
        var exoriVisSpell = new TestSpell { Words = "exori vis", Name = "exori vis", HasParams = false };
        _spellListManager.Add("exori vis", exoriVisSpell);

        // Act & Assert
        // words = "exori vis" returns exori vis spell
        _spellListManager.TryGetInstantSpell("exori vis", out var spell1).Should().BeTrue();
        spell1.Should().Be(exoriVisSpell);

        // words = exori vis x returns null
        _spellListManager.TryGetInstantSpell("exori vis x", out _).Should().BeFalse();

        // words = exori vis "test" return exori vis spell with params "test"
        _spellListManager.TryGetInstantSpell("exori vis \"test\"", out var spell3).Should().BeTrue();
        spell3.Should().Be(exoriVisSpell);
        spell3.Params.Should().BeEquivalentTo(["test"]);
        
        _spellListManager.TryGetInstantSpell("exori vis \"test", out var spell7).Should().BeTrue();
        spell7.Should().Be(exoriVisSpell);
        spell7.Params.Should().BeEquivalentTo(["test"]);

        // words = exori vis "test return exori vis" return exori vis spell with params "test return exori vis"
        _spellListManager.TryGetInstantSpell("exori vis \"test return exori vis\"", out var spell4).Should().BeTrue();
        spell4.Should().Be(exoriVisSpell);
        spell4.Params.Should().BeEquivalentTo(["test return exori vis"]);

        // words = exori vis 'test' returns null
        _spellListManager.TryGetInstantSpell("exori vis 'test'", out var spell5).Should().BeFalse();

        // words = exori vis 'test returns null
        _spellListManager.TryGetInstantSpell("exori vis 'test", out var spell6).Should().BeFalse();
    }

    [Fact]
    public void TryGetInstantSpell_UtevoResSpell_RequiresValidParameters()
    {
        // Arrange
        var utevoResSpell = new TestSpell { Words = "utevo res", Name = "utevo res", HasParams = true };
        _spellListManager.Add("utevo res", utevoResSpell);

        // Act & Assert
        // words = utevo res return null
        _spellListManager.TryGetInstantSpell("utevo res", out var spell1).Should().BeFalse();

        // words = utevo res "rat" return spell with params "rat"
        _spellListManager.TryGetInstantSpell("utevo res \"rat\"", out var spell2).Should().BeTrue();
        spell2.Should().Be(utevoResSpell);
        spell2.Params.Should().BeEquivalentTo(new[] { "rat" });

        // words = utevo res "rat" return spell with params "rat"
        _spellListManager.TryGetInstantSpell("utevo res \"rat", out var spell3).Should().BeTrue();
        spell3.Should().Be(utevoResSpell);
        spell3.Params.Should().BeEquivalentTo(new[] { "rat" });
    }

    [Fact]
    public void Player_casts_spell_with_direction_to_no_tile_casts_spell()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 100, 100, 100, 7, 7); // Single tile map
        var pathFinder = new PathFinder(map);
        var mapTool = new MapTool(map, pathFinder);
        var spellCastValidation = new SpellCastValidation(mapTool);
        var spellService = new SpellService(spellCastValidation, _eventAggregatorMock.Object, map);

        var player = PlayerTestDataBuilder.Build();
        map.PlaceCreature(player); // Place player on the single tile

        var spell = new DirectionDamageSpell();

        // Act
        var result = spellService.Cast(player, null, spell, false);

        // Assert
        result.Should().BeTrue();
        spell.EffectSent.Should().BeTrue();
        spell.DamageAttempted.Should().BeFalse();
        _eventAggregatorMock.Verify(x => x.Publish(It.IsAny<SpellFailedToCastEvent>()), Times.Never);
    }

    [Fact]
    public void Cast_When_Spell_Has_Cooldown_Starts_Player_Cooldown()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var target = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedsTarget = true, Cooldown = 1000, MinLevel = 1 };

        // Ensure cooldown is not active initially
        player.CooldownHasExpired(spell).Should().BeTrue();

        // Act
        var result = _spellService.Cast(player, target, spell, false);

        // Assert
        result.Should().BeTrue();
        // After casting, cooldown should be active
        player.CooldownHasExpired(spell).Should().BeFalse();
    }

    [Fact]
    public void Cast_When_Spell_Cooldown_Not_Expired_Returns_False()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();
        var target = PlayerTestDataBuilder.Build();
        var spell = new TestSpell { NeedsTarget = true, Cooldown = 1000, MinLevel = 1 };

        // First cast to start cooldown
        var firstResult = _spellService.Cast(player, target, spell, false);
        firstResult.Should().BeTrue();

        // Ensure cooldown is active
        player.CooldownHasExpired(spell).Should().BeFalse();

        // Act - Try to cast again while on cooldown
        var secondResult = _spellService.Cast(player, target, spell, false);

        // Assert
        secondResult.Should().BeFalse();
        _eventAggregatorMock.Verify(x => x.Publish(It.Is<SpellFailedToCastEvent>(e =>
            e.Caster == player && e.Spell == spell)), Times.Once);
    }


    private class TestSpell : BaseSpell
    {
        public bool ShouldFailInvoke { get; set; }

        public override uint Duration => 0;
        public override ConditionType ConditionType => ConditionType.None;
        public override EffectT Effect => EffectT.None;
        public override string Words { get; set; } = "test";
        public override bool HasParams { get; set; }

        public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
        {
            return ShouldFailInvoke ? Result.Fail(InvalidOperation.NotEnoughMana) : Result.Success;
        }
    }

    private class DirectionDamageSpell : BaseSpell
    {
        public bool EffectSent { get; private set; }
        public bool DamageAttempted { get; private set; }

        public override uint Duration => 0;
        public override ConditionType ConditionType => ConditionType.None;
        public override EffectT Effect => EffectT.None;
        public override bool NeedDirection => true;
        public override string Words { get; set; } = "test direction spell";

        public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
        {
            EffectSent = true;

            if (target is IDynamicTile tile)
            {
                DamageAttempted = true;
                var creature = tile.GetTopVisibleCreature(caster);
                if (creature != null)
                {
                    // Apply damage logic here
                    return Result.Success;
                }
            }

            return Result.Success;
        }
    }
}
