using System;
using NeoServer.Game.Common.Combat.Enums;

namespace NeoServer.Game.Common.Contracts.Creatures;

public delegate void SkullUpdated(IPlayer player);
public interface IPlayerSkull
{
    event SkullUpdated OnSkullUpdated;
    Skull Skull { get; }
    DateTime? SkullEndsAt { get; }
    DateTime? YellowSkullEndsAt { get; }
    void SetSkull(Skull skull, DateTime? endingDate = null, IPlayer enemy = null);
    void RemoveSkull();
    void RemoveYellowSkull();
    bool IsYellowSkull(IPlayer enemy);
    Skull GetSkull(IPlayer enemy);
}