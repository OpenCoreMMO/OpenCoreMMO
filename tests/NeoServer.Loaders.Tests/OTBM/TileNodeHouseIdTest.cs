using System.Linq;
using FluentAssertions;
using NeoServer.Domain.Common.Location;
using NeoServer.Loaders.OTB.Enums;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTBM.Loaders;
using NeoServer.Loaders.OTBM.Structure;
using Xunit;

namespace NeoServer.Loaders.Tests.OTBM;

public class TileNodeHouseIdTest
{
    /// <summary>
    ///     Verifies that a HouseTile node's HouseId is read correctly and
    ///     subsequent attributes (tile flags, inline items) parse intact,
    ///     proving the uint32 house-id read does not misalign the stream.
    /// </summary>
    [Fact]
    public void HouseTile_node_reads_houseId_and_parses_subsequent_attributes_correctly()
    {
        // Arrange
        var data = new byte[]
        {
            // Version (4 bytes, skipped by OtbBinaryTreeBuilder.Deserialize)
            0x00, 0x00, 0x00, 0x00,

            // Start RootVersion1 — the OTBM root node
            (byte)OtbMarkupByte.Start, (byte)NodeType.RootVersion1,

            // RootVersion1 data — Header fields (all zeros = valid)
            0x00, 0x00, 0x00, 0x00, // Version = 0
            0x00, 0x00,              // Width  = 0
            0x00, 0x00,              // Height = 0
            0x00,                    // MajorVersionItems = 0
            0x00, 0x00, 0x00,        // Skip 3 padding bytes
            0x00, 0x00, 0x00, 0x00, // MinorVersionItems = 0

            // Start MapData
            (byte)OtbMarkupByte.Start, (byte)NodeType.MapData,

            // MapData attribute: WorldDescription (1) + empty string (length 0)
            0x01, 0x00, 0x00,

            // Start TileArea (X=0, Y=0, Z=7)
            (byte)OtbMarkupByte.Start, (byte)NodeType.TileArea,
            0x00, 0x00, 0x00, 0x00, 0x07,

            // Start HouseTile
            (byte)OtbMarkupByte.Start, (byte)NodeType.HouseTile,

            // HouseTile data: x=5, y=10
            0x05, 0x0A,
            // houseId = 100 (uint32, little-endian)
            0x64, 0x00, 0x00, 0x00,

            // Attribute: TileFlags (3) = ProtectionZone (1)
            0x03, 0x01, 0x00, 0x00, 0x00,

            // Attribute: Item (9) with itemId = 2000 (0x07D0, little-endian)
            0x09, 0xD0, 0x07,

            // End markers
            (byte)OtbMarkupByte.End, // HouseTile
            (byte)OtbMarkupByte.End, // TileArea
            (byte)OtbMarkupByte.End, // MapData
            (byte)OtbMarkupByte.End  // RootVersion1
        };

        // Act — full OTB deserialization + OTBM parsing pipeline
        var rootNode = OtbBinaryTreeBuilder.Deserialize(data);
        var otbm = new OTBMNodeParser().Parse(rootNode);
        var tileArea = otbm.TileAreas.First();
        var tileNode = tileArea.Tiles[0];

        // Assert
        tileNode.HouseId.Should().Be(100);
        tileNode.Flag.Should().Be(TileFlags.ProtectionZone);
        tileNode.Items.Should().HaveCount(1);
        tileNode.Items[0].ItemId.Should().Be(2000);

        // Coordinate offsets should be correct (base 0,0 + x=5, y=10, z=7)
        tileNode.Coordinate.X.Should().Be(5);
        tileNode.Coordinate.Y.Should().Be(10);
        tileNode.Coordinate.Z.Should().Be(7);
    }
}
