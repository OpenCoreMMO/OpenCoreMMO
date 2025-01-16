namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface IGlobalEventsScripts
{
    void GlobalEventExecuteRecord(int current, int old);
    void GlobalEventExecuteShutdown();
    void GlobalEventExecuteSave();
}