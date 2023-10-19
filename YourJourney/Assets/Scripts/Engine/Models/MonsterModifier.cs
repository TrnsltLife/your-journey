using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MonsterModifier
{
    public int id { get; set; }
    public string name { get; set; } = "";
    public int cost { get; set; } = 0;
    int additionalCost { get; set; } = 0;
    public int health { get; set; } = 0;
    public int armor { get; set; } = 0;
    public int sorcery { get; set; } = 0;
    public int damage { get; set; } = 0;
    public int fear { get; set; } = 0;
    public bool immuneCleave { get; set; } = false;
    public bool immuneLethal { get; set; } = false;
    public bool immunePierce { get; set; } = false;
    public bool immuneSmite { get; set; } = false;
    public bool immuneStun { get; set; } = false;
    public bool immuneSunder { get; set; } = false;

    public bool fakeCleave { get; set; } = false;
    public bool fakeLethal { get; set; } = false;
    public bool fakePierce { get; set; } = false;
    public bool fakeSmite { get; set; } = false;
    public bool fakeStun { get; set; } = false;
    public bool fakeSunder { get; set; } = false;
    public List<MonsterType> applicableTo { get; set; } = new List<MonsterType>();

    public MonsterModifier(int id, string name, int cost, int additionalCost, int health, int armor, int sorcery, int damage, int fear,
        bool immuneCleave, bool immuneLethal, bool immunePierce, bool immuneSmite, bool immuneStun, bool immuneSunder,
        bool fakeCleave, bool fakeLethal, bool fakePierce, bool fakeSmite, bool fakeStun, bool fakeSunder)
    {
        this.id = id;
        this.name = name;
        this.cost = cost;
        this.additionalCost = additionalCost;
        this.health = health;
        this.armor = armor;
        this.sorcery = sorcery;
        this.damage = damage;
        this.fear = fear;
        this.fakeCleave = fakeCleave;
        this.fakeLethal = fakeLethal;
        this.fakePierce = fakePierce;
        this.fakeSmite = fakeSmite;
        this.fakeStun = fakeStun;
        this.fakeSunder = fakeSunder;
    }

    public MonsterModifier(int id, string name)
    {
        this.id = id;
        this.name = name;
    }

    public MonsterModifier(int id, string name, int cost, int additionalCost)
    {
        this.id = id;
        this.name = name;
        this.cost = cost;
        this.additionalCost = additionalCost;
    }

    public MonsterModifier AddGoblins()
    {
        applicableTo.AddRange(Monster.Goblins());
        return this;
    }

    public MonsterModifier AddOrcs()
    {
        applicableTo.AddRange(Monster.Orcs());
        return this;
    }

    public MonsterModifier AddHumans()
    {
        applicableTo.AddRange(Monster.Humans());
        return this;
    }

    public MonsterModifier AddSpirits()
    {
        applicableTo.AddRange(Monster.Spirits());
        return this;
    }

    public MonsterModifier AddTrolls()
    {
        applicableTo.AddRange(Monster.Trolls());
        return this;
    }

    public MonsterModifier AddHumanoids()
    {
        applicableTo.AddRange(Monster.Trolls());
        return this;
    }

    public MonsterModifier AddVargs()
    {
        applicableTo.AddRange(Monster.Vargs());
        return this;
    }

    public MonsterModifier AddSpiders()
    {
        applicableTo.AddRange(Monster.Spiders());
        return this;
    }

    public MonsterModifier AddFlying()
    {
        applicableTo.AddRange(Monster.Flying());
        return this;
    }

    public MonsterModifier AddOtherBeasts()
    {
        applicableTo.AddRange(Monster.OtherBeasts());
        return this;
    }
    public MonsterModifier AddAllBeasts()
    {
        applicableTo.AddRange(Monster.AllBeasts());
        return this;
    }

    public MonsterModifier Add(MonsterType monsterType)
    {
        applicableTo.Add(monsterType);
        return this;
    }


    public bool IsApplicableTo(MonsterType monsterType)
    {
        if (applicableTo == null || applicableTo.Count == 0) { return true; }
        return applicableTo.Contains(monsterType);
    }

    public int CalculateCost(int monsterCount)
    {
        //Inititial cost + additionalCost for each additional monster
        return cost + ((monsterCount - 1) * additionalCost);
    }

    public bool IsAvailableFor(MonsterType monsterType, int monsterCount, int pointsAvailable)
    {
        if(IsApplicableTo(monsterType))
        {
            return CalculateCost(monsterCount) <= pointsAvailable;
        }
        return false;
    }

    public static List<MonsterModifier> ListAvailableModifiersFor(MonsterType monsterType, int monsterCount, int pointsAvailable)
    {
        List<MonsterModifier> list = new List<MonsterModifier>();
        foreach(var mod in MonsterModifier.BasicValues)
        {
            if(mod.IsAvailableFor(monsterType, monsterCount, pointsAvailable))
            {
                list.Add(mod);
            }
        }
        return list;
    }

    private static List<object> monsterModifiers = new List<object>();
    private MonsterModifier AddToList()
    {
        monsterModifiers.Add(this);
        return this;
    }
    public static MonsterModifier FromID(int id)
    {
        if(id < monsterModifiers.Count)
        {
            return (MonsterModifier)monsterModifiers[id];
        }
        return null;
    }

    public static readonly MonsterModifier NONE = new MonsterModifier(0, "None", 0, 0) { }.AddToList();
    public static readonly MonsterModifier ALERT = new MonsterModifier(1, "Alert", 10, 5) { immuneStun = true }.AddToList();
    public static readonly MonsterModifier ARMORED = new MonsterModifier(2, "Armored", 4, 3) { armor = 1 }.AddToList();
    public static readonly MonsterModifier BLOODTHIRSTY = new MonsterModifier(3, "Bloodthirsty", 3, 1) { damage = 1 }.AddToList();
    public static readonly MonsterModifier GUARDED = new MonsterModifier(4, "Guarded", 9, 9) { immuneCleave = true, immuneStun = true }.AddToList();
    public static readonly MonsterModifier HARDENED = new MonsterModifier(5, "Hardened", 6, 6) { immunePierce = true, immuneSunder = true }.AddToList();
    public static readonly MonsterModifier HUGE = new MonsterModifier(6, "Huge", 4, 3) { health = 3 }.AddToList();
    public static readonly MonsterModifier LARGE = new MonsterModifier(7, "Large", 3, 2) { health = 2 }.AddToList();
    public static readonly MonsterModifier SHROUDED = new MonsterModifier(8, "Shrouded", 7, 5) { sorcery = 1 }.AddToList();
    public static readonly MonsterModifier SPIKE_ARMOR = new MonsterModifier(9, "Spike Armor", 6, 4) { armor = 1, damage = 1 }.AddHumanoids().AddTrolls().AddSpirits().Add(MonsterType.WarElephant).AddToList();
    public static readonly MonsterModifier TERRIFYING = new MonsterModifier(10, "Terrifying", 3, 1) { fear = 1 }.AddToList();
    public static readonly MonsterModifier VETERAN = new MonsterModifier(11, "Veteran", 10, 7) { health = 5, damage = 1 }.AddHumanoids().AddToList();
    public static readonly MonsterModifier WARY = new MonsterModifier(12, "Wary", 8, 8) { immuneStun = true, immuneLethal = true }.AddToList();
    public static readonly MonsterModifier WELL_EQUIPPED = new MonsterModifier(13, "Well-Equipped", 6, 5) { health = 2, armor = 1 }.AddHumanoids().AddTrolls().AddSpirits().Add(MonsterType.WarElephant).Add(MonsterType.SiegeEngine).AddToList();

    public static readonly MonsterModifier BLIND_RAGE = new MonsterModifier(14, "Blind Rage") { armor = 2, immunePierce = true, immuneSunder = true }.AddToList();
    public static readonly MonsterModifier BURNING_HATRED = new MonsterModifier(15, "Burning Hatred") { armor = 1, health = 4 }.AddToList();
    public static readonly MonsterModifier ELDEST_WORM_1 = new MonsterModifier(16, "Eldest Worm") { health = -4, armor = 1, immunePierce = true, immuneSunder = true }.Add(MonsterType.FoulBeast).Add(MonsterType.AnonymousThing).Add(MonsterType.LichKing).AddToList();
    public static readonly MonsterModifier ELDEST_WORM_2 = new MonsterModifier(17, "Eldest Worm") { armor = 1, immunePierce = true, immuneSunder = true }.Add(MonsterType.FoulBeast).Add(MonsterType.AnonymousThing).Add(MonsterType.LichKing).AddToList();
    public static readonly MonsterModifier ELDEST_WORM_3 = new MonsterModifier(18, "Eldest Worm") { health = 4, armor = 1, immunePierce = true, immuneSunder = true }.Add(MonsterType.FoulBeast).Add(MonsterType.AnonymousThing).Add(MonsterType.LichKing).AddToList();
    public static readonly MonsterModifier ETERNAL_FLAME = new MonsterModifier(19, "Eternal Flame") { sorcery = 1, health = 15 }.AddSpirits().Add(MonsterType.Balerock).Add(MonsterType.FoulBeast).AddToList();
    public static readonly MonsterModifier ETERNAL_LORD = new MonsterModifier(20, "Eternal Lord") { armor = 2, health = 2, fear = 1, damage = 1 }.AddSpirits().Add(MonsterType.Balerock).AddToList();
    public static readonly MonsterModifier HATCHLING = new MonsterModifier(21, "Hatchling") { health = -5 }.Add(MonsterType.GiantSpider).Add(MonsterType.FoulBeast).Add(MonsterType.AnonymousThing).AddToList();
    public static readonly MonsterModifier KINGS_ARMOR = new MonsterModifier(22, "King's Armor") { armor = 2, health = 5 }.AddHumanoids().AddSpirits().AddToList();
    public static readonly MonsterModifier LAST_GASP = new MonsterModifier(23, "Last Gasp") { armor = -2, damage = 1, fear = 1 }.AddHumanoids().AddTrolls().AddAllBeasts().AddToList();
    public static readonly MonsterModifier NUMENOREAN_BLOOD = new MonsterModifier(24, "Númenórean Blood") { health = 4 }.AddHumans().AddToList();
    public static readonly MonsterModifier ORC_CAPTAIN = new MonsterModifier(25, "Orc Captain") { armor = 1, health = 2, fear = 1, immuneStun = true }.AddOrcs().AddToList();
    public static readonly MonsterModifier ORC_CHAMPION = new MonsterModifier(26, "Orc Champion") { damage = 1, fear = 1 }.AddOrcs().AddToList();
    public static readonly MonsterModifier PACKS_VENGEANCE_1 = new MonsterModifier(27, "Pack's Vengeance") { health = 2, immuneStun = true, immuneLethal = true }.AddVargs().AddToList();
    public static readonly MonsterModifier PACKS_VENGEANCE_2 = new MonsterModifier(28, "Pack's Vengeance") { health = 2, immuneStun = true , fakeLethal=true }.AddVargs().AddToList();
    public static readonly MonsterModifier POSSESSED = new MonsterModifier(29, "Possessed") { fear = 1, immuneSmite = true }.AddHumanoids().AddAllBeasts().AddToList();
    public static readonly MonsterModifier SHADOWMAN_CAPTAIN = new MonsterModifier(30, "Shadowman Captain") { health = 4, sorcery = 1, immuneStun = true }.Add(MonsterType.Shadowman).AddToList();
    public static readonly MonsterModifier SPECTRAL = new MonsterModifier(31, "Spectral") { sorcery = 2, immuneLethal = true }.AddSpirits().AddToList();
    public static readonly MonsterModifier SPIDER_CAPTAIN = new MonsterModifier(32, "Spider Captain") { health = 7, damage = 1, immuneStun = true }.AddSpiders().AddToList();
    public static readonly MonsterModifier SPIRIT_GUARD = new MonsterModifier(33, "Spirit Guard") { fear = 2, damage = 2, sorcery = 2 }.AddSpirits().AddToList();
    public static readonly MonsterModifier SWIFT_DEATH = new MonsterModifier(34, "Swift Death") { damage = 1 }.AddToList();
    public static readonly MonsterModifier TRAINED_FOR_BATTLE = new MonsterModifier(35, "Trained for Battle") { armor = 2, health = 5 }.AddHumanoids().AddTrolls().AddToList();
    public static readonly MonsterModifier UNDYING_HATE = new MonsterModifier(36, "Undying Hate") { health = -12, fear = 1, damage = 1, immuneStun = true }.AddToList();
    public static readonly MonsterModifier WARBAND_LEADER = new MonsterModifier(37, "Warband Leader") { armor = 2, health = 2, damage = 2 }.AddHumanoids().AddToList();
    public static readonly MonsterModifier WISP = new MonsterModifier(38, "Wisp") { health = -1, sorcery = -1 }.AddSpirits().AddToList();

    public static readonly MonsterModifier DWARVEN_BANE_1 = new MonsterModifier(39, "Dwarven Bane") { armor = -2, fear = -1 }.Add(MonsterType.Balerock).AddToList();
    public static readonly MonsterModifier DWARVEN_BANE_2 = new MonsterModifier(40, "Dwarven Bane") { armor = 1, immuneLethal = true, immuneStun = true }.Add(MonsterType.Balerock).AddToList();
    public static readonly MonsterModifier DWARVEN_BANE_3 = new MonsterModifier(41, "Dwarven Bane") { armor = 1, sorcery = 1, health = 4, fear = 1, immuneLethal = true, immuneSunder = true }.Add(MonsterType.Balerock).AddToList();
    public static readonly MonsterModifier DWARVEN_BANE_4 = new MonsterModifier(42, "Dwarven Bane") { armor = 2, sorcery = 2, health = 5, fear = 2, damage = 2, immuneLethal = true, immuneSunder = true, immuneStun = true }.Add(MonsterType.Balerock).AddToList();
    public static readonly MonsterModifier MASTER_OF_THE_PIT_1 = new MonsterModifier(43, "Master of the Pit") {sorcery=1, immuneStun= true }.AddToList();
    public static readonly MonsterModifier MASTER_OF_THE_PIT_2 = new MonsterModifier(44, "Master of the Pit") {sorcery=2, immuneStun= true }.AddToList();
    public static readonly MonsterModifier MASTER_OF_THE_PIT_3 = new MonsterModifier(45, "Master of the Pit") {sorcery = 3, immuneStun = true }.AddToList();
    public static readonly MonsterModifier HERALD_OF_THE_BALEROCK = new MonsterModifier(46, "Herald of the Balerock") { health = 2, sorcery = 3, immuneSmite = true, immuneStun = true }.AddToList();
    public static readonly MonsterModifier URSULAS_VENGEANCE = new MonsterModifier(47, "Ursula's Vengeance") { health = 4, armor = 1, sorcery = 2, immuneLethal = true, immuneStun = true }.Add(MonsterType.Oliver).AddToList();
    public static readonly MonsterModifier LICH_KING = new MonsterModifier(48, "Lich-king") { immuneStun = true }.Add(MonsterType.LichKing).AddToList();
    public static readonly MonsterModifier CHIEF_OF_THE_NINE = new MonsterModifier(49, "Chief of the Nine") { armor = 1, immuneStun = true }.Add(MonsterType.LichKing).AddToList();
    public static readonly MonsterModifier LORD_OF_MINE_HOUSE_MORE_GHOUL = new MonsterModifier(50, "Lord of Mine-House More-Ghoul") { health = 5, damage = 1, fear = 1, immuneLethal = true, immuneSunder = true, immuneStun = true }.Add(MonsterType.LichKing).AddToList();


    /*
    Golden Mask	+1 armor, +2 health, +2 sorcery, +1 fear
    Favored Beast(Worm Witch Steed)    +1 armor, +4 health, Stun immune
    Mother of Rot(Worm Witch)  +1 armor, +1 fear, +1 sorcery, Stun immune
    */

    public static IEnumerable<MonsterModifier> BasicValues
    {
        get
        {
            yield return ALERT;
            yield return ARMORED;
            yield return BLOODTHIRSTY;
            yield return GUARDED;
            yield return HARDENED;
            yield return HUGE;
            yield return LARGE;
            yield return SHROUDED;
            yield return SPIKE_ARMOR;
            yield return TERRIFYING;
            yield return VETERAN;
            yield return WARY;
            yield return WELL_EQUIPPED;
        }
    }

    public static IEnumerable<MonsterModifier> ExtendedValues
    {
        get
        {
            yield return BURNING_HATRED;
            yield return BLIND_RAGE;
            yield return ELDEST_WORM_1;
            yield return ELDEST_WORM_2;
            yield return ELDEST_WORM_3;
            yield return ETERNAL_FLAME;
            yield return ETERNAL_LORD;
            yield return HATCHLING;
            yield return KINGS_ARMOR;
            yield return LAST_GASP;
            yield return NUMENOREAN_BLOOD;
            yield return ORC_CAPTAIN;
            yield return ORC_CHAMPION;
            yield return PACKS_VENGEANCE_1;
            yield return PACKS_VENGEANCE_2;
            yield return POSSESSED;
            yield return SHADOWMAN_CAPTAIN;
            yield return SPECTRAL;
            yield return SPIDER_CAPTAIN;
            yield return SPIRIT_GUARD;
            yield return SWIFT_DEATH;
            yield return TRAINED_FOR_BATTLE;
            yield return UNDYING_HATE;
            yield return WARBAND_LEADER;
            yield return WISP;
        }
    }

    public static IEnumerable<MonsterModifier> NamedValues
    {
        get
        {
            yield return DWARVEN_BANE_1;
            yield return DWARVEN_BANE_2;
            yield return DWARVEN_BANE_3;
            yield return DWARVEN_BANE_4;
            yield return MASTER_OF_THE_PIT_1;
            yield return MASTER_OF_THE_PIT_2;
            yield return MASTER_OF_THE_PIT_3;
            yield return HERALD_OF_THE_BALEROCK;
            yield return URSULAS_VENGEANCE;
            yield return LICH_KING;
            yield return CHIEF_OF_THE_NINE;
            yield return LORD_OF_MINE_HOUSE_MORE_GHOUL;
        }
    }

    public static IEnumerable<MonsterModifier> Values
    {
        get
        {
            yield return NONE;

            yield return ALERT;
            yield return ARMORED;
            yield return BLOODTHIRSTY;
            yield return GUARDED;
            yield return HARDENED;
            yield return HUGE;
            yield return LARGE;
            yield return SHROUDED;
            yield return SPIKE_ARMOR;
            yield return TERRIFYING;
            yield return VETERAN;
            yield return WARY;
            yield return WELL_EQUIPPED;

            yield return BURNING_HATRED;
            yield return BLIND_RAGE;
            yield return ELDEST_WORM_1;
            yield return ELDEST_WORM_2;
            yield return ELDEST_WORM_3;
            yield return ETERNAL_FLAME;
            yield return ETERNAL_LORD;
            yield return HATCHLING;
            yield return KINGS_ARMOR;
            yield return LAST_GASP;
            yield return NUMENOREAN_BLOOD;
            yield return ORC_CAPTAIN;
            yield return ORC_CHAMPION;
            yield return PACKS_VENGEANCE_1;
            yield return PACKS_VENGEANCE_2;
            yield return POSSESSED;
            yield return SHADOWMAN_CAPTAIN;
            yield return SPECTRAL;
            yield return SPIDER_CAPTAIN;
            yield return SPIRIT_GUARD;
            yield return SWIFT_DEATH;
            yield return TRAINED_FOR_BATTLE;
            yield return UNDYING_HATE;
            yield return WARBAND_LEADER;
            yield return WISP;

            yield return DWARVEN_BANE_1;
            yield return DWARVEN_BANE_2;
            yield return DWARVEN_BANE_3;
            yield return DWARVEN_BANE_4;
            yield return MASTER_OF_THE_PIT_1;
            yield return MASTER_OF_THE_PIT_2;
            yield return MASTER_OF_THE_PIT_3;
            yield return HERALD_OF_THE_BALEROCK;
            yield return URSULAS_VENGEANCE;
            yield return LICH_KING;
            yield return CHIEF_OF_THE_NINE;
            yield return LORD_OF_MINE_HOUSE_MORE_GHOUL;
        }
    }
}
