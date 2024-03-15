﻿using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Common.Services;

public static class OperationFailService
{
    public static event Action<uint, string, EffectT> OnOperationFailed;
    public static event Action<uint, InvalidOperation, EffectT> OnInvalidOperation;

    public static void Send(uint playerId, string message, EffectT effectT = EffectT.None)
    {
        OnOperationFailed?.Invoke(playerId, message, effectT);
    }

    public static void Send(uint playerId, InvalidOperation error, EffectT effectT = EffectT.None)
    {
        OnInvalidOperation?.Invoke(playerId, error, effectT);
    }

    public static void Send(IPlayer player, string message, EffectT effectT = EffectT.None)
    {
        Send(player.CreatureId, message, effectT);
    }

    public static void Send(IPlayer player, InvalidOperation error, EffectT effectT = EffectT.None)
    {
        if (error is InvalidOperation.None) return;

        Send(player.CreatureId, error, effectT);
    }
}