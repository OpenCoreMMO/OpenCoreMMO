print('print from core.lua')
print(configKeys.BASE_DIRECTORY)

logger.debug('Starting lua.')
print(os.getenv('LOCAL_LUA_DEBUGGER_VSCODE'))
if os.getenv('LOCAL_LUA_DEBUGGER_VSCODE') == '1' then
  require('lldebugger').start()
  logger.debug('Started LUA debugger.')
end

CORE_DIRECTORY = configKeys.BASE_DIRECTORY .. 'DataLuaJit'

dofile(CORE_DIRECTORY .. '/global.lua')
dofile(CORE_DIRECTORY .. '/libs/libs.lua')