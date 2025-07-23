---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Spell
Spell = {}

---Registers a callback for spell casting.
---@param callback fun(spell: Spell)
function Spell:onCastSpell(callback) end

---Registers the spell in the system.
---@return boolean
function Spell:register() end

---Gets or sets the spell name.
---@param name string|nil
---@return string|boolean
function Spell:name(name) end

---Gets or sets the spell id.
---@param id number|nil
---@return number|boolean
function Spell:id(id) end

---Gets or sets the spell group.
---@param primaryGroup number|string|nil
---@param secondaryGroup number|nil
---@return number|boolean
function Spell:group(primaryGroup, secondaryGroup) end

---Gets or sets the spell cooldown.
---@param cooldown number|nil
---@return number|boolean
function Spell:cooldown(cooldown) end

---Gets or sets the group cooldowns.
---@param primaryGroupCd number|nil
---@param secondaryGroupCd number|nil
---@return number|boolean
function Spell:groupCooldown(primaryGroupCd, secondaryGroupCd) end

---Gets or sets the required level for the spell.
---@param level number|nil
---@return number|boolean
function Spell:level(level) end

---Gets or sets the required magic level for the spell.
---@param magicLevel number|nil
---@return number|boolean
function Spell:magicLevel(magicLevel) end

---Gets or sets the mana cost for the spell.
---@param mana number|nil
---@return number|boolean
function Spell:mana(mana) end

---Gets or sets the mana percent cost for the spell.
---@param percent number|nil
---@return number|boolean
function Spell:manaPercent(percent) end

---Gets or sets the soul cost for the spell.
---@param soul number|nil
---@return number|boolean
function Spell:soul(soul) end

---Gets or sets the range for the spell.
---@param range number|nil
---@return number|boolean
function Spell:range(range) end

---Gets or sets if the spell requires premium.
---@param isPremium boolean|nil
---@return boolean
function Spell:isPremium(isPremium) end

---Gets or sets if the spell is enabled.
---@param isEnabled boolean|nil
---@return boolean
function Spell:isEnabled(isEnabled) end

---Gets or sets if the spell needs a target.
---@param needTarget boolean|nil
---@return boolean
function Spell:needTarget(needTarget) end

---Gets or sets if the spell needs a weapon.
---@param needWeapon boolean|nil
---@return boolean
function Spell:needWeapon(needWeapon) end

---Gets or sets if the spell needs to be learned.
---@param needLearn boolean|nil
---@return boolean
function Spell:needLearn(needLearn) end

---Gets or sets if the spell is PZ locked.
---@param pzLocked boolean|nil
---@return boolean
function Spell:setPzLocked(pzLocked) end

---Gets or sets if the spell is self-targeted.
---@param isSelfTarget boolean|nil
---@return boolean
function Spell:isSelfTarget(isSelfTarget) end

---Gets or sets if the spell is blocking.
---@param blockingSolid boolean|nil
---@param blockingCreature boolean|nil
---@return boolean
function Spell:isBlocking(blockingSolid, blockingCreature) end

---Gets or sets if the spell is aggressive.
---@param isAggressive boolean|nil
---@return boolean
function Spell:isAggressive(isAggressive) end

---Gets or sets the vocations for the spell.
---@param vocation string|nil
---@return table|boolean
function Spell:vocation(vocation) end

---Gets or sets the spell words (for instant spells).
---@param words string|nil
---@param separator string|nil
---@return string|boolean
function Spell:words(words, separator) end

---Gets or sets if the spell needs direction (for instant spells).
---@param needDirection boolean|nil
---@return boolean
function Spell:needDirection(needDirection) end

---Gets or sets if the spell has parameters (for instant spells).
---@param hasParams boolean|nil
---@return boolean
function Spell:hasParams(hasParams) end

---Gets or sets if the spell needs caster target or direction (for instant spells).
---@param needCasterTargetOrDirection boolean|nil
---@return boolean
function Spell:needCasterTargetOrDirection(needCasterTargetOrDirection) end

---Gets or sets the rune id (for rune spells).
---@param id number|nil
---@return number|boolean
function Spell:runeId(id) end

---Gets or sets the charges (for rune spells).
---@param charges number|nil
---@return number|boolean
function Spell:charges(charges) end

---Gets or sets if the rune spell allows far use.
---@param allow boolean|nil
---@return boolean
function Spell:allowFarUse(allow) end

---Gets or sets if the rune spell blocks walls.
---@param block boolean|nil
---@return boolean
function Spell:blockWalls(block) end

---Gets or sets if the rune spell checks the floor.
---@param check boolean|nil
---@return boolean
function Spell:checkFloor(check) end

_G.Spell = Spell  -- Set as global for LSP

return Spell