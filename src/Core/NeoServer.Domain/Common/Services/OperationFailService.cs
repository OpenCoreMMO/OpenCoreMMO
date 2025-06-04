using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Common.Services;

public static class OperationFailService
{
    public static event Action<uint, string, EffectT> OnOperationFailed;
    public static event Action<uint, InvalidOperation, EffectT> OnInvalidOperation;

    public static void Send(uint playerId, string message, EffectT effectT = EffectT.None)
    {
        OnOperationFailed?.Invoke(playerId, message, effectT);
    }

    public static void Send(uint playerId, InvalidOperation operation, EffectT effectT = EffectT.None)
    {
        OnInvalidOperation?.Invoke(playerId, operation, effectT);
    }

    public static void Send(IPlayer player, string message, EffectT effectT = EffectT.None)
    {
        Send(player.CreatureId, message, effectT);
    }

    public static void Send(IPlayer player, InvalidOperation operation, EffectT effectT = EffectT.None)
    {
        Send(player.CreatureId, operation, effectT);
    }
}