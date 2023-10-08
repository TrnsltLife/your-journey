using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MonsterModifier
{
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
    public List<MonsterType> applicableTo { get; set; } = new List<MonsterType>();

    public MonsterModifier(string name, int cost, int additionalCost, int health, int armor, int sorcery, int damage, int fear,
        bool immuneCleave, bool immuneLethal, bool immunePierce, bool immuneSmite, bool immuneStun, bool immuneSunder)
    {
        this.name = name;
        this.cost = cost;
        this.additionalCost = additionalCost;
        this.health = health;
        this.armor = armor;
        this.sorcery = sorcery;
        this.damage = damage;
        this.fear = fear;
        this.immuneCleave = immuneCleave;
        this.immuneLethal = immuneLethal;
        this.immunePierce = immunePierce;
        this.immuneSmite = immuneSmite;
        this.immuneStun = immuneStun;
        this.immuneSunder = immuneSunder;
    }

    public MonsterModifier(string name)
    {
        this.name = name;
    }

    public MonsterModifier(string name, int cost, int additionalCost)
    {
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

    public static readonly MonsterModifier ALERT = new MonsterModifier("Alert", 10, 5) { immuneStun = true };
    public static readonly MonsterModifier ARMORED = new MonsterModifier("Armored", 4, 3) { armor = 1 };
    public static readonly MonsterModifier BLOODTHIRSTY = new MonsterModifier("Bloodthirsty", 3, 1) { damage = 1 };
    public static readonly MonsterModifier GUARDED = new MonsterModifier("Guarded", 9, 9) { immuneCleave = true, immuneStun = true };
    public static readonly MonsterModifier HARDENED = new MonsterModifier("Hardened", 6, 6) { immunePierce = true, immuneSunder = true };
    public static readonly MonsterModifier HUGE = new MonsterModifier("Huge", 4, 3) { health = 3 };
    public static readonly MonsterModifier LARGE = new MonsterModifier("Large", 3, 2) { health = 2 };
    public static readonly MonsterModifier SHROUDED = new MonsterModifier("Shrouded", 7, 5) { sorcery = 1 };
    public static readonly MonsterModifier SPIKE_ARMOR = new MonsterModifier("Spike Armor", 6, 4) { armor = 1, damage = 1 }.AddHumanoids().AddTrolls().AddSpirits().Add(MonsterType.WarElephant);
    public static readonly MonsterModifier TERRIFYING = new MonsterModifier("Terrifying", 3, 1) { fear = 1 };
    public static readonly MonsterModifier VETERAN = new MonsterModifier("Veteran", 10, 7) { health = 5, damage = 1 }.AddHumanoids();
    public static readonly MonsterModifier WARY = new MonsterModifier("Wary", 8, 8) { immuneStun = true, immuneLethal = true };
    public static readonly MonsterModifier WELL_EQUIPPED = new MonsterModifier("Well-Equipped", 6, 5) { health = 2, armor = 1 }.AddHumanoids().AddTrolls().AddSpirits().Add(MonsterType.WarElephant).Add(MonsterType.SiegeEngine);

    public static readonly MonsterModifier BLIND_RAGE = new MonsterModifier("Blind Rage") { armor = 2, immunePierce = true, immuneSunder = true };
    public static readonly MonsterModifier BURNING_HATRED = new MonsterModifier("Burning Hatred") { armor = 1, health = 4 };
    public static readonly MonsterModifier ELDEST_WORM_1 = new MonsterModifier("Eldest Worm") { health = -4, armor = 1, immunePierce = true, immuneSunder = true }.Add(MonsterType.FoulBeast).Add(MonsterType.AnonymousThing).Add(MonsterType.LichKing);
    public static readonly MonsterModifier ELDEST_WORM_2 = new MonsterModifier("Eldest Worm") { armor = 1, immunePierce = true, immuneSunder = true }.Add(MonsterType.FoulBeast).Add(MonsterType.AnonymousThing).Add(MonsterType.LichKing);
    public static readonly MonsterModifier ELDEST_WORM_3 = new MonsterModifier("Eldest Worm") { health = 4, armor = 1, immunePierce = true, immuneSunder = true }.Add(MonsterType.FoulBeast).Add(MonsterType.AnonymousThing).Add(MonsterType.LichKing);
    public static readonly MonsterModifier ETERNAL_FLAME = new MonsterModifier("Eternal Flame") { sorcery = 1, health = 15 };
    public static readonly MonsterModifier ETERNAL_LORD = new MonsterModifier("Eternal Lord") { armor = 2, health = 2, fear = 1, damage = 1 };
    public static readonly MonsterModifier HATCHLING = new MonsterModifier("Hatchling") { health = -5 }.Add(MonsterType.GiantSpider).Add(MonsterType.FoulBeast).Add(MonsterType.AnonymousThing);
    public static readonly MonsterModifier KINGS_ARMOR = new MonsterModifier("King's Armor") { armor = 2, health = 5 }.AddHumanoids().AddSpirits();
    public static readonly MonsterModifier LAST_GASP = new MonsterModifier("Last Gasp") { armor = -2, damage = 1, fear = 1 }.AddHumanoids().AddTrolls().AddAllBeasts();
    public static readonly MonsterModifier NUMENOREAN_BLOOD = new MonsterModifier("Númenórean Blood") { health = 4 }.AddHumans();
    public static readonly MonsterModifier ORC_CAPTAIN = new MonsterModifier("Orc Captain") { armor = 1, health = 2, fear = 1, immuneStun = true }.AddOrcs();
    public static readonly MonsterModifier ORC_CHAMPION = new MonsterModifier("Orc Champion") { damage = 1, fear = 1 }.AddOrcs();
    public static readonly MonsterModifier PACKS_VENGEANCE_1 = new MonsterModifier("Pack's Vengeance") { health = 2, immuneStun = true, immuneLethal = true }.AddVargs();
    public static readonly MonsterModifier PACKS_VENGEANCE_2 = new MonsterModifier("Pack's Vengeance") { health = 2, immuneStun = true /*, fakeLethal=true */ }.AddVargs();
    public static readonly MonsterModifier POSSESSED = new MonsterModifier("Possessed") { fear = 1, immuneSmite = true }.AddHumanoids().AddAllBeasts();
    public static readonly MonsterModifier SHADOWMAN_CAPTAIN = new MonsterModifier("Shadowman Captain") { health = 4, sorcery = 1, immuneStun = true }.Add(MonsterType.Shadowman);
    public static readonly MonsterModifier SPECTRAL = new MonsterModifier("Spectral") { sorcery = 2, immuneLethal = true }.AddSpirits();
    public static readonly MonsterModifier SPIDER_CAPTAIN = new MonsterModifier("Spider Captain") { health = 7, damage = 1, immuneStun = true }.AddSpiders();
    public static readonly MonsterModifier SPIRIT_GUARD = new MonsterModifier("Spirit Guard") { fear = 2, damage = 2, sorcery = 2 }.AddSpirits();
    public static readonly MonsterModifier SWIFT_DEATH = new MonsterModifier("Swift Death") { damage = 1 };
    public static readonly MonsterModifier TRAINED_FOR_BATTLE = new MonsterModifier("Trained for Battle") { armor = 2, health = 5 }.AddHumanoids().AddTrolls();
    public static readonly MonsterModifier UNDYING_HATE = new MonsterModifier("Undying Hate") { health = -12, fear = 1, damage = 1, immuneStun = true };
    public static readonly MonsterModifier WARBAND_LEADER = new MonsterModifier("Warband Leader") { armor = 2, health = 2, damage = 2 }.AddHumanoids();
    public static readonly MonsterModifier WISP = new MonsterModifier("Wisp") { health = -1, sorcery = -1 }.AddSpirits();

    /*
    Durin's Bane 1	-2 armor, -1 fear
    Durin's Bane 2	+1 armor, Lethal/Stun immune
    Durin's Bane 3	+1 armor, +1 sorcery, +4 health, +1 fear, Lethal/Sunder immune
    Durin's Bane 4	+2 armor, +2 sorcery, +5 health, +2 fear, +2 damage, Lethal/Sunder/Stun immune
    Master of the Pit 1	+1 sorcery, Stun immune
    Master of the Pit 2	+2 sorcery, Stun immune
    Master of the Pit 3	+3 sorcery, Stun immune
    Herald of the Balrog	+2 health, +3 sorcery, Smite/Stun immune
    Golden Mask	+1 armor, +2 health, +2 sorcery, +1 fear
    Ursa's Vengeance (Ollie)	+4 health, +1 armor, +2 sorcery, Lethal/Stun immune
    Favored Beast(Worm Witch Steed)    +1 armor, +4 health, Stun immune
    Mother of Rot(Worm Witch)  +1 armor, +1 fear, +1 sorcery, Stun immune
    Witch-king Stun immune
    Chief of the Nine	+1 armor, Stun immune
    Lord of Minas Morgul	+5 health, +1 damage, +1 fear, Lethal/Sunder/Stun immune
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

    public static IEnumerable<MonsterModifier> Values
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
}
