---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Monster : Creature
Monster = {}

---Checks if the object is a monster.
---@return boolean
function Monster:isMonster() end

_G.Monster = Monster  -- Set as global for LSP

return Monster