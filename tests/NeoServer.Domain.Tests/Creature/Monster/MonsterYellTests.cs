using AutoFixture;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Creatures.Monster.Combat;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Monster;

public class MonsterYellTests
{
    [Fact]
    public void Monster_doesnt_yell_if_has_no_voices()
    {
        var monster = MonsterTestDataBuilder.Build();
        monster.Metadata.Voices = [];
        monster.Metadata.VoiceConfig = new IntervalChance(100, 50);
        var yelled = false;
        EventAggregatorTestHelper.SetupEventAggregator<CreatureSayEvent>(_ => yelled = true);

        monster.Yell([]);

        yelled.Should().BeFalse();
    }

    [Fact]
    public void Monster_yells()
    {
        var sentence = new Fixture().Create<string>();

        var monster = MonsterTestDataBuilder.Build();
        monster.Metadata.Voices = [new Voice(sentence, SpeechType.Say)];
        monster.Metadata.VoiceConfig = new IntervalChance(100, 100);
        string yelledMessage = null;
        var listener = PlayerTestDataBuilder.Build();

        EventAggregatorTestHelper.SetupEventAggregator<CreatureSayEvent>(e => yelledMessage = e.Message);

        monster.Yell([listener]);

        yelledMessage.Should().Be(sentence);
    }
}
