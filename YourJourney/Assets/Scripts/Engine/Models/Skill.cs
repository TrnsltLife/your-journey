using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Skill
{
    public int id { get; set; }
    public int collection { get; set; }
    public Role role { get; set; }
    public int skillCost { get; set; }
    public string dataName { get; set; }
    public string originalName { get; set; }
    public int skillId { get; set; }

    public Skill() { }

    public Skill(int id)
    {
        this.id = id;
    }
}
