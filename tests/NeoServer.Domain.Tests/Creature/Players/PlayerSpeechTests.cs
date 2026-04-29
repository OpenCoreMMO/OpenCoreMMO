using System.Collections.Generic;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Events;
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
        //arrange
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

        var creatureSayEventHandler = new CreatureSayEventHandler(map);
        speaker.OnSay += creatureSayEventHandler.Execute;

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        var yellConfiguration = new YellConfiguration
        {
            YellAllowedPremium = true,
            YellCooldownSeconds = 30_000,
            YellMinimumLevel = 2
        };

        //act
        speaker.Yell("Test yell message", yellConfiguration);

        //assert
        heardEvents.Should().Contain(e => e.Receiver == listener1, "Listener within yell range should hear the yell");
        heardEvents.Should().NotContain(e => e.Receiver == listener2, "Listener outside yell range should not hear the yell");
    }

    [ThreadBlocking]
    [Fact]
    public void Player_whisper_should_be_heard_by_creatures_within_range()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        var listener1 = PlayerTestDataBuilder.Build();
        var listener2 = PlayerTestDataBuilder.Build();

        // Place speaker at (105,105)
        speaker.SetNewLocation(new Location(105, 105, 7));
        listener1.SetNewLocation(new Location(106, 105, 7));
        listener2.SetNewLocation(new Location(107, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener1);
        map.PlaceCreature(listener2);

        var creatureSayEventHandler = new CreatureSayEventHandler(map);
        speaker.OnSay += creatureSayEventHandler.Execute;

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        //act
        speaker.Whisper("Test whisper message");

        //assert
        heardEvents.Should().Contain(e => e.Receiver == listener1, "Listener within whisper range should hear the whisper");
        heardEvents.Should().NotContain(e => e.Receiver == listener2, "Listener outside whisper range should not hear the whisper");
    }

    [ThreadBlocking]
    [Fact]
    public void Player_say_should_be_heard_by_visible_creatures()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        var listener1 = PlayerTestDataBuilder.Build();
        var listener2 = PlayerTestDataBuilder.Build();

        // Place speaker at (105,105)
        speaker.SetNewLocation(new Location(105, 105, 7));
        listener1.SetNewLocation(new Location(106, 105, 7));
        listener2.SetNewLocation(new Location(120, 105, 7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener1);
        map.PlaceCreature(listener2);

        var creatureSayEventHandler = new CreatureSayEventHandler(map);
        speaker.OnSay += creatureSayEventHandler.Execute;

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        //act
        speaker.Say("Test say message", SpeechType.Say);

        //assert
        heardEvents.Should().Contain(e => e.Receiver == listener1, "Listener within say range and visible should hear the message");
        heardEvents.Should().NotContain(e => e.Receiver == listener2, "Listener outside say range should not hear the message");
    }

    [ThreadBlocking]
    [Fact]
    public void Player_yell_should_be_heard_across_floors()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);

        var speaker = PlayerTestDataBuilder.Build();
        var listener = PlayerTestDataBuilder.Build();

        // Place speaker at (105,105,7)
        speaker.SetNewLocation(new Location(105, 105, 7));
        listener.SetNewLocation(new Location(106, 105, 8));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener);

        var creatureSayEventHandler = new CreatureSayEventHandler(map);
        speaker.OnSay += creatureSayEventHandler.Execute;

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        var yellConfiguration = new YellConfiguration
        {
            YellAllowedPremium = true,
            YellCooldownSeconds = 30_000,
            YellMinimumLevel = 2
        };

        //act
        speaker.Yell("Test yell message", yellConfiguration);

        //assert
        heardEvents.Should().Contain(e => e.Receiver == listener, "Listener on different floor should hear the yell");
    }

    [ThreadBlocking]
    [Fact]
    public void Player_whisper_should_not_be_heard_across_floors()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);

        var speaker = PlayerTestDataBuilder.Build();
        var listener = PlayerTestDataBuilder.Build();

        // Place speaker at (105,105,7)
        speaker.SetNewLocation(new Location(105, 105, 7));
        listener.SetNewLocation(new Location(106, 105, 8));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener);

        var creatureSayEventHandler = new CreatureSayEventHandler(map);
        speaker.OnSay += creatureSayEventHandler.Execute;

        var heardEvents = new List<CreatureHearEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureHearEvent>(e => heardEvents.Add(e));

        //act
        speaker.Whisper("Test whisper message");

        //assert
        heardEvents.Should().NotContain(e => e.Receiver == listener, "Listener on different floor should not hear the whisper");
    }

}
