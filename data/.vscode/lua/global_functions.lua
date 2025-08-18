---@diagnostic disable: lowercase-global

--- Gets the metatable by name.
-- @param metatableName string The name of the metatable.
-- @return table The metatable.
function rawgetmetatable(metatableName)
    -- Implementation should call the C# backend.
end

--- Adds a timed event with a callback.
-- @param callback function The function to call.
-- @param delay number The delay in milliseconds.
-- @param ... any Additional parameters for the callback.
-- @return number The event ID.
function addEvent(callback, delay, ...)
    -- Implementation should call the C# backend.
end

--- Stops a previously added event.
-- @param eventId number The ID of the event to stop.
-- @return boolean True if the event was stopped, false otherwise.
function stopEvent(eventId)
    -- Implementation should call the C# backend.
end

--- Sends a message to a chat channel.
-- @param channelId number The ID of the channel.
-- @param type number The type of message (SpeakClassesType).
-- @param message string The message to send.
-- @return boolean True if the message was sent, false otherwise.
function sendChannelMessage(channelId, type, message)
    -- Implementation should call the C# backend.
end

--- Returns the current world time.
-- @return number The current world time.
function getWorldTime()
    -- Implementation should call the C# backend.
end

--- Returns the current world light level and color.
-- @return number The light level.
-- @return number The light color.
function getWorldLight()
    -- Implementation should call the C# backend.
end

--- Creates a combat area.
-- @param area table The area definition.
-- @param extArea table Optional extended area definition.
-- @return table The created combat area.
function createCombatArea(area, extArea)
    -- Implementation should call the C# backend.
end