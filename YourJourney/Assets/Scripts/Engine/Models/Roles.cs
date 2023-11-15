using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Roles
{
	public static RoleData FromRole(Role role)
	{
		return list.FirstOrDefault(it => it.role == role);
	}

	public static bool RoleAvailable(Role role, List<CharacterSheet> characters, int charIndex)
	{
		RoleData roleData = Roles.FromRole(role);
		if (roleData == null) { return false; }

		for (int i = 0; i < characters.Count; i++)
		{
			CharacterSheet character = characters[i];
			if (i == charIndex)
			{
				if (character.role == role) { return true; }
			}
			else
			{
				if (character.role == role) { return false; }
			}
		}
		return true;
	}

	public static readonly List<RoleData> list = new List<RoleData> {
		new RoleData(Role.NONE) {collection=0, dataName="None", skillCount= 0, skillCost= new int[]{}},
		new RoleData(Role.BEAST_FRIEND) {collection=6, dataName="Beast-friend", skillCount= 6, skillCost= new int[]{0, 0, 6, 6, 8, 8}},
		new RoleData(Role.BURGLAR) {collection=1, dataName="Burglar", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.CAPTAIN) {collection=1, dataName="Captain", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.DELVER) {collection=3, dataName="Delver", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.GUARDIAN) {collection=1, dataName="Guardian", skillCount=  12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.GUIDE) {collection=5, dataName="Guide", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.HERBALIST) {collection=3, dataName="Herbalist", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.HUNTER) {collection=1, dataName="Hunter", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.LOREKEEPER) {collection=5, dataName="Lorekeeper", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.MEDDLER) {collection=3, dataName="Meddler", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 7, 7, 12, 12, 12, 16, 16}},
		new RoleData(Role.MUSICIAN) {collection=1, dataName="Musician", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.PATHFINDER) {collection=1, dataName="Pathfinder", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.PROVISIONER) {collection=5, dataName="Provisioner", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.SHIELDMAIDEN) {collection=5, dataName="Shieldmaiden", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.SMITH) {collection=3, dataName="Smith", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.SOLDIER) {collection=5, dataName="Soldier", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.TRAVELLER) {collection=3, dataName="Traveller", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}},
		new RoleData(Role.TRICKSTER) {collection=5, dataName="Trickster", skillCount= 12, skillCost= new int[]{0, 0, 0, 3, 3, 3, 7, 7, 7, 7, 12, 12}}
	};
}
