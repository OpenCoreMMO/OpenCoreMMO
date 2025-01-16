using System;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures.Player;

public class PlayerSkull: IPlayerSkull
{
    public PlayerSkull(IPlayer player, Skull skull = Skull.None, DateTime? skullEndsAt = null)
    {
        Player = player;
        Skull = skull;
        SkullEndsAt = skullEndsAt;
    }
    public IPlayer Player { get; }
    public Skull Skull { get; private set; }
    public DateTime? SkullEndsAt { get; private set; }
    
    public DateTime? YellowSkullEndsAt { get; private set; }
    public PlayerEnemyList PlayersAttackedList { get; } = new();
    public void SetSkull(Skull skull, DateTime? endingDate = null, IPlayer enemy = null)
    {
        if (skull == Skull.Yellow)
        {
            SetYellowSkull(enemy, endingDate);
            return;
        }

        var oldSkull = Skull;
        Skull = skull;
        SkullEndsAt = endingDate;

        if (oldSkull != Skull)
        {
            OnSkullUpdated?.Invoke(Player);
        }
    }

    public void SetYellowSkull(IPlayer enemy, DateTime? endingDate)
    {
        if (enemy is null)
        {
            throw new ArgumentNullException(nameof(enemy), "Enemy must have a value when setting yellow skull");
        }

        if (PlayersAttackedList.HasEnemy(enemy))
        {
            return;
        }

        PlayersAttackedList.AddEnemy(enemy);

        YellowSkullEndsAt = endingDate;

        OnSkullUpdated?.Invoke(Player);
    }

    public void RemoveSkull()
    {
        var oldSkull = Skull;
        Skull = Skull.None;
        SkullEndsAt = null;

        PlayersAttackedList.Clear();

        if (oldSkull != Skull)
        {
            OnSkullUpdated?.Invoke(Player);
        }
    }

    public void RemoveYellowSkull()
    {
        if (!PlayersAttackedList.Any()) return;

        PlayersAttackedList.Clear();
        OnSkullUpdated?.Invoke(Player);
    }


    public bool IsYellowSkull(IPlayer enemy) => PlayersAttackedList.HasEnemy(enemy);

    public Skull GetSkull(IPlayer enemy)
    {
        if (enemy.CreatureId == Player.CreatureId)
        {
            return Skull;
        }

        return IsYellowSkull(enemy) ? Skull.Yellow : Skull;
    }
    public event SkullUpdated OnSkullUpdated;
}