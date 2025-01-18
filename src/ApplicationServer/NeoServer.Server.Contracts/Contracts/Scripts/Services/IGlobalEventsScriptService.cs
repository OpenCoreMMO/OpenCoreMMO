namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface IGlobalEventsScriptService
{
    void ExecuteRecord(int current, int old);
    void ExecuteShutdown();
    void ExecuteSave();
}