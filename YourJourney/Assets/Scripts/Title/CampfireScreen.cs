using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LanguageManager;

public enum CampfireState { SETUP, VIEW, UPGRADE };

public class CampfireScreen : MonoBehaviour
{
	public CampaignScreen campaignScreen;
	public Image finalFader;
	public Button continueButton, backButton, saveButton, clearTrinketsButton, clearMountsButton;
	public TextMeshProUGUI nameText, remainingPointsNumberText, stateText;
	public Button[] heroButtons;
	public Image[] heroImage;
	public Text[] heroCorruption;
	public Image[] heroCorruptionImage;
	public Image maleSilhouette;
	public Image femaleSilhouette;
	public Image portraitBackground;
	public float heroImageWidth;
	public float portraitMaxWidth;
	private float portraitExtraWidth = 0f;
	public Button[] skillButtons;
	public TextTranslation[] skillTextTranslations;
	public TextMeshProUGUI[] skillCostText;
	//public TextTranslation remainingPointsTextTranslation, skillsTranslationText, roleItemsTranslationText;
	public TextTranslation stateTextTranslation, roleTextTranslation, armorTextTranslation, hand1TextTranslation, hand2TextTranslation, trinketTextTranslation, mountTextTranslation;
	public GameObject skillsHeading, skillPointCartouche, roleCartouche, armorCartouche, hand1Cartouche, hand2Cartouche, trinketCartouche, mountCartouche;
	public TMP_Dropdown skillDropdown, roleDropdown, armorDropdown, hand1Dropdown, hand2Dropdown, trinketDropdown, mountDropdown, titleDropdown;
	List<RoleData> roleList = new List<RoleData>();
	List<Item> armorList = new List<Item>();
	List<Item> hand1List = new List<Item>();
	List<Item> hand2List = new List<Item>();
	List<Item> trinketList = new List<Item>();
	List<Item> mountList = new List<Item>();

	TitleManager tm;
	TitleMetaData titleMetaData;
	CampaignState campaignState;
	List<CharacterSheet> startingCharacterSheets; //loaded from campaignState
	List<CharacterSheet> characterSheets; //as updated on the campfire screen
	List<int> startingTrinkets; //loaded from campaignState
	List<int> campfireTrinkets; //as updated on the campfire screen
	List<int> startingMounts; //loaded from campaignState
	List<int> campfireMounts; //as updated on the campfire screen

	List<string> startingCampaignStateTriggers; //loaded from campaignState
	List<string> campaignTriggerState; //as updated behind the scenes on the campfire screen

	CampfireState campfireState;

	SkillRecord currentSkillRecord;
	int currentRemainingXP;
	int selectedHero = 0;
	int maxHanded = 0;
	int hand1Handed = 0;
	int hand2Handed = 0;
	int currentLore = 0;

	public void ActivateScreen( TitleMetaData metaData, CampfireState campfireState = CampfireState.VIEW )
	{
		titleMetaData = metaData;
		this.campfireState = campfireState;

		campaignState = metaData.campaignState;
		LoadCharacterSheets();
		//characterSheets[0].AddTitle(14); //TODO Remove after testing titles

		stateTextTranslation.Change("campfire.title." + campfireState.ToString(), campfireState.ToString());

		portraitExtraWidth = portraitMaxWidth - (heroImageWidth * SelectHeroes.maxHeroes);

		tm = FindObjectOfType<TitleManager>();

		//Turn Your Journey game title off since we need the screen real estate
		tm.gameTitle.SetActive(false);
		tm.gameTitleFlash.SetActive(false);

		//Calculate currentLore if needed, by adding the final lore of each scenario prior to the one being played
		if(campfireState == CampfireState.UPGRADE)
        {
			currentLore = 0;
			for(int i=0; i<campaignState.scenarioPlayingIndex; i++)
            {
				currentLore += campaignState.scenarioLore[i];
			}
        }

		//Show/Hide Dropdowns or Cartouches based on CampfireState
		if (campfireState == CampfireState.SETUP || campfireState == CampfireState.UPGRADE)
		{
			ActivateDropdowns(true);
			ActivateCartouches(false);
			saveButton.gameObject.SetActive(true);
		}
		else if (campfireState == CampfireState.VIEW)
		{
			ActivateDropdowns(false);
			ActivateCartouches(true);
			saveButton.gameObject.SetActive(false);
		}
		//Save button also inactive on Replays
		if(campaignState.scenarioPlayingIndex != campaignState.currentScenarioIndex)
        {
			saveButton.gameObject.SetActive(false);
        }

		SetImages();
		OnHeroSelect(0);

		//Show/Hide Skill Selection Form based on CampfireState
		if (campfireState == CampfireState.SETUP)
        {
			ActivateSkillForm(false, false);
        }
		else if(campfireState == CampfireState.UPGRADE)
        {
			ActivateSkillForm(true, true);
			OnSkillDropdownSelect();
		}
		else if(campfireState == CampfireState.VIEW)
        {
			ActivateSkillForm(true, false);
			OnSkillDropdownSelect();
		}

		gameObject.SetActive( true );

		finalFader.DOFade( 0, .5f ).OnComplete( () =>
		{
			backButton.interactable = true;
		} );
	}

	public void StartGame()
    {
		gameObject.SetActive(false); //hide the SpecialInstructions form but leave scenarioOverlay with coverImage showing
		TitleManager tm = FindObjectOfType<TitleManager>();
		tm.LoadScenario();
	}

