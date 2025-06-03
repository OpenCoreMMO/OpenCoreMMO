using System;
using NeoServer.Game.Common;

namespace NeoServer.Game.World;

/// <summary>
/// Represents the world light system in Tibia, managing the dynamic day-night cycle
/// and adjusting light levels accordingly.
/// reference: https://tibia.fandom.com/wiki/World_light
/// </summary>
public class WorldLight
{
    private const int MINUTES_IN_DAY = 1440;
    private const int UPDATE_INTERVAL_SECONDS = 10;
    private const int SECONDS_IN_GAME_DAY = 3600;
    public const ushort EVENT_WORLD_LIGHT_INTERVAL = 10000;

    private const byte LIGHT_LEVEL_DAY = 250;
    private const byte LIGHT_LEVEL_NIGHT = 40;
    private const int SUNSET_MINUTE = 1305;  // 21:45 real time
    private const int SUNRISE_MINUTE = 430;  // 07:15 real time

    private byte _lightLevel = LIGHT_LEVEL_DAY;
    private int _currentMinute = SUNRISE_MINUTE + (SUNSET_MINUTE - SUNRISE_MINUTE) / 2;
    private Period _currentPeriod = Period.Day;
    public byte LightLevel => _lightLevel;

    // Represents how many in-game minutes pass every update tick
    private readonly int _minuteIncrement = MINUTES_IN_DAY * UPDATE_INTERVAL_SECONDS / SECONDS_IN_GAME_DAY;

    /// <summary>
    /// Updates the in-game time, period state (e.g., sunrise or sunset), and light level.
    /// Publishes a WorldLightChangedEvent after each update.
    /// </summary>
    public void UpdateWorldLightState()
    {
        AdvanceTime();
        UpdatePeriod();
        UpdateLightLevel();
        PublishLightChange();
    }

    /// <summary>
    /// Advances the current in-game minute by the configured interval.
    /// Wraps around after 1440 (24h).
    /// </summary>
    private void AdvanceTime()
    {
        _currentMinute += _minuteIncrement;

        if (_currentMinute >= MINUTES_IN_DAY)
            _currentMinute -= MINUTES_IN_DAY;
    }

    /// <summary>
    /// Determines the current light period based on proximity to sunrise or sunset.
    /// </summary>
    private void UpdatePeriod()
    {
        if (IsNear(_currentMinute, SUNRISE_MINUTE))
            _currentPeriod = Period.Sunrise;
        if (IsNear(_currentMinute, SUNSET_MINUTE))
            _currentPeriod = Period.Sunset;
    }

    /// <summary>
    /// Gradually increases or decreases the light level based on the current period.
    /// Handles transitions between day, night, sunrise, and sunset.
    /// </summary>
    private void UpdateLightLevel()
    {
        var transitionStep = (LIGHT_LEVEL_DAY - LIGHT_LEVEL_NIGHT) / 30;

        switch (_currentPeriod)
        {
            case Period.Sunrise:
                _lightLevel = (byte) Math.Min(_lightLevel + transitionStep, LIGHT_LEVEL_DAY);
                if (_lightLevel == LIGHT_LEVEL_DAY)
                    _currentPeriod = Period.Day;
                break;

            case Period.Sunset:
                _lightLevel = (byte) Math.Max(_lightLevel - transitionStep, LIGHT_LEVEL_NIGHT);
                if (_lightLevel == LIGHT_LEVEL_NIGHT)
                    _currentPeriod = Period.Night;
                break;
        }
    }

    /// <summary>
    /// Publishes a WorldLightChangedEvent to notify other systems of the new light level.
    /// </summary>
    private void PublishLightChange()
    {
        EventAggregator.Instance.Publish(new WorldLightChangedEvent(_lightLevel));
    }

    /// <summary>
    /// Determines if the given value is within range of the target value, using 2 * increment as tolerance.
    /// </summary>
    /// <param name="value">The current minute value.</param>
    /// <param name="target">The target minute (sunrise or sunset).</param>
    /// <returns>True if near the target minute, otherwise false.</returns>
    private bool IsNear(int value, int target)
    {
        return Math.Abs(value - target) < 2 * _minuteIncrement;
    }

    public void SetWorldLight(Period period)
    {
        var timeChanged = false;

        switch (period)
        {
            case Period.Day:
                _currentMinute = SUNRISE_MINUTE + (SUNSET_MINUTE - SUNRISE_MINUTE) / 2;
                timeChanged = true;
                _lightLevel = LIGHT_LEVEL_DAY;
                break;
            case Period.Night:
                _currentMinute = (SUNSET_MINUTE + MINUTES_IN_DAY + SUNRISE_MINUTE) / 2 % MINUTES_IN_DAY;
                timeChanged = true;
                _lightLevel = LIGHT_LEVEL_NIGHT;
                break;
        }

        if (!timeChanged) return;
        
        _currentMinute = (_currentMinute % MINUTES_IN_DAY + MINUTES_IN_DAY) % MINUTES_IN_DAY;

        UpdatePeriod();
        UpdateLightLevel();
        PublishLightChange();
    }
}

/// <summary>
/// Defines the possible light periods in the game world.
/// </summary>
public enum Period : byte
{
    Day,
    Night,
    Sunset,
    Sunrise,
}

/// <summary>
/// Event triggered when the world light level changes.
/// </summary>
/// <param name="Level">The current light level (0–255).</param>
public record WorldLightChangedEvent(byte Level) : IEvent;
