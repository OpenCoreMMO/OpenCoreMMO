print('print from config.lua')

coreDirectory = "Data/LuaJit"
showScriptsLogInConsole = true

-- NOTE: true will allow the /reload command to be used
-- NOTE: Using this script might cause unwanted changes
-- This script forces a reload in the entire server, this means that everything that is stored in memory might stop to work properly and/or completely, this script should be used in test environments only
allowReload = true

-- Save interval per time
-- NOTE: toggleSaveInterval: true = enable the save interval, false = disable the save interval
-- NOTE: saveIntervalType: "minute", "second" or "hour"
-- NOTE: toggleSaveIntervalCleanMap: true = enable the clean map, false = disable the clean map
-- NOTE: saveIntervalTime: time based on what was set in "saveIntervalType"
toggleSaveInterval = true
saveIntervalType = "minute"
toggleSaveIntervalCleanMap = true
saveIntervalTime = 1

-- Connection Config
-- NOTE: maxPlayers set to 0 means no limit
-- NOTE: MaxPacketsPerSeconds if you change you will be subject to bugs by WPE, keep the default value of 25
ip = "127.0.0.1"
bindOnlyGlobalAddress = false
loginProtocolPort = 7171
gameProtocolPort = 7172
statusProtocolPort = 7171
maxPlayers = 0
serverName = "OpenCoreMMO"
serverMotd = "Welcome to theOpenCoreMMO!"
onePlayerOnlinePerAccount = true
statusTimeout = 5 * 1000
replaceKickOnLogin = true
maxPacketsPerSecond = 25
maxItem = 2000
maxContainer = 100