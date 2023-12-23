using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public  class Titles
{
	public static List<int> ListGiveableTitlesFromIds(List<int> titleList, List<CharacterSheet> characterSheets)
    {
        //Loop over the titleList and loop over all the characters. Only keep the titles that aren't already owned.
		List<int> giveableTitles = new List<int>(titleList);
        foreach (int titleId in titleList)
        {
            foreach (CharacterSheet characterSheet in characterSheets)
            {
                if (characterSheet.titles.Contains(titleId))
                {
                    giveableTitles.Remove(titleId);
                }
            }
        }
		return giveableTitles;
    }

    public static Title FromID(int id)
    {
        return list.FirstOrDefault(it => it.id == id);
    }

    public static readonly List<Title> list = new List<Title>
        {
            new Title(0){originalName="None", dataName="None", collection=1},
            new Title(1){originalName="Mist-Walker", dataName="Fog-Pacer", collection=1},
            new Title(2){originalName="Stone-Talker", dataName="Rock-Whisperer", collection=1},
            new Title(3){originalName="Pack-Dweller", dataName="Herd-Habitant", collection=1},
            new Title(4){originalName="Dwarf-Friend", dataName="Dwarf-Buddy", collection=1},
            new Title(5){originalName="Word-Wielder", dataName="Orator", collection=1},
            new Title(6){originalName="Clue-Finder", dataName="Detail-Discoverer", collection=1},
            new Title(7){originalName="Stinging Fly", dataName="Biting Gnat", collection = 1},
            new Title(8){originalName="Wingfoot", dataName="Flight-Footed", collection = 1},
            new Title(9){originalName="Friend of Bears", dataName="Bear-Buddy", collection = 1},
            new Title(10){originalName="Fire-Giver", dataName="Prometheus", collection = 1},
            new Title(11){originalName="Gale-Rock", dataName="Hurrican-Haven", collection = 1},
            new Title(12){originalName="Stormcrow", dataName="Rainraven", collection = 1},
            new Title(13){originalName="Unfallen", dataName="Still Standing", collection = 1},
            new Title(14){originalName="Elf-Friend", dataName="Elf-Buddy", collection = 1},
            new Title(15){originalName="Coney-Foot", dataName="Rabbit-Foot", collection = 1},
            new Title(16){originalName="Skin-Changer", dataName="Shape-Shifter", collection = 1},
            new Title(17){originalName="Barrel Rider", dataName="Cask Rider", collection = 1},
            new Title(18){originalName="Flame-Bearer", dataName="Light-Bringer", collection = 1},
            new Title(19){originalName="Guest of Eagles", dataName="Falcon-Friend", collection = 1},
            new Title(20){originalName="Luckwearer", dataName="Chance-Bearer", collection = 1},
            new Title(21){originalName="Lone Survivor", dataName="Sole Remnant", collection=1},
            new Title(22){originalName="Nemesis", dataName="Doom-Bringer", collection=2},
            new Title(23){originalName="Lore-Master", dataName="Chief Historian", collection=2},
            new Title(24){originalName="Ever-Wary", dataName="Always-Careful", collection=2},
            new Title(25){originalName="Treasure Seeker", dataName="Trove Searcher", collection=3},
            new Title(26){originalName="Storyteller", dataName="Tale-Weaver", collection=3},
            new Title(27){originalName="Dawn-Bringer", dataName="Daybreaker", collection=3},
            new Title(28){originalName="Undying", dataName="Unfading", collection=4},
            new Title(29){originalName="Sly-Tongue", dataName="Trick-Talker", collection=4},
            new Title(30){originalName="Spirit-Bonded", dataName="Ghost-Buddy", collection=4},
            new Title(31){originalName="Friend of Gondor", dataName="[Gondorf-DE]-Buddy", collection=5},
            new Title(32){originalName="Friend of Rohan", dataName="[Rohan-FR]-Buddy", collection=5},
            new Title(33){originalName="Beast-Singer", dataName="Creature-Crooner", collection=5}
        };
}
