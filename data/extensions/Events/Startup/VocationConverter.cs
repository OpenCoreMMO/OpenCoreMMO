using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Item;
using NeoServer.Server.Common.Contracts;
using Serilog;

namespace NeoServer.Extensions.Events.Startup;

/// <summary>
///     Converts vocations array string to byte type. ie: ["paladins"] -> [2]
/// </summary>
public class VocationConverter : IStartup
{
    private readonly IItemTypeStore _itemTypeStore;
    private readonly ILogger _logger;
    private readonly IVocationStore _vocationStore;

    public VocationConverter(IItemTypeStore itemTypeStore, ILogger logger, IVocationStore vocationStore)
    {
        _itemTypeStore = itemTypeStore;
        _logger = logger;
        _vocationStore = vocationStore;
    }

    public void Run()
    {
        foreach (var itemType in _itemTypeStore.All)
        {
            var vocationsAttr = itemType.Attributes.GetAttributeArray(ItemAttribute.Vocation);

            if (vocationsAttr is not string[] vocations) continue;
            
            itemType.Attributes.SetAttribute(ItemAttribute.VocationNames, vocations);

            var vocationsType = new List<byte>(vocations.Length);
            foreach (var vocation in vocations)
            {
                var vocationFound = _vocationStore.All.FirstOrDefault(x =>
                    vocation.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
                if (vocationFound is null) continue;

                vocationsType.Add(vocationFound.VocationType);
            }

            itemType.Attributes.SetAttribute(ItemAttribute.Vocation, vocationsType.ToArray());
        }

        _logger.Debug("Extensions: Converting vocations...");
    }
}