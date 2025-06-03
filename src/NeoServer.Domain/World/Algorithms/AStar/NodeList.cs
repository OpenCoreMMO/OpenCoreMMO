using System.Runtime.CompilerServices;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.World.Algorithms.AStar;

internal class NodeList
{
    private readonly PriorityQueue<ushort, int> bestNodes = new();
    private readonly List<Node> nodes = new();
    private readonly Dictionary<uint, ushort> nodesMap = new();

    public NodeList(Location location)
    {
        ClosedNodes = 0;

        var startNode = new Node(location.X, location.Y)
        {
            F = 0
        };

        AddNewNode(startNode);
    }

    public int ClosedNodes { get; private set; }

    private void AddNewNode(Node node)
    {
        nodes.Add(node);

        var index = (ushort)(nodes.Count - 1);

        nodesMap.Add((uint)((node.X << 16) | node.Y), index);
        bestNodes.Enqueue(index, node.Weight);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Node GetBestNode()
    {
        while (true)
        {
            if (!bestNodes.TryDequeue(out var bestNodeIndex, out _)) return null;

            var node = nodes[bestNodeIndex];

            if (!node.IsOpen)
                continue;

            return node;
        }
    }

    internal void CloseNode(Node node)
    {
        node.Close();
        ++ClosedNodes;
    }

    internal Node CreateOpenNode(Node parent, ushort x, ushort y, int newF, int heuristic, byte extraCost)
    {
        if (nodes.Count >= 512) return null;

        var node = new Node(x, y)
        {
            F = newF,
            Heuristic = heuristic,
            ExtraCost = extraCost,
            Parent = parent
        };

        AddNewNode(node);
        return node;
    }

    internal Node GetNodeByPosition(Location location)
    {
        return nodesMap.TryGetValue((uint)((location.X << 16) | location.Y), out var foundNodeIndex)
            ? nodes[foundNodeIndex]
            : null;
    }

    internal void OpenNode(Node node)
    {
        var index = nodesMap[(uint)((node.X << 16) | node.Y)];

        if (index >= 512) return;

        ClosedNodes -= node.IsOpen ? 0 : 1;
        node.Open();
        bestNodes.Enqueue(index, node.Weight);
    }
}