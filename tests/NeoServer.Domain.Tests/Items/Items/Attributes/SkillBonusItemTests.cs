using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items.Items.Attributes;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Items.Items.Attributes;

public class SkillBonusItemTests
{
    [Fact]
    public void AddSkillBonus_Null_DoNotThrow()
    {
        var sut = ItemTestData.CreateDefenseEquipmentItem(1);

        sut.AddSkillBonus(null);
    }

    [Fact]
    public void AddSkillBonus_ToPlayer_AddSkills()
    {
        var skills = PlayerTestDataBuilder.GenerateSkills(10);
        var player = PlayerTestDataBuilder.Build(skills: skills);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.SkillAxe, 5),
            (ItemTypeAttribute.SkillShield, 15)
        });

        sut.AddSkillBonus(player);

        player.GetSkillBonus(SkillType.Axe).Should().Be(5);
        player.GetSkillBonus(SkillType.Sword).Should().Be(0);
        player.GetSkillBonus(SkillType.Distance).Should().Be(0);
        player.GetSkillBonus(SkillType.Fishing).Should().Be(0);
        player.GetSkillBonus(SkillType.Fist).Should().Be(0);
        player.GetSkillBonus(SkillType.Level).Should().Be(0);
        player.GetSkillBonus(SkillType.Magic).Should().Be(0);
        player.GetSkillBonus(SkillType.Shielding).Should().Be(15);
        player.GetSkillBonus(SkillType.Speed).Should().Be(0);
        player.GetSkillBonus(SkillType.Club).Should().Be(0);
    }

    [Fact]
    public void RemoveSkillBonus_Null_DoNotThrow()
    {
        var sut = ItemTestData.CreateDefenseEquipmentItem(1);

        sut.RemoveSkillBonus(null);
    }

    [Fact]
    public void RemoveSkillBonus_ToPlayer_RemoveSkills()
    {
        var skills = PlayerTestDataBuilder.GenerateSkills(10);
        var player = PlayerTestDataBuilder.Build(skills: skills);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.SkillAxe, 5);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.SkillShield, 15);


        sut.AddSkillBonus(player);

        sut.RemoveSkillBonus(player);

        player.GetSkillBonus(SkillType.Axe).Should().Be(0);
        player.GetSkillBonus(SkillType.Sword).Should().Be(0);
        player.GetSkillBonus(SkillType.Distance).Should().Be(0);
        player.GetSkillBonus(SkillType.Fishing).Should().Be(0);
        player.GetSkillBonus(SkillType.Fist).Should().Be(0);
        player.GetSkillBonus(SkillType.Level).Should().Be(0);
        player.GetSkillBonus(SkillType.Magic).Should().Be(0);
        player.GetSkillBonus(SkillType.Shielding).Should().Be(0);
        player.GetSkillBonus(SkillType.Speed).Should().Be(0);
        player.GetSkillBonus(SkillType.Club).Should().Be(0);
    }


    [Fact]
    public void AddSkillBonus_2ItemsToPlayer_AddSkills()
    {
        var skills = PlayerTestDataBuilder.GenerateSkills(10);
        var player = PlayerTestDataBuilder.Build(skills: skills);

        var item1 = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.SkillAxe, 5),
            (ItemTypeAttribute.SkillShield, 15)
        });
        var item2 = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.SkillAxe, 15),
            (ItemTypeAttribute.SkillShield, 25)
        });

        item1.AddSkillBonus(player);
        item2.AddSkillBonus(player);

        player.GetSkillBonus(SkillType.Axe).Should().Be(20);

        player.GetSkillBonus(SkillType.Shielding).Should().Be(40);
    }

    [Fact]
    public void SkillBonuses_MetadataChanged_Changes()
    {
        //arrange
        var skills = PlayerTestDataBuilder.GenerateSkills(10);
        var player = PlayerTestDataBuilder.Build(skills: skills);

        var item2 = ItemTestData.CreateDefenseEquipmentItem(2, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.SkillSword, 10)
        });

        var itemStore = ItemTestData.GetItemTypeStore(item2.Metadata);

        var item1 = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.SkillAxe, 5),
            (ItemTypeAttribute.TransformEquipTo, 2)
        }, itemTypeFinder: itemStore.Get);

        //act
        item1.AddSkillBonus(player);

        //assert
        player.GetSkillBonus(SkillType.Axe).Should().Be(5);

        //act
        item1.RemoveSkillBonus(player);

        item1.TransformOnEquip();

        item1.AddSkillBonus(player);

        //assert
        player.GetSkillBonus(SkillType.Axe).Should().Be(0);
        player.GetSkillBonus(SkillType.Sword).Should().Be(10);
    }

    [Fact]
    public void ChangeSkillBonus_Null_DoNotChange()
    {
        //arrange
        var skills = PlayerTestDataBuilder.GenerateSkills(10);
        var player = PlayerTestDataBuilder.Build(skills: skills);

        var item1 = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.SkillAxe, 5)
        });

        //act
        item1.AddSkillBonus(player);

        //assert
        player.GetSkillBonus(SkillType.Axe).Should().Be(5);

        //act
        item1.RemoveSkillBonus(player);
        // item1.ChangeSkillBonuses(null);
        item1.AddSkillBonus(player);

        //assert
        player.GetSkillBonus(SkillType.Axe).Should().Be(5);
        player.GetSkillBonus(SkillType.Sword).Should().Be(0);
    }

    [Fact]
    public void ToString_NoAttribute_ReturnsEmpty()
    {
        //arrange
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
        });

        //act
        var actual = new SkillBonus(sut).ToString();

        //assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void ToString_ReturnsText()
    {
        //arrange
        var item = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.SkillAxe, 5),
            (ItemTypeAttribute.SkillClub, 15),
            (ItemTypeAttribute.SkillSword, 25),
            (ItemTypeAttribute.SkillDistance, 35),
            (ItemTypeAttribute.SkillFist, 45),
            (ItemTypeAttribute.SkillShield, 55),
            (ItemTypeAttribute.MagicPoints, 3),
            (ItemTypeAttribute.SkillFishing, 10),
            (ItemTypeAttribute.Speed, 20)
        });
        var sut = new SkillBonus(item);

        //act
        var actual = sut.ToString();

        //assert
        actual.Should()
            .Be(
                "axe fighting +5, club fighting +15, sword fighting +25, distance fighting +35, fist fighting +45, shielding +55, magic level +3, fishing +10, speed +20");
    }

    [Fact]
    public void ToString_0Bonus_DoNotAddToText()
    {
        //arrange
        var item = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.SkillAxe, 5),
            (ItemTypeAttribute.Speed, 0)
        });
        var sut = new SkillBonus(item);

        //act
        var actual = sut.ToString();

        //assert
        actual.Should().Be("axe fighting +5");
    }

    [Fact]
    public void Player_loose_skill_bonus_when_dress_a_negative_skill_bonus_item()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, ISkill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10),
            [SkillType.Sword] = new Skill(SkillType.Sword, 10)
        });

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.SkillAxe, -5),
            (ItemTypeAttribute.SkillSword, -15)
        });

        //act
        player.Inventory.AddItem(sut);

        //assert
        player.GetSkillBonus(SkillType.Axe).Should().Be(-5);
        player.GetSkillLevel(SkillType.Axe).Should().Be(5);

        player.GetSkillBonus(SkillType.Sword).Should().Be(-15);
        player.GetSkillLevel(SkillType.Sword).Should().Be(0);
    }

    [Fact]
    public void Player_add_back_skill_when_undress_a_negative_skill_bonus_item()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(skills: new Dictionary<SkillType, ISkill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, 10),
            [SkillType.Sword] = new Skill(SkillType.Sword, 10)
        });

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "body", attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.SkillAxe, -5),
            (ItemTypeAttribute.SkillSword, -15)
        });

        player.Inventory.AddItem(sut);

        //act
        player.Inventory.RemoveItem(sut, 1, (byte)Slot.Body, out _);

        //assert
        player.GetSkillBonus(SkillType.Axe).Should().Be(0);
        player.GetSkillLevel(SkillType.Axe).Should().Be(10);
    }
}