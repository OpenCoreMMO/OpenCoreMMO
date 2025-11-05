using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Texts;

namespace NeoServer.Networking.Packets.Outgoing;

public static class TextMessageOutgoingParser
{
    public static string Parse(InvalidOperation error)
    {
        return error switch
        {
            InvalidOperation.NotEnoughRoom => "There is not enough room.",
            InvalidOperation.Impossible => "This is impossible",
            InvalidOperation.BothHandsNeedToBeFree => "Both hands need to be free.",
            InvalidOperation.CannotDress => "You cannot dress this object there.",
            InvalidOperation.NotPossible => TextConstants.NOT_POSSIBLE,
            InvalidOperation.PlayerNotFound => "Sorry, player is offline.",
            InvalidOperation.TooHeavy => "This object is too heavy for you to carry.",
            InvalidOperation.VocationCannotUseSpell => "Your vocation cannot use this spell.",
            InvalidOperation.NotEnoughMana => "You do not have enough mana.",
            InvalidOperation.NotEnoughLevel => "You do not have enough level.",
            InvalidOperation.NotEnoughMagicLevel => "You do not have enough magic level.",
            InvalidOperation.Exhausted => "You are exhausted.",
            InvalidOperation.CreatureIsNotReachable => "Creature is not reachable.",
            InvalidOperation.CannotAttackThatFast => "You cannot attack that fast.",
            InvalidOperation.NotPermittedInProtectionZone => TextConstants.NOT_PERMITTED_IN_PROTECTION_ZONE,
            InvalidOperation.PlayerLocationInvalid => "Player location is invalid.",
            InvalidOperation.AdjustCombatSettingsToAttackPlayer => "Adjust your combat settings to attack this person.",
            InvalidOperation.SpellNeedsWeapon => "You need to equip a weapon to use this spell.",
            InvalidOperation.PremiumTimeIsRequired => "Premium time is required.",
            InvalidOperation.SpellRequiresPremium => "Premium time is required to use this spell.",
            InvalidOperation.CanOnlyUseOnCreatures => "You can only use this on creatures.",
            InvalidOperation.CannotUseThisObject => "You cannot use this object.",
            InvalidOperation.FirstGoDownStairs => "First go down the stairs.",
            InvalidOperation.FirstGoUpStairs => "First go up the stairs.",
            InvalidOperation.CannotThrowThere => "You cannot throw there.",
            InvalidOperation.TooFar => "You are too far.",
            InvalidOperation.DestinationOutOfReach => "Destination is out of reach.",
            InvalidOperation.CannotAttackWhileInProtectionZone => "You cannot attack while in protection zone.",
            InvalidOperation.CannotInvite => "You cannot invite.",
            InvalidOperation.NotAPartyLeader => "You are not a party leader.",
            InvalidOperation.NotAPartyMember => "You are not a party member.",
            InvalidOperation.CannotLeavePartyWhenInFight => "You cannot leave party when in fight.",
            InvalidOperation.NotInvited => "You are not invited.",
            InvalidOperation.TurnSecureModeToAttackUnmarkedPlayers => "Turn secure mode to attack unmarked players.",
            InvalidOperation.TargetLost => "Target lost.",
            InvalidOperation.YouMayNotAttackThisPlayer => "You may not attack this person.",
            InvalidOperation.YouMayNotAttackThisCreature => "You may not attack this creature.",
            _ => string.Empty
        };
    }
}