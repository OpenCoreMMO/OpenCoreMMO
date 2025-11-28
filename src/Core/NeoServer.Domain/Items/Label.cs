using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Items;

public class Label(IItemType metadata, Location location) : Paper(metadata, location)
{
    public bool HasDestination => !string.IsNullOrWhiteSpace(Destination);

    public string Destination
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Text)) return null;

            var newLineIndex = Text.IndexOf('\n');
            var firstLine = newLineIndex >= 0 ? Text[..newLineIndex] : Text;
            return firstLine.Trim();
        }
    }
}