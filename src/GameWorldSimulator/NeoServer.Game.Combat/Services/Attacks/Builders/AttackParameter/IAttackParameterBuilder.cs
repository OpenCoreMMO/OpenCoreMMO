using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Combat.Services.Attacks.Builders.AttackParameter;

public interface IAttackParameterBuilder
{
    Common.Combat.Structs.AttackParameter Build(IThing aggressor);
}