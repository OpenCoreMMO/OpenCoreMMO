using System.Collections.Generic;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Services;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerSpeechTests
{
    [ThreadBlocking]
    [Fact]
    public void Player_yell_should_be_heard_by_creatures_within_range()
    {
        var map = MapTestDataBuilder.Build(100, 120, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build(name: "Speaker");
        speaker.SetNewLocation(new Location(105, 105, 7));

        var listener1 = PlayerTestDataBuilder.Build(name: "Listener1");
        listener1.SetNewLocation(new Location(115, 105, 7));

        var listener2 = PlayerTestDataBuilder.Build(name: "Listener2");
        listener2.SetNewLocation(new Location(120, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener1);
        map.PlaceCreature(listener2);

        var creatureSpeechService = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        creatureSpeechService.Speak(speaker, "Test yell message", SpeechType.Yell);

        heardEvents.Should().Contain(e => e.Receiver == listener1, "Listener within yell range should hear the yell");
        heardEvents.Should().NotContain(e => e.Receiver == listener2, "Listener outside yell range should not hear the yell");
    }

    [ThreadBlocking]
    [Fact]
    public void Player_whisper_should_be_heard_by_creatures_within_range()
    {
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        var listener1 = PlayerTestDataBuilder.Build();
        var listener2 = PlayerTestDataBuilder.Build();

        speaker.SetNewLocation(new Location(105, 105, 7));
        listener1.SetNewLocation(new Location(106, 105, 7));
        listener2.SetNewLocation(new Location(107, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener1);
        map.PlaceCreature(listener2);

        var creatureSpeechService = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        creatureSpeechService.Speak(speaker, "Test whisper message", SpeechType.Whisper);

        heardEvents.Should().Contain(e => e.Receiver == listener1, "Listener within whisper range should hear the whisper");
        heardEvents.Should().NotContain(e => e.Receiver == listener2, "Listener outside whisper range should not hear the whisper");
    }

    [ThreadBlocking]
    [Fact]
    public void Player_say_should_be_heard_by_visible_creatures()
    {
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        var listener1 = PlayerTestDataBuilder.Build();
        var listener2 = PlayerTestDataBuilder.Build();

        speaker.SetNewLocation(new Location(105, 105, 7));
        listener1.SetNewLocation(new Location(106, 105, 7));
        listener2.SetNewLocation(new Location(120, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener1);
        map.PlaceCreature(listener2);

        var creatureSpeechService = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        creatureSpeechService.Speak(speaker, "Test say message", SpeechType.Say);

        heardEvents.Should().Contain(e => e.Receiver == listener1, "Listener within say range and visible should hear the message");
        heardEvents.Should().NotContain(e => e.Receiver == listener2, "Listener outside say range should not hear the message");
    }

    [ThreadBlocking]
    [Fact]
    public void Player_yell_should_be_heard_across_floors()
    {
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);

        var speaker = PlayerTestDataBuilder.Build();
        var listener = PlayerTestDataBuilder.Build();

        speaker.SetNewLocation(new Location(105, 105, 7));
        listener.SetNewLocation(new Location(106, 105, 8));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener);

        var creatureSpeechService = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        creatureSpeechService.Speak(speaker, "Test yell message", SpeechType.Yell);

        heardEvents.Should().Contain(e => e.Receiver == listener, "Listener on different floor should hear the yell");
    }

    [ThreadBlocking]
    [Fact]
    public void Player_whisper_should_not_be_heard_across_floors()
    {
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);

        var speaker = PlayerTestDataBuilder.Build();
        var listener = PlayerTestDataBuilder.Build();

        speaker.SetNewLocation(new Location(105, 105, 7));
        listener.SetNewLocation(new Location(106, 105, 8));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener);

        var creatureSpeechService = new CreatureSpeechService(map);

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        creatureSpeechService.Speak(speaker, "Test whisper message", SpeechType.Whisper);

        heardEvents.Should().NotContain(e => e.Receiver == listener, "Listener on different floor should not hear the whisper");
    }
}
