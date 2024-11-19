using NeoServer.BuildingBlocks.Application.IntegrationEvents.World;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Combat;
using NeoServer.Modules.Combat.Attacks.ConditionAttack;

namespace NeoServer.Modules.Combat.MagicFieldDamage;

public class CreatureWalkedThoughMagicFieldEventHandler(ConditionAttackStrategy conditionAttackStrategy)
    : IIntegrationEventHandler<CreatureWalkedThoughMagicFieldEvent>
{
    public void Handle(CreatureWalkedThoughMagicFieldEvent @event)
    {
        var (victim, magicField) = @event;
        conditionAttackStrategy.Execute(new AttackInput(magicField, victim)
        {
            Parameters = new AttackParameter()
            {
                Name = "Condition",
                DamageType = magicField.DamageType,
                MaxDamage = (ushort)Math.Max(0, magicField.Damage.Max),
                MinDamage = (ushort)Math.Max(0, magicField.Damage.Min),
                Condition = new AttackParameter.AttackCondition(magicField.DamageCount, magicField.Interval),
                IsMagicalAttack = true
            }
        });
    }
}