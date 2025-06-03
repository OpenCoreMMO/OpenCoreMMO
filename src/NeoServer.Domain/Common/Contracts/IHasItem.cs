using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts;

/// <summary>
///     A contract to represent anything that can store things
/// </summary>
public interface IHasItem
{
    /// <summary>
    ///     Checks if thing can be added to destination
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amount"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    Result CanAddItem(IItem item, byte amount = 1, byte? slot = null);

    /// <summary>
    ///     Gives amount that can be added to store
    /// </summary>
    /// <param name="thing"></param>
    /// <param name="toPosition"></param>
    /// <returns></returns>
    uint PossibleAmountToAdd(IItem thing, byte? toPosition = null);

    /// <summary>
    ///     Stores thing
    /// </summary>
    /// <param name="item">thing to be stored</param>
    /// <param name="position">position where thing will be stored</param>
    /// <returns></returns>
    Result<OperationResultList<IItem>> AddItem(IItem item, byte? position = null);

    /// <summary>
    ///     Removes thing from store
    /// </summary>
    /// <param name="thing">thing to be removed</param>
    /// <param name="amount">amount of the thing to be removed</param>
    /// <param name="fromPosition">position where thing will be removed</param>
    /// <param name="removedThing">removed thing instance from store</param>
    /// <returns></returns>
    Result<OperationResultList<IItem>> RemoveItem(IItem thing, byte amount, byte fromPosition, out IItem removedThing);

    /// <summary>
    ///     Checks if item cam be added to any containers within current container
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns>Returns true when any amount is possible to add</returns>
    Result<uint> CanAddItem(IItemType itemType);
}