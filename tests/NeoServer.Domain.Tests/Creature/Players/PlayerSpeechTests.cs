using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerSpeechTests
{
    [Fact]
    public void Player_yell_should_be_heard_by_creatures_within_range()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 120, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build(name:"Speaker");
        speaker.SetNewLocation(new Location(105,105,7));
        
        var listener1 = PlayerTestDataBuilder.Build(name: "Listener1");
        listener1.SetNewLocation(new Location(115,105,7));
        
        var listener2 = PlayerTestDataBuilder.Build(name: "Listener2");
        listener2.SetNewLocation(new Location(120,105,7));
        
        map.PlaceCreature(speaker);
        map.PlaceCreature(listener1); 
        map.PlaceCreature(listener2);

        var creatureSayEventHandler = new CreatureSayEventHandler(map);
        speaker.OnSay += creatureSayEventHandler.Execute;

        var listener1Heard = false;
        var listener2Heard = false;

        listener1.OnHear += (_, _, _, _) => listener1Heard = true;
        listener2.OnHear += (_, _, _, _) => listener2Heard = true;

        //act
        speaker.Yell("Test yell message");

        //assert
        listener1Heard.Should().BeTrue("Listener within yell range should hear the yell");
        listener2Heard.Should().BeFalse("Listener outside yell range should not hear the yell");
    }

    [Fact]
    public void Player_whisper_should_be_heard_by_creatures_within_range()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        var listener1 = PlayerTestDataBuilder.Build();
        var listener2 = PlayerTestDataBuilder.Build();

        // Place speaker at (105,105)
        speaker.SetNewLocation(new Location(105,105,7));
        listener1.SetNewLocation(new Location(106,105,7));
        listener2.SetNewLocation(new Location(107,105,7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener1);
        map.PlaceCreature(listener2);

        var creatureSayEventHandler = new CreatureSayEventHandler(map);
        speaker.OnSay += creatureSayEventHandler.Execute;

        var listener1Heard = false;
        var listener2Heard = false;

        listener1.OnHear += (_, _, _, _) => listener1Heard = true;
        listener2.OnHear += (_, _, _, _) => listener2Heard = true;

        //act
        speaker.Whisper("Test whisper message");

        //assert
        listener1Heard.Should().BeTrue("Listener within whisper range should hear the whisper");
        listener2Heard.Should().BeFalse("Listener outside whisper range should not hear the whisper");
    }

    [Fact]
    public void Player_say_should_be_heard_by_visible_creatures()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var speaker = PlayerTestDataBuilder.Build();
        var listener1 = PlayerTestDataBuilder.Build();
        var listener2 = PlayerTestDataBuilder.Build();

        // Place speaker at (105,105)
        speaker.SetNewLocation(new Location(105,105,7));
        listener1.SetNewLocation(new Location(106,105,7));
        listener2.SetNewLocation(new Location(120,105,7));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener1);
        map.PlaceCreature(listener2);

        var creatureSayEventHandler = new CreatureSayEventHandler(map);
        speaker.OnSay += creatureSayEventHandler.Execute;

        var listener1Heard = false;
        var listener2Heard = false;

        listener1.OnHear += (_, _, _, _) => listener1Heard = true;
        listener2.OnHear += (_, _, _, _) => listener2Heard = true;

        //act
        speaker.Say("Test say message", SpeechType.Say);

        //assert
        listener1Heard.Should().BeTrue("Listener within say range and visible should hear the message");
        listener2Heard.Should().BeFalse("Listener outside say range should not hear the message");
    }

    [Fact]
    public void Player_yell_should_be_heard_across_floors()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);

        var speaker = PlayerTestDataBuilder.Build();
        var listener = PlayerTestDataBuilder.Build();

        // Place speaker at (105,105,7)
        speaker.SetNewLocation(new Location(105,105,7));
        listener.SetNewLocation(new Location(106,105,8));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener);

        var creatureSayEventHandler = new CreatureSayEventHandler(map);
        speaker.OnSay += creatureSayEventHandler.Execute;

        var listenerHeard = false;
        listener.OnHear += (_, _, _, _) => listenerHeard = true;

        //act
        speaker.Yell("Test yell message");

        //assert
        listenerHeard.Should().BeTrue("Listener on different floor should hear the yell");
    }

    [Fact]
    public void Player_whisper_should_not_be_heard_across_floors()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);

        var speaker = PlayerTestDataBuilder.Build();
        var listener = PlayerTestDataBuilder.Build();

        // Place speaker at (105,105,7)
        speaker.SetNewLocation(new Location(105,105,7));
        listener.SetNewLocation(new Location(106,105,8));

        map.PlaceCreature(speaker);
        map.PlaceCreature(listener);

        var creatureSayEventHandler = new CreatureSayEventHandler(map);
        speaker.OnSay += creatureSayEventHandler.Execute;

        var listenerHeard = false;
        listener.OnHear += (_, _, _, _) => listenerHeard = true;

        //act
        speaker.Whisper("Test whisper message");

        //assert
        listenerHeard.Should().BeFalse("Listener on different floor should not hear the whisper");
    }
}