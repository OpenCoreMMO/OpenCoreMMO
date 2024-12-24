using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Extensions
{
    public static class ReturnMessageExtensions
    {
        public static string GetReturnMessage(this ReturnValueType value)
        {
            switch (value)
            {
                case ReturnValueType.RETURNVALUE_DESTINATIONOUTOFREACH:
                    return "Destination is out of range.";

                case ReturnValueType.RETURNVALUE_NOTMOVEABLE:
                    return "You cannot move this object.";

                case ReturnValueType.RETURNVALUE_DROPTWOHANDEDITEM:
                    return "Drop the double-handed object first.";

                case ReturnValueType.RETURNVALUE_BOTHHANDSNEEDTOBEFREE:
                    return "Both hands need to be free.";

                case ReturnValueType.RETURNVALUE_CANNOTBEDRESSED:
                    return "You cannot dress this object there.";

                case ReturnValueType.RETURNVALUE_PUTTHISOBJECTINYOURHAND:
                    return "Put this object in your hand.";

                case ReturnValueType.RETURNVALUE_PUTTHISOBJECTINBOTHHANDS:
                    return "Put this object in both hands.";

                case ReturnValueType.RETURNVALUE_CANONLYUSEONEWEAPON:
                    return "You may only use one weapon.";

                case ReturnValueType.RETURNVALUE_TOOFARAWAY:
                    return "You are too far away.";

                case ReturnValueType.RETURNVALUE_FIRSTGODOWNSTAIRS:
                    return "First go downstairs.";

                case ReturnValueType.RETURNVALUE_FIRSTGOUPSTAIRS:
                    return "First go upstairs.";

                case ReturnValueType.RETURNVALUE_NOTENOUGHCAPACITY:
                    return "This object is too heavy for you to carry.";

                case ReturnValueType.RETURNVALUE_CONTAINERNOTENOUGHROOM:
                    return "You cannot put more objects in this container.";

                case ReturnValueType.RETURNVALUE_NEEDEXCHANGE:
                case ReturnValueType.RETURNVALUE_NOTENOUGHROOM:
                    return "There is not enough room.";

                case ReturnValueType.RETURNVALUE_CANNOTPICKUP:
                    return "You cannot take this object.";

                case ReturnValueType.RETURNVALUE_CANNOTTHROW:
                    return "You cannot throw there.";

                case ReturnValueType.RETURNVALUE_THEREISNOWAY:
                    return "There is no way.";

                case ReturnValueType.RETURNVALUE_THISISIMPOSSIBLE:
                    return "This is impossible.";

                case ReturnValueType.RETURNVALUE_PLAYERISPZLOCKED:
                    return "You can not enter a protection zone after attacking another player.";

                case ReturnValueType.RETURNVALUE_PLAYERISNOTINVITED:
                    return "You are not invited.";

                case ReturnValueType.RETURNVALUE_CREATUREDOESNOTEXIST:
                    return "Creature does not exist.";

                case ReturnValueType.RETURNVALUE_DEPOTISFULL:
                    return "You cannot put more items in this depot.";

                case ReturnValueType.RETURNVALUE_CANNOTUSETHISOBJECT:
                    return "You cannot use this object.";

                case ReturnValueType.RETURNVALUE_PLAYERWITHTHISNAMEISNOTONLINE:
                    return "A player with this name is not online.";

                case ReturnValueType.RETURNVALUE_NOTREQUIREDLEVELTOUSERUNE:
                    return "You do not have the required magic level to use this rune.";

                case ReturnValueType.RETURNVALUE_YOUAREALREADYTRADING:
                    return "You are already trading. Finish this trade first.";

                case ReturnValueType.RETURNVALUE_THISPLAYERISALREADYTRADING:
                    return "This player is already trading.";

                case ReturnValueType.RETURNVALUE_YOUMAYNOTLOGOUTDURINGAFIGHT:
                    return "You may not logout during or immediately after a fight!";

                case ReturnValueType.RETURNVALUE_DIRECTPLAYERSHOOT:
                    return "You are not allowed to shoot directly on players.";

                case ReturnValueType.RETURNVALUE_NOTENOUGHLEVEL:
                    return "Your level is too low.";

                case ReturnValueType.RETURNVALUE_NOTENOUGHMAGICLEVEL:
                    return "You do not have enough magic level.";

                case ReturnValueType.RETURNVALUE_NOTENOUGHMANA:
                    return "You do not have enough mana.";

                case ReturnValueType.RETURNVALUE_NOTENOUGHSOUL:
                    return "You do not have enough soul.";

                case ReturnValueType.RETURNVALUE_YOUAREEXHAUSTED:
                    return "You are exhausted.";

                case ReturnValueType.RETURNVALUE_YOUCANNOTUSEOBJECTSTHATFAST:
                    return "You cannot use objects that fast.";

                case ReturnValueType.RETURNVALUE_CANONLYUSETHISRUNEONCREATURES:
                    return "You can only use it on creatures.";

                case ReturnValueType.RETURNVALUE_PLAYERISNOTREACHABLE:
                    return "Player is not reachable.";

                case ReturnValueType.RETURNVALUE_CREATUREISNOTREACHABLE:
                    return "Creature is not reachable.";

                case ReturnValueType.RETURNVALUE_ACTIONNOTPERMITTEDINPROTECTIONZONE:
                    return "This action is not permitted in a protection zone.";

                case ReturnValueType.RETURNVALUE_YOUMAYNOTATTACKTHISPLAYER:
                    return "You may not attack this person.";

                case ReturnValueType.RETURNVALUE_YOUMAYNOTATTACKTHISCREATURE:
                    return "You may not attack this creature.";

                case ReturnValueType.RETURNVALUE_YOUMAYNOTATTACKAPERSONINPROTECTIONZONE:
                    return "You may not attack a person in a protection zone.";

                case ReturnValueType.RETURNVALUE_YOUMAYNOTATTACKAPERSONWHILEINPROTECTIONZONE:
                    return "You may not attack a person while you are in a protection zone.";

                case ReturnValueType.RETURNVALUE_YOUCANONLYUSEITONCREATURES:
                    return "You can only use it on creatures.";

                case ReturnValueType.RETURNVALUE_TURNSECUREMODETOATTACKUNMARKEDPLAYERS:
                    return "Turn secure mode off if you really want to attack unmarked players.";

                case ReturnValueType.RETURNVALUE_YOUNEEDPREMIUMACCOUNT:
                    return "You need a premium account.";

                case ReturnValueType.RETURNVALUE_YOUNEEDTOLEARNTHISSPELL:
                    return "You must learn this spell first.";

                case ReturnValueType.RETURNVALUE_YOURVOCATIONCANNOTUSETHISSPELL:
                    return "You have the wrong vocation to cast this spell.";

                case ReturnValueType.RETURNVALUE_YOUNEEDAWEAPONTOUSETHISSPELL:
                    return "You need to equip a weapon to use this spell.";

                case ReturnValueType.RETURNVALUE_PLAYERISPZLOCKEDLEAVEPVPZONE:
                    return "You can not leave a pvp zone after attacking another player.";

                case ReturnValueType.RETURNVALUE_PLAYERISPZLOCKEDENTERPVPZONE:
                    return "You can not enter a pvp zone after attacking another player.";

                case ReturnValueType.RETURNVALUE_ACTIONNOTPERMITTEDINANOPVPZONE:
                    return "This action is not permitted in a non pvp zone.";

                case ReturnValueType.RETURNVALUE_YOUCANNOTLOGOUTHERE:
                    return "You can not logout here.";

                case ReturnValueType.RETURNVALUE_YOUNEEDAMAGICITEMTOCASTSPELL:
                    return "You need a magic item to cast this spell.";

                case ReturnValueType.RETURNVALUE_CANNOTCONJUREITEMHERE:
                    return "You cannot conjure items here.";

                case ReturnValueType.RETURNVALUE_YOUNEEDTOSPLITYOURSPEARS:
                    return "You need to split your spears first.";

                case ReturnValueType.RETURNVALUE_NAMEISTOOAMBIGUOUS:
                    return "Player name is ambiguous.";

                case ReturnValueType.RETURNVALUE_CANONLYUSEONESHIELD:
                    return "You may use only one shield.";

                case ReturnValueType.RETURNVALUE_NOPARTYMEMBERSINRANGE:
                    return "No party members in range.";

                case ReturnValueType.RETURNVALUE_YOUARENOTTHEOWNER:
                    return "You are not the owner.";

                case ReturnValueType.RETURNVALUE_NOSUCHRAIDEXISTS:
                    return "No such raid exists.";

                case ReturnValueType.RETURNVALUE_ANOTHERRAIDISALREADYEXECUTING:
                    return "Another raid is already executing.";

                case ReturnValueType.RETURNVALUE_TRADEPLAYERFARAWAY:
                    return "Trade player is too far away.";

                case ReturnValueType.RETURNVALUE_YOUDONTOWNTHISHOUSE:
                    return "You don't own this house.";

                case ReturnValueType.RETURNVALUE_TRADEPLAYERALREADYOWNSAHOUSE:
                    return "Trade player already owns a house.";

                case ReturnValueType.RETURNVALUE_TRADEPLAYERHIGHESTBIDDER:
                    return "Trade player is currently the highest bidder of an auctioned house.";

                case ReturnValueType.RETURNVALUE_YOUCANNOTTRADETHISHOUSE:
                    return "You can not trade this house.";

                case ReturnValueType.RETURNVALUE_YOUDONTHAVEREQUIREDPROFESSION:
                    return "You don't have the required profession.";

                case ReturnValueType.RETURNVALUE_ITEMCANNOTBEMOVEDTHERE:
                    return "This item cannot be moved there.";

                case ReturnValueType.RETURNVALUE_YOUCANNOTUSETHISBED:
                    return "This bed can't be used, but Premium Account players can rent houses and sleep in beds there to regain health and mana.";

                default: // ReturnValueType.RETURNVALUE_NOTPOSSIBLE, etc
                    return "Sorry, not possible.";
            }
        }
    }
}
