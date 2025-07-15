namespace NeoServer.Scripts.LuaJIT.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class LuaEnumNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}