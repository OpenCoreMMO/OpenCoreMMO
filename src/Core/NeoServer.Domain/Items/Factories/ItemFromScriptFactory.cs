using System.Linq.Expressions;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Items.Factories;

/// <summary>
///     A factory class that creates an instance of <see cref="IItem" /> based on a C# script file.
/// </summary>
public static class ItemFromScriptFactory
{
    private static readonly
        Dictionary<Type, Func<IItemType, Location, IDictionary<ItemTypeAttribute, IConvertible>, IItem>>
        ScriptFactoryMap = new();

    /// <summary>
    ///     Creates an instance of <see cref="IItem" /> based on the specified <paramref name="script" /> file.
    /// </summary>
    /// <param name="itemType">The <see cref="IItemType" /> of the item.</param>
    /// <param name="location">The <see cref="Location" /> of the item.</param>
    /// <param name="attributes">The attributes of the item.</param>
    /// <param name="script">The script file name without the ".cs" extension.</param>
    /// <returns>An instance of <see cref="IItem" /> if the script file was found, otherwise null.</returns>
    public static IItem Create(IItemType itemType, Location location,
        IDictionary<ItemTypeAttribute, IConvertible> attributes, string script)
    {
        if (string.IsNullOrWhiteSpace(script)) return null;

        script = script.Replace(".cs", string.Empty);

        var type = GameAssemblyCache.Cache
            .FirstOrDefault(x => x.Name.Equals(script) || (x.FullName?.Equals(script) ?? false));

        if (type is null) return null;

        if (!ScriptFactoryMap.TryGetValue(type, out var factory))
        {
            factory = CreateFactory(type);
            ScriptFactoryMap[type] = factory;
        }

        return factory(itemType, location, attributes);
    }

    private static Func<IItemType, Location, IDictionary<ItemTypeAttribute, IConvertible>, IItem>
        CreateFactory(Type type)
    {
        var itemTypeParam = Expression.Parameter(typeof(IItemType), "itemType");
        var locationParam = Expression.Parameter(typeof(Location), "location");
        var attributesParam = Expression.Parameter(typeof(IDictionary<ItemTypeAttribute, IConvertible>), "attributes");

        var constructorInfo = type.GetConstructor(new[]
        {
            typeof(IItemType), typeof(Location), typeof(IDictionary<ItemTypeAttribute, IConvertible>)
        });

        var newInstance = Expression.New(constructorInfo, itemTypeParam, locationParam, attributesParam);

        var castExpression = Expression.Convert(newInstance, type);

        return Expression.Lambda<Func<IItemType, Location, IDictionary<ItemTypeAttribute, IConvertible>, IItem>>(
            castExpression,
            itemTypeParam, locationParam, attributesParam).Compile();
    }
}