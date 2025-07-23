---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Bank
Bank = {}

---Credita um valor ao banco (player ou guild).
---@param entity table -- player ou guild
---@param amount number
function Bank:credit(entity, amount) end

---Debita um valor do banco (player ou guild).
---@param entity table -- player ou guild
---@param amount number
function Bank:debit(entity, amount) end

---Retorna o saldo do banco (player ou guild).
---@param entity table -- player ou guild
---@return number
function Bank:getBalance(entity) end

---Registra o banco no sistema.
function Bank:register() end

_G.Bank = Bank  -- Set as global for LSP

return Bank