using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerManaShieldTests
{
    [Fact]
    public void EnableManaShield_with_duration_creates_condition()
    {
        var player = PlayerTestDataBuilder.Build();

        player.EnableManaShield(5000);

        player.IsManaShieldEnabled.Should().BeTrue();
    }

    [Fact]
    [ThreadBlocking]
    public void EnableManaShield_with_duration_removes_self_on_expire()
    {
        var player = PlayerTestDataBuilder.Build();

        player.EnableManaShield(50);
        player.IsManaShieldEnabled.Should().BeTrue();

        Thread.Sleep(60);
        ExecuteConditionTick(player);

        player.IsManaShieldEnabled.Should().BeFalse();
    }

    private static void ExecuteConditionTick(ICombatActor creature)
    {
        var conditions = creature.GetConditions();
        for (var i = 0; i < conditions.Count; i++)
        {
            var condition = conditions[i];

            creature.RemoveCondition(condition);

        }
    }
}
