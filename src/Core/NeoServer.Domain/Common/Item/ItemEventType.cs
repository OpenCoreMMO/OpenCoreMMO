namespace NeoServer.Domain.Common.Item;

public enum ItemEventType : byte
{
    Use,
    MultiUse,
    Movement,
    Collision,
    Separation
}