using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Extensions;

public static class ReturnMessageExtensions
{
    public static string GetReturnMessage(this ReturnValueType value)
    {
        return value switch
        {
            ReturnValueType.RETURNVALUE_DESTINATIONOUTOFREACH => "Destination is out of range.",
            ReturnValueType.RETURNVALUE_NOTMOVEABLE => "You cannot move this object.",
            ReturnValueType.RETURNVALUE_DROPTWOHANDEDITEM => "Drop the double-handed object first.",
            ReturnValueType.RETURNVALUE_BOTHHANDSNEEDTOBEFREE => "Both hands need to be free.",
            ReturnValueType.RETURNVALUE_CANNOTBEDRESSED => "You cannot dress this object there.",
            ReturnValueType.RETURNVALUE_PUTTHISOBJECTINYOURHAND => "Put this object in your hand.",
            ReturnValueType.RETURNVALUE_PUTTHISOBJECTINBOTHHANDS => "Put this object in both hands.",
            ReturnValueType.RETURNVALUE_CANONLYUSEONEWEAPON => "You may only use one weapon.",
            ReturnValueType.RETURNVALUE_TOOFARAWAY => "You are too far away.",
            ReturnValueType.RETURNVALUE_FIRSTGODOWNSTAIRS => "First go downstairs.",
            ReturnValueType.RETURNVALUE_FIRSTGOUPSTAIRS => "First go upstairs.",
            ReturnValueType.RETURNVALUE_NOTENOUGHCAPACITY => "This object is too heavy for you to carry.",
            ReturnValueType.RETURNVALUE_CONTAINERNOTENOUGHROOM => "You cannot put more objects in this container.",
            ReturnValueType.RETURNVALUE_NEEDEXCHANGE or ReturnValueType.RETURNVALUE_NOTENOUGHROOM =>
                "There is not enough room.",
            ReturnValueType.RETURNVALUE_CANNOTPICKUP => "You cannot take this object.",
            ReturnValueType.RETURNVALUE_CANNOTTHROW => "You cannot throw there.",
            ReturnValueType.RETURNVALUE_THEREISNOWAY => "There is no way.",
            ReturnValueType.RETURNVALUE_THISISIMPOSSIBLE => "This is impossible.",
            ReturnValueType.RETURNVALUE_PLAYERISPZLOCKED =>
                "You can not enter a protection zone after attacking another player.",
            ReturnValueType.RETURNVALUE_PLAYERISNOTINVITED => "You are not invited.",
            ReturnValueType.RETURNVALUE_CREATUREDOESNOTEXIST => "Creature does not exist.",
            ReturnValueType.RETURNVALUE_DEPOTISFULL => "You cannot put more items in this depot.",
            ReturnValueType.RETURNVALUE_CANNOTUSETHISOBJECT => "You cannot use this object.",
            ReturnValueType.RETURNVALUE_PLAYERWITHTHISNAMEISNOTONLINE => "A player with this name is not online.",
            ReturnValueType.RETURNVALUE_NOTREQUIREDLEVELTOUSERUNE =>
                "You do not have the required magic level to use this rune.",
            ReturnValueType.RETURNVALUE_YOUAREALREADYTRADING => "You are already trading. Finish this trade first.",
            ReturnValueType.RETURNVALUE_THISPLAYERISALREADYTRADING => "This player is already trading.",
            ReturnValueType.RETURNVALUE_YOUMAYNOTLOGOUTDURINGAFIGHT =>
                "You may not logout during or immediately after a fight!",
            ReturnValueType.RETURNVALUE_DIRECTPLAYERSHOOT => "You are not allowed to shoot directly on players.",
            ReturnValueType.RETURNVALUE_NOTENOUGHLEVEL => "Your level is too low.",
            ReturnValueType.RETURNVALUE_NOTENOUGHMAGICLEVEL => "You do not have enough magic level.",
            ReturnValueType.RETURNVALUE_NOTENOUGHMANA => "You do not have enough mana.",
            ReturnValueType.RETURNVALUE_NOTENOUGHSOUL => "You do not have enough soul.",
            ReturnValueType.RETURNVALUE_YOUAREEXHAUSTED => "You are exhausted.",
            ReturnValueType.RETURNVALUE_YOUCANNOTUSEOBJECTSTHATFAST => "You cannot use objects that fast.",
            ReturnValueType.RETURNVALUE_CANONLYUSETHISRUNEONCREATURES => "You can only use it on creatures.",
            ReturnValueType.RETURNVALUE_PLAYERISNOTREACHABLE => "Player is not reachable.",
            ReturnValueType.RETURNVALUE_CREATUREISNOTREACHABLE => "Creature is not reachable.",
            ReturnValueType.RETURNVALUE_ACTIONNOTPERMITTEDINPROTECTIONZONE =>
                "This action is not permitted in a protection zone.",
            ReturnValueType.RETURNVALUE_YOUMAYNOTATTACKTHISPLAYER => "You may not attack this person.",
            ReturnValueType.RETURNVALUE_YOUMAYNOTATTACKTHISCREATURE => "You may not attack this creature.",
            ReturnValueType.RETURNVALUE_YOUMAYNOTATTACKAPERSONINPROTECTIONZONE =>
                "You may not attack a person in a protection zone.",
            ReturnValueType.RETURNVALUE_YOUMAYNOTATTACKAPERSONWHILEINPROTECTIONZONE =>
                "You may not attack a person while you are in a protection zone.",
            ReturnValueType.RETURNVALUE_YOUCANONLYUSEITONCREATURES => "You can only use it on creatures.",
            ReturnValueType.RETURNVALUE_TURNSECUREMODETOATTACKUNMARKEDPLAYERS =>
                "Turn secure mode off if you really want to attack unmarked players.",
            ReturnValueType.RETURNVALUE_YOUNEEDPREMIUMACCOUNT => "You need a premium account.",
            ReturnValueType.RETURNVALUE_YOUNEEDTOLEARNTHISSPELL => "You must learn this spell first.",
            ReturnValueType.RETURNVALUE_YOURVOCATIONCANNOTUSETHISSPELL =>
                "You have the wrong vocation to cast this spell.",
            ReturnValueType.RETURNVALUE_YOUNEEDAWEAPONTOUSETHISSPELL =>
                "You need to equip a weapon to use this spell.",
            ReturnValueType.RETURNVALUE_PLAYERISPZLOCKEDLEAVEPVPZONE =>
                "You can not leave a pvp zone after attacking another player.",
            ReturnValueType.RETURNVALUE_PLAYERISPZLOCKEDENTERPVPZONE =>
                "You can not enter a pvp zone after attacking another player.",
            ReturnValueType.RETURNVALUE_ACTIONNOTPERMITTEDINANOPVPZONE =>
                "This action is not permitted in a non pvp zone.",
            ReturnValueType.RETURNVALUE_YOUCANNOTLOGOUTHERE => "You can not logout here.",
            ReturnValueType.RETURNVALUE_YOUNEEDAMAGICITEMTOCASTSPELL => "You need a magic item to cast this spell.",
            ReturnValueType.RETURNVALUE_CANNOTCONJUREITEMHERE => "You cannot conjure items here.",
            ReturnValueType.RETURNVALUE_YOUNEEDTOSPLITYOURSPEARS => "You need to split your spears first.",
            ReturnValueType.RETURNVALUE_NAMEISTOOAMBIGUOUS => "Player name is ambiguous.",
            ReturnValueType.RETURNVALUE_CANONLYUSEONESHIELD => "You may use only one shield.",
            ReturnValueType.RETURNVALUE_NOPARTYMEMBERSINRANGE => "No party members in range.",
            ReturnValueType.RETURNVALUE_YOUARENOTTHEOWNER => "You are not the owner.",
            ReturnValueType.RETURNVALUE_NOSUCHRAIDEXISTS => "No such raid exists.",
            ReturnValueType.RETURNVALUE_ANOTHERRAIDISALREADYEXECUTING => "Another raid is already executing.",
            ReturnValueType.RETURNVALUE_TRADEPLAYERFARAWAY => "Trade player is too far away.",
            ReturnValueType.RETURNVALUE_YOUDONTOWNTHISHOUSE => "You don't own this house.",
            ReturnValueType.RETURNVALUE_TRADEPLAYERALREADYOWNSAHOUSE => "Trade player already owns a house.",
            ReturnValueType.RETURNVALUE_TRADEPLAYERHIGHESTBIDDER =>
                "Trade player is currently the highest bidder of an auctioned house.",
            ReturnValueType.RETURNVALUE_YOUCANNOTTRADETHISHOUSE => "You can not trade this house.",
            ReturnValueType.RETURNVALUE_YOUDONTHAVEREQUIREDPROFESSION => "You don't have the required profession.",
            ReturnValueType.RETURNVALUE_ITEMCANNOTBEMOVEDTHERE => "This item cannot be moved there.",
            ReturnValueType.RETURNVALUE_YOUCANNOTUSETHISBED =>
                "This bed can't be used, but Premium Account players can rent houses and sleep in beds there to regain health and mana.",
            _ => "Sorry, not possible."
        };
    }
}