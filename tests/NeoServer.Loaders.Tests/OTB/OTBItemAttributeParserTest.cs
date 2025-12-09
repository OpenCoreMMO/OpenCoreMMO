using System;
using System.Collections.Generic;
using FluentAssertions;
using NeoServer.Loaders.OTB.Enums;
using NeoServer.Loaders.OTB.Parsers;
using Xunit;

namespace NeoServer.Loaders.Tests.OTB;

public class OTBItemAttributeParserTest
{
    [Fact]
    public void Instance_StreamNull_Throws()
    {
        var sut = new OtbParsingItemAttribute(null);
        sut.Attributes.Should().BeEmpty();
    }

    [Fact]
    public void Instance_StreamEmpty_ReturnsNoAttributes()
    {
        var instance = new OtbParsingItemAttribute(new OtbParsingStream(new ReadOnlyMemory<byte>()));

        Assert.Empty(instance.Attributes);
    }

    [Fact]
    public void Instance_Stream_ReturnsAllAttributes()
    {
        var stream = new OtbParsingStream(new ReadOnlyMemory<byte>([
            0x10, //serverId
            0x02, //data length 2
            0x00,
            0xA4, // serverId
            0x00,
            0x11, //clientId
            0x02, //data length 2
            0x00,
            0x86,
            0x00,
            0x2A, //light
            0x04,
            0x00,
            0x22, //light level
            0x00,
            0x24, //light color
            0x00,
            0x14, //speed
            0x02, //data length
            0x00,
            0xCC,
            0x00,
            0x2B, //topOrder
            0x01,
            0x00,
            0x05,
            0x2D, //wareId
            0x02,
            0x00,
            0x5A,
            0x00
        ]));

        var instance = new OtbParsingItemAttribute(stream);

        var expected = new Dictionary<OtbItemAttribute, IConvertible>
        {
            { OtbItemAttribute.ServerId, (ushort)164 },
            { OtbItemAttribute.ClientId, (ushort)134 },
            { OtbItemAttribute.LightLevel, (byte)34 },
            { OtbItemAttribute.LightColor, (byte)36 },
            { OtbItemAttribute.Speed, (ushort)204 },
            { OtbItemAttribute.TopOrder, (byte)5 },
            { OtbItemAttribute.WareId, (ushort)90 }
        };

        foreach (var expect in expected) Assert.Equal(expect.Value, instance.Attributes[expect.Key]);
    }

    [Fact]
    public void Instance_DuplicatedAttributeStream_ReturnsAllSingleAttributes()
    {
        var stream = new OtbParsingStream(new ReadOnlyMemory<byte>([
            0x10, //serverId
            0x02, //data length 2
            0x00,
            0xA4, // serverId
            0x00,
            0x11, //clientId
            0x02, //data length 2
            0x00,
            0x86,
            0x00,
            0x2A, //light
            0x04,
            0x00,
            0x22, //light level
            0x00,
            0x24, //light color
            0x00,
            0x2A, //light
            0x04,
            0x00,
            0x22, //light level
            0x00,
            0x24, //light color
            0x00,
            0x14, //speed
            0x02, //data length
            0x00,
            0xCC,
            0x00,
            0x2B, //topOrder
            0x01,
            0x00,
            0x05,
            0x2D, //wareId
            0x02,
            0x00,
            0x5A,
            0x00
        ]));

        var instance = new OtbParsingItemAttribute(stream);

        var expected = new Dictionary<OtbItemAttribute, IConvertible>
        {
            { OtbItemAttribute.ServerId, (ushort)164 },
            { OtbItemAttribute.ClientId, (ushort)134 },
            { OtbItemAttribute.LightLevel, (byte)34 },
            { OtbItemAttribute.LightColor, (byte)36 },
            { OtbItemAttribute.Speed, (ushort)204 },
            { OtbItemAttribute.TopOrder, (byte)5 },
            { OtbItemAttribute.WareId, (ushort)90 }
        };

        foreach (var expect in expected) Assert.Equal(expect.Value, instance.Attributes[expect.Key]);
    }
}