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
    public int corruption { get; set; }
    public int armorId { get; set; }
    public int hand1Id { get; set; }
    public int hand2Id { get; set; }
    public int trinketId { get; set; }
    public int mountId { get; set; }
    public List<SkillRecord> skillRecords { get; set; } = new List<SkillRecord>();
    public List<int> titles { get; set; } = new List<int>();

    public CharacterSheet() { }

    public CharacterSheet(string name, int portraitIndex)
    {
        this.name = name;
        this.portraitIndex = portraitIndex;
    }

    public void AddTitle(int titleId)
    {
        if(titleId <= 0 || titleId > 33) { return; }
        if (!titles.Contains(titleId))
        {
            titles.Add(titleId);
        }
    }

    public CharacterSheet Clone()
    {
        List<SkillRecord> skillClone = new List<SkillRecord>();
        foreach(var skill in skillRecords)
        {
            skillClone.Add(skill.Clone());
        }
        CharacterSheet clone = new CharacterSheet(name, portraitIndex) { race=race, sex=sex, maxHanded=maxHanded, role=role, corruption=corruption,
            armorId=armorId, hand1Id=hand1Id, hand2Id=hand2Id, trinketId=trinketId, mountId=mountId, skillRecords=skillClone, titles=new List<int>(titles)};
        return clone;
    }
}

