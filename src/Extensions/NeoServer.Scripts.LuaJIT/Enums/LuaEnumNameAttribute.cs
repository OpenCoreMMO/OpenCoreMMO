namespace NeoServer.Scripts.LuaJIT.Enums;

[AttributeUsage(AttributeTargets.Field)]
public class LuaEnumNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}