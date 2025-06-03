using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;

namespace NeoServer.Domain.Items.Items.Containers.Container;

public class ContainerWeight
{
    public ContainerWeight(IContainer container)
    {
        Weight = container.Metadata.Weight;
        SubscribeToEvents(container);
    }

    public float Weight { get; private set; }

    private void ChangeWeight(float weight)
    {
        var oldWeight = Weight;
        Weight = weight;
        OnWeightChanged?.Invoke(weight - oldWeight);
    }

    internal void UpdateWeight(IContainer onContainer, float weightChange)
    {
        ChangeWeight(Weight + weightChange);
        UpdateParent(onContainer, weightChange);
    }

    private void UpdateWeight(IContainer onContainer, byte slot, IItem item, sbyte differenceAmount)
    {
        var weight = differenceAmount * item.Metadata.Weight;
        ChangeWeight(Weight + weight);
        UpdateParent(onContainer, weight);
    }

    private void IncreaseWeight(IItem item, IContainer container)
    {
        var weight = item.Weight;

        ChangeWeight(Weight + weight);
        UpdateParent(container, weight);
    }

    private void UpdateParent(IContainer initialContainer, float weight)
    {
        var parent = initialContainer.Parent;
        if (parent is not Container container) return;

        container.OnChildWeightUpdated(weight);
    }

    private void DecreaseWeight(IContainer fromContainer, byte slot, IItem item, byte amountRemoved)
    {
        var weight = item.Weight > 0
            ? item.Weight
            : item.Metadata.Weight * amountRemoved;

        ChangeWeight(Weight - weight);
        UpdateParent(fromContainer, -weight);
    }

    private void SubscribeToEvents(IContainer container)
    {
        container.OnItemAdded += IncreaseWeight;
        container.OnItemRemoved += DecreaseWeight;
        container.OnItemUpdated += UpdateWeight;
    }

    internal void SubscribeToWeightChangeEvent(WeightChange weightChange)
    {
        OnWeightChanged += weightChange;
    }

    internal void UnsubscribeFromWeightChangeEvent(WeightChange weightChange)
    {
        OnWeightChanged -= weightChange;
    }

    internal event WeightChange OnWeightChanged;
}