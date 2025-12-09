using System;
using System.Collections.Generic;
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
        var serializedOtbmData = otbmStream[4..];
        var memoryStream = new ReadOnlyMemoryStream(serializedOtbmData);

        return BuildTree(new OtbNode(NodeType.NotSetYet), memoryStream).Children.Span[0];
    }

    private static OtbNode BuildTree(OtbNode node, ReadOnlyMemoryStream stream)
    {
        var nodeStack = new Stack<OtbNode>();
        nodeStack.Push(node);

        while (!stream.IsOver && nodeStack.Count > 0)
        {
            var currentNode = nodeStack.Peek();
            var currentByte = stream.ReadByte();

            switch ((OtbMarkupByte)currentByte)
            {
                case OtbMarkupByte.Start:
                    var childNode = new OtbNode((NodeType)stream.ReadByte());
                    currentNode.AddChild(childNode);
                    nodeStack.Push(childNode);
                    break;

                case OtbMarkupByte.Escape:
                    currentNode.AddData(currentByte);
                    currentNode.AddData(stream.ReadByte());
                    break;

                case OtbMarkupByte.End:
                    nodeStack.Pop();
                    break;

                default:
                    currentNode.AddData(currentByte);
                    break;
            }
        }

        return node;
    }
}