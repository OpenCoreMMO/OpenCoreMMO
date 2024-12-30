using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Player;

namespace NeoServer.Extensions.Players;

public class Tutor : Player
{
    public Tutor(uint id, string characterName, IVocation vocation, IGroup group, Gender gender, bool online,
        IDictionary<SkillType, ISkill> skills, IDictionary<int, int> storages, IOutfit outfit, ushort speed, Location location,
        IMapTool mapTool, ITown town) :
        base(id, characterName, ChaseMode.Follow, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, vocation, group,
            gender, online, ushort.MaxValue, ushort.MaxValue, FightMode.Balanced, 100, 100, skills, storages, 60, outfit,
            speed, location, mapTool, town)
    {
    }

    public override bool CanBeAttacked => false;

    public override void GainExperience(long exp)
    {
    } //tutor do not gain experience

    public override void LoseExperience(long exp)
    {
        Console.WriteLine("tutor do not lose experience");
    }

    public override void OnDamage(IThing enemy, CombatDamage damage)
    {
    }

    public override void OnAppear(Location location, ICylinderSpectator[] spectators)
    {
    }
}