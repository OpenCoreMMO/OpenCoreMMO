using NeoServer.Domain.Combat.Calculations;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Combat;

public class FistVsWeaponDamageTests
{
    private const int ComparisonIterations = 20;

    public static IEnumerable<object[]> DamageRangeData =>
    [
        [(ushort)5, (ushort)12, (ushort)30, (ushort)55],
        [(ushort)8, (ushort)16, (ushort)35, (ushort)60],
        [(ushort)12, (ushort)22, (ushort)45, (ushort)80]
    ];

    [Fact]
    public void FistDamage_GivenFistAttack_WhenComparedToWeaponAttack_ThenDamageIsLower()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build(name: "FistUser", hp: 100, mana: 50);
        var target = PlayerTestDataBuilder.Build(name: "TargetPlayer", hp: 200);
        var fistCombatParameter = CreateMeleeParameter(10, 20, false);
        var weaponCombatParameter = CreateMeleeParameter(30, 50, true);

        var fistDamage = DamageCalculation.Calculate(new AttackInput(player, target, fistCombatParameter));
        var weaponDamage = DamageCalculation.Calculate(new AttackInput(player, target, weaponCombatParameter));

        Assert.True(fistDamage.MainDamage.Damage < weaponDamage.MainDamage.Damage,
            "Fist damage should be lower than weapon damage.");
    }

    [Theory]
    [MemberData(nameof(DamageRangeData))]
    public void FistDamage_GivenVariousRanges_WhenComparedToWeapon_ThenDamageRemainsLower(
        ushort fistMin,
        ushort fistMax,
        ushort weaponMin,
        ushort weaponMax)
    {
        var player = PlayerTestDataBuilder.Build(name: "RangeFistUser", hp: 120, mana: 60);
        var target = PlayerTestDataBuilder.Build(name: "RangeTargetPlayer", hp: 210);
        var fistCombatParameter = CreateMeleeParameter(fistMin, fistMax, false);
        var weaponCombatParameter = CreateMeleeParameter(weaponMin, weaponMax, true);

        for (var iteration = 1; iteration <= ComparisonIterations; iteration++)
        {
            var fistDamage = DamageCalculation.Calculate(new AttackInput(player, target, fistCombatParameter));
            var weaponDamage = DamageCalculation.Calculate(new AttackInput(player, target, weaponCombatParameter));

            Assert.True(
                fistDamage.MainDamage.Damage < weaponDamage.MainDamage.Damage,
                $"Iteration {iteration}: expected fist damage ({fistDamage.MainDamage.Damage}) to stay below weapon damage ({weaponDamage.MainDamage.Damage}).");
        }
    }

    [Fact]
    public void FistDamage_GivenMonsterTarget_WhenComparedToWeapon_ThenDamageStaysLower()
    {
        var player = PlayerTestDataBuilder.Build(name: "MonsterHunter", hp: 130, mana: 70);
        var monster = MonsterTestDataBuilder.Build(name: "TargetMonster");
        var fistCombatParameter = CreateMeleeParameter(15, 28, false);
        var weaponCombatParameter = CreateMeleeParameter(35, 65, true);

        var fistDamage = DamageCalculation.Calculate(new AttackInput(player, monster, fistCombatParameter));
        var weaponDamage = DamageCalculation.Calculate(new AttackInput(player, monster, weaponCombatParameter));

        Assert.True(fistDamage.MainDamage.Damage < weaponDamage.MainDamage.Damage,
            "Against monsters, fist damage should still be lower than weapon damage.");
    }

    private static CombatParameter CreateMeleeParameter(ushort minDamage, ushort maxDamage, bool usingWeapon,
        ExtraAttack extraAttack = default)
    {
        return new CombatParameter
        {
            DamageType = DamageType.Melee,
            MinDamage = minDamage,
            MaxDamage = maxDamage,
            UsingWeapon = usingWeapon,
            ExtraAttack = extraAttack
        };
    }
}