	public void LoadCharacterSheets()
    {
		//We also load the campaignTriggerState here because it's stored in the same place and follows basically the same pattern as the characterSheets, trinkets, and mounts.

		if (campfireState == CampfireState.SETUP)
		{
			//Start with the starting state of the first scenario
			startingCharacterSheets = campaignState.startingCharacterSheets[0];
			startingTrinkets = campaignState.startingTrinkets[0];
			startingMounts = campaignState.startingMounts[0];
			startingCampaignStateTriggers = campaignState.startingCampaignTriggerState[0];
		}
		else if (campfireState == CampfireState.VIEW)
		{
			//Use the current state of the scenario being played right now (continue or replay)
			startingCharacterSheets = campaignState.currentCharacterSheets[campaignState.scenarioPlayingIndex];
			startingTrinkets = campaignState.currentTrinkets[campaignState.scenarioPlayingIndex];
			startingMounts = campaignState.currentMounts[campaignState.scenarioPlayingIndex];
			startingCampaignStateTriggers = campaignState.currentCampaignTriggerState[campaignState.scenarioPlayingIndex];
		}
		else if (campfireState == CampfireState.UPGRADE)
		{
			int previousIndex = campaignState.scenarioPlayingIndex - 1;
			if (previousIndex < 0)
			{
				//Start with the starting state of the first scenario
				startingCharacterSheets = campaignState.startingCharacterSheets[0];
				startingTrinkets = campaignState.startingTrinkets[0];
				startingMounts = campaignState.startingMounts[0];
				startingCampaignStateTriggers = campaignState.startingCampaignTriggerState[0];
			}
			else
			{
				//Start with the final state of the previous scenario
				startingCharacterSheets = campaignState.currentCharacterSheets[previousIndex];
				startingTrinkets = campaignState.currentTrinkets[previousIndex];
				startingMounts = campaignState.currentMounts[previousIndex];
				startingCampaignStateTriggers = campaignState.currentCampaignTriggerState[previousIndex];
			}
		}

		if (campaignState.currentCharactersSaved[campaignState.scenarioPlayingIndex])
		{
			//Player used the Save button to save their character sheet state before starting the adventure. They then left the game and came back.
			//Load their saved data as the currentCharacterSheets. The startingCharacterSheets should still be the end of the last scenario so their
			////previously selected item tier can still be shown in the list for reselection of the previous tier, or reselection of the upgrade tier.
			characterSheets = campaignState.startingCharacterSheets[campaignState.scenarioPlayingIndex];
			campfireTrinkets = campaignState.startingTrinkets[campaignState.scenarioPlayingIndex];
			campfireMounts = campaignState.startingMounts[campaignState.scenarioPlayingIndex];
			campaignTriggerState = campaignState.startingCampaignTriggerState[campaignState.scenarioPlayingIndex];
		}
		else
		{
			characterSheets = CampaignState.CloneCharacterSheetList(startingCharacterSheets);
			campfireTrinkets = new List<int>(startingTrinkets);
			campfireMounts = new List<int>(startingMounts);
			campaignTriggerState = new List<string>(startingCampaignStateTriggers);
		}
	}

	public void ActivateDropdowns(bool active)
	{
		roleDropdown.gameObject.SetActive(active);
		armorDropdown.gameObject.SetActive(active);
		hand1Dropdown.gameObject.SetActive(active);
		hand2Dropdown.gameObject.SetActive(active);
		trinketDropdown.gameObject.SetActive(active);
		mountDropdown.gameObject.SetActive(active);

		clearTrinketsButton.gameObject.SetActive(active);
		clearMountsButton.gameObject.SetActive(active);
	}
	public void ActivateCartouches(bool active)
	{
		roleCartouche.gameObject.SetActive(active);
		armorCartouche.gameObject.SetActive(active);
		hand1Cartouche.gameObject.SetActive(active);
		hand2Cartouche.gameObject.SetActive(active);
		trinketCartouche.gameObject.SetActive(active);
		mountCartouche.gameObject.SetActive(active);
	}

	public void ActivateSkillForm(bool active, bool interact)
    {
		skillsHeading.gameObject.SetActive(active);
		skillPointCartouche.gameObject.SetActive(active);
		skillDropdown.gameObject.SetActive(active);

		foreach (var skillButton in skillButtons)
        {
			skillButton.gameObject.SetActive(active);
			skillButton.interactable = interact;
        }
    }

