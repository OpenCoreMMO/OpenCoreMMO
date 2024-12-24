namespace NeoServer.Server.Common.Enums;

public enum OperatingSystem
{
    None = 0,
    Linux = 1,
    Windows = 2,
    Flash = 3,
    OtcLinux = 10,
    OtcWindows = 11,
    OtcMac = 12,

    // by default OTCv8 uses CLIENTOS_WINDOWS for backward compatibility
    // for correct value enable g_game.enableFeature(GameExtendedOpcode)
    OtcV8Linux = 20,
    OtcV8Windows = 21,
    OtcV8Mac = 22,
    OtcV8Android = 23,
    OtcV8Ios = 24,
    OtcV8Web = 25
}