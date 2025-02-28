using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum Role { NONE, BEAST_FRIEND, BURGLAR, CAPTAIN, DELVER, GUARDIAN, GUIDE, HERBALIST, HUNTER, 
        LOREKEEPER, MEDDLER, MUSICIAN, PATHFINDER, PROVISIONER, SHIELDMAIDEN, SMITH, SOLDIER, TRAVELLER, TRICKSTER,
        //Custom Fan Roles
        CHIEFTAIN, COUNSELOR, DARKFIGHTER, DEFENDER, GARDENER, HORSE_LORD, RANGER, RINGBEARER, RING_BEARER, SEER, SHEPHERD, TRACKER, WARRIOR
        //Always add new roles below so enum order isn't disrupted
};

public class RoleData
{
    public Role role { get; set; }
    public int collection { get; set; }
    public string dataName { get; set; }
    public int skillCount { get; set; }
    public int indexOffset { get; set; } = 3;
    public int[] skillCost { get; set; }

    public RoleData() { }
    public RoleData(Role role)
    {
        this.role = role;
    }
}