	public void SetImages()
	{
		for (int i = 0; i < SelectHeroes.maxHeroes; i++)
		{
			if (i < characterSheets.Count)
			{
				heroImage[i].gameObject.SetActive(true);
				heroButtons[i].gameObject.SetActive(true);
				heroImage[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Portraits/p" + campaignState.heroesIndex[i]);

				int corruption = startingCharacterSheets[i].corruption;
				if (corruption > 0)
				{
					heroCorruptionImage[i].gameObject.SetActive(true);
					heroCorruption[i].text = corruption.ToString();
				}
                else 
				{
					heroCorruptionImage[i].gameObject.SetActive(false);
				}
			}
			else
            {
				heroImage[i].gameObject.SetActive(false);
				heroButtons[i].gameObject.SetActive(false);
                heroCorruptionImage[i].gameObject.SetActive(false);
			}
		}

		//Set the size of the background image behind the hero images so it fits however many heroes we have
		float portraitBackgroundWidth = portraitExtraWidth + (heroImageWidth * characterSheets.Count);
		portraitBackground.rectTransform.sizeDelta = new Vector2(portraitBackgroundWidth, portraitBackground.rectTransform.sizeDelta.y);
	}

	public void SetSilhouette(int index)
    {
		int j = campaignState.heroesIndex[index];
		Hero hero = Heroes.list[j];
		if(hero?.sex == Sex.FEMALE)
        {
			maleSilhouette.gameObject.SetActive(false);
			femaleSilhouette.gameObject.SetActive(true);
        }
		else
        {
			maleSilhouette.gameObject.SetActive(true);
			femaleSilhouette.gameObject.SetActive(false);
		}
    }

	public string AppendCollectionSpacing(Collection collection, string collectionText)
	{
		if (collection == Collection.NONE) { collectionText = "    "; }
		else if (collection == Collection.SPREADING_WAR) { collectionText += " "; }
		return collectionText;
	}


	public void PopulateRoleCartouche()
    {
		RoleData role = Roles.FromRole(characterSheets[selectedHero].role);
		Collection collection = Collection.FromID(role.collection);
		string collectionText = AppendCollectionSpacing(collection, "<font=\"Icon\">" + collection.FontCharacter + "</font>");
		string key = role.dataName;
		roleTextTranslation.Change("role." + key, "{0}" + key, new List<string> {collectionText});
    }

	public void PopulateItemCartouche(TextTranslation textTranslation, Slot slot, int hand = 0)
    {
		Item item = null;
		if (slot == Slot.ARMOR)
		{
			item = Items.FromID(characterSheets[selectedHero].armorId);
		}
		else if(slot == Slot.HAND && hand == 1)
        {
			item = Items.FromID(characterSheets[selectedHero].hand1Id);
		}
		else if (slot == Slot.HAND && hand == 2)
		{
			item = Items.FromID(characterSheets[selectedHero].hand2Id);
		}
		else if (slot == Slot.TRINKET)
		{
			item = Items.FromID(characterSheets[selectedHero].trinketId);
		}
		else if (slot == Slot.MOUNT)
		{
			item = Items.FromID(characterSheets[selectedHero].mountId);
		}
		if(item == null) { item = Items.list[0]; } //None
		if (item != null)
		{
			string key = "item." + item.slot + "." + item.seriesId.ToString() + "." + item.tier + "." + item.id;
			Collection collection = Collection.FromID(item.collection);
			string collectionText = AppendCollectionSpacing(collection, "<font=\"Icon\">" + collection.FontCharacter + "</font>");
			textTranslation.Change(key, "{0}" + item.dataName, new List<string> { collectionText });
		}
	}

	public void PopulateRoleDropdown(TMP_Dropdown dropdown, List<RoleData> roleList)
	{
		//populate dropdown
		List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();

		int selectedIndex = 0;

		List<RoleData> availableRoles = Roles.list.Where(it => it.role != Role.NONE && Roles.RoleAvailable(it.role, characterSheets, selectedHero)).ToList();
		roleList.Clear();
		roleList.AddRange(availableRoles);

		int i = 0;
		foreach (var roleData in roleList)
		{
			Collection collection = Collection.FromID(roleData.collection);
			string collectionText = AppendCollectionSpacing(collection, "<font=\"Icon\">" + collection.FontCharacter + "</font>");
			var dropdownOption = new TMP_Dropdown.OptionData(collectionText + Translate("role." + roleData.dataName, roleData.dataName));
			optionList.Add(dropdownOption);
			if(roleData.role == characterSheets[selectedHero].role)
            {
				selectedIndex = i;
            }
			i++;
		}
		dropdown.ClearOptions();
		dropdown.AddOptions(optionList);
		dropdown.SetValueWithoutNotify(selectedIndex);
	}

	public void PopulateTitleDropdown()
    {
		//populate dropdown
		List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();
		string collectionText = "    "; //TODO Add collection icons
		foreach(int titleId in characterSheets[selectedHero].titles)
        {
			var dropdownOption = new TMP_Dropdown.OptionData(collectionText + Translate("title." + titleId, "Title " + titleId.ToString()));
			optionList.Add(dropdownOption);
		}
		if(optionList.Count == 0)
        {
			var dropdownOption = new TMP_Dropdown.OptionData(collectionText + Translate("title.None", "None"));
			optionList.Add(dropdownOption);
		}
		titleDropdown.ClearOptions();
		titleDropdown.AddOptions(optionList);
		titleDropdown.SetValueWithoutNotify(0);
	}

	public void PopulateItemDropdown(TMP_Dropdown dropdown, List<Item> itemList, Slot slot)
	{
		PopulateItemDropdown(dropdown, itemList, slot, 0, 0);
	}

	public void PopulateItemDropdown(TMP_Dropdown dropdown, List<Item> itemList, Slot slot, int hand)
	{
		PopulateItemDropdown(dropdown, itemList, slot, hand, 1);
	}

	public void PopulateItemDropdown(TMP_Dropdown dropdown, List<Item> itemList, Slot slot, int hand, int tier)
	{
		if (campfireState == CampfireState.SETUP)
		{
			PopulateItemSetupDropdown(dropdown, itemList, slot, hand, tier);
		}
		else if (campfireState == CampfireState.UPGRADE)
		{
			PopulateItemUpgradeDropdown(dropdown, itemList, slot, hand, currentLore);
		}
	}

	public void PopulateItemSetupDropdown(TMP_Dropdown dropdown, List<Item> itemList, Slot slot, int hand, int tier)
    {
		//populate dropdown
		List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();

		int selectedIndex = 0;
		itemList.Clear();

		//Allow None to be selected for HAND 2, TRINKET, and MOUNT, but not for ARMOR or HAND 1
		if (slot == Slot.TRINKET || slot == Slot.MOUNT || (slot == Slot.HAND && hand == 2))
		{
			itemList.Add(Items.list[0]); //None
		}

		//Special treatment for Snow horse. Add it to the list for characters who can have it.
		ItemSeries mountSeries = Heroes.FromID(characterSheets[selectedHero].portraitIndex).mount;
		if (slot == Slot.MOUNT && mountSeries != ItemSeries.NONE)
        {
            itemList.Add(Items.FromSeriesID(mountSeries)[0]);
        }

		int handedLimit = 2;
		if(hand == 1) { handedLimit = maxHanded - hand2Handed; }
		else if(hand == 2) { handedLimit = maxHanded - hand1Handed; }

		if (handedLimit == 0)
		{
			//No ability to wield another item. Halflings can only wield one-handed. Everyone but the default Shieldmaiden hero is limited to two-handed. The default Shieldmaiden hero can wield one one-handed and one two-handed; or two one-handed.
		}
		else if (slot == Slot.ARMOR || slot == Slot.HAND ||
				(slot == Slot.TRINKET && campaignState.campaign.startWithTrinkets == true) ||
				(slot == Slot.MOUNT && campaignState.campaign.startWithMounts == true))
		{
			List<Item> availableItems = Items.list.Where(item =>
				item.slotId == slot &&
				item.tier == tier &&
				item.handed <= handedLimit &&
				Items.ItemAvailable(item.id, characterSheets, selectedHero, hand)
			).ToList();
			itemList.AddRange(availableItems);
		}

		int i = 0;
		foreach (var item in itemList)
		{
			Collection collection = Collection.FromID(item.collection);
			string collectionText = AppendCollectionSpacing(collection, "<font=\"Icon\">" + collection.FontCharacter + "</font>");
			string handsText = "";
			if(item.handed == 1) { handsText = " <font=\"Icon\">O</font>"; }
			else if(item.handed == 2) { handsText = " <font=\"Icon\">T</font>"; }
			var dropdownOption = new TMP_Dropdown.OptionData(collectionText + Translate("item." + item.seriesName + "." + item.tier + "." + item.dataName, item.dataName) + handsText);
			optionList.Add(dropdownOption);
			if (
				(slot == Slot.ARMOR && item.id == characterSheets[selectedHero].armorId) ||
				(slot == Slot.HAND && hand==1 && item.id == characterSheets[selectedHero].hand1Id) ||
				(slot == Slot.HAND && hand == 2 && item.id == characterSheets[selectedHero].hand2Id) ||
				(slot == Slot.TRINKET && item.id == characterSheets[selectedHero].trinketId) ||
				(slot == Slot.MOUNT && item.id == characterSheets[selectedHero].mountId)
			)
			{
				selectedIndex = i;
			}
			i++;
		}
		dropdown.ClearOptions();
		dropdown.AddOptions(optionList);
		dropdown.SetValueWithoutNotify(selectedIndex);

		//Do this because of heroes that don't have default armor and hand1 items set from the Heroes.list, e.g. the Friend of Beasts portrait and all the extra portraits that don't correspond to physical character sheets.
		//Make sure to explicitly set an item by default for every slot in the character's inventory. It will be the role default, or the first in the list, which could include None for some slots (HAND2, TRINKET, MOUNT)
		if (itemList.Count > 0)
		{
			Item selectedItem = itemList[selectedIndex];
			if (slot == Slot.ARMOR) { characterSheets[selectedHero].armorId = selectedItem.id; }
			else if (slot == Slot.HAND && hand == 1) { characterSheets[selectedHero].hand1Id = selectedItem.id; }
			else if (slot == Slot.HAND && hand == 2) { characterSheets[selectedHero].hand2Id = selectedItem.id; }
			else if (slot == Slot.TRINKET)
			{
				characterSheets[selectedHero].trinketId = selectedItem.id;
			}
			else if (slot == Slot.MOUNT)
			{
				characterSheets[selectedHero].mountId = selectedItem.id;
			}
		}
	}

	public void PopulateItemUpgradeDropdown(TMP_Dropdown dropdown, List<Item> itemList, Slot slot, int hand, int lore)
	{
		//There are different rules for:
		//1. Armor/Hand1/Hand2: these can be upgraded, and some items (e.g. Cloak) can be owned by multiple heroes
		//2. Trinkets: these can be upgraded, but a Trinket from the same Series can only be owned by one hero. Heroes can choose None, or switch Trinkets at the campfire.
		//3. Mounts: these cannot be upgraded. Heroes can choose None or switch Mounts at the Campfire. There's a special Snow horse that one particular character can always choose.

		//populate dropdown with the current item and any available upgrades
		List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();

		int selectedIndex = 0;

		itemList.Clear();

		//Allow None selection for Trinket and Mount
		if(slot == Slot.TRINKET || slot == Slot.MOUNT)
        {
			itemList.Add(Items.list[0]); //None
        }

		Item startingItem = null;
		Item currentItem = null;
		if (slot == Slot.ARMOR)
		{
			startingItem = Items.FromID(startingCharacterSheets[selectedHero].armorId);
			currentItem = Items.FromID(characterSheets[selectedHero].armorId);
		}
		else if(slot == Slot.HAND && hand == 1)
        {
			startingItem = Items.FromID(startingCharacterSheets[selectedHero].hand1Id);
			currentItem = Items.FromID(characterSheets[selectedHero].hand1Id);
		}
		else if (slot == Slot.HAND && hand == 2)
		{
			startingItem = Items.FromID(startingCharacterSheets[selectedHero].hand2Id);
			currentItem = Items.FromID(characterSheets[selectedHero].hand2Id);
		}

		if(slot == Slot.TRINKET)
		{
			//Debug.Log("startingTrinkets: " + String.Join(", ", startingTrinkets));

			//Show all trinkets at the current tier and all upgradeable trinkets
			List<Item> availableTrinkets = startingTrinkets
				.ConvertAll(it => Items.FromID(it))
				.Where(item => Items.TrinketSeriesAvailable(item.seriesId, characterSheets, selectedHero))
				.ToList();

			//Debug.Log("availableTrinkets: " + String.Join(", ", availableTrinkets.ConvertAll(it => it.id + "/" + it.seriesName + "/" + it.tier)));
			
			foreach(var startingTrinket in availableTrinkets)
            {
				Item currentTrinket = Items.FromID(characterSheets[selectedHero].trinketId);
				itemList.AddRange(AvailableUpgrades(startingTrinket, currentTrinket, slot, hand));
			}
		}
		else if (slot == Slot.MOUNT)
		{
			//Special treatment for Snow horse. Add it to the list for characters who can have it.
			ItemSeries mountSeries = Heroes.FromID(characterSheets[selectedHero].portraitIndex).mount;
			if (slot == Slot.MOUNT && mountSeries != ItemSeries.NONE)
			{
				itemList.Add(Items.FromSeriesID(mountSeries)[0]);
			}

			List<Item> availableMounts = startingMounts
				.ConvertAll(it => Items.FromID(it))
				.Where(item => 
					item.tier == 0 && //The Snow horse is set to tier -1 so this excludes it from players that aren't granted access explicitly
					Items.ItemAvailable(item.id, characterSheets, selectedHero, hand))
				.ToList();
			itemList.AddRange(availableMounts);
		}
		else //ARMOR, HAND 1, HAND 2
		{
			//Debug.Log("AvailableUpgrades(starting: " + startingItem.seriesName + " " + startingItem.tier + ", current: " + currentItem.seriesName + " " + currentItem.tier + ", " + slot.ToString() + ", " + hand + ")");
			itemList.AddRange(AvailableUpgrades(startingItem, currentItem, slot, hand));
		}

		//If there is no item, show None in the dropdown
		if (itemList.Count == 0)
        {
			itemList.Add(Items.list[0]); //None
		}

		int i = 0;
		foreach (var item in itemList)
		{
			Collection collection = Collection.FromID(item.collection);
			string collectionText = AppendCollectionSpacing(collection, "<font=\"Icon\">" + collection.FontCharacter + "</font>");
			string tierText = "";
			if(item.tier == 1) { tierText = " I"; }
			else if (item.tier == 2) { tierText = " II"; }
			else if (item.tier == 3) { tierText = " III"; }
			else if (item.tier == 4) { tierText = " IV"; }

			var dropdownOption = new TMP_Dropdown.OptionData(collectionText + Translate("item." + item.seriesName + "." + item.tier + "." + item.dataName, item.dataName) + tierText);
			optionList.Add(dropdownOption);
			if (
				(slot == Slot.ARMOR && item.id == characterSheets[selectedHero].armorId) ||
				(slot == Slot.HAND && hand == 1 && item.id == characterSheets[selectedHero].hand1Id) ||
				(slot == Slot.HAND && hand == 2 && item.id == characterSheets[selectedHero].hand2Id) ||
				(slot == Slot.TRINKET && item.id == characterSheets[selectedHero].trinketId) ||
				(slot == Slot.MOUNT && item.id == characterSheets[selectedHero].mountId)
			)
			{
				selectedIndex = i;
			}
			i++;
		}
		dropdown.ClearOptions();
		dropdown.AddOptions(optionList);
		dropdown.SetValueWithoutNotify(selectedIndex);
	}

	public List<Item> AvailableUpgrades(Item startingItem, Item currentItem, Slot slot, int hand)
    {
		//All the stuff about tier 4 is because of the rule that a given hero can only upgrade one item to tier 4. So once they've selected one tier 4 item, all the other
		//item dropdowns should hide the tier 4 upgrades. But the dropdown that has the hero's one tier 4 item should still show that tier 4 item, as well as its previous
		//tier 3 item in case the hero wants to reselect the tier 3 before they continue into the adventure.

		int upgradeCost = 0;
		int startingTier = 1;
		bool hasTier4Item =
			Items.FromID(characterSheets[selectedHero].armorId).tier == 4 ||
			Items.FromID(characterSheets[selectedHero].hand1Id).tier == 4 ||
			Items.FromID(characterSheets[selectedHero].hand2Id).tier == 4;

		List<Item> itemList = new List<Item>();
		if (startingItem != null)
		{
			itemList.Add(startingItem);
			upgradeCost = startingItem.upgrade;
			//Get the upgrade cost for each tier so we can present the right ones to the player. 
			int upgradeCost2 = Items.CostToUpgradeTo(startingItem.seriesId, 2);
			int upgradeCost3 = Items.CostToUpgradeTo(startingItem.seriesId, 3);
			int upgradeCost4 = Items.CostToUpgradeTo(startingItem.seriesId, 4);
			startingTier = startingItem.tier;

			//Find items in the next tier if the player has enough lore to upgrade, and respecting the only-one-tier-4 rule. (upgradeCost==0 indicates there are no higher upgrades available).
			//Debug.Log("startingTier: " + startingTier + " hasTier4Item: " + hasTier4Item + " currentItem.tier: " + currentItem.tier + " upgradeCost: " + upgradeCost + " currentLore: " + currentLore + " upgradeCost: " + upgradeCost);
			if ((startingTier < 3 || !hasTier4Item || currentItem.tier == 4) && upgradeCost > 0 && currentLore >= upgradeCost)
			{
				//Debug.Log("Do check for availableUpgrades");
				List<Item> availableUpgrades = Items.list.Where(item =>
					item.slotId == slot &&
					item.seriesId == startingItem.seriesId &&
					item.tier > startingTier &&
					(
						(item.tier == 2 && currentLore > upgradeCost2) ||
						(item.tier == 3 && currentLore > upgradeCost3) ||
						(item.tier == 4 && currentLore > upgradeCost4 && (!hasTier4Item || currentItem.tier == 4))
					) &&
					Items.ItemAvailable(item.id, characterSheets, selectedHero, hand)
				).ToList();
				itemList.AddRange(availableUpgrades);
			}
		}
		//Debug.Log("AvailableUpgrades: " + String.Join(", ", itemList.ConvertAll(it => it.seriesName + " " + it.tier)));
		return itemList;
	}

	public void PopulateSkillDropdown()
    {
		//populate dropdown
		List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();

		int selectedIndex = 0;

		foreach (var skillRecord in characterSheets[selectedHero].skillRecords)
		{
			RoleData roleData = Roles.FromRole(skillRecord.role);

			var dropdownOption = new TMP_Dropdown.OptionData(Translate("role." + roleData.dataName, roleData.dataName));
			optionList.Add(dropdownOption);
		}
		skillDropdown.ClearOptions();
		skillDropdown.AddOptions(optionList);
		skillDropdown.SetValueWithoutNotify(selectedIndex);
	}

	public void PopulateSkillButtons()
    {
		SkillRecord skillRecord = currentSkillRecord;
		//Calculate remaining XP and set the reaminingPointsNumberText indicator
		int remainingXP = skillRecord.xp;
		RoleData role = Roles.FromRole(skillRecord.role);
		for (int i = 0; i < skillButtons.Length; i++)
		{
			int offsetIndex = i + role.indexOffset;
			bool egoOwns = SkillOwnedByCurrentHero(role, offsetIndex);
			if(egoOwns)
            {
				remainingXP -= role.skillCost[offsetIndex];
            }
		}
		remainingPointsNumberText.text = remainingXP.ToString();
		currentRemainingXP = remainingXP;

		//Populate buttons
		for(int i=0; i<skillButtons.Length; i++)
        {
			int offsetIndex = i + role.indexOffset;

			Button button = skillButtons[i];

			bool isAvailable = SkillAvailable(role, offsetIndex);
			bool egoOwns = SkillOwnedByCurrentHero(role, offsetIndex);

			//Hide buttons if needed
			button.gameObject.SetActive(true);
			if (offsetIndex >= role.skillCount)
			{
				button.gameObject.SetActive(false);
				continue;
			}
			/*
			else if(!isAvailable && !egoOwns)
            {
				button.gameObject.SetActive(false);
            }
			*/

			//Button interactable?
			if (campfireState != CampfireState.VIEW && (egoOwns || (isAvailable && role.skillCost[offsetIndex] <= remainingXP)))
			{
				button.interactable = true;
			}
			else
            {
				button.interactable = false;
            }

			//Set button text
			int skillIndex = offsetIndex + 1;
			skillTextTranslations[i].Change("skill." + role.dataName + "." + skillIndex, Skills.SkillFromRoleID(skillRecord.role, skillIndex).dataName);

            //Set point cost
            skillCostText[i].text = role.skillCost[offsetIndex].ToString();


			//Set button visibility in VIEW mode if the hero has that skill
			if (campfireState == CampfireState.VIEW)
            {
				if (egoOwns)
				{
					button.gameObject.SetActive(true);
				}
				else
                {
					button.gameObject.SetActive(false);
				}
			}


			//Set color
			if (egoOwns)
			{
				//Yellow for selected
				button.GetComponent<Image>().color = new Color(255f / 255f, 255f / 255f, 0, 255f / 255f);
			}
			else if (!egoOwns && isAvailable)
            {
				//White color for selectable
				button.GetComponent<Image>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
			}
			else if ((!egoOwns && !isAvailable) || role.skillCost[offsetIndex] > remainingXP)
			{
				//Red for not selectable (owned by someone else or not enough XP)
				button.GetComponent<Image>().color = new Color(255f / 255f, 0, 0, 255f / 255f);
			}
		}
	}

	public bool SkillAvailable(RoleData role, int skillIndex)
	{
		for (int i = 0; i < characterSheets.Count; i++)
		{
			CharacterSheet character = characterSheets[i];
			SkillRecord skillRecord = character.skillRecords.FirstOrDefault(it => it.role == role.role);
			if(skillRecord == null) { continue; }

			bool foundSkill = skillRecord.selectedSkillIndex.Contains(skillIndex);
            if (foundSkill) { return false; }
		}
		return true;
	}

	public bool SkillOwnedByCurrentHero(RoleData role, int skillIndex)
	{
		CharacterSheet character = characterSheets[selectedHero];
		SkillRecord skillRecord = character.skillRecords.FirstOrDefault(it => it.role == role.role);
		if (skillRecord == null) { return false; }

		bool foundSkill = skillRecord.selectedSkillIndex.Contains(skillIndex);
		if (foundSkill) { return true; }

		return false;
	}


	public void OnSkillDropdownSelect()
    {
		int index = skillDropdown.GetComponent<TMP_Dropdown>().value;
		if(index < 0 || index >= characterSheets[selectedHero].skillRecords.Count) { return; }
		currentSkillRecord = characterSheets[selectedHero].skillRecords[index];

		PopulateSkillButtons();
    }

	public void OnSkillButtonSelect(int buttonIndex)
    {
		Role role = currentSkillRecord.role;
		RoleData roleData = Roles.FromRole(role);
		int skillIndex = buttonIndex + roleData.indexOffset;
		bool isAvailable = SkillAvailable(roleData, skillIndex);
		bool egoOwns = SkillOwnedByCurrentHero(roleData, skillIndex);
		if (isAvailable && roleData.skillCost[skillIndex] <= currentRemainingXP)
		{
			currentSkillRecord.selectedSkillIndex.Add(skillIndex);
		}
		else if(egoOwns)
        {
			currentSkillRecord.selectedSkillIndex.Remove(skillIndex);
        }
		PopulateSkillButtons();
    }

	public void OnRoleSelect()
    {
		int index = roleDropdown.GetComponent<TMP_Dropdown>().value;
		characterSheets[selectedHero].role = roleList[index].role;
	}

	public void OnItemSelect(string slotHand)
    {
		bool previousItemWasTier4 = false;
		Item item = null;
		if(slotHand == "armor")
        {
			previousItemWasTier4 = (Items.FromID(characterSheets[selectedHero].armorId).tier == 4);
			int index = armorDropdown.GetComponent<TMP_Dropdown>().value;
			item = armorList[index];
			characterSheets[selectedHero].armorId = item.id;
			if(campfireState == CampfireState.UPGRADE) { PopulateItemDropdown(armorDropdown, armorList, Slot.ARMOR); }
		}
		else if(slotHand == "hand1")
        {
			previousItemWasTier4 = (Items.FromID(characterSheets[selectedHero].hand1Id).tier == 4);
			int index = hand1Dropdown.GetComponent<TMP_Dropdown>().value;
			item = hand1List[index];
			characterSheets[selectedHero].hand1Id = item.id;
			hand1Handed = item.handed;
			if (campfireState == CampfireState.UPGRADE) { PopulateItemDropdown(hand1Dropdown, hand1List, Slot.HAND, 1); }
			PopulateItemDropdown(hand2Dropdown, hand2List, Slot.HAND, 2, 1); //Repopulate to hide/reveal one- or two- handed weapons in hand2 based on hand1 selection
		}
		else if (slotHand == "hand2")
		{
			previousItemWasTier4 = (Items.FromID(characterSheets[selectedHero].hand2Id).tier == 4);
			int index = hand2Dropdown.GetComponent<TMP_Dropdown>().value;
			item = hand2List[index];
			characterSheets[selectedHero].hand2Id = item.id;
			hand2Handed = item.handed;
			if (campfireState == CampfireState.UPGRADE) { PopulateItemDropdown(hand2Dropdown, hand2List, Slot.HAND, 2); }
			PopulateItemDropdown(hand1Dropdown, hand1List, Slot.HAND, 1, 1); //Repopulate to hide/reveal one- or two- handed weapons in hand1 based on hand2 selection
		}
		else if (slotHand == "trinket")
		{
			int index = trinketDropdown.GetComponent<TMP_Dropdown>().value;
			item = trinketList[index];
			characterSheets[selectedHero].trinketId = item.id;
			if (campfireState == CampfireState.UPGRADE) { PopulateItemDropdown(trinketDropdown, trinketList, Slot.TRINKET); }
		}
		else if (slotHand == "mount")
		{
			int index = mountDropdown.GetComponent<TMP_Dropdown>().value;
			item = mountList[index];
			characterSheets[selectedHero].mountId = item.id;
		}

		if(item.tier == 4 || previousItemWasTier4)
        {
			//Repopulate armor to update the tier-4 availability.
			PopulateItemDropdown(armorDropdown, armorList, Slot.ARMOR);

			//Hand1 and Hand2 already handled Hand2 and Hand1, so only repopulate them when Armor changes.
			if (slotHand == "armor")
            {
				PopulateItemDropdown(hand1Dropdown, hand1List, Slot.HAND, 1);
				PopulateItemDropdown(hand2Dropdown, hand2List, Slot.HAND, 2);
			}
		}
	}

	public void OnHeroSelect(int index)
	{
		if (index >= characterSheets.Count) { return; }

		ResetHeroes();

		ColorBlock cb = heroButtons[index].colors;
		heroButtons[index].colors = new ColorBlock()
		{
			normalColor = new Color(255f / 255f, 167f / 255f, 124f / 255f, 128f / 255f),
			pressedColor = cb.pressedColor,
			selectedColor = cb.selectedColor,
			colorMultiplier = cb.colorMultiplier,
			disabledColor = cb.disabledColor,
			fadeDuration = cb.fadeDuration,
			highlightedColor = cb.highlightedColor
		};

		selectedHero = index;
		nameText.text = campaignState.heroes[index];
		SetSilhouette(index);

		maxHanded = characterSheets[selectedHero].maxHanded;
		hand1Handed = Items.FromID(characterSheets[selectedHero].hand1Id).handed;
		hand2Handed = Items.FromID(characterSheets[selectedHero].hand2Id).handed;

		if (campfireState == CampfireState.SETUP || campfireState == CampfireState.UPGRADE)
		{
			//Hide Hand 2 dropdown if character has maxHanded == 1, e.g. Halflings
			if (maxHanded == 1)
			{
				hand2Dropdown.gameObject.SetActive(false);
			}
			else
			{
				hand2Dropdown.gameObject.SetActive(true);
			}


			PopulateRoleDropdown(roleDropdown, roleList);

			PopulateItemDropdown(armorDropdown, armorList, Slot.ARMOR, 0, 1);
			PopulateItemDropdown(hand1Dropdown, hand1List, Slot.HAND, 1, 1);
			PopulateItemDropdown(hand2Dropdown, hand2List, Slot.HAND, 2, 1);
			PopulateItemDropdown(trinketDropdown, trinketList, Slot.TRINKET, 0, 1);
			PopulateItemDropdown(mountDropdown, mountList, Slot.MOUNT, 0, 0);

			PopulateTitleDropdown();

			if (campfireState == CampfireState.UPGRADE)
			{
				PopulateSkillDropdown();
				OnSkillDropdownSelect();
			}
		}
		else if(campfireState == CampfireState.VIEW)
        {
			PopulateRoleCartouche();
			PopulateItemCartouche(armorTextTranslation, Slot.ARMOR);
			PopulateItemCartouche(hand1TextTranslation, Slot.HAND, 1);
			PopulateItemCartouche(hand2TextTranslation, Slot.HAND, 2);
			PopulateItemCartouche(trinketTextTranslation, Slot.TRINKET);
			PopulateItemCartouche(mountTextTranslation, Slot.MOUNT);

			PopulateTitleDropdown();

			PopulateSkillDropdown();
			OnSkillDropdownSelect();
		}

		//beginButton.interactable = selectedHeroes.Any(b => b);
	}

	void ResetHeroes()
	{
		for (int i = 0; i < characterSheets.Count; i++)
		{
			ColorBlock cb = heroButtons[i].colors;
			heroButtons[i].colors = new ColorBlock()
			{
				normalColor = new Color(1, 1, 1, 0),
				pressedColor = cb.pressedColor,
				selectedColor = cb.selectedColor,
				colorMultiplier = cb.colorMultiplier,
				disabledColor = cb.disabledColor,
				fadeDuration = cb.fadeDuration,
				highlightedColor = cb.highlightedColor
			};
		}
	}

	void UpdatePartyTrinkets()
    {
		if (campfireState == CampfireState.SETUP && campaignState.campaign.startWithTrinkets)
		{
			List<int> trinkets = new List<int>();
			foreach (var characterSheet in characterSheets)
			{
				if (characterSheet.trinketId != 0)
				{
					trinkets.Add(characterSheet.trinketId);
				}
			}
			campfireTrinkets = trinkets;
		}
		else if (campfireState == CampfireState.UPGRADE)
		{
			List<int> trinkets = new List<int>(campfireTrinkets);
			foreach (var characterSheet in characterSheets)
			{
				if (characterSheet.trinketId != 0)
				{
					//Look for other versions (tiers) of the trinket's series and remove them
					Item newTrinket = Items.FromID(characterSheet.trinketId);
					List<int> removalList = new List<int>();
					foreach(int oldTrinketId in trinkets)
                    {
						Item oldTrinket = Items.FromID(oldTrinketId);
						if(oldTrinket.seriesId == newTrinket.seriesId)
                        {
							removalList.Add(oldTrinket.id);
                        }
                    }
					foreach(int removeTrinketId in removalList)
                    {
						trinkets.Remove(removeTrinketId);
                    }

					if (!trinkets.Contains(characterSheet.trinketId))
					{
						//Add the character's current trinket
						trinkets.Add(characterSheet.trinketId);
					}
				}
			}
			campfireTrinkets = trinkets;
		}
	}

	void UpdatePartyMounts()
    {
		if (campfireState == CampfireState.SETUP && campaignState.campaign.startWithMounts)
		{
			List<int> mounts = new List<int>();
			foreach (var characterSheet in characterSheets)
			{
				if (characterSheet.mountId != 0)
				{
					mounts.Add(characterSheet.mountId);
				}
			}
			campfireMounts = mounts;
		}
		//No upgrades for mounts, so no code needed for UPGRADE.
	}

	public void OnClearTrinkets()
    {
		foreach(var characterSheet in characterSheets)
        {
			characterSheet.trinketId = 0;
        }
		PopulateItemDropdown(trinketDropdown, trinketList, Slot.TRINKET);
    }

	public void OnClearMounts()
    {
		foreach (var characterSheet in characterSheets)
		{
			characterSheet.mountId = 0;
		}
		PopulateItemDropdown(mountDropdown, mountList, Slot.MOUNT);
	}

	public void OnBack()
	{
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			tm.gameTitle.SetActive(true);
			tm.gameTitleFlash.SetActive(true);
			campaignScreen.ActivateScreen(titleMetaData);
		} );
	}

	public void OnSave()
    {
		if (campfireState == CampfireState.SETUP)
		{
			//Start with the starting state of the first scenario
			campaignState.startingCharacterSheets[0] = CampaignState.CloneCharacterSheetList(characterSheets);
			campaignState.startingTrinkets[0] = new List<int>(campfireTrinkets);
			campaignState.startingMounts[0] = new List<int>(campfireMounts);
			campaignState.startingCampaignTriggerState[0] = new List<string>(campaignTriggerState);
		}
		else if (campfireState == CampfireState.UPGRADE)
		{
			campaignState.startingCharacterSheets[campaignState.scenarioPlayingIndex] = CampaignState.CloneCharacterSheetList(characterSheets);
			campaignState.startingTrinkets[campaignState.scenarioPlayingIndex] = new List<int>(campfireTrinkets);
			campaignState.startingMounts[campaignState.scenarioPlayingIndex] = new List<int>(campfireMounts);
			campaignState.startingCampaignTriggerState[campaignState.scenarioPlayingIndex] = new List<string>(campaignTriggerState);
		}

		//Set the save flag on this scenario so CampfireScreen will load from this scenario's startCharacterSheets instead of the last scenario's currentCharacterSheets
		campaignState.currentCharactersSaved[campaignState.scenarioPlayingIndex] = true;

		//Save the campaign state that's been updated on this screen
		var gs = GameState.LoadState(campaignState.saveStateIndex);
		gs.SaveCampaignState(campaignState.saveStateIndex, campaignState);
	}

	public void OnNext()
	{
		//Add the SkillRecord for the selected role to each characterSheet
		foreach(var characterSheet in characterSheets)
        {
			if (!characterSheet.skillRecords.Exists(it => it.role == characterSheet.role))
			{
				SkillRecord record = new SkillRecord(characterSheet.role);
				characterSheet.skillRecords.Add(record);
			}
        }

		UpdatePartyTrinkets();
		UpdatePartyMounts();

		//Set the currentCharacterSheets for this scenario index from a copy of the data gathered on this screen
		campaignState.currentCharacterSheets[campaignState.scenarioPlayingIndex] = CampaignState.CloneCharacterSheetList(characterSheets);
		campaignState.currentTrinkets[campaignState.scenarioPlayingIndex] = campfireTrinkets;
		campaignState.currentMounts[campaignState.scenarioPlayingIndex] = campfireMounts;
		campaignState.currentCampaignTriggerState[campaignState.scenarioPlayingIndex] = campaignTriggerState;
		Bootstrap.ResetCorruption();
		for (int i=0; i<Bootstrap.PlayerCount; i++)
        {
			Bootstrap.corruptionCounter[i] = campaignState.currentCharacterSheets[campaignState.scenarioPlayingIndex][i].corruption;
		}

		StartGame();
	}

}
