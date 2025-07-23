---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class MonsterType
MonsterType = {}

---Checks or sets if the monster is attackable.
---@param value? boolean
---@return boolean|nil
function MonsterType:isAttackable(value) end

---Checks or sets if the monster is convinceable.
---@param value? boolean
---@return boolean|nil
function MonsterType:isConvinceable(value) end

---Checks or sets if the monster is summonable.
---@param value? boolean
---@return boolean|nil
function MonsterType:isSummonable(value) end

---Checks or sets if the monster is illusionable.
---@param value? boolean
---@return boolean|nil
function MonsterType:isIllusionable(value) end

---Checks or sets if the monster is hostile.
---@param value? boolean
---@return boolean|nil
function MonsterType:isHostile(value) end

---Checks or sets if the monster is a reward boss.
---@param value? boolean
---@return boolean|nil
function MonsterType:isRewardBoss(value) end

---Checks or sets if the monster is pushable.
---@param value? boolean
---@return boolean|nil
function MonsterType:isPushable(value) end

---Checks or sets if the monster can push items.
---@param value? boolean
---@return boolean|nil
function MonsterType:canPushItems(value) end

---Checks or sets if the monster can push creatures.
---@param value? boolean
---@return boolean|nil
function MonsterType:canPushCreatures(value) end

---Gets or sets the monster's name.
---@param name? string
---@return string|boolean|nil
function MonsterType:name(name) end

---Gets or sets the monster's name description.
---@param desc? string
---@return string|boolean|nil
function MonsterType:nameDescription(desc) end

---Gets or sets the monster's health.
---@param health? number
---@return number|boolean|nil
function MonsterType:health(health) end

---Gets or sets the monster's max health.
---@param maxHealth? number
---@return number|boolean|nil
function MonsterType:maxHealth(maxHealth) end

---Gets or sets the monster's corpse id.
---@param id? number
---@return number|boolean|nil
function MonsterType:corpseId(id) end

---Gets or sets the monster's mana cost.
---@param mana? number
---@return number|boolean|nil
function MonsterType:manaCost(mana) end

_G.MonsterType = MonsterType

return MonsterType