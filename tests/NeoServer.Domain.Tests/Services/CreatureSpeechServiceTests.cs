using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Services;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Services;

public class CreatureSpeechServiceTests
{
    #region Speak Validation Tests

    [Fact]
    [Trait("Category", "Validation")]
    public void Speak_does_nothing_when_sender_is_null()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var service = new CreatureSpeechService(map);
        var eventFired = false;
        EventAggregatorTestHelper.SetupEventAggregator<CreatureSayEvent>(_ => eventFired = true);

        // Act
        service.Speak(null, "Hello", SpeechType.Say);

        // Assert
        eventFired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Speak_does_nothing_when_message_is_null()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var service = new CreatureSpeechService(map);
        var sender = PlayerTestDataBuilder.Build();
        var eventFired = false;
        EventAggregatorTestHelper.SetupEventAggregator<CreatureSayEvent>(_ => eventFired = true);

        // Act
        service.Speak(sender, null, SpeechType.Say);

        // Assert
        eventFired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Speak_does_nothing_when_message_is_empty()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var service = new CreatureSpeechService(map);
        var sender = PlayerTestDataBuilder.Build();
        var eventFired = false;
        EventAggregatorTestHelper.SetupEventAggregator<CreatureSayEvent>(_ => eventFired = true);

        // Act
        service.Speak(sender, string.Empty, SpeechType.Say);

        // Assert
        eventFired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Speak_does_nothing_when_message_is_whitespace()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var service = new CreatureSpeechService(map);
        var sender = PlayerTestDataBuilder.Build();
        var eventFired = false;
        EventAggregatorTestHelper.SetupEventAggregator<CreatureSayEvent>(_ => eventFired = true);

        // Act
        service.Speak(sender, "   ", SpeechType.Say);

        // Assert
        eventFired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Speak_does_nothing_when_talkType_is_none()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var service = new CreatureSpeechService(map);
        var sender = PlayerTestDataBuilder.Build();
        var eventFired = false;
        EventAggregatorTestHelper.SetupEventAggregator<CreatureSayEvent>(_ => eventFired = true);

        // Act
        service.Speak(sender, "Hello", SpeechType.None);

        // Assert
        eventFired.Should().BeFalse();
    }

    #endregion

    #region Speak Broadcast Tests

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Speak_broadcasts_to_spectators_within_say_range()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        speaker.SetNewLocation(new Location(105, 105, 7));

        var listener = PlayerTestDataBuilder.Build();
        listener.SetNewLocation(new Location(106, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener);

        var service = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        // Act
        service.Speak(speaker, "Hello", SpeechType.Say);

        // Assert
        heardEvents.Should().Contain(e => e.Receiver == listener, "listener within say range should hear the message");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Speak_does_not_broadcast_to_creatures_outside_range()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 120, 100, 120, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        speaker.SetNewLocation(new Location(105, 105, 7));

        var farListener = PlayerTestDataBuilder.Build();
        farListener.SetNewLocation(new Location(119, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(farListener);

        var service = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        // Act
        service.Speak(speaker, "Hello", SpeechType.Say);

        // Assert
        heardEvents.Should().NotContain(e => e.Receiver == farListener, "far creature outside say range should not hear");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Speak_with_yell_broadcasts_to_creatures_on_different_floors()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);

        var speaker = PlayerTestDataBuilder.Build();
        speaker.SetNewLocation(new Location(105, 105, 7));

        var listener = PlayerTestDataBuilder.Build();
        listener.SetNewLocation(new Location(105, 105, 8));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener);

        var service = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        // Act
        service.Speak(speaker, "Yell!", SpeechType.Yell);

        // Assert
        heardEvents.Should().Contain(e => e.Receiver == listener, "listener on different floor should hear the yell");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Speak_with_whisper_does_not_broadcast_to_different_floors()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);

        var speaker = PlayerTestDataBuilder.Build();
        speaker.SetNewLocation(new Location(105, 105, 7));

        var listener = PlayerTestDataBuilder.Build();
        listener.SetNewLocation(new Location(105, 105, 8));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener);

        var service = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        // Act
        service.Speak(speaker, "Whisper", SpeechType.Whisper);

        // Assert
        heardEvents.Should().NotContain(e => e.Receiver == listener, "listener on different floor should not hear the whisper");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Speak_with_yell_has_extended_range()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 130, 100, 130, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        speaker.SetNewLocation(new Location(105, 105, 7));

        // Yell range covers ~18 tiles on X axis
        var farListener = PlayerTestDataBuilder.Build();
        farListener.SetNewLocation(new Location(115, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(farListener);

        var service = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        // Act
        service.Speak(speaker, "Yell!", SpeechType.Yell);

        // Assert
        heardEvents.Should().Contain(e => e.Receiver == farListener, "listener 10 tiles away should hear the yell");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Speak_with_monster_yell_has_extended_range()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 130, 100, 130, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        speaker.SetNewLocation(new Location(105, 105, 7));

        var farListener = PlayerTestDataBuilder.Build();
        farListener.SetNewLocation(new Location(115, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(farListener);

        var service = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        // Act
        service.Speak(speaker, "Monster yell!", SpeechType.MonsterYell);

        // Assert
        heardEvents.Should().Contain(e => e.Receiver == farListener, "listener 10 tiles away should hear the monster yell");
    }

    #endregion

    #region GetYellSpectators Tests

    [Fact]
    [Trait("Category", "HappyPath")]
    public void GetYellSpectators_returns_spectators_within_yell_range()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 130, 100, 130, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        speaker.SetNewLocation(new Location(105, 105, 7));

        var nearListener = PlayerTestDataBuilder.Build();
        nearListener.SetNewLocation(new Location(106, 105, 7));

        var farListener = PlayerTestDataBuilder.Build();
        farListener.SetNewLocation(new Location(115, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(nearListener);
        map.PlaceCreature(farListener);

        var service = new CreatureSpeechService(map);

        // Act
        var spectators = service.GetYellSpectators(speaker);

        // Assert
        spectators.Should().Contain(nearListener, "near creature should be a yell spectator");
        spectators.Should().Contain(farListener, "far creature within yell range should be a yell spectator");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void GetYellSpectators_returns_empty_when_no_creatures_nearby()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        speaker.SetNewLocation(new Location(100, 100, 7));

        // speaker is not placed on the map via PlaceCreature, so no creatures exist in the map

        var service = new CreatureSpeechService(map);

        // Act
        var spectators = service.GetYellSpectators(speaker);

        // Assert
        spectators.Should().BeEmpty("no creatures are placed on the map");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void GetYellSpectators_returns_other_creatures_not_the_speaker()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        speaker.SetNewLocation(new Location(105, 105, 7));

        var listener = PlayerTestDataBuilder.Build();
        listener.SetNewLocation(new Location(106, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener);

        var service = new CreatureSpeechService(map);

        // Act
        var spectators = service.GetYellSpectators(speaker);

        // Assert
        spectators.Should().Contain(listener, "nearby listener should be a yell spectator");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void GetYellSpectators_includes_creatures_on_different_floors()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);

        var speaker = PlayerTestDataBuilder.Build();
        speaker.SetNewLocation(new Location(105, 105, 7));

        var listenerSameFloor = PlayerTestDataBuilder.Build();
        listenerSameFloor.SetNewLocation(new Location(106, 105, 7));

        var listenerOtherFloor = PlayerTestDataBuilder.Build();
        listenerOtherFloor.SetNewLocation(new Location(105, 105, 8));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listenerSameFloor);
        map.PlaceCreature(listenerOtherFloor);

        var service = new CreatureSpeechService(map);

        // Act
        var spectators = service.GetYellSpectators(speaker);

        // Assert
        spectators.Should().Contain(listenerSameFloor, "creature on same floor should be a yell spectator");
        spectators.Should().Contain(listenerOtherFloor, "creature on different floor should be a yell spectator");
    }

    #endregion
}
