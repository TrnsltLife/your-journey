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
	public Button continueButton, backButton;
	public TextMeshProUGUI nameText, remainingPointsNumberText, stateText;
	public Button[] heroButtons;
	public Image[] heroImage;
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
	List<CharacterSheet> characterSheets;

	CampfireState campfireState;

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
		characterSheets[0].AddTitle(14); //TODO Remove after testing titles

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
		}
		else if (campfireState == CampfireState.VIEW)
		{
			ActivateDropdowns(false);
			ActivateCartouches(true);
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
		}
		else if(campfireState == CampfireState.VIEW)
        {
			ActivateSkillForm(true, false);
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
		if (campfireState == CampfireState.SETUP)
		{
			//Start with the starting state of the first scenario
			characterSheets = campaignState.startingCharacterSheets[0];
		}
		else if (campfireState == CampfireState.VIEW)
		{
			//Use the current state of the scenario being played right now (continue or replay)
			characterSheets = campaignState.currentCharacterSheets[campaignState.scenarioPlayingIndex];
		}
		else if (campfireState == CampfireState.UPGRADE)
		{
			int tempIndex = campaignState.scenarioPlayingIndex - 1;
			if (tempIndex < 0)
			{
				//Start with the starting state of the first scenario
				characterSheets = campaignState.startingCharacterSheets[0];
			}
			else
			{
				//Start with the final state of the previous scenario
				characterSheets = campaignState.currentCharacterSheets[campaignState.scenarioPlayingIndex - 1];
			}
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
			}
			else
            {
				heroImage[i].gameObject.SetActive(false);
				heroButtons[i].gameObject.SetActive(false);
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
		//TODO Translation
		RoleData role = Roles.FromRole(characterSheets[selectedHero].role);
		Collection collection = Collection.FromID(role.collection);
		string collectionText = AppendCollectionSpacing(collection, "<font=\"Icon\">" + collection.FontCharacter + "</font>");
		string key = role.dataName;
		roleTextTranslation.Change("role." + key, "{0}" + key, new List<string> {collectionText});
    }

	public void PopulateItemCartouche(TextTranslation textTranslation, Slot slot, int hand = 0)
    {
		//TODO Translation
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

		List<RoleData> availableRoles = Roles.list.Where(it => Roles.RoleAvailable(it.role, characterSheets, selectedHero)).ToList();
		roleList.Clear();
		roleList.AddRange(availableRoles);

		int i = 0;
		foreach (var roleData in roleList)
		{
			//TODO Translate
			Collection collection = Collection.FromID(roleData.collection);
			string collectionText = AppendCollectionSpacing(collection, "<font=\"Icon\">" + collection.FontCharacter + "</font>");
			var dropdownOption = new TMP_Dropdown.OptionData(collectionText + roleData.dataName);
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

	public void PopulateItemDropdown(TMP_Dropdown dropdown, List<Item> itemList, Slot slot, int tier, int hand = 0)
    {
		if (campfireState == CampfireState.SETUP)
		{
			PopulateItemSetupDropdown(dropdown, itemList, slot, tier, hand);
		}
		else if(campfireState == CampfireState.UPGRADE)
        {
			PopulateItemUpgradeDropdown(dropdown, itemList, slot, currentLore, hand);
		}
	}

	public void PopulateTitleDropdown()
    {
		//populate dropdown
		List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();
		foreach(int titleId in characterSheets[selectedHero].titles)
        {
			var dropdownOption = new TMP_Dropdown.OptionData(Translate("title." + titleId, "Title " + titleId.ToString()));
			optionList.Add(dropdownOption);
		}
		if(optionList.Count == 0)
        {
			var dropdownOption = new TMP_Dropdown.OptionData(Translate("title.None", "None"));
			optionList.Add(dropdownOption);
		}
		titleDropdown.ClearOptions();
		titleDropdown.AddOptions(optionList);
		titleDropdown.SetValueWithoutNotify(0);
	}

	public void PopulateItemSetupDropdown(TMP_Dropdown dropdown, List<Item> itemList, Slot slot, int tier, int hand=0)
    {
		//populate dropdown
		List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();

		int selectedIndex = 0;
		itemList.Clear();
		itemList.Add(Items.list[0]);

		int handedLimit = 2;
		if(hand == 1) { handedLimit = maxHanded - hand2Handed; }
		else if(hand == 2) { handedLimit = maxHanded - hand1Handed; }

		if (handedLimit == 0)
		{
			//No ability to wield another item. Halflings can only wield one-handed. Everyone but shieldmaiden is limited to two-handed. Shieldmaiden can wield one one-handed and one two-handed; or two one-handed.
		}
		else
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
			//TODO Translate
			Collection collection = Collection.FromID(item.collection);
			string collectionText = AppendCollectionSpacing(collection, "<font=\"Icon\">" + collection.FontCharacter + "</font>");
			string handsText = "";
			if(item.handed == 1) { handsText = " <font=\"Icon\">O</font>"; }
			else if(item.handed == 2) { handsText = " <font=\"Icon\">T</font>"; }
			var dropdownOption = new TMP_Dropdown.OptionData(collectionText + item.dataName + handsText);
			optionList.Add(dropdownOption);
			if (
				(slot == Slot.ARMOR && item.id == characterSheets[selectedHero].armorId) ||
				(slot == Slot.HAND && hand==1 && item.id == characterSheets[selectedHero].hand1Id) ||
				(slot == Slot.HAND && hand == 2 && item.id == characterSheets[selectedHero].hand2Id) ||
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

	public void PopulateItemUpgradeDropdown(TMP_Dropdown dropdown, List<Item> itemList, Slot slot, int lore, int hand = 0)
	{
		//populate dropdown with the current item and any available upgrades
		List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();

		int selectedIndex = 0;
		int upgradeCost = 0;
		int currentTier = 1;
		bool hasTier4Item =
			Items.FromID(characterSheets[selectedHero].armorId).tier == 4 ||
			Items.FromID(characterSheets[selectedHero].hand1Id).tier == 4 ||
			Items.FromID(characterSheets[selectedHero].hand2Id).tier == 4;

		itemList.Clear();

		Item currentItem = null;
		if (slot == Slot.ARMOR)
		{
			currentItem = Items.FromID(characterSheets[selectedHero].armorId);
		}
		else if(slot == Slot.HAND && hand == 1)
        {
			currentItem = Items.FromID(characterSheets[selectedHero].hand1Id);
		}
		else if (slot == Slot.HAND && hand == 2)
		{
			currentItem = Items.FromID(characterSheets[selectedHero].hand2Id);
		}
		else if (slot == Slot.MOUNT)
		{
			currentItem = Items.FromID(characterSheets[selectedHero].mountId);
		}

		if (currentItem != null)
		{
			itemList.Add(currentItem);
			upgradeCost = currentItem.upgrade;
			//Get the upgrade cost for each tier so we can present the right ones to the player. 
			int upgradeCost2 = Items.CostToUpgradeTo(currentItem.seriesId, 2);
			int upgradeCost3 = Items.CostToUpgradeTo(currentItem.seriesId, 3);
			int upgradeCost4 = Items.CostToUpgradeTo(currentItem.seriesId, 4);
			currentTier = currentItem.tier;

			//Find items in the next tier if the player has enough lore to upgrade, and respecting the only-one-tier-4 rule. (upgradeCost==0 indicates there are no higher upgrades available).
			if ((currentTier < 3 || !hasTier4Item) && upgradeCost > 0 && currentLore >= upgradeCost)
			{
				List<Item> availableUpgrades = Items.list.Where(item =>
					item.slotId == slot &&
					item.seriesId == currentItem.seriesId &&
					item.tier > currentTier &&
					(
						(item.tier == 2 && currentLore > upgradeCost2) ||
						(item.tier == 3 && currentLore > upgradeCost3) ||
						(item.tier == 4 && currentLore > upgradeCost4 && !hasTier4Item)
					) &&
					Items.ItemAvailable(item.id, characterSheets, selectedHero, hand)
				).ToList();
				itemList.AddRange(availableUpgrades);
			}
		}

		//If there is no item, show None in the dropdown
		if(itemList.Count == 0)
        {
			itemList.Add(Items.list[0]);
		}

		int i = 0;
		foreach (var item in itemList)
		{
			//TODO Translate
			Collection collection = Collection.FromID(item.collection);
			string collectionText = AppendCollectionSpacing(collection, "<font=\"Icon\">" + collection.FontCharacter + "</font>");
			string tierText = "";
			if(item.tier == 1) { tierText = " I"; }
			else if (item.tier == 2) { tierText = " II"; }
			else if (item.tier == 3) { tierText = " III"; }
			else if (item.tier == 4) { tierText = " IV"; }

			var dropdownOption = new TMP_Dropdown.OptionData(collectionText + item.dataName + tierText);
			optionList.Add(dropdownOption);
			if (
				(slot == Slot.ARMOR && item.id == characterSheets[selectedHero].armorId) ||
				(slot == Slot.HAND && hand == 1 && item.id == characterSheets[selectedHero].hand1Id) ||
				(slot == Slot.HAND && hand == 2 && item.id == characterSheets[selectedHero].hand2Id) ||
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

	public void PopulateSkillDropdown()
    {
		//populate dropdown
		List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();

		int selectedIndex = 0;

		foreach (var skillRecord in characterSheets[selectedHero].skillRecords)
		{
			//TODO Translate
			RoleData roleData = Roles.FromRole(skillRecord.role);

			var dropdownOption = new TMP_Dropdown.OptionData(roleData.dataName);
			optionList.Add(dropdownOption);
		}
		skillDropdown.ClearOptions();
		skillDropdown.AddOptions(optionList);
		skillDropdown.SetValueWithoutNotify(selectedIndex);
	}

	public void PopulateSkillButtons(SkillRecord skillRecord)
    {
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
			else if(!isAvailable)
            {
				button.gameObject.SetActive(false);
            }

			//Button interactable?
			if (campfireState != CampfireState.VIEW && (isAvailable || egoOwns))
			{
				button.interactable = true;
			}
			else
            {
				button.interactable = false;
            }

			//Set button text
			int skillIndex = offsetIndex + 1;
			skillTextTranslations[i].Change("skill." + role.dataName + "." + skillIndex, Skills.SkillFromRoleID(skillRecord.role, skillIndex).originalName);

            //Set point cost
            skillCostText[i].text = role.skillCost[offsetIndex].ToString();

			//Set color

			//Grey for selectable
			//Green for selected
			//Red for not selectable (not enough xp, or owned by someone else)
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


	public void OnSkillSelect()
    {
		int index = skillDropdown.GetComponent<TMP_Dropdown>().value;
		Debug.Log("OnSkillSelect index: " + index);
		if(index < 0 || index >= characterSheets[selectedHero].skillRecords.Count) { return; }
		SkillRecord skillRecord = characterSheets[selectedHero].skillRecords[index];
		Debug.Log("skillRecord role:" + skillRecord.role + " xp:" + skillRecord.xp);

		PopulateSkillButtons(skillRecord);
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
		}
		else if(slotHand == "hand1")
        {
			previousItemWasTier4 = (Items.FromID(characterSheets[selectedHero].hand1Id).tier == 4);
			int index = hand1Dropdown.GetComponent<TMP_Dropdown>().value;
			item = hand1List[index];
			characterSheets[selectedHero].hand1Id = item.id;
			hand1Handed = item.handed;
			PopulateItemDropdown(hand2Dropdown, hand2List, Slot.HAND, 1, 2); //Repopulate to hide/reveal one- or two- handed weapons in hand2 based on hand1 selection
		}
		else if (slotHand == "hand2")
		{
			previousItemWasTier4 = (Items.FromID(characterSheets[selectedHero].hand2Id).tier == 4);
			int index = hand2Dropdown.GetComponent<TMP_Dropdown>().value;
			item = hand2List[index];
			characterSheets[selectedHero].hand2Id = item.id;
			hand2Handed = item.handed;
			PopulateItemDropdown(hand1Dropdown, hand1List, Slot.HAND, 1, 1); //Repopulate to hide/reveal one- or two- handed weapons in hand1 based on hand2 selection
		}
		else if (slotHand == "trinket")
		{
			int index = trinketDropdown.GetComponent<TMP_Dropdown>().value;
			item = trinketList[index];
			characterSheets[selectedHero].trinketId = item.id;
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
			PopulateItemDropdown(hand1Dropdown, hand1List, Slot.HAND, 1, 1);

			//Hand1 and Hand2 already handled Hand2 and Hand1.
			if (slotHand == "armor")
            {
				PopulateItemDropdown(hand1Dropdown, hand1List, Slot.HAND, 1, 1);
				PopulateItemDropdown(hand2Dropdown, hand2List, Slot.HAND, 1, 2);
			}
		}
	}

	public void OnHeroSelect(int index)
	{
		Debug.Log("Hero select " + index);

		if (index >= characterSheets.Count) { return; }

		ResetHeroes();

		ColorBlock cb = heroButtons[index].colors;
		heroButtons[index].colors = new ColorBlock()
		{
			normalColor = new Color(0, 255f / 255f, 45f / 255f, 128f / 255f),
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
			PopulateRoleDropdown(roleDropdown, roleList);

			PopulateItemDropdown(armorDropdown, armorList, Slot.ARMOR, 1);
			PopulateItemDropdown(hand1Dropdown, hand1List, Slot.HAND, 1, 1);
			PopulateItemDropdown(hand2Dropdown, hand2List, Slot.HAND, 1, 2);
			PopulateItemDropdown(trinketDropdown, trinketList, Slot.TRINKET, 1);
			PopulateItemDropdown(mountDropdown, mountList, Slot.MOUNT, 0);

			PopulateTitleDropdown();

			if (campfireState == CampfireState.UPGRADE)
			{
				PopulateSkillDropdown();
				OnSkillSelect();
			}
		}
		else if(campfireState == CampfireState.VIEW)
        {
			PopulateRoleCartouche();
			PopulateItemCartouche(armorTextTranslation, Slot.ARMOR);
			PopulateItemCartouche(hand1TextTranslation, Slot.HAND, 1);
			PopulateItemCartouche(hand2TextTranslation, Slot.HAND, 2);
			PopulateItemCartouche(trinketTextTranslation, Slot.TRINKET, 1);
			PopulateItemCartouche(mountTextTranslation, Slot.MOUNT);
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

		//Set the currentCharacterSheets for this scenario index to a copy of the data gathered on this screen
		campaignState.currentCharacterSheets[campaignState.scenarioPlayingIndex] = CampaignState.CloneCharacterSheetList(characterSheets);

		StartGame();
	}

}
