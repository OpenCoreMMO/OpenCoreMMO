using NeoServer.Loaders.OTB.Parsers;

namespace NeoServer.Loaders.OTBM.Structure.TileArea;

public class CustomAttribute
{
    public string Key { get; set; }
    public object Value { get; set; }

    public CustomAttribute(OtbParsingStream stream)
    {
        Key = stream.ReadString();

        var pos = stream.ReadByte();

        switch (pos)
        {
            case 1:
                Value = stream.ReadString();
                break;
            case 2:
                Value = stream.ReadUInt64();
                break;
            case 3:
                Value = stream.ReadDouble();
                break;
            case 4:
                Value = stream.ReadBool();
                break;
            default:
                Value = null;
                break;
        }
    }
}