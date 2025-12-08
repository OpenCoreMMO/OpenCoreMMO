using System.Collections.Generic;
using System.Linq;

namespace NeoServer.Loaders.OTB.Structure;

/// <summary>
///     OTB structure class.
///     OTB files only have Header and Items Node
/// </summary>
public class Otb
{
    //todo: implement header class

    /// <summary>
    ///     Item nodes data of this OTB structure
    /// </summary>
    /// <value></value>
    public IReadOnlyCollection<ItemNode> ItemNodes { get; set; }

    /// <summary>
    ///     Creates a new instance of a <see cref="Otb" />.
    /// </summary>
    /// <param name="node"></param>
    public Otb(OtbNode node)
    {
        ItemNodes = node.Children.AsParallel().Select(c => new ItemNode(c)).ToList();
    }
}