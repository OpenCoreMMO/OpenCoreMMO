using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Items.Items.Attributes;

public delegate void PauseDecay(Decayable item);

public delegate void StartDecay(IItem item);

public class Decayable : IDecay
{
    private readonly IItem _item;

    private uint _duration;

    private uint _lastElapsed;
    private ulong _startedToDecayTime;

    public Decayable(IItem item)
    {
        _lastElapsed = item.Metadata.Attributes.GetAttribute<uint>(ItemTypeAttribute.DecayElapsed);
        _item = item;
    }

    private bool ShowDuration =>
        !_item.Metadata.Attributes.TryGetAttribute<byte>(ItemTypeAttribute.ShowDuration, out var showDuration) ||
        showDuration == 1;

    public bool StartedToDecay => _startedToDecayTime != default;
    public bool IsPaused { get; private set; } = true;
    public ushort DecaysTo => _item.Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.ExpireTarget);

    public uint Duration => _duration = _item.Metadata.Attributes.GetAttribute<uint>(ItemTypeAttribute.Duration) == 0
        ? _duration
        : _item.Metadata.Attributes.GetAttribute<uint>(ItemTypeAttribute.Duration);

    public uint Remaining => Duration <= Elapsed ? 0 : Math.Max(0, Duration - Elapsed);

    public uint Elapsed
    {
        get
        {
            if (IsPaused) return _lastElapsed;
            var elapsedSeconds = _startedToDecayTime == 0
                ? 0
                : (uint)Math.Ceiling(((ulong)DateTime.Now.Ticks - _startedToDecayTime) /
                                     (decimal)TimeSpan.TicksPerSecond);

            return _lastElapsed + elapsedSeconds;
        }
    }

    public bool Expired => Elapsed >= Duration;
    public bool ShouldDisappear => DecaysTo == default;

    public void StartDecay()
    {
        if (Expired) return;
        IsPaused = false;
        _startedToDecayTime = (ulong)DateTime.Now.Ticks;

        OnStarted?.Invoke(_item);
    }

    public void PauseDecay()
    {
        if (_startedToDecayTime == 0) return;
        IsPaused = true;
        _lastElapsed += (uint)(((ulong)DateTime.Now.Ticks - _startedToDecayTime) / TimeSpan.TicksPerSecond);
        OnPaused?.Invoke(this);
    }

    public event PauseDecay OnPaused;

    public bool TryDecay()
    {
        if (!Expired) return false;

        Reset();

        return true;
    }

    public static event StartDecay OnStarted;

    private void Reset()
    {
        _startedToDecayTime = default;
        IsPaused = default;
        _lastElapsed = default;
        _duration = default;
    }

    public override string ToString()
    {
        if (!ShowDuration) return string.Empty;
        if (Elapsed == 0) return "is brand-new";

        var minutes = Math.Max(0, Remaining / 60);
        var seconds = Math.Max(0, (int)Math.Truncate(Remaining % 60d));
        return
            $"will expire in {minutes} minute{(minutes > 1 ? "s" : "")} and {seconds} second{(seconds > 1 ? "s" : "")}";
    }
}