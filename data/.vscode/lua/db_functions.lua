---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class DB
DB = {}

---Executes a SQL query.
---@param query string
---@return boolean
function DB:query(query) end

---Executes a SQL query asynchronously.
---@param query string
---@return boolean
function DB:asyncQuery(query) end

---Executes a SQL query and stores the result.
---@param query string
---@return number|boolean
function DB:storeQuery(query) end

---Executes a SQL query asynchronously and stores the result.
---@param query string
---@return number|boolean
function DB:asyncStoreQuery(query) end

---Escapes a string for use in SQL queries.
---@param value string
---@return string
function DB:escapeString(value) end

---Checks if a table exists in the database.
---@param name string
---@return boolean
function DB:tableExists(name) end

_G.db = DB  -- Set as global for LSP

return DB