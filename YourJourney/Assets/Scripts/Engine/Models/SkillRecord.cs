using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SkillRecord
{
    public Role role { get; set; } = Role.NONE;
    public int xp { get; set; } = 0;
    public List<int> selectedSkillIndex { get; set; } = new List<int>();

    public SkillRecord() { }

    public SkillRecord(Role role)
    {
        this.role = role;
    }

    public SkillRecord Clone()
    {
        List<int> listClone = new List<int>();
        listClone.AddRange(selectedSkillIndex);
        SkillRecord clone = new SkillRecord(role) { xp = xp, selectedSkillIndex = listClone };
        return clone;
    }
}
