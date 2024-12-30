using System;
using NeoServer.Loaders.OTB.DataStructures;
using NeoServer.Loaders.OTB.Enums;
using NeoServer.Loaders.OTB.Structure;

namespace NeoServer.Loaders.OTB.Parsers;

public static class OtbBinaryTreeBuilder
{
    /// <summary>
    ///     Creates a OTBNode Binary Tree from otbm stream
    /// </summary>
    /// <param name="otbmStream"></param>
    /// <returns></returns>
    public static OtbNode Deserialize(ReadOnlyMemory<byte> otbmStream)
    {
        var serializedOtbmData = otbmStream.Slice(4); 
        var memoryStream = new ReadOnlyMemoryStream(serializedOtbmData);

        return BuildTree(new OtbNode(NodeType.NotSetYet), memoryStream).Children[0];
    }

    private static OtbNode BuildTree(OtbNode node, ReadOnlyMemoryStream stream) //recursive method to create a binary tree
    {
        var currentByte = stream.ReadByte();

        switch ((OtbMarkupByte)currentByte)
        {
            case OtbMarkupByte.Start:
                while (currentByte == (byte)OtbMarkupByte.Start)
                {
                    var childNode = new OtbNode((NodeType)stream.ReadByte());
                    node.AddChild(BuildTree(childNode, stream));
                    if (stream.IsOver) break;
                    currentByte = stream.ReadByte();
                }

                return node;

            case OtbMarkupByte.Escape:
                node.AddData(currentByte);
                node.AddData(stream.ReadByte());
                return BuildTree(node, stream);
            case OtbMarkupByte.End:
                return node;
            default:
                node.AddData(currentByte);
                return BuildTree(node, stream);
        }
    }
}