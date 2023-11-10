using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Heroes
{
    public static readonly List<Hero> list = new List<Hero>
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

        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Rider-king
        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Horse lord
        new Hero(){sex = Sex.FEMALE, race=Race.MAN }, //Mounted princess
        new Hero(){sex = Sex.MALE, race=Race.HALFLING }, //Halfling gardener
        new Hero(){sex = Sex.MALE, race=Race.HALFLING }, //Gentle-halfling
        new Hero(){sex = Sex.MALE, race=Race.HALFLING }, //Halfling prankster

        new Hero(){sex = Sex.MALE, race=Race.DWARF }, //Dwarf
        new Hero(){sex = Sex.MALE, race=Race.DWARF }, //Dwarven lord
        new Hero(){sex = Sex.MALE, race=Race.DWARF }, //Dwarf
        new Hero(){sex = Sex.MALE, race=Race.DWARF }, //Dwarf
        new Hero(){sex = Sex.MALE, race=Race.DWARF }, //Dwarven noble
        new Hero(){sex = Sex.MALE, race=Race.DWARF }, //Dwarven noble

        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elven king
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elf ship wright
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elven consort
        new Hero(){sex = Sex.FEMALE, race=Race.ELF }, //Elven queen
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Half-elven
        new Hero(){sex = Sex.MALE, race=Race.WIZARD }, //Wizard

        new Hero(){sex = Sex.MALE, race=Race.DWARF }, //Dwarven king
        new Hero(){sex = Sex.MALE, race=Race.DWARF }, //Dwarven scribe
        new Hero(){sex = Sex.MALE, race=Race.DWARF }, //Dwarven minstrel
        new Hero(){sex = Sex.MALE, race=Race.DWARF }, //Dwarven jeweler
        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Adventurer
        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Barge master

        new Hero(){sex = Sex.FEMALE, race=Race.ELF }, //Elven huntress
        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Archer
        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Ranger
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elven archer
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elven archer
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elven archer

        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Archer
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elf
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elf kin slayer
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elven harper
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elven warbard
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elven smith

        new Hero(){sex = Sex.MALE, race=Race.MAN }, //King of man
        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Soldier
        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Steward
        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Border ranger
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Warrior
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Paladin

        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elven smith
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Elven mage
        new Hero(){sex = Sex.MALE, race=Race.ELF }, //Hound master
        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Blade master
        new Hero(){sex = Sex.MALE, race=Race.WIZARD }, //Healer
        new Hero(){sex = Sex.MALE, race=Race.MAN }, //Mounted warrior
    };
}
