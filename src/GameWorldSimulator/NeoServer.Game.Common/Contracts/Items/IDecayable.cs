namespace NeoServer.Game.Common.Contracts.Items;

public delegate void PauseDecay(IDecayable item);

public delegate void StartDecay(IItem item);

public interface IDecayable : IDecay
{
    ushort DecaysTo { get; }
    uint Duration { get; }
    bool ShouldDisappear { get; }
    bool Expired { get; }
    uint Elapsed { get; }
    uint Remaining { get; }
    bool IsPaused { get; }
    bool TryDecay();
    event PauseDecay OnPaused;
    static event StartDecay OnStarted;
}

public interface IDecay
{
    void StartDecay();
    void PauseDecay();
}

public interface IHasDecay
{
    public IDecayable Decay { get; }
}