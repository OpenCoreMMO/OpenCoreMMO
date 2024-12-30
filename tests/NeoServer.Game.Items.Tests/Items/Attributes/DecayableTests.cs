using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Items.Items.Attributes;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Server;
using Xunit;

namespace NeoServer.Game.Items.Tests.Items.Attributes;

public class DecayableTests : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        ThreadBlocking.Wait();
        // Cleanup logic after each test
        EventSubscriptionCleanUp.CleanUp<Decayable>(nameof(Decayable.OnStarted));

        ThreadBlocking.Release();
        return Task.CompletedTask;
    }

    [Fact]
    public void Expired_DidNotStartToDecay_ReturnsFalse()
    {
        //arrange

        var item = ItemTestData.CreateDefenseEquipmentItem(2, attributes: new (ItemAttribute, IConvertible)[]
        {
        });
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 60),
            (ItemAttribute.ExpireTarget, 2)
        });
        var sut = new Decayable(decayableItem);

        //assert
        sut.Expired.Should().BeFalse();
    }

    [Fact]
    public void Expired_ElapsedLessThanDuration_ReturnsFalse()
    {
        //arrange
        var item = ItemTestData.CreateDefenseEquipmentItem(2, attributes: new (ItemAttribute, IConvertible)[]
        {
        });
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 60),
            (ItemAttribute.ExpireTarget, 2)
        });
        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(2000);
        sut.PauseDecay();
        //assert
        sut.Expired.Should().BeFalse();
    }

    [Fact]
    public void Expired_ElapsedGreaterThanDuration_ReturnsTrue()
    {
        //arrange
        var item = ItemTestData.CreateDefenseEquipmentItem(2, attributes: new (ItemAttribute, IConvertible)[]
        {
        });
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 2),
            (ItemAttribute.ExpireTarget, 2)
        });
        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(2500);
        sut.PauseDecay();
        //assert
        sut.Expired.Should().BeTrue();
    }

    [Fact]
    public void StartedToDecay_CalledStart_ReturnsTrue()
    {
        //arrange
        var item = ItemTestData.CreateDefenseEquipmentItem(2, attributes: new (ItemAttribute, IConvertible)[]
        {
        });
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 2),
            (ItemAttribute.ExpireTarget, 2)
        });
        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(2500);
        sut.PauseDecay();
        //assert
        sut.StartedToDecay.Should().BeTrue();
    }

    [Fact]
    public void StartedToDecay_DidNotStart_ReturnsFalse()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 2),
            (ItemAttribute.ExpireTarget, 2)
        });
        var sut = new Decayable(decayableItem);

        //assert
        sut.StartedToDecay.Should().BeFalse();
    }

    [Fact]
    public void Start_IncreaseElapsedTime()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 20),
            (ItemAttribute.ExpireTarget, 2)
        });
        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(1000);
        sut.PauseDecay();

        //assert
        var elapsed = sut.Elapsed;
        sut.Elapsed.Should().BeGreaterThan(0);

        //act
        sut.StartDecay();
        Thread.Sleep(1000);
        sut.PauseDecay();

        //assert
        sut.Elapsed.Should().BeGreaterThan(elapsed);
    }

    [Fact]
    public void ShouldDisappear_DecaysToNull_ReturnsTrue()
    {
        //arrange

        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 20)
        });
        var sut = new Decayable(decayableItem);

        //assert
        sut.ShouldDisappear.Should().BeTrue();
    }

    [Fact]
    public void ShouldDisappear_DecaysTo10_ReturnsFalse()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 20),
            (ItemAttribute.ExpireTarget, 2)
        });
        var sut = new Decayable(decayableItem);

        //assert
        sut.ShouldDisappear.Should().BeFalse();
    }

    [Fact]
    public void Elapsed_2Secs_Returns2()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 20),
            (ItemAttribute.ExpireTarget, 2)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(2000);
        sut.PauseDecay();

        //assert
        sut.Elapsed.Should().Be(2);
    }

    [Fact]
    public void Elapsed_AfterPause_DoNotChange()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 20),
            (ItemAttribute.ExpireTarget, 2)
        });
        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(2000);
        sut.PauseDecay();
        Thread.Sleep(2000);

        //assert
        sut.Elapsed.Should().Be(2);
    }

    [Fact]
    public void Remaining_DidNotStarted_ReturnsDuration()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 20),
            (ItemAttribute.ExpireTarget, 2)
        });

        var sut = new Decayable(decayableItem);

        //assert
        sut.Remaining.Should().Be(20);
    }

    [Fact]
    public void Remaining_Elapsed2SecsOf20_Returns18()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 20),
            (ItemAttribute.ExpireTarget, 2)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(2000);
        sut.PauseDecay();

        //assert
        sut.Remaining.Should().Be(18);
    }

    [Fact]
    public void Remaining_Elapsed2SecsOf2_Returns0()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 2),
            (ItemAttribute.ExpireTarget, 2)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(2000);
        sut.PauseDecay();

        //assert
        sut.Remaining.Should().Be(0);
    }

    [Fact]
    public void Pause_EmitEvent()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 2),
            (ItemAttribute.ExpireTarget, 2)
        });

        var sut = new Decayable(decayableItem);

        var emitted = false;
        sut.OnPaused += _ => emitted = true;
        //act
        sut.StartDecay();
        Thread.Sleep(2000);
        sut.PauseDecay();

        //assert
        emitted.Should().BeTrue();
    }

    [Fact]
    public void StartDecay_Expired_DoNotStart()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 0),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        var emitted = false;
        Decayable.OnStarted += _ => emitted = true;
        //act
        sut.StartDecay();
        //assert
        sut.StartedToDecay.Should().BeFalse();
        emitted.Should().BeFalse();
    }

    [Fact]
    public void Start_EmitEvent()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 2),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        var emitted = false;
        Decayable.OnStarted += _ => emitted = true;
        //act
        sut.StartDecay();
        //assert
        emitted.Should().BeTrue();
    }

    [Fact]
    public void Elapsed_AfterStarted_Changes()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 20),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(1000);
        //assert
        sut.Elapsed.Should().Be(2);
        Thread.Sleep(1000);
        sut.Elapsed.Should().Be(3);
    }

    [Fact]
    public void Remaining_AfterStarted_Changes()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 20),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(500);
        //assert
        sut.Remaining.Should().Be(19);
        Thread.Sleep(1000);
        sut.Remaining.Should().Be(18);
    }

    [Fact]
    public void ToString_DidNotStart_ReturnBrandNewMessage()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 20),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        //assert
        sut.ToString().Should().Be("is brand-new");
    }

    [Fact]
    public void ToString_DidStart_ReturnTimeRemaining()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 180),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(1200);
        //assert
        sut.ToString().Should().Be("will expire in 2 minutes and 58 seconds");
    }

    [Fact]
    public void ToString_DidStart_ReturnTimeRemainingWellFormatted()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 62),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(500);
        //assert
        sut.ToString().Should().Be("will expire in 1 minute and 1 second");
    }

    [Fact]
    public void ToString_Expired_ReturnTimeRemainingWellFormatted()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 1),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(1100);
        //assert
        sut.ToString().Should().Be("will expire in 0 minute and 0 second");
    }

    [Fact]
    public void ToString_ShowDurationFalse_ReturnEmpty()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 62),
            (ItemAttribute.ExpireTarget, 10),
            (ItemAttribute.ShowDuration, 0)
        });

        var sut = new Decayable(decayableItem);
        //act
        sut.StartDecay();
        //assert
        sut.ToString().Should().BeEmpty();
    }

    [Fact]
    public void Decay_DidNotExpire_ReturnsFalse()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 62),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(1000);
        var actual = sut.TryDecay();
        //assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void Decay_DidNotExpire_DoNotEmitEvent()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 62),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);
        using var monitor = sut.Monitor();


        //act
        sut.StartDecay();
        Thread.Sleep(1000);
        sut.TryDecay();
        //assert
        monitor.Should().Raise(nameof(sut.OnStarted));
    }

    [Fact]
    public void Decay_Expired_ReturnsTrue()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 1),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(1500);
        var actual = sut.TryDecay();
        //assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void Decay_ExpiredAndDecaysToItem_0Elapsed()
    {
        //arrange
        var decayableItem = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Duration, 1),
            (ItemAttribute.ExpireTarget, 10)
        });

        var sut = new Decayable(decayableItem);

        //act
        sut.StartDecay();
        Thread.Sleep(1500);
        sut.TryDecay();
        //assert
        sut.Elapsed.Should().Be(0);
    }

    //[Fact]
    //[ThreadBlocking]
    //public async Task Item_that_has_decay_behavior_decays_after_expiration()
    //{
    //    //arrange
    //    var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);

    //    var item1 = (IEquipment)ItemTestData.CreateWeaponItem(1, attributes: new (ItemAttribute, IConvertible)[]
    //    {
    //        (ItemAttribute.Duration, 1)
    //    });

    //    var item2 = (IEquipment)ItemTestData.CreateWeaponItem(2, attributes: new (ItemAttribute, IConvertible)[]
    //    {
    //        (ItemAttribute.Duration, 3)
    //    });

    //    ((IDynamicTile)map[100, 100, 7]).AddItem(item1);
    //    ((IDynamicTile)map[101, 100, 7]).AddItem(item2);

    //    var itemTypeStore = ItemTestData.GetItemTypeStore();
    //    ItemTestData.AddItemTypeStore(itemTypeStore, item1.Metadata, item2.Metadata);

    //    var decayableItemManager = DecayableItemManagerTestBuilder.Build(map, itemTypeStore);

    //    Decayable.OnStarted += decayableItemManager.Add;

    //    //act
    //    item1.StartDecay();
    //    item2.StartDecay();

    //    //assert
    //    await Task.Delay(1050);

    //    decayableItemManager.DecayExpiredItems();
    //    map[100, 100, 7].TopItemOnStack.Should().NotBe(item1);
    //    map[101, 100, 7].TopItemOnStack.Should().Be(item2);

    //    await Task.Delay(2050);

    //    decayableItemManager.DecayExpiredItems();
    //    map[101, 100, 7].TopItemOnStack.Should().NotBe(item2);
    //}
}