namespace NeoServer.Domain.Houses;

public enum HouseAccessLevel : byte
{
    NotInvited = 0,
    Guest = 1,
    SubOwner = 2,
    Owner = 3
}
