using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Combat.Services.Attacks.Builders.AttackParameter;

public interface IAttackParameterBuilder
{
    CombatParameter Build(IThing aggressor);
}