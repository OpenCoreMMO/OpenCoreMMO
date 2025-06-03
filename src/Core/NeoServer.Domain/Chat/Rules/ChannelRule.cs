namespace NeoServer.Domain.Chat.Rules;

public struct ChannelRule
{
    public bool None => AllowedVocations?.Length == 0 && MinMaxAllowedLevel.Item1 == 0 &&
                        MinMaxAllowedLevel.Item2 == 0;

    public byte[] AllowedVocations { get; set; }
    public (ushort, ushort) MinMaxAllowedLevel { get; set; }
}