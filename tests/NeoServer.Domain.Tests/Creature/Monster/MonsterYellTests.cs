using AutoFixture;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Monster.Combat;
using NeoServer.Domain.Tests.Helpers;

namespace NeoServer.Domain.Tests.Creature.Monster;

public class MonsterYellTests
{
    [Fact]
    public void Monster_doesnt_yell_if_has_no_voices()
    {
        //arrange
        var monster = MonsterTestDataBuilder.Build();
        monster.Metadata.Voices = Array.Empty<Voice>();
        monster.Metadata.VoiceConfig = new IntervalChance(100, 50);
        using var monitor = monster.Monitor();

        //act
        monster.Yell();

        //assert
        monitor.Should().NotRaise(nameof(monster.OnSay));
    }

    [Fact]
    public void Monster_yells()
    {
        //arrange
        var sentence = new Fixture().Create<string>();

        var monster = MonsterTestDataBuilder.Build();
        monster.Metadata.Voices = new[] { new Voice(sentence, SpeechType.Say) };
        monster.Metadata.VoiceConfig = new IntervalChance(100, 100);

        using var monitor = monster.Monitor();

        //act
        monster.Yell();

        //assert
        monitor.Should().Raise(nameof(monster.OnSay));
    }
}