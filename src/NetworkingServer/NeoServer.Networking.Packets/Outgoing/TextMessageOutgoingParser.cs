using NeoServer.Game.Common;
using NeoServer.Game.Common.Texts;

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
            InvalidOperation.Exhausted => "You are exhausted.",
            InvalidOperation.CreatureIsNotReachable => "Creature is not reachable.",
            InvalidOperation.CannotAttackThatFast => "You cannot attack that fast.",
            InvalidOperation.NotPermittedInProtectionZone => TextConstants.NOT_PERMITTED_IN_PROTECTION_ZONE,
            InvalidOperation.PlayerLocationInvalid => "Player location is invalid.",
            InvalidOperation.AdjustCombatSettingsToAttackPlayer => "Adjust your combat settings to attack this person.",
            _ => string.Empty
        };
    }
}