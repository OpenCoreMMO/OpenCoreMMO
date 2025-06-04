using NeoServer.Domain.Items.Items.Attributes;

namespace NeoServer.Domain.Common.Contracts.Items;

public delegate void PauseDecay(Decayable item);

public delegate void StartDecay(IItem item);

public interface IDecay
{
    void StartDecay();
    void PauseDecay();
}

public interface IHasDecay
{
    public Decayable Decay { get; }
}