using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Creatures.Party;

public class Party : IParty
{
    private readonly HashSet<uint> invites = new();

    private readonly Dictionary<uint, PartyMember> members = new();
    private ushort memberCount;

    public Party(IPlayer player, IChatChannel channel)
    {
        Leader = player;
        Channel = channel;
        player.Channels.JoinChannel(channel);
        Heals = new Dictionary<IPlayer, DateTime>();
    }

    private PartyMember FirstMemberJoined
    {
        get
        {
            PartyMember partyMember = new();
            var min = uint.MaxValue;
            foreach (var member in members)
                if (member.Value.Order < min)
                {
                    min = member.Value.Order;
                    partyMember = member.Value;
                }

            return partyMember;
        }
    }

    public event Action<IParty> OnPartyOver;
    public event PlayerJoinedParty OnPlayerJoin;
    public event PlayerLeftParty OnPlayerLeave;

    public IPlayer Leader { get; private set; }

    public IReadOnlyCollection<IPlayer> Members
    {
        get
        {
            var membersList = new List<IPlayer>(members.Count + 1);
            foreach (var member in members.Values) membersList.Add(member.Player);
            membersList.Add(Leader);
            return membersList;
        }
    }

    public IReadOnlyCollection<uint> Invites => invites.ToList();
    public IChatChannel Channel { get; }
    public bool IsOver => !members.Any();

    public bool IsSharedExperienceEnabled { get; set; }

    /// <summary>
    ///     Heals done by party members to other party members.
    ///     Key: Healer, Value: LastHealedOn
    /// </summary>
    public IDictionary<IPlayer, DateTime> Heals { get; }

    public bool IsMember(IPlayer player)
    {
        return members.ContainsKey(player.CreatureId);
    }

    public bool IsMember(uint creatureId)
    {
        return members.ContainsKey(creatureId);
    }

    public bool IsInvited(IPlayer player)
    {
        return invites.Contains(player.CreatureId);
    }

    public bool IsLeader(IPlayer player)
    {
        return player == Leader;
    }

    public bool IsLeader(uint creatureId)
    {
        return creatureId == Leader.CreatureId;
    }

    public Result JoinPlayer(IPlayer player)
    {
        if (player.IsNull()) return Result.NotPossible;

        if (!IsInvited(player)) return Result.Fail(InvalidOperation.NotInvited);

        invites.Remove(player.CreatureId);
        members.Add(player.CreatureId, new PartyMember(player, ++memberCount));

        player.Channels.JoinChannel(Channel);
        OnPlayerJoin?.Invoke(this, player);
        player.OnHeal += TrackPlayerHeal;
        return Result.Success;
    }

    public void RemoveInvite(IPlayer invitedPlayer)
    {
        invites.Remove(invitedPlayer.CreatureId);
    }

    public Result Invite(IPlayer by, IPlayer invitedPlayer)
    {
        if (invitedPlayer.PlayerParty.IsInParty) return new Result(InvalidOperation.CannotInvite);
        if (!IsLeader(by)) return new Result(InvalidOperation.CannotInvite);

        invites.Add(invitedPlayer.CreatureId);

        return Result.Success;
    }

    public void RevokeInvite(IPlayer by, IPlayer invitedPlayer)
    {
        if (!IsLeader(by)) return;
        if (!invites.Remove(invitedPlayer.CreatureId)) return;

        if (IsOver && !invites.Any()) OnPartyOver?.Invoke(this);
    }

    public void RemoveMember(IPlayer player)
    {
        if (player.IsNull()) return;
        if (player.IsLogoutBlocked) return;

        members.Remove(player.CreatureId);
        player.Channels.ExitChannel(Channel);

        player.OnHeal -= TrackPlayerHeal;
        OnPlayerLeave?.Invoke(this, player);
        if (IsOver) OnPartyOver?.Invoke(this);
    }

    public Result ChangeLeadership(IPlayer from, IPlayer to)
    {
        if (Guard.AnyNull(from, to)) return Result.NotPossible;

        if (!IsMember(to)) return new Result(InvalidOperation.NotAPartyMember);
        if (!IsLeader(from)) return new Result(InvalidOperation.NotAPartyLeader);

        Leader = to;
        members.Add(from.CreatureId, new PartyMember(from, ++memberCount));
        members.Remove(to.CreatureId);

        return Result.Success;
    }

    /// <summary>
    ///     Pass leadership to first member joined
    /// </summary>
    /// <param name="from"></param>
    /// <returns></returns>
    public Result PassLeadership(IPlayer from)
    {
        if (!IsLeader(from)) return new Result(InvalidOperation.NotAPartyLeader);
        return ChangeLeadership(from, FirstMemberJoined.Player);
    }

    public string InspectionText(IPlayer player)
    {
        return
            $"{player.GenderPronoun} is in a party with {memberCount} members and {invites.Count} pending invitations.";
    }

    /// <summary>
    ///     When a player heals another party member the time is tracked to know how recently they've healed.
    /// </summary>
    /// <param name="healedCreature">The one that received the healing.</param>
    /// <param name="healerCreature">The one that caused the healing.</param>
    /// <param name="amount">Amount the creature was healed.</param>
    private void TrackPlayerHeal(ICombatActor healedCreature, ICreature healerCreature, ushort amount)
    {
        if (amount <= 0) return;
        if (healedCreature is not IPlayer healed) return;
        if (healerCreature is not IPlayer healer) return;
        if (healed == healer) return;

        if (Heals.TryGetValue(healer, out _))
            Heals[healer] = DateTime.UtcNow;
        else
            Heals.Add(healer, DateTime.UtcNow);
    }
}

internal readonly struct PartyMember
{
    public PartyMember(IPlayer player, ushort order)
    {
        Player = player;
        Order = order;
    }

    public IPlayer Player { get; }
    public ushort Order { get; }
}