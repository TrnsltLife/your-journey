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
            new Title(1){originalName="Mist-Walker", dataName="Mist-Walker", collection=1},
            new Title(2){originalName="Stone-Talker", dataName="Stone-Talker", collection=1},
            new Title(3){originalName="Pack-Dweller", dataName="Pack-Dweller", collection=1},
            new Title(4){originalName="Dwarf-Friend", dataName="Dwarf-Friend", collection=1},
            new Title(5){originalName="Word-Wielder", dataName="Word-Wielder", collection=1},
            new Title(6){originalName="Clue-Finder", dataName="Clue-Finder", collection=1},
            new Title(7){originalName="Stinging Fly", dataName="Stinging Fly", collection = 1},
            new Title(8){originalName="Wingfoot", dataName="Wingfoot", collection = 1},
            new Title(9){originalName="Friend of Bears", dataName="Friend of Bears", collection = 1},
            new Title(10){originalName="Fire-Giver", dataName="Fire-Giver", collection = 1},
            new Title(11){originalName="Gale-Rock", dataName="Gale-Rock", collection = 1},
            new Title(12){originalName="Stormcrow", dataName="Stormcrow", collection = 1},
            new Title(13){originalName="Unfallen", dataName="Unfallen", collection = 1},
            new Title(14){originalName="Elf-Friend", dataName="Elf-Friend", collection = 1},
            new Title(15){originalName="Coney-Foot", dataName="Coney-Foot", collection = 1},
            new Title(16){originalName="Skin-Changer", dataName="Skin-Changer", collection = 1},
            new Title(17){originalName="Barrel Rider", dataName="Barrel Rider", collection = 1},
            new Title(18){originalName="Flame-Bearer", dataName="Flame-Bearer", collection = 1},
            new Title(19){originalName="Guest of Eagles", dataName="Guest of Eagles", collection = 1},
            new Title(20){originalName="Luckwearer", dataName="Luckwearer", collection = 1},
            new Title(21){originalName="Lone Survivor", dataName="Lone Survivor", collection=1},
            new Title(22){originalName="Nemesis", dataName="Nemesis", collection=2},
            new Title(23){originalName="Lore-Master", dataName="Lore-Master", collection=2},
            new Title(24){originalName="Ever-Wary", dataName="Ever-Wary", collection=2},
            new Title(25){originalName="Treasure Seeker", dataName="Treasure Seeker", collection=3},
            new Title(26){originalName="Storyteller", dataName="Storyteller", collection=3},
            new Title(27){originalName="Dawn-Bringer", dataName="Dawn-Bringer", collection=3},
            new Title(28){originalName="Undying", dataName="Undying", collection=4},
            new Title(29){originalName="Sly-Tongue", dataName="Sly-Tongue", collection=4},
            new Title(30){originalName="Spirit-Bonded", dataName="Spirit-Bonded", collection=4},
            new Title(31){originalName="Friend of Gondor", dataName="Friend of Gondor", collection=5},
            new Title(32){originalName="Friend of Rohan", dataName="Friend of Rohan", collection=5},
            new Title(33){originalName="Beast-Singer", dataName="Beast-Singer", collection=5}
        };
}
