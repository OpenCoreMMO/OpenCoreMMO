using Moq;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Creatures.Player.Outfit;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Npcs;

public class NpcTest
{
    private readonly Mock<Outfit> outfit = new();
    private readonly Mock<ISpawnPoint> spawnPoint = new();

    [Fact]
    public void Advertise_Should_Call_Say_With_Message()
    {
        var npcType = new Mock<INpcType>();

        npcType.Setup(x => x.Name).Returns("Eryn");
        npcType.Setup(x => x.Marketings).Returns(["this is a advertise"]);

        var advertise = "";
        var speechType = SpeechType.None;

        var sut = NpcTestDataBuilder.Build("Eryn", npcType.Object);

        var listener = PlayerTestDataBuilder.Build(name: "Listener");

        EventAggregatorTestHelper.SetupEventAggregator<CreatureSayEvent>(e =>
        {
            advertise = e.Message;
            speechType = e.SpeechType;
        });

        Thread.Sleep(10_000); //todo: try remove this
        sut.Advertise([listener]);

        advertise.Should().Be("this is a advertise");
        speechType.Should().Be(SpeechType.Say);
    }

    [ThreadBlocking]
    [Fact]
    public void WalkRandomStep_Should_Emit_OnStartedWalking()
    {
        var npcType = new Mock<INpcType>();

        npcType.Setup(x => x.Name).Returns("Eryn");
        npcType.Setup(x => x.Speed).Returns(200);

        var sut = NpcTestDataBuilder.Build("Eryn", npcType.Object);

        var startedWalkingEvents = new List<CreatureStartedWalkingEvent>();
        EventAggregatorTestHelper.SetupEventAggregator<CreatureStartedWalkingEvent>(e => startedWalkingEvents.Add(e));

        Thread.Sleep(5_000); //todo: try remove this

        var result = sut.WalkRandomStep();

        startedWalkingEvents.Should().NotBeEmpty();
        Assert.True(result);
    }
}
