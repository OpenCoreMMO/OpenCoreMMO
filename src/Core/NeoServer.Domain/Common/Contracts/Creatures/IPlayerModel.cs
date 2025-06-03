//using NeoServer.Game.Contracts.Items;
//using NeoServer.Game.Common.Creatures;
//using NeoServer.Game.Common.Location.Structs;
//using NeoServer.Game.Common.Players;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace NeoServer.Game.Contracts.Creatures
//{
//    public interface IPlayerModel
//    {
//        int Id { get; }
//        int AccountId { get; }
//        string CharacterName { get; set; }

//        uint Capacity { get; set; }
//        ushort Level { get; set; }
//        ushort Mana { get; set; }
//        ushort MaxMana { get; set; }
//        ushort HealthPoints { get; set; }
//        ushort MaxHealthPoints { get; set; }
//        byte MaxSoulPoints { get; set; }
//        bool Online { get; set; }
//        byte SoulPoints { get; set; }
//        ushort Speed { get; set; }
//        ushort StaminaMinutes { get; set; }
//        byte AmmoAmount { get; set; }

//        ChaseMode ChaseMode { get; set; }
//        FightMode FightMode { get; }
//        Gender Gender { get; set; }
//        VocationType Vocation { get; set; }
//        Location Location { get; set; }

//        Dictionary<Slot, ushort> Inventory { get; set; }

//        //IEnumerable<IItemModel> Items { get; set; }
//        //IOutfit Outfit { get; set; }
//        //IDictionary<SkillType, ISkillModel> Skills { get; set; }

//        bool IsMounted();
//    }
//}

