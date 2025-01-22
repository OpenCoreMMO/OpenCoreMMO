local onPlayerOnThinkTest = CreatureEvent("PlayerOnThinkTest")

function onPlayerOnThinkTest.onThink(creature)
	if not creature then
		return true
	end

	local name = creature:getName()

	logger.info(name)

	creature:say("WAAAUUUUUUU10000", TALKTYPE_MONSTER_SAY)
	creature:getPosition():sendMagicEffect(CONST_ME_POFF)

	return true
end

onPlayerOnThinkTest:register()