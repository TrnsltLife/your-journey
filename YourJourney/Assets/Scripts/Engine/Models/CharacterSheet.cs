using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CharacterSheet
{
    public string name { get; set; }
    public int portraitIndex { get; set; }
    public Race race { get; set; }
    public Sex sex { get; set; }
    public int maxHanded { get; set; }
    public Role role { get; set; }
    public int armorId { get; set; }
    public int hand1Id { get; set; }
    public int hand2Id { get; set; }
    public int trinketId { get; set; }
    public int mountId { get; set; }
    public List<SkillRecord> skillRecords { get; set; } = new List<SkillRecord>();

    public CharacterSheet() { }

    public CharacterSheet(string name, int portraitIndex)
    {
        this.name = name;
        this.portraitIndex = portraitIndex;
    }
}

