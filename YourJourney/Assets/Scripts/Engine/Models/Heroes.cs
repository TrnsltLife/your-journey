using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Heroes
{
    public static Hero FromID(int id)
    {
        return list.First(it => it.id == id);
    }

    public static readonly List<Hero> list = new List<Hero>
    {
        new Hero(0){name="Ranger chief", sex = Sex.MALE, race=Race.MAN, role=Role.CAPTAIN, armor=ItemSeries.TRAVEL_GARB, hand1=ItemSeries.SWORD, hand2=ItemSeries.BANNER},
        new Hero(1){name="Ranger heroine", sex = Sex.FEMALE, race=Race.MAN, role=Role.PATHFINDER, armor=ItemSeries.TRAVEL_GARB, hand1=ItemSeries.STAFF},
        new Hero(2){name="Halfing burglar", sex = Sex.MALE, race=Race.HALFLING, maxHanded=1, role=Role.BURGLAR, armor=ItemSeries.CLOAK, hand1=ItemSeries.DAGGER},
        new Hero(3){name="Elven harpist", sex = Sex.FEMALE, race=Race.ELF, role=Role.MUSICIAN, armor=ItemSeries.CLOAK, hand1=ItemSeries.DAGGER, hand2=ItemSeries.HARP},
        new Hero(4){name="Dwarven axe-poet", sex = Sex.MALE, race=Race.DWARF, role=Role.GUARDIAN, armor=ItemSeries.RING_MAIL, hand1=ItemSeries.BATTLE_AXE},
        new Hero(5){name="Elven prince", sex = Sex.MALE, race=Race.ELF, role=Role.HUNTER, armor=ItemSeries.CLOAK, hand1=ItemSeries.GREAT_BOW},

        new Hero(6){name="Elven maiden", sex = Sex.FEMALE, race=Race.ELF, role=Role.HERBALIST, armor=ItemSeries.CLOAK, hand1=ItemSeries.KNIFE, hand2=ItemSeries.SLING},
        new Hero(7){name="Dwarven miner", sex = Sex.MALE, race=Race.DWARF, role=Role.DELVER, armor=ItemSeries.TRAVEL_GARB, hand1=ItemSeries.HATCHET, hand2=ItemSeries.SHIELD},
        new Hero(8){name="Dwarven matron", sex = Sex.FEMALE, race=Race.DWARF, role=Role.SMITH, armor=ItemSeries.RING_MAIL, hand1=ItemSeries.HAMMER},
        new Hero(9){name="Noble woman", sex = Sex.FEMALE, race=Race.MAN, role=Role.TRAVELLER, armor=ItemSeries.TRAVEL_GARB, hand1=ItemSeries.SWORD, hand2=ItemSeries.HORN},
        new Hero(10){name="Grey wizard", sex = Sex.MALE, race=Race.WIZARD, role=Role.MEDDLER, armor=ItemSeries.CLOAK, hand1=ItemSeries.WALKING_STICK, hand2=ItemSeries.SWORD},
        new Hero(11){name="Were-bear", sex = Sex.MALE, race=Race.WEREBEAR, role=Role.GUIDE, armor=ItemSeries.TRAVEL_GARB, hand1=ItemSeries.BATTLE_AXE},

        new Hero(12){name="Steward's son", sex = Sex.MALE, race=Race.MAN, role=Role.SOLDIER, armor=ItemSeries.PLATE_ARMOR, hand1=ItemSeries.SWORD, hand2=ItemSeries.HORN},
        new Hero(13){name="Halfling lass", sex = Sex.FEMALE, race=Race.HALFLING, maxHanded=1, role=Role.PROVISIONER, armor=ItemSeries.CLOAK, hand1=ItemSeries.MACE},
        new Hero(14){name="Dwarven scholar", sex = Sex.MALE, race=Race.DWARF, role=Role.LOREKEEPER, armor=ItemSeries.CLOAK, hand1=ItemSeries.SPEAR},
        new Hero(15){name="Lancelass", sex = Sex.FEMALE, race=Race.MAN, maxHanded=3, role=Role.SHIELDMAIDEN, armor=ItemSeries.PADDED_ARMOR, hand1=ItemSeries.SPEAR, hand2=ItemSeries.SHIELD, mount=ItemSeries.SNOWBRIGHT},
        new Hero(16){name="Elven shipmaid", sex = Sex.FEMALE, race=Race.ELF, role=Role.TRICKSTER, armor=ItemSeries.PADDED_ARMOR, hand1=ItemSeries.SHORT_BOW},
        new Hero(17){name="Friend of beasts", sex = Sex.MALE, race=Race.MAN},

        new Hero(18){name="Rider-king", sex = Sex.MALE, race=Race.MAN, maxHanded=3, role=Role.SHIELDMAIDEN, mount=ItemSeries.SNOWBRIGHT },
        new Hero(19){name="Horse lord", sex = Sex.MALE, race=Race.MAN, maxHanded=3, role=Role.SHIELDMAIDEN, mount=ItemSeries.SNOWBRIGHT },
        new Hero(20){name="Mounted princess", sex = Sex.FEMALE, race=Race.MAN, maxHanded=3, role=Role.SHIELDMAIDEN, mount=ItemSeries.SNOWBRIGHT },
        new Hero(21){name="Halfling gardener", sex = Sex.MALE, maxHanded=1, race=Race.HALFLING },
        new Hero(22){name="Gentle-halfling", sex = Sex.MALE, maxHanded=1, race=Race.HALFLING },
        new Hero(23){name="Halfling prankster", sex = Sex.MALE, maxHanded=1, race=Race.HALFLING },

        new Hero(24){name="Dwarf", sex = Sex.MALE, race=Race.DWARF },
        new Hero(25){name="Dwarven lord", sex = Sex.MALE, race=Race.DWARF },
        new Hero(26){name="Dwarf", sex = Sex.MALE, race=Race.DWARF },
        new Hero(27){name="Dwarf", sex = Sex.MALE, race=Race.DWARF },
        new Hero(28){name="Dwarven noble", sex = Sex.MALE, race=Race.DWARF },
        new Hero(29){name="Dwarven noble", sex = Sex.MALE, race=Race.DWARF },

        new Hero(30){name="Elven king", sex = Sex.MALE, race=Race.ELF },
        new Hero(31){name="Elf ship wright", sex = Sex.MALE, race=Race.ELF },
        new Hero(32){name="Elven consort", sex = Sex.MALE, race=Race.ELF },
        new Hero(33){name="Elven queen", sex = Sex.FEMALE, race=Race.ELF },
        new Hero(34){name="Half-elven", sex = Sex.MALE, race=Race.ELF },
        new Hero(35){name="Wizard", sex = Sex.MALE, race=Race.WIZARD },

        new Hero(36){name="Dwarven king", sex = Sex.MALE, race=Race.DWARF },
        new Hero(37){name="Dwarven scribe", sex = Sex.MALE, race=Race.DWARF },
        new Hero(38){name="Dwarven minstrel", sex = Sex.MALE, race=Race.DWARF },
        new Hero(39){name="Dwarven jeweler", sex = Sex.MALE, race=Race.DWARF },
        new Hero(40){name="Adventurer", sex = Sex.MALE, race=Race.MAN },
        new Hero(41){name="Barge master", sex = Sex.MALE, race=Race.MAN },

        new Hero(42){name="Elven huntress", sex = Sex.FEMALE, race=Race.ELF },
        new Hero(43){name="Archer", sex = Sex.MALE, race=Race.MAN },
        new Hero(44){name="Ranger", sex = Sex.MALE, race=Race.MAN },
        new Hero(45){name="Elven archer", sex = Sex.MALE, race=Race.ELF },
        new Hero(46){name="Elven archer", sex = Sex.MALE, race=Race.ELF },
        new Hero(47){name="Elven archer", sex = Sex.MALE, race=Race.ELF },

        new Hero(48){name="Archer", sex = Sex.MALE, race=Race.MAN },
        new Hero(49){name="Elf", sex = Sex.MALE, race=Race.ELF },
        new Hero(50){name="Elf kin slayer", sex = Sex.MALE, race=Race.ELF },
        new Hero(51){name="Elven harper", sex = Sex.MALE, race=Race.ELF },
        new Hero(52){name="Elven warbard", sex = Sex.MALE, race=Race.ELF },
        new Hero(53){name="Elven smith", sex = Sex.MALE, race=Race.ELF },

        new Hero(54){name="King of men", sex = Sex.MALE, race=Race.MAN },
        new Hero(55){name="Soldier", sex = Sex.MALE, race=Race.MAN },
        new Hero(56){name="Steward", sex = Sex.MALE, race=Race.MAN },
        new Hero(57){name="Border ranger", sex = Sex.MALE, race=Race.MAN },
        new Hero(58){name="Warrior", sex = Sex.MALE, race=Race.ELF },
        new Hero(59){name="Paladin", sex = Sex.MALE, race=Race.ELF },

        new Hero(60){name="Elven smith", sex = Sex.MALE, race=Race.ELF },
        new Hero(61){name="Elven mage", sex = Sex.MALE, race=Race.ELF },
        new Hero(62){name="Hound master", sex = Sex.MALE, race=Race.ELF },
        new Hero(63){name="Blade master", sex = Sex.MALE, race=Race.MAN },
        new Hero(64){name="Healer", sex = Sex.MALE, race=Race.WIZARD },
        new Hero(65){name="Mounted warrior", sex = Sex.MALE, race=Race.MAN },
    };
}
