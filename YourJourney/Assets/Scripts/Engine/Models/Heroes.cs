using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Heroes
{
    public static List<Hero> heroes = new List<Hero>
    {
        new Hero(){sex = Sex.MALE, race=Race.MAN}, //Ranger chief
        new Hero(){sex = Sex.FEMALE, race=Race.MAN}, //Ranger heroine
        new Hero(){sex = Sex.MALE, race=Race.HALFLING}, //Halfing burglar
        new Hero(){sex = Sex.FEMALE, race=Race.ELF}, //Elven harpist
        new Hero(){sex = Sex.MALE, race=Race.DWARF}, //Dwarven axe-poet
        new Hero(){sex = Sex.MALE, race=Race.ELF}, //Elven prince

        new Hero(){sex = Sex.FEMALE, race=Race.ELF}, //Elven maiden
        new Hero(){sex = Sex.MALE, race=Race.DWARF}, //Dwarven miner
        new Hero(){sex = Sex.FEMALE, race=Race.DWARF}, //Dwarven matron
        new Hero(){sex = Sex.FEMALE, race=Race.MAN}, //Noble woman
        new Hero(){sex = Sex.MALE, race=Race.WIZARD}, //Grey wizard
        new Hero(){sex = Sex.MALE, race=Race.WEREBEAR}, //Were-bear

        new Hero(){sex = Sex.MALE, race=Race.MAN}, //Steward's son
        new Hero(){sex = Sex.FEMALE, race=Race.HALFLING}, //Halfling lass
        new Hero(){sex = Sex.MALE, race=Race.DWARF}, //Dwarven scholar
        new Hero(){sex = Sex.FEMALE, race=Race.MAN}, //Lancelass
        new Hero(){sex = Sex.FEMALE, race=Race.ELF}, //Elven shipmaid
        new Hero(){sex = Sex.MALE, race=Race.MAN}, //Friend of beasts
    };
}
