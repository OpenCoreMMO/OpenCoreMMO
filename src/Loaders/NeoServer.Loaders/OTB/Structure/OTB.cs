namespace NeoServer.Loaders.OTB.Structure;

/// <summary>
///     OTB structure class.
///     OTB files only have Header and Items Node
/// </summary>
public class Otb
{
    /// <summary>
    ///     Creates a new instance of a <see cref="Otb" />.
    /// </summary>
    /// <param name="node"></param>
    public Otb(OtbNode node)
    {
        ItemNodes = new ItemNode[node.Children.Length];

        for (var i = 0; i < node.Children.Length; i++)
        {
            var child = node.Children.Span[i];
            ItemNodes[i] = new ItemNode(child);
        }
    }
    //todo: implement header class

    /// <summary>
    ///     Item nodes data of this OTB structure
    /// </summary>
    /// <value></value>
    public ItemNode[] ItemNodes { get; }
}