using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Combat.Services.Attacks;

public interface IAttackService
{
    Result Execute( AttackInput attackInput);
}