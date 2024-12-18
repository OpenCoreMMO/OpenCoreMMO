namespace NeoServer.Application.Common.Contracts.Scripts;

public interface ILuaManager
{
    void Start();
    ITalkAction GetTalkAction(string name);
}
