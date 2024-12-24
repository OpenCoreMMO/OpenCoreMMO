-- for use of: data\scripts\globalevents\customs\save_interval.lua

SAVE_INTERVAL_TYPE = configManager.getString(configKeys.SAVE_INTERVAL_TYPE)
SAVE_INTERVAL_CONFIG_TIME = configManager.getNumber(configKeys.SAVE_INTERVAL_TIME)
SAVE_INTERVAL_TIME = 0
if SAVE_INTERVAL_TYPE == "second" then
	SAVE_INTERVAL_TIME = 1000
elseif SAVE_INTERVAL_TYPE == "minute" then
	SAVE_INTERVAL_TIME = 60 * 1000
elseif SAVE_INTERVAL_TYPE == "hour" then
	SAVE_INTERVAL_TIME = 60 * 60 * 1000
end

table.contains = function(array, value)
	for _, targetColumn in pairs(array) do
		if targetColumn == value then
			return true
		end
	end
	return false
end