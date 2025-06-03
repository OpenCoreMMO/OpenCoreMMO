using System.Threading;
using Moq;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Npcs;
using NeoServer.Game.Tests.Helpers;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Npcs;

public class NpcTest
{
    private readonly Mock<IOutfit> outfit = new();
    private readonly Mock<ISpawnPoint> spawnPoint = new();

    [Fact]
    public void Advertise_Should_Call_Say_With_Message()
    {
        var npcType = new Mock<INpcType>();

        npcType.Setup(x => x.Name).Returns("Eryn");
        npcType.Setup(x => x.Marketings).Returns(new[] { "this is a advertise" });

        var advertise = "";
        var speechType = SpeechType.None;

        var sut = NpcTestDataBuilder.Build("Eryn", npcType.Object);

        sut.OnSay += (_, b, message, _) =>
        {
            advertise = message;
            speechType = b;
        };

        Thread.Sleep(10_000); //todo: try remove this
        sut.Advertise();

        Assert.Equal("this is a advertise", advertise);
        Assert.Equal(SpeechType.Say, speechType);
    }

    [Fact]
    public void WalkRandomStep_Should_Emit_OnStartedWalking()
    {
        //arrange
        var npcType = new Mock<INpcType>();

        npcType.Setup(x => x.Name).Returns("Eryn");
        npcType.Setup(x => x.Speed).Returns(200);

        var startedWalking = false;

        var sut = NpcTestDataBuilder.Build("Eryn", npcType.Object);

        sut.OnStartedWalking += _ => startedWalking = true;

        Thread.Sleep(5_000); //todo: try remove this

        //act
        var result = sut.WalkRandomStep();

        //assert
        Assert.True(startedWalking);
        Assert.True(result);
    }
}