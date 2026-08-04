using NeoServer.Domain.Items.Items.Attributes;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface IDecay
{
    void StartDecay();
    void PauseDecay();
}

public interface IHasDecay
{
    public DecayTracker Decay { get; }
}