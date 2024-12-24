logger.debug('Starting lua.')
logger.debug(os.getenv('LOCAL_LUA_DEBUGGER_VSCODE'))
if os.getenv('LOCAL_LUA_DEBUGGER_VSCODE') == '1' then
  require('lldebugger').start()
  logger.debug('Started LUA debugger.')
end

CORE_DIRECTORY = configKeys.BASE_DIRECTORY

dofile(CORE_DIRECTORY .. '/global.lua')
dofile(CORE_DIRECTORY .. '/libs/libs.lua')