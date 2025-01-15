using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Creatures.Players;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Common.Services;
using NeoServer.Game.Common.Texts;

namespace NeoServer.Game.Creatures.Player;

public class PlayerParty : IPlayerParty
{
    private readonly IPlayer _player;
    private HashSet<IParty> _partyInvites;

    public PlayerParty(IPlayer player)
    {
        _player = player;
    }

    private uint CreatureId => _player.CreatureId;

    private bool IsPartyLeader => Party?.IsLeader(_player) ?? false;
    public IParty Party { get; private set; }
    public bool IsInParty => Party is not null;

    public void InviteToParty(IPlayer invitedPlayer, IParty party)
    {
        if (invitedPlayer is null) return;
        if (invitedPlayer.CreatureId == CreatureId)
        {
            OperationFailService.Send(CreatureId, "You cannot invite yourself.");
            return;
        }

        if (invitedPlayer.PlayerParty.IsInParty)
        {
            OperationFailService.Send(CreatureId, $"{invitedPlayer.Name} is already in a party");
            return;
        }

        var result = party.Invite(_player, invitedPlayer);

        if (!result.Succeeded)
        {
            OperationFailService.Send(CreatureId, TextConstants.ONLY_LEADERS_CAN_INVITE_TO_PARTY);
            return;
        }

        var partyCreatedNow = Party is null;
        Party = party;
        invitedPlayer.PlayerParty.AddPartyInvite(_player, party);
        OnInviteToParty?.Invoke(_player, invitedPlayer, Party);

        if (partyCreatedNow) Party.OnPartyOver += PartyEmptyHandler;
    }

    public void AddPartyInvite(IPlayer from, IParty party)
    {
        _partyInvites ??= new HashSet<IParty>();
        _partyInvites.Add(party);
        OnInvitedToParty?.Invoke(from, _player, party);
        party.OnPartyOver += RejectInvite;
    }

    public void RejectInvite(IParty party)
    {
        if (_partyInvites is null || !_partyInvites.Any()) return;

        if (!_partyInvites.TryGetValue(party, out var invitedParty)) return;

        invitedParty.RemoveInvite(_player);

        invitedParty.OnPartyOver -= RejectInvite;

        OnRejectedPartyInvite?.Invoke(_player, invitedParty);
        _partyInvites.Remove(invitedParty);
    }

    public void RejectAllInvites()
    {
        if (_partyInvites is null) return;

        foreach (var partyInvite in _partyInvites) partyInvite.RemoveInvite(_player);
        _partyInvites?.Clear();
    }

    public void RevokePartyInvite(IPlayer invitedPlayer)
    {
        if (Party is null) return;
        Party.RevokeInvite(_player, invitedPlayer);
        OnRevokePartyInvite?.Invoke(_player, invitedPlayer, Party);
    }

    public Result LeaveParty()
    {
        if (Party is null) return Result.NotPossible;
        if (_player.IsLogoutBlocked)
        {
            OperationFailService.Send(CreatureId, TextConstants.YOU_CANNOT_LEAVE_PARTY_WHEN_IN_FIGHT);
            return Result.Fail(InvalidOperation.CannotLeavePartyWhenInFight);
        }

        Party.OnPartyOver -= PartyEmptyHandler;

        var passedLeadership = false;
        if (IsPartyLeader) passedLeadership = Party.PassLeadership(_player).Succeeded;

        Party?.RemoveMember(_player);

        if (passedLeadership && !Party.IsOver) OnPassedPartyLeadership?.Invoke(_player, Party.Leader, Party);

        OnLeftParty?.Invoke(_player, Party);
        Party = null;

        return Result.Success;
    }

    public Result JoinParty(IParty party)
    {
        if (party is null) return Result.NotPossible;

        var alreadyInAParty = Party is not null;
        if (alreadyInAParty)
        {
            OperationFailService.Send(CreatureId, TextConstants.ALREADY_IN_PARTY);
            return Result.Fail(InvalidOperation.AlreadyInParty);
        }

        var joinResult = party.JoinPlayer(_player);
        if (joinResult.Failed) return joinResult;

        party.OnPartyOver += PartyEmptyHandler;
        party.OnPartyOver -= RejectInvite;

        Party = party;

        RejectAllInvites();

        OnJoinedParty?.Invoke(_player, party);
        return Result.Success;
    }

    public Result PassPartyLeadership(IPlayer toPlayer)
    {
        if (Party is null) return Result.NotPossible;

        var result = _player.PlayerParty.Party.ChangeLeadership(_player, toPlayer);
        if (result.Succeeded)
        {
            OnPassedPartyLeadership?.Invoke(_player, toPlayer, Party);
            return Result.Success;
        }

        switch (result.Error)
        {
            case InvalidOperation.NotAPartyMember:
                OperationFailService.Send(CreatureId, TextConstants.PLAYER_IS_NOT_PARTY_MEMBER);
                break;
            case InvalidOperation.NotAPartyLeader:
                OperationFailService.Send(CreatureId, TextConstants.ONLY_LEADERS_CAN_PASS_LEADERSHIP);
                break;
        }

        return result;
    }

    public event InviteToParty OnInviteToParty;
    public event InviteToParty OnInvitedToParty;
    public event RevokePartyInvite OnRevokePartyInvite;
    public event RejectPartyInvite OnRejectedPartyInvite;
    public event JoinParty OnJoinedParty;
    public event LeaveParty OnLeftParty;
    public event PassPartyLeadership OnPassedPartyLeadership;

    private void PartyEmptyHandler(IParty party)
    {
        Party.OnPartyOver -= PartyEmptyHandler;
        LeaveParty();
    }
}