using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public  class Items
{
    public static Item FromID(int id)
    {
        return list.FirstOrDefault(it => it.id == id);
    }

    public static List<Item> FromSeriesID(ItemSeries seriesId)
    {
        return list.Where(it => it.seriesId == seriesId).ToList();
    }

	public static Item FromSeriesIDAndTier(ItemSeries seriesId, int tier)
	{
		return list.Where(it => it.seriesId == seriesId && it.tier == tier).ToList().FirstOrDefault();
	}

	public static bool ItemAvailable(int id, List<CharacterSheet> characters, int charIndex, int handIndex)
    {
		Item item = Items.FromID(id);
		if(item == null) { return false; }

		for(int i=0; i<characters.Count; i++)
        {
			CharacterSheet character = characters[i];
			if(i == charIndex)
            {
				if (character.armorId == id) { return true; }
				else if (character.hand1Id == id && handIndex == 1) { return true; }
				else if (character.hand1Id == id && handIndex == 2) { return false; }
				else if (character.hand2Id == id && handIndex == 2) { return true; }
				else if (character.hand2Id == id && handIndex == 1) { return false; }
				else if (character.trinketId == id) { return true; }
				else if (character.mountId == id) { return true; }
			}
			else
            {
				if (character.armorId == id) { return false; }
				else if (character.hand1Id == id) { return false; }
				else if (character.hand2Id == id) { return false; }
				else if (character.trinketId == id) { return false; }
				else if (character.mountId == id) { return false; }
            }
        }
        return true;
    }

	public static List<int> ListGiveableItemsFromIds(List<int> itemList, List<int> currentTrinkets, List<int> currentMounts)
    {
		//With the initial itemList which contains tier-1 trinkets and/or tier-? mounts, and the currentTrinkets and currentMounts owned by the party (unknown tier),
		//what tier-1 trinkets, and what mounts, can actually be given to the party? (Exclude items they already have a version of. Exclude trinkets if they have a higher tier.)

		//Convert the current list of trinkets at whatever upgraded tier they are, to a list of the tier 1 trinket ids
		List<int> starterTrinkets = new List<int>();
		foreach(int itemId in currentTrinkets)
        {
			starterTrinkets.Add(Items.FromSeriesIDAndTier(Items.FromID(itemId).seriesId, 1).id);
        }

		//We don't need to convert the mounts because there are no upgrades for mounts

		//Cull the itemList by removing items that are already owned by the players/party
		List<int> newList = new List<int>();
		foreach(int itemId in itemList)
        {
			if(starterTrinkets.Contains(itemId) || currentMounts.Contains(itemId)) { continue; }
			else
            {
				newList.Add(itemId);
            }
        }

		return newList;
    }

	public static bool TrinketSeriesAvailable(ItemSeries seriesId, List<CharacterSheet> characters, int charIndex)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			CharacterSheet character = characters[i];
			if(i != charIndex) //a different character than the selected one
			{
				if(Items.FromID(character.trinketId).seriesId == seriesId) { return false; }
			}
		}
		return true;
	}

	public static int FirstAvailable(ItemSeries seriesId, int tier, List<CharacterSheet> characters, int charIndex, int handIndex)
    {
		List<Item> items = FromSeriesID(seriesId);
		foreach(Item item in items)
        {
			if(ItemAvailable(item.id, characters, charIndex, handIndex))
            {
				return item.id;
            }
        }
		return 0;
    }

	public static int CostToUpgradeTo(ItemSeries seriesId, int upgradeToTier)
    {
		int upgradeFromTier = upgradeToTier - 1;
		if(upgradeFromTier < 1 || upgradeFromTier >= 4) { return 0; }
		Item item = list.FirstOrDefault(it => it.seriesId == seriesId && it.tier == upgradeFromTier); //all items in the same series should have the same upgrade price at each tier level
		if(item == null) { return 0; }
		return item.upgrade;
    }

    public static readonly List<Item> list =new List<Item>
    {
		new Item(0){collection=0, slotId=Slot.NONE, slot="None", seriesId=ItemSeries.NONE, seriesName="None", dataName="None", originalName="None", tier=0, stats=new string[]{}, upgrade=0, handed=0, ranged=0},
		new Item(1){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.CLOAK, seriesName="Cloak", dataName="Cloak", originalName="Cloak", tier=1, stats=new string[]{}, upgrade=28, handed=0, ranged=0},
		new Item(2){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.CLOAK, seriesName="Cloak", dataName="Cloak", originalName="Cloak", tier=1, stats=new string[]{}, upgrade=28, handed=0, ranged=0},
		new Item(3){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.CLOAK, seriesName="Cloak", dataName="Cloak", originalName="Cloak", tier=1, stats=new string[]{}, upgrade=28, handed=0, ranged=0},
		new Item(4){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.CLOAK, seriesName="Cloak", dataName="Fangorn Cloak", originalName="Fangorn Cloak", tier=2, stats=new string[]{"Agility"}, upgrade=65, handed=0, ranged=1},
		new Item(5){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.CLOAK, seriesName="Cloak", dataName="Ranger Cloak", originalName="Ranger Cloak", tier=2, stats=new string[]{}, upgrade=65, handed=0, ranged=0},
		new Item(6){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.CLOAK, seriesName="Cloak", dataName="Tuckborough Cloak", originalName="Tuckborough Cloak", tier=2, stats=new string[]{}, upgrade=65, handed=0, ranged=0},
		new Item(7){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.CLOAK, seriesName="Cloak", dataName="Slip-Thorn", originalName="Slip-Thorn", tier=3, stats=new string[]{"Agility"}, upgrade=92, handed=0, ranged=1},
		new Item(8){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.CLOAK, seriesName="Cloak", dataName="Splendor-Well", originalName="Splendor-Well", tier=3, stats=new string[]{}, upgrade=92, handed=0, ranged=0},
		new Item(9){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.CLOAK, seriesName="Cloak", dataName="Wind-Walker", originalName="Wind-Walker", tier=3, stats=new string[]{}, upgrade=92, handed=0, ranged=0},
		new Item(10){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.CLOAK, seriesName="Cloak", dataName="Storm-Shroud", originalName="Storm-Shroud", tier=4, stats=new string[]{"Wild"}, upgrade=0, handed=0, ranged=1},
		new Item(11){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.HOARY_COAT, seriesName="Hoary Coat", dataName="Hoary Coat", originalName="Hoary Coat", tier=0, stats=new string[]{}, upgrade=0, handed=0, ranged=0},
		new Item(12){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PADDED_ARMOR, seriesName="Padded Armor", dataName="Padded Armor", originalName="Padded Armor", tier=1, stats=new string[]{}, upgrade=24, handed=0, ranged=0},
		new Item(13){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PADDED_ARMOR, seriesName="Padded Armor", dataName="Padded Armor", originalName="Padded Armor", tier=1, stats=new string[]{}, upgrade=24, handed=0, ranged=0},
		new Item(14){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PADDED_ARMOR, seriesName="Padded Armor", dataName="Guard's Tabard", originalName="Guard's Tabard", tier=2, stats=new string[]{}, upgrade=60, handed=0, ranged=0},
		new Item(15){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PADDED_ARMOR, seriesName="Padded Armor", dataName="Vanguard Armor", originalName="Vanguard Armor", tier=2, stats=new string[]{}, upgrade=60, handed=0, ranged=0},
		new Item(16){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PADDED_ARMOR, seriesName="Padded Armor", dataName="Ever-Vigilant", originalName="Ever-Vigilant", tier=3, stats=new string[]{}, upgrade=99, handed=0, ranged=0},
		new Item(17){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PADDED_ARMOR, seriesName="Padded Armor", dataName="Glory-Mantle", originalName="Glory-Mantle", tier=3, stats=new string[]{}, upgrade=99, handed=0, ranged=0},
		new Item(18){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PADDED_ARMOR, seriesName="Padded Armor", dataName="Heart-Swell", originalName="Heart-Swell", tier=3, stats=new string[]{}, upgrade=99, handed=0, ranged=0},
		new Item(19){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PADDED_ARMOR, seriesName="Padded Armor", dataName="Star of the West", originalName="Star of the West", tier=4, stats=new string[]{}, upgrade=0, handed=0, ranged=0},
		new Item(20){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PLATE_ARMOR, seriesName="Plate Armor", dataName="Plate Armor", originalName="Plate Armor", tier=1, stats=new string[]{}, upgrade=30, handed=0, ranged=0},
		new Item(21){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PLATE_ARMOR, seriesName="Plate Armor", dataName="Iron Hills Plate", originalName="Iron Hills Plate", tier=2, stats=new string[]{}, upgrade=66, handed=0, ranged=0},
		new Item(22){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PLATE_ARMOR, seriesName="Plate Armor", dataName="Defender of the Citadel", originalName="Defender of the Citadel", tier=3, stats=new string[]{}, upgrade=99, handed=0, ranged=0},
		new Item(23){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PLATE_ARMOR, seriesName="Plate Armor", dataName="Iron-Soul", originalName="Iron-Soul", tier=3, stats=new string[]{}, upgrade=99, handed=0, ranged=0},
		new Item(24){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.PLATE_ARMOR, seriesName="Plate Armor", dataName="King's Crest", originalName="King's Crest", tier=4, stats=new string[]{}, upgrade=0, handed=0, ranged=0},
		new Item(25){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.RING_MAIL, seriesName="Ring Mail", dataName="Ring Mail", originalName="Ring Mail", tier=1, stats=new string[]{}, upgrade=26, handed=0, ranged=0},
		new Item(26){collection=3, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.RING_MAIL, seriesName="Ring Mail", dataName="Ring Mail", originalName="Ring Mail", tier=1, stats=new string[]{}, upgrade=26, handed=0, ranged=0},
		new Item(27){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.RING_MAIL, seriesName="Ring Mail", dataName="Evendim Ring Mail", originalName="Evendim Ring Mail", tier=2, stats=new string[]{}, upgrade=62, handed=0, ranged=0},
		new Item(28){collection=3, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.RING_MAIL, seriesName="Ring Mail", dataName="Twice-Wrought Ring Mail", originalName="Twice-Wrought Ring Mail", tier=2, stats=new string[]{}, upgrade=62, handed=0, ranged=0},
		new Item(29){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.RING_MAIL, seriesName="Ring Mail", dataName="Blade-Bane", originalName="Blade-Bane", tier=3, stats=new string[]{}, upgrade=98, handed=0, ranged=0},
		new Item(30){collection=3, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.RING_MAIL, seriesName="Ring Mail", dataName="Second Skin", originalName="Second Skin", tier=3, stats=new string[]{}, upgrade=98, handed=0, ranged=0},
		new Item(31){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.RING_MAIL, seriesName="Ring Mail", dataName="Wrath's-End", originalName="Wrath's-End", tier=3, stats=new string[]{}, upgrade=98, handed=0, ranged=0},
		new Item(32){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.RING_MAIL, seriesName="Ring Mail", dataName="Dragon Scale", originalName="Dragon Scale", tier=4, stats=new string[]{}, upgrade=0, handed=0, ranged=0},
		new Item(33){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.TRAVEL_GARB, seriesName="Travel Garb", dataName="Travel Garb", originalName="Travel Garb", tier=1, stats=new string[]{}, upgrade=24, handed=0, ranged=0},
		new Item(34){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.TRAVEL_GARB, seriesName="Travel Garb", dataName="Travel Garb", originalName="Travel Garb", tier=1, stats=new string[]{}, upgrade=24, handed=0, ranged=0},
		new Item(35){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.TRAVEL_GARB, seriesName="Travel Garb", dataName="Greenwood Garb", originalName="Greenwood Garb", tier=2, stats=new string[]{}, upgrade=57, handed=0, ranged=0},
		new Item(36){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.TRAVEL_GARB, seriesName="Travel Garb", dataName="Westmarch Garb", originalName="Westmarch Garb", tier=2, stats=new string[]{}, upgrade=57, handed=0, ranged=0},
		new Item(37){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.TRAVEL_GARB, seriesName="Travel Garb", dataName="Ever-Bloom", originalName="Ever-Bloom", tier=3, stats=new string[]{}, upgrade=88, handed=0, ranged=0},
		new Item(38){collection=1, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.TRAVEL_GARB, seriesName="Travel Garb", dataName="Wanderer's Wish", originalName="Wanderer's Wish", tier=3, stats=new string[]{}, upgrade=88, handed=0, ranged=0},
		new Item(39){collection=5, slotId=Slot.ARMOR, slot="Armor", seriesId=ItemSeries.TRAVEL_GARB, seriesName="Travel Garb", dataName="Endless Road", originalName="Endless Road", tier=4, stats=new string[]{}, upgrade=0, handed=0, ranged=0},
		new Item(40){collection=1, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.BANNER, seriesName="Banner", dataName="Banner", originalName="Banner", tier=1, stats=new string[]{}, upgrade=22, handed=1, ranged=0},
		new Item(41){collection=1, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.BANNER, seriesName="Banner", dataName="Dunedain Banner", originalName="Dunedain Banner", tier=2, stats=new string[]{}, upgrade=61, handed=1, ranged=0},
		new Item(42){collection=1, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.BANNER, seriesName="Banner", dataName="War-Haven", originalName="War-Haven", tier=3, stats=new string[]{}, upgrade=94, handed=1, ranged=0},
		new Item(43){collection=1, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.BANNER, seriesName="Banner", dataName="War-Maker", originalName="War-Maker", tier=3, stats=new string[]{"Might", "Wit"}, upgrade=94, handed=1, ranged=0},
		new Item(44){collection=5, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.BANNER, seriesName="Banner", dataName="Hope's Beacon", originalName="Hope's Beacon", tier=4, stats=new string[]{}, upgrade=0, handed=1, ranged=0},
		new Item(45){collection=1, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HARP, seriesName="Harp", dataName="Harp", originalName="Harp", tier=1, stats=new string[]{}, upgrade=24, handed=1, ranged=0},
		new Item(46){collection=1, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HARP, seriesName="Harp", dataName="Forlindon Harp", originalName="Forlindon Harp", tier=2, stats=new string[]{}, upgrade=58, handed=1, ranged=0},
		new Item(47){collection=1, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HARP, seriesName="Harp", dataName="Heart's-Rest", originalName="Heart's-Rest", tier=3, stats=new string[]{}, upgrade=90, handed=1, ranged=0},
		new Item(48){collection=1, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HARP, seriesName="Harp", dataName="River-Calling", originalName="River-Calling", tier=3, stats=new string[]{"Spirit"}, upgrade=90, handed=1, ranged=0},
		new Item(49){collection=5, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HARP, seriesName="Harp", dataName="Chorus of Light", originalName="Chorus of Light", tier=4, stats=new string[]{"Wild"}, upgrade=0, handed=1, ranged=1},
		new Item(50){collection=3, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HORN, seriesName="Horn", dataName="Horn", originalName="Horn", tier=1, stats=new string[]{}, upgrade=30, handed=1, ranged=0},
		new Item(51){collection=5, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HORN, seriesName="Horn", dataName="Horn", originalName="Horn", tier=1, stats=new string[]{}, upgrade=30, handed=1, ranged=0},
		new Item(52){collection=3, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HORN, seriesName="Horn", dataName="Gondorian Horn", originalName="Gondorian Horn", tier=2, stats=new string[]{}, upgrade=60, handed=1, ranged=0},
		new Item(53){collection=5, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HORN, seriesName="Horn", dataName="Sentry's Horn", originalName="Sentry's Horn", tier=2, stats=new string[]{}, upgrade=60, handed=1, ranged=0},
		new Item(54){collection=5, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HORN, seriesName="Horn", dataName="Clarion Call", originalName="Clarion Call", tier=3, stats=new string[]{}, upgrade=80, handed=1, ranged=0},
		new Item(55){collection=3, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HORN, seriesName="Horn", dataName="Ringing Glory", originalName="Ringing Glory", tier=3, stats=new string[]{"Wisdom", "Spirit"}, upgrade=80, handed=1, ranged=1},
		new Item(56){collection=3, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HORN, seriesName="Horn", dataName="Starsong", originalName="Starsong", tier=3, stats=new string[]{}, upgrade=80, handed=1, ranged=0},
		new Item(57){collection=5, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.HORN, seriesName="Horn", dataName="Thunderclap", originalName="Thunderclap", tier=4, stats=new string[]{"Wild"}, upgrade=0, handed=1, ranged=1},
		new Item(58){collection=3, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.SHIELD, seriesName="Shield", dataName="Shield", originalName="Shield", tier=1, stats=new string[]{}, upgrade=27, handed=1, ranged=0},
		new Item(59){collection=5, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.SHIELD, seriesName="Shield", dataName="Shield", originalName="Shield", tier=1, stats=new string[]{}, upgrade=27, handed=1, ranged=0},
		new Item(60){collection=5, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.SHIELD, seriesName="Shield", dataName="Bossed Shield", originalName="Bossed Shield", tier=2, stats=new string[]{}, upgrade=61, handed=1, ranged=0},
		new Item(61){collection=3, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.SHIELD, seriesName="Shield", dataName="Reinforced Shield", originalName="Reinforced Shield", tier=2, stats=new string[]{}, upgrade=61, handed=1, ranged=0},
		new Item(62){collection=3, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.SHIELD, seriesName="Shield", dataName="Flame-Guard", originalName="Flame-Guard", tier=3, stats=new string[]{"Might"}, upgrade=81, handed=1, ranged=0},
		new Item(63){collection=5, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.SHIELD, seriesName="Shield", dataName="Foe-Blind", originalName="Foe-Blind", tier=3, stats=new string[]{"Spirit"}, upgrade=81, handed=1, ranged=0},
		new Item(64){collection=3, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.SHIELD, seriesName="Shield", dataName="Ironhide", originalName="Ironhide", tier=3, stats=new string[]{}, upgrade=81, handed=1, ranged=0},
		new Item(65){collection=5, slotId=Slot.HAND, slot="Support", seriesId=ItemSeries.SHIELD, seriesName="Shield", dataName="Hrinanbenn", originalName="Hrinanbenn", tier=4, stats=new string[]{}, upgrade=0, handed=1, ranged=0},
		new Item(66){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.BATTLE_AXE, seriesName="Battle Axe", dataName="Battle Axe", originalName="Battle Axe", tier=1, stats=new string[]{"Might"}, upgrade=33, handed=2, ranged=0},
		new Item(67){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.BATTLE_AXE, seriesName="Battle Axe", dataName="Ered Luin Battle Axe", originalName="Ered Luin Battle Axe", tier=2, stats=new string[]{"Might"}, upgrade=71, handed=2, ranged=0},
		new Item(68){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.BATTLE_AXE, seriesName="Battle Axe", dataName="Iron Hills Battle Axe", originalName="Iron Hills Battle Axe", tier=2, stats=new string[]{"Might"}, upgrade=71, handed=2, ranged=0},
		new Item(69){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.BATTLE_AXE, seriesName="Battle Axe", dataName="Grief-Bearer", originalName="Grief-Bearer", tier=3, stats=new string[]{"Might"}, upgrade=105, handed=2, ranged=0},
		new Item(70){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.BATTLE_AXE, seriesName="Battle Axe", dataName="Honor-Knell", originalName="Honor-Knell", tier=3, stats=new string[]{"Might", "Spirit"}, upgrade=105, handed=2, ranged=0},
		new Item(71){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.BATTLE_AXE, seriesName="Battle Axe", dataName="Sorrow-Sworn", originalName="Sorrow-Sworn", tier=4, stats=new string[]{"Might", "Spirit"}, upgrade=0, handed=2, ranged=0},
		new Item(72){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.DAGGER, seriesName="Dagger", dataName="Dagger", originalName="Dagger", tier=1, stats=new string[]{"Wit"}, upgrade=25, handed=1, ranged=0},
		new Item(73){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.DAGGER, seriesName="Dagger", dataName="Dagger", originalName="Dagger", tier=1, stats=new string[]{"Wit"}, upgrade=25, handed=1, ranged=0},
		new Item(74){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.DAGGER, seriesName="Dagger", dataName="Ered Luin Dagger", originalName="Ered Luin Dagger", tier=2, stats=new string[]{"Wit"}, upgrade=55, handed=1, ranged=2},
		new Item(75){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.DAGGER, seriesName="Dagger", dataName="Gondolin Dagger", originalName="Gondolin Dagger", tier=2, stats=new string[]{"Wit"}, upgrade=55, handed=1, ranged=0},
		new Item(76){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.DAGGER, seriesName="Dagger", dataName="Blood-Wright", originalName="Blood-Wright", tier=3, stats=new string[]{"Wit"}, upgrade=85, handed=1, ranged=1},
		new Item(77){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.DAGGER, seriesName="Dagger", dataName="Shade-Breaker", originalName="Shade-Breaker", tier=3, stats=new string[]{"Wit"}, upgrade=85, handed=1, ranged=0},
		new Item(78){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.DAGGER, seriesName="Dagger", dataName="Widow's Warning", originalName="Widow's Warning", tier=3, stats=new string[]{"Wit"}, upgrade=85, handed=1, ranged=0},
		new Item(79){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.DAGGER, seriesName="Dagger", dataName="Worm's Tooth", originalName="Worm's Tooth", tier=4, stats=new string[]{"Wit"}, upgrade=0, handed=1, ranged=0},
		new Item(80){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.GREAT_BOW, seriesName="Great Bow", dataName="Great Bow", originalName="Great Bow", tier=1, stats=new string[]{"Agility"}, upgrade=32, handed=2, ranged=1},
		new Item(81){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.GREAT_BOW, seriesName="Great Bow", dataName="Great Hunting Bow", originalName="Great Hunting Bow", tier=2, stats=new string[]{"Agility"}, upgrade=67, handed=2, ranged=1},
		new Item(82){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.GREAT_BOW, seriesName="Great Bow", dataName="Mirkwood Great Bow", originalName="Mirkwood Great Bow", tier=2, stats=new string[]{"Agility"}, upgrade=67, handed=2, ranged=1},
		new Item(83){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.GREAT_BOW, seriesName="Great Bow", dataName="Mourning Song", originalName="Mourning Song", tier=3, stats=new string[]{"Agility"}, upgrade=99, handed=2, ranged=1},
		new Item(84){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.GREAT_BOW, seriesName="Great Bow", dataName="Silver-Fall", originalName="Silver-Fall", tier=3, stats=new string[]{"Agility"}, upgrade=99, handed=2, ranged=1},
		new Item(85){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.GREAT_BOW, seriesName="Great Bow", dataName="Bolt-Thrower", originalName="Bolt-Thrower", tier=4, stats=new string[]{"Agility"}, upgrade=0, handed=2, ranged=1},
		new Item(86){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.HAMMER, seriesName="Hammer", dataName="Hammer", originalName="Hammer", tier=1, stats=new string[]{"Might"}, upgrade=32, handed=2, ranged=0},
		new Item(87){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.HAMMER, seriesName="Hammer", dataName="Hollowbold Hammer", originalName="Hollowbold Hammer", tier=2, stats=new string[]{"Might"}, upgrade=67, handed=2, ranged=0},
		new Item(88){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.HAMMER, seriesName="Hammer", dataName="Mountain-Fall", originalName="Mountain-Fall", tier=3, stats=new string[]{"Might"}, upgrade=99, handed=2, ranged=0},
		new Item(89){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.HAMMER, seriesName="Hammer", dataName="Sleeping Bell", originalName="Sleeping Bell", tier=3, stats=new string[]{"Might"}, upgrade=99, handed=2, ranged=0},
		new Item(90){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.HAMMER, seriesName="Hammer", dataName="Bone-Grinder", originalName="Bone-Grinder", tier=4, stats=new string[]{"Might"}, upgrade=0, handed=2, ranged=0},
		new Item(91){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.HATCHET, seriesName="Hatchet", dataName="Hatchet", originalName="Hatchet", tier=1, stats=new string[]{"Might", "Agility"}, upgrade=22, handed=1, ranged=0},
		new Item(92){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.HATCHET, seriesName="Hatchet", dataName="Wanderer's Hatchet", originalName="Wanderer's Hatchet", tier=2, stats=new string[]{"Might", "Agility"}, upgrade=54, handed=1, ranged=2},
		new Item(93){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.HATCHET, seriesName="Hatchet", dataName="Drake-Tooth", originalName="Drake-Tooth", tier=3, stats=new string[]{"Might", "Agility"}, upgrade=89, handed=1, ranged=1},
		new Item(94){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.HATCHET, seriesName="Hatchet", dataName="Throat-Seeker", originalName="Throat-Seeker", tier=3, stats=new string[]{"Might", "Agility"}, upgrade=89, handed=1, ranged=1},
		new Item(95){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.HATCHET, seriesName="Hatchet", dataName="Hunger", originalName="Hunger", tier=4, stats=new string[]{"Might", "Agility"}, upgrade=0, handed=1, ranged=1},
		new Item(96){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.KNIFE, seriesName="Knife", dataName="Knife", originalName="Knife", tier=1, stats=new string[]{"Agility"}, upgrade=22, handed=1, ranged=0},
		new Item(97){collection=4, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.KNIFE, seriesName="Knife", dataName="Knife", originalName="Knife", tier=1, stats=new string[]{"Agility"}, upgrade=22, handed=1, ranged=0},
		new Item(98){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.KNIFE, seriesName="Knife", dataName="Hunting Knife", originalName="Hunting Knife", tier=2, stats=new string[]{"Agility"}, upgrade=49, handed=1, ranged=0},
		new Item(99){collection=4, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.KNIFE, seriesName="Knife", dataName="Lindish Knife", originalName="Lindish Knife", tier=2, stats=new string[]{"Agility"}, upgrade=49, handed=1, ranged=0},
		new Item(100){collection=4, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.KNIFE, seriesName="Knife", dataName="Lassemaica", originalName="Lassemaica", tier=3, stats=new string[]{"Agility"}, upgrade=80, handed=1, ranged=0},
		new Item(101){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.KNIFE, seriesName="Knife", dataName="Skinner", originalName="Skinner", tier=3, stats=new string[]{"Agility"}, upgrade=80, handed=1, ranged=0},
		new Item(102){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.KNIFE, seriesName="Knife", dataName="Star-Swift", originalName="Star-Swift", tier=3, stats=new string[]{"Agility"}, upgrade=80, handed=1, ranged=0},
		new Item(103){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.KNIFE, seriesName="Knife", dataName="Wicked Smile", originalName="Wicked Smile", tier=4, stats=new string[]{"Agility"}, upgrade=0, handed=1, ranged=0},
		new Item(104){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.MACE, seriesName="Mace", dataName="Mace", originalName="Mace", tier=1, stats=new string[]{"Might"}, upgrade=24, handed=1, ranged=0},
		new Item(105){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.MACE, seriesName="Mace", dataName="Deft Mace", originalName="Deft Mace", tier=2, stats=new string[]{"Might", "Wit"}, upgrade=51, handed=1, ranged=0},
		new Item(106){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.MACE, seriesName="Mace", dataName="Bolger's Pride", originalName="Bolger's Pride", tier=3, stats=new string[]{"Might", "Wit"}, upgrade=82, handed=1, ranged=0},
		new Item(107){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.MACE, seriesName="Mace", dataName="Hobbler", originalName="Hobbler", tier=3, stats=new string[]{"Might", "Wit"}, upgrade=82, handed=1, ranged=0},
		new Item(108){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.MACE, seriesName="Mace", dataName="World-Root", originalName="World-Root", tier=4, stats=new string[]{"Might", "Wit"}, upgrade=0, handed=1, ranged=0},
		new Item(109){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.RENDING_CLAWS, seriesName="Rending Claws", dataName="Rending Claws", originalName="Rending Claws", tier=0, stats=new string[]{"Might"}, upgrade=0, handed=2, ranged=0},
		new Item(110){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SHORT_BOW, seriesName="Short Bow", dataName="Short Bow", originalName="Short Bow", tier=1, stats=new string[]{"Wit"}, upgrade=30, handed=2, ranged=1},
		new Item(111){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SHORT_BOW, seriesName="Short Bow", dataName="Cavalry Bow", originalName="Cavalry Bow", tier=2, stats=new string[]{"Wit"}, upgrade=64, handed=2, ranged=1},
		new Item(112){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SHORT_BOW, seriesName="Short Bow", dataName="Foe-Blood", originalName="Foe-Blood", tier=3, stats=new string[]{"Wit", "Agility"}, upgrade=95, handed=2, ranged=1},
		new Item(113){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SHORT_BOW, seriesName="Short Bow", dataName="Rain-of-Stars", originalName="Rain-of-Stars", tier=3, stats=new string[]{"Wit", "Spirit"}, upgrade=95, handed=2, ranged=1},
		new Item(114){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SHORT_BOW, seriesName="Short Bow", dataName="Needle-Threader", originalName="Needle-Threader", tier=4, stats=new string[]{"Wit", "Agility", "Spirit"}, upgrade=0, handed=2, ranged=1},
		new Item(115){collection=6, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SHORT_SWORD, seriesName="Short Sword", dataName="Short Sword", originalName="Short Sword", tier=1, stats=new string[]{"Spirit"}, upgrade=32, handed=1, ranged=0},
		new Item(116){collection=6, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SHORT_SWORD, seriesName="Short Sword", dataName="Eredain Short Sword", originalName="Eredain Short Sword", tier=2, stats=new string[]{"Spirit"}, upgrade=67, handed=1, ranged=0},
		new Item(117){collection=6, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SHORT_SWORD, seriesName="Short Sword", dataName="Dancer on the Wind", originalName="Dancer on the Wind", tier=3, stats=new string[]{"Spirit", "Wit"}, upgrade=90, handed=1, ranged=0},
		new Item(118){collection=6, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SHORT_SWORD, seriesName="Short Sword", dataName="Revenant", originalName="Revenant", tier=3, stats=new string[]{"Spirit", "Might"}, upgrade=90, handed=1, ranged=0},
		new Item(119){collection=6, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SHORT_SWORD, seriesName="Short Sword", dataName="Scour", originalName="Scour", tier=4, stats=new string[]{"Spirit", "Might", "Wit"}, upgrade=0, handed=1, ranged=0},
		new Item(120){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SLING, seriesName="Sling", dataName="Sling", originalName="Sling", tier=1, stats=new string[]{"Wit", "Agility"}, upgrade=25, handed=1, ranged=1},
		new Item(121){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SLING, seriesName="Sling", dataName="Leathern Sling", originalName="Leathern Sling", tier=2, stats=new string[]{"Wit", "Agility"}, upgrade=45, handed=1, ranged=1},
		new Item(122){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SLING, seriesName="Sling", dataName="Giantsbane", originalName="Giantsbane", tier=3, stats=new string[]{"Wit", "Agility"}, upgrade=85, handed=1, ranged=1},
		new Item(123){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SLING, seriesName="Sling", dataName="Whistler", originalName="Whistler", tier=3, stats=new string[]{"Wit", "Agility"}, upgrade=85, handed=1, ranged=1},
		new Item(124){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SLING, seriesName="Sling", dataName="Shooting Star", originalName="Shooting Star", tier=4, stats=new string[]{"Wit", "Agility"}, upgrade=0, handed=1, ranged=0},
		new Item(125){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SPEAR, seriesName="Spear", dataName="Spear", originalName="Spear", tier=1, stats=new string[]{"Spirit"}, upgrade=29, handed=2, ranged=0},
		new Item(126){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SPEAR, seriesName="Spear", dataName="Spear", originalName="Spear", tier=1, stats=new string[]{"Spirit"}, upgrade=29, handed=2, ranged=0},
		new Item(127){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SPEAR, seriesName="Spear", dataName="Long Spear", originalName="Long Spear", tier=2, stats=new string[]{"Spirit", "Might"}, upgrade=68, handed=2, ranged=0},
		new Item(128){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SPEAR, seriesName="Spear", dataName="Rohirric Spear", originalName="Rohirric Spear", tier=2, stats=new string[]{"Spirit", "Agility"}, upgrade=68, handed=2, ranged=2},
		new Item(129){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SPEAR, seriesName="Spear", dataName="Bough of the White Tree", originalName="Bough of the White Tree", tier=3, stats=new string[]{"Spirit", "Might"}, upgrade=98, handed=2, ranged=0},
		new Item(130){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SPEAR, seriesName="Spear", dataName="Dancing Steel", originalName="Dancing Steel", tier=3, stats=new string[]{"Spirit", "Agility"}, upgrade=98, handed=2, ranged=0},
		new Item(131){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SPEAR, seriesName="Spear", dataName="Faengwyr", originalName="Faengwyr", tier=3, stats=new string[]{"Spirit", "Agility"}, upgrade=98, handed=2, ranged=1},
		new Item(132){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SPEAR, seriesName="Spear", dataName="Isenhild", originalName="Isenhild", tier=4, stats=new string[]{"Spirit", "Might", "Agility"}, upgrade=0, handed=2, ranged=1},
		new Item(133){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.STAFF, seriesName="Staff", dataName="Staff", originalName="Staff", tier=1, stats=new string[]{"Agility"}, upgrade=28, handed=2, ranged=0},
		new Item(134){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.STAFF, seriesName="Staff", dataName="Staff", originalName="Staff", tier=1, stats=new string[]{"Agility"}, upgrade=28, handed=2, ranged=0},
		new Item(135){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.STAFF, seriesName="Staff", dataName="Lone-Land Staff", originalName="Lone-Land Staff", tier=2, stats=new string[]{"Agility", "Wisdom"}, upgrade=60, handed=2, ranged=0},
		new Item(136){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.STAFF, seriesName="Staff", dataName="Staff of the Flame", originalName="Staff of the Flame", tier=2, stats=new string[]{"Agility", "Spirit"}, upgrade=64, handed=2, ranged=0},
		new Item(137){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.STAFF, seriesName="Staff", dataName="Ent-Crook", originalName="Ent-Crook", tier=3, stats=new string[]{"Agility", "Wisdom"}, upgrade=91, handed=2, ranged=0},
		new Item(138){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.STAFF, seriesName="Staff", dataName="Maiden-Wrath", originalName="Maiden-Wrath", tier=3, stats=new string[]{"Agility"}, upgrade=91, handed=2, ranged=0},
		new Item(139){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.STAFF, seriesName="Staff", dataName="Maranwe", originalName="Maranwe", tier=3, stats=new string[]{"Agility", "Wisdom", "Spirit"}, upgrade=91, handed=2, ranged=0},
		new Item(140){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.STAFF, seriesName="Staff", dataName="World's Burden", originalName="World's Burden", tier=4, stats=new string[]{"Agility", "Wisdom"}, upgrade=0, handed=2, ranged=0},
		new Item(141){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Sword", originalName="Sword", tier=1, stats=new string[]{"Might"}, upgrade=27, handed=1, ranged=0},
		new Item(142){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Sword", originalName="Sword", tier=1, stats=new string[]{"Might"}, upgrade=27, handed=1, ranged=0},
		new Item(143){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Sword", originalName="Sword", tier=1, stats=new string[]{"Might"}, upgrade=27, handed=1, ranged=0},
		new Item(144){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Duelling Sword", originalName="Duelling Sword", tier=2, stats=new string[]{"Might", "Wit"}, upgrade=58, handed=1, ranged=0},
		new Item(145){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Elf-Forged Sword", originalName="Elf-Forged Sword", tier=2, stats=new string[]{"Might", "Wisdom"}, upgrade=58, handed=1, ranged=0},
		new Item(146){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Northern Blade", originalName="Northern Blade", tier=2, stats=new string[]{"Might"}, upgrade=58, handed=1, ranged=0},
		new Item(147){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Numenorean Sword", originalName="Numenorean Sword", tier=2, stats=new string[]{"Might"}, upgrade=58, handed=1, ranged=0},
		new Item(148){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Fate-Bender", originalName="Fate-Bender", tier=3, stats=new string[]{"Might", "Wisdom"}, upgrade=90, handed=1, ranged=0},
		new Item(149){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Iron-Render", originalName="Iron-Render", tier=3, stats=new string[]{"Might", "Wit"}, upgrade=90, handed=1, ranged=0},
		new Item(150){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Moon-Caller", originalName="Moon-Caller", tier=3, stats=new string[]{"Might", "Spirit"}, upgrade=90, handed=1, ranged=0},
		new Item(151){collection=1, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Sun-Silver", originalName="Sun-Silver", tier=3, stats=new string[]{"Might"}, upgrade=90, handed=1, ranged=0},
		new Item(152){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.SWORD, seriesName="Sword", dataName="Will-Carver", originalName="Will-Carver", tier=4, stats=new string[]{"Might", "Wisdom"}, upgrade=0, handed=1, ranged=0},
		new Item(153){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.WALKING_STICK, seriesName="Walking Stick", dataName="Walking Stick", originalName="Walking Stick", tier=1, stats=new string[]{"Wisdom"}, upgrade=26, handed=1, ranged=0},
		new Item(154){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.WALKING_STICK, seriesName="Walking Stick", dataName="Trusted Walking Stick", originalName="Trusted Walking Stick", tier=2, stats=new string[]{"Wisdom"}, upgrade=55, handed=1, ranged=0},
		new Item(155){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.WALKING_STICK, seriesName="Walking Stick", dataName="Constant Companion", originalName="Constant Companion", tier=3, stats=new string[]{"Wisdom"}, upgrade=85, handed=1, ranged=0},
		new Item(156){collection=3, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.WALKING_STICK, seriesName="Walking Stick", dataName="Quick-Nick", originalName="Quick-Nick", tier=3, stats=new string[]{"Wisdom"}, upgrade=85, handed=1, ranged=0},
		new Item(157){collection=5, slotId=Slot.HAND, slot="Weapon", seriesId=ItemSeries.WALKING_STICK, seriesName="Walking Stick", dataName="Cloud-Chaser", originalName="Cloud-Chaser", tier=4, stats=new string[]{"Wisdom"}, upgrade=0, handed=1, ranged=0},
		new Item(158){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.BOOTS, seriesName="Boots", dataName="Boots", originalName="Boots", tier=1, stats=new string[]{}, upgrade=22, tokens=2, ranged=0},
		new Item(159){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.BOOTS, seriesName="Boots", dataName="Greenway Boots", originalName="Greenway Boots", tier=2, stats=new string[]{}, upgrade=48, tokens=2, ranged=0},
		new Item(160){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.BOOTS, seriesName="Boots", dataName="Dusk Treaders", originalName="Dusk Treaders", tier=3, stats=new string[]{}, upgrade=80, tokens=3, ranged=0},
		new Item(161){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.BROOCH, seriesName="Brooch", dataName="Brooch", originalName="Brooch", tier=1, stats=new string[]{}, upgrade=26, tokens=2, ranged=0},
		new Item(162){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.BROOCH, seriesName="Brooch", dataName="Eriador Brooch", originalName="Eriador Brooch", tier=2, stats=new string[]{}, upgrade=54, tokens=2, ranged=0},
		new Item(163){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.BROOCH, seriesName="Brooch", dataName="Mark of Arnor", originalName="Mark of Arnor", tier=3, stats=new string[]{}, upgrade=82, tokens=3, ranged=0},
		new Item(164){collection=4, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.CIRCLET, seriesName="Circlet", dataName="Circlet", originalName="Circlet", tier=1, stats=new string[]{}, upgrade=32, tokens=1, ranged=0},
		new Item(165){collection=4, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.CIRCLET, seriesName="Circlet", dataName="Silver Circlet", originalName="Silver Circlet", tier=2, stats=new string[]{}, upgrade=56, tokens=2, ranged=0},
		new Item(166){collection=4, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.CIRCLET, seriesName="Circlet", dataName="Seven-Flame Crown", originalName="Seven-Flame Crown", tier=3, stats=new string[]{}, upgrade=99, tokens=2, ranged=0},
		new Item(167){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.EXTRA_RATIONS, seriesName="Extra Rations", dataName="Extra Rations", originalName="Extra Rations", tier=1, trait="Food", upgrade=24, tokens=2, ranged=0},
		new Item(168){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.EXTRA_RATIONS, seriesName="Extra Rations", dataName="Breeland Rations", originalName="Breeland Rations", tier=2, trait="Food", upgrade=48, tokens=2, ranged=0},
		new Item(169){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.EXTRA_RATIONS, seriesName="Extra Rations", dataName="Hobbit Rations", originalName="Hobbit Rations", tier=2, trait="Food", upgrade=48, tokens=4, ranged=0},
		new Item(170){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.EXTRA_RATIONS, seriesName="Extra Rations", dataName="Butterbur Biscuits", originalName="Butterbur Biscuits", tier=3, trait="Food", upgrade=78, tokens=3, ranged=0},
		new Item(171){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.EXTRA_RATIONS, seriesName="Extra Rations", dataName="Tookish Apple Cakes", originalName="Tookish Apple Cakes", tier=3, trait="Food", upgrade=78, tokens=4, ranged=0},
		new Item(172){collection=2, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.FANG_PENDANT, seriesName="Fang Pendant", dataName="Fang Pendant", originalName="Fang Pendant", tier=1, stats=new string[]{"Agility"}, upgrade=30, tokens=1, ranged=0},
		new Item(173){collection=2, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.FANG_PENDANT, seriesName="Fang Pendant", dataName="Warg-Fang Pendant", originalName="Warg-Fang Pendant", tier=2, stats=new string[]{"Agility"}, upgrade=65, tokens=2, ranged=0},
		new Item(174){collection=2, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.FANG_PENDANT, seriesName="Fang Pendant", dataName="Vengeance's Bite", originalName="Vengeance's Bite", tier=3, stats=new string[]{"Agility", "Wit"}, upgrade=80, tokens=2, ranged=0},
		new Item(175){collection=3, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.HAMMER_AND_TONGS, seriesName="Hammer and Tongs", dataName="Hammer and Tongs", originalName="Hammer and Tongs", tier=0, stats=new string[]{}, upgrade=0, tokens=3, ranged=0},
		new Item(176){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.HANDKERCHIEF, seriesName="Handkerchief", dataName="Handkerchief", originalName="Handkerchief", tier=1, stats=new string[]{}, upgrade=20, tokens=1, ranged=0},
		new Item(177){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.HANDKERCHIEF, seriesName="Handkerchief", dataName="Heirloom Handkerchief", originalName="Heirloom Handkerchief", tier=2, stats=new string[]{}, upgrade=50, tokens=2, ranged=0},
		new Item(178){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.HANDKERCHIEF, seriesName="Handkerchief", dataName="Forget-Me-Never", originalName="Forget-Me-Never", tier=3, stats=new string[]{}, upgrade=85, tokens=2, ranged=0},
		new Item(179){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.HELMET, seriesName="Helmet", dataName="Helmet", originalName="Helmet", tier=1, stats=new string[]{}, upgrade=23, tokens=2, ranged=0},
		new Item(180){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.HELMET, seriesName="Helmet", dataName="Dwarf-Forged Helmet", originalName="Dwarf-Forged Helmet", tier=2, stats=new string[]{}, upgrade=52, tokens=4, ranged=0},
		new Item(181){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.HELMET, seriesName="Helmet", dataName="Fire-Scale", originalName="Fire-Scale", tier=3, stats=new string[]{}, upgrade=80, tokens=4, ranged=0},
		new Item(182){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.OLD_MAP, seriesName="Old Map", dataName="Old Map", originalName="Old Map", tier=1, stats=new string[]{}, upgrade=21, tokens=3, ranged=0},
		new Item(183){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.OLD_MAP, seriesName="Old Map", dataName="Bounder's Map", originalName="Bounder's Map", tier=2, stats=new string[]{}, upgrade=64, tokens=3, ranged=0},
		new Item(184){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.OLD_MAP, seriesName="Old Map", dataName="Bullroarer's Course", originalName="Bullroarer's Course", tier=3, stats=new string[]{}, upgrade=88, tokens=3, ranged=0},
		new Item(185){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.OLD_PIPE, seriesName="Old Pipe", dataName="Old Pipe", originalName="Old Pipe", tier=1, stats=new string[]{}, upgrade=25, tokens=2, ranged=0},
		new Item(186){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.OLD_PIPE, seriesName="Old Pipe", dataName="Long-Stemmed Pipe", originalName="Long-Stemmed Pipe", tier=2, stats=new string[]{}, upgrade=65, tokens=2, ranged=0},
		new Item(187){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.OLD_PIPE, seriesName="Old Pipe", dataName="Storm-Maker", originalName="Storm-Maker", tier=3, stats=new string[]{}, upgrade=90, tokens=2, ranged=0},
		new Item(188){collection=2, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.OLD_SCEPTER, seriesName="Old Scepter", dataName="Old Scepter", originalName="Old Scepter", tier=1, stats=new string[]{}, upgrade=15, tokens=2, ranged=0},
		new Item(189){collection=2, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.OLD_SCEPTER, seriesName="Old Scepter", dataName="Silver Scepter", originalName="Silver Scepter", tier=2, stats=new string[]{}, upgrade=30, tokens=3, ranged=0},
		new Item(190){collection=2, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.OLD_SCEPTER, seriesName="Old Scepter", dataName="The Silent Scepter", originalName="The Silent Scepter", tier=3, stats=new string[]{}, upgrade=65, tokens=4, ranged=0},
		new Item(191){collection=5, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.PROVISIONS, seriesName="Provisions", dataName="Provisions", originalName="Provisions", tier=1, trait="Food", upgrade=26, tokens=2, ranged=0},
		new Item(192){collection=5, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.PROVISIONS, seriesName="Provisions", dataName="Wayfarer's Provisions", originalName="Wayfarer's Provisions", tier=2, trait="Food", upgrade=55, tokens=2, ranged=0},
		new Item(193){collection=5, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.PROVISIONS, seriesName="Provisions", dataName="Horselord Supply", originalName="Horselord Supply", tier=3, trait="Food", upgrade=81, tokens=3, ranged=0},
		new Item(194){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.ROPE, seriesName="Rope", dataName="Rope", originalName="Rope", tier=1, stats=new string[]{}, upgrade=28, tokens=2, ranged=0},
		new Item(195){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.ROPE, seriesName="Rope", dataName="Hobson Rope", originalName="Hobson Rope", tier=2, stats=new string[]{}, upgrade=70, tokens=2, ranged=0},
		new Item(196){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.ROPE, seriesName="Rope", dataName="Ninnyhammer Braid", originalName="Ninnyhammer Braid", tier=3, stats=new string[]{}, upgrade=96, tokens=2, ranged=0},
		new Item(197){collection=4, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.THE_CROWN_OF_SHADOWS, seriesName="The Crown of Shadows", dataName="The Crown of Shadows", originalName="The Crown of Shadows", tier=0, stats=new string[]{}, upgrade=0, tokens=1, ranged=0},
		new Item(198){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.TOME, seriesName="Tome", dataName="Tome", originalName="Tome", tier=1, stats=new string[]{}, upgrade=27, tokens=2, ranged=0},
		new Item(199){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.TOME, seriesName="Tome", dataName="Tome of Battle", originalName="Tome of Battle", tier=2, stats=new string[]{}, upgrade=59, tokens=3, ranged=0},
		new Item(200){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.TOME, seriesName="Tome", dataName="Legendarium of Thror", originalName="Legendarium of Thror", tier=3, stats=new string[]{}, upgrade=92, tokens=2, ranged=0},
		new Item(201){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.TORCH, seriesName="Torch", dataName="Torch", originalName="Torch", tier=1, stats=new string[]{}, upgrade=31, tokens=1, ranged=0},
		new Item(202){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.TORCH, seriesName="Torch", dataName="Weathertop Torch", originalName="Weathertop Torch", tier=2, stats=new string[]{}, upgrade=64, tokens=2, ranged=0},
		new Item(203){collection=1, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.TORCH, seriesName="Torch", dataName="Under-Sun", originalName="Under-Sun", tier=3, stats=new string[]{}, upgrade=86, tokens=2, ranged=0},
		new Item(204){collection=5, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.WATERSKIN, seriesName="Waterskin", dataName="Waterskin", originalName="Waterskin", tier=1, stats=new string[]{}, upgrade=23, tokens=1, ranged=0},
		new Item(205){collection=5, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.WATERSKIN, seriesName="Waterskin", dataName="Elven Waterskin", originalName="Elven Waterskin", tier=2, stats=new string[]{}, upgrade=49, tokens=2, ranged=0},
		new Item(206){collection=5, slotId=Slot.TRINKET, slot="Trinket", seriesId=ItemSeries.WATERSKIN, seriesName="Waterskin", dataName="Clear-Water", originalName="Clear-Water", tier=3, stats=new string[]{}, upgrade=73, tokens=3, ranged=0},
		new Item(207){collection=5, slotId=Slot.MOUNT, slot="Mount", seriesId=ItemSeries.FRIENDLY_PONY, seriesName="Friendly Pony", dataName="Friendly Pony", originalName="Friendly Pony", tier=0, trait="Creature", upgrade=0, handed=0, ranged=0},
		new Item(208){collection=5, slotId=Slot.MOUNT, slot="Mount", seriesId=ItemSeries.GRUMBLE_BUM, seriesName="Grumble Bum", dataName="Grumble Bum", originalName="Grumble Bum", tier=0, stats=new string[]{"Might"}, trait="Creature", upgrade=0, handed=0, ranged=0},
		new Item(209){collection=5, slotId=Slot.MOUNT, slot="Mount", seriesId=ItemSeries.MEADOW_HART, seriesName="Meadow Hart", dataName="Meadow Hart", originalName="Meadow Hart", tier=0, stats=new string[]{"Agility", "Spirit"}, trait="Creature", upgrade=0, handed=0, ranged=0},
		new Item(210){collection=5, slotId=Slot.MOUNT, slot="Mount", seriesId=ItemSeries.PACK_MULE, seriesName="Pack Mule", dataName="Pack Mule", originalName="Pack Mule", tier=0, trait="Creature", upgrade=0, handed=0, ranged=0},
		new Item(211){collection=5, slotId=Slot.MOUNT, slot="Mount", seriesId=ItemSeries.QUICKBEAM, seriesName="Quickbeam", dataName="Quickbeam", originalName="Quickbeam", tier=0, stats=new string[]{"Wild"}, upgrade=0, handed=0, ranged=0},
		new Item(212){collection=5, slotId=Slot.MOUNT, slot="Mount", seriesId=ItemSeries.SNOWBRIGHT, seriesName="Snowbright", dataName="Snowbright", originalName="Snowbright", tier=-1, trait="Creature", upgrade=0, handed=0, ranged=0},
		new Item(213){collection=5, slotId=Slot.MOUNT, slot="Mount", seriesId=ItemSeries.SWIFT_STEED, seriesName="Swift Steed", dataName="Swift Steed", originalName="Swift Steed", tier=0, trait="Creature", upgrade=0, handed=0, ranged=0},
		new Item(214){collection=5, slotId=Slot.MOUNT, slot="Mount", seriesId=ItemSeries.TRAVELLERS_HORSE, seriesName="Traveller's Horse", dataName="Traveller's Horse", originalName="Traveller's Horse", tier=0, trait="Creature", upgrade=0, handed=0, ranged=0},
		new Item(215){collection=5, slotId=Slot.MOUNT, slot="Mount", seriesId=ItemSeries.WAR_CHARGER, seriesName="War Charger", dataName="War Charger", originalName="War Charger", tier=0, stats=new string[]{"Might", "Wisdom"}, trait="Creature", upgrade=0, handed=0, ranged=0},
		new Item(216){collection=5, slotId=Slot.MOUNT, slot="Mount", seriesId=ItemSeries.WITNESS_OF_MARANWE, seriesName="Witness of Maranwe", dataName="Witness of Maranwe", originalName="Witness of Maranwe", tier=0, stats=new string[]{}, upgrade=0, handed=0, ranged=0}
	};
}
