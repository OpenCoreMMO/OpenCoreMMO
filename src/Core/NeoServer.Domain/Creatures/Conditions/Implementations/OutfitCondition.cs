#nullable enable
using System;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class OutfitCondition : BaseCondition
{
    public override ConditionType Type => ConditionType.Outfit;
    public ushort LookType { get; }
    public byte Head { get; }
    public byte Body { get; }
    public byte Legs { get; }
    public byte Feet { get; }
    public byte Addon { get; }

    public OutfitCondition(uint duration, ushort lookType, byte head, byte body, byte legs, byte feet, byte addon)
        : base(duration)
    {
        LookType = lookType;
        Head = head;
        Body = body;
        Legs = legs;
        Feet = feet;
        Addon = addon;
    }

    internal override bool Start(ICreature creature)
    {
        if (!base.Start(creature)) return false;
        
        creature.SetTemporaryOutfit(LookType, Head, Body, Legs, Feet, Addon);
        EndAction = creature.BackToOldOutfit;

        return true;
    }

    public OutfitConditionState CaptureState()
    {
        return new OutfitConditionState(
            Type,
            LookType,
            Head,
            Body,
            Legs,
            Feet,
            Addon,
            Math.Max(0, RemainingTime));
    }

    public static OutfitCondition? Restore(OutfitConditionState state)
    {
        var durationMs = Math.Max(0, state.RemainingTimeMilliseconds);

        if (durationMs <= 0)
            return null;

        return new OutfitCondition(
            (uint)durationMs,
            state.LookType,
            state.Head,
            state.Body,
            state.Legs,
            state.Feet,
            state.Addon);
    }
}

public sealed record OutfitConditionState(
    ConditionType Type,
    ushort LookType,
    byte Head,
    byte Body,
    byte Legs,
    byte Feet,
    byte Addon,
    long RemainingTimeMilliseconds);
