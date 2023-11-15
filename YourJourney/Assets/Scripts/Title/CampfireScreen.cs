using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CampfireState { SETUP, VIEW, UPGRADE, REPLAY };

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
	//public TextTranslation remainingPointsTextTranslation, skillsTranslationText, roleItemsTranslationText;
	public TextTranslation stateTextTranslation, roleTextTranslation, armorTextTranslation, hand1TextTranslation, hand2TextTranslation, trinketTextTranslation, mountTextTranslation;
	public GameObject skillsHeading, skillPointCartouche, roleCartouche, armorCartouche, hand1Cartouche, hand2Cartouche, trinketCartouche, mountCartouche;
	public TMP_Dropdown skillDropdown, roleDropdown, armorDropdown, hand1Dropdown, hand2Dropdown, trinketDropdown, mountDropdown;
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

	public void ActivateScreen( TitleMetaData metaData, CampfireState campfireState = CampfireState.VIEW )
	{
		titleMetaData = metaData;
		campaignState = metaData.campaignState;
		characterSheets = campaignState.characterSheets;

		this.campfireState = campfireState;
		stateTextTranslation.Change("campfire.title." + campfireState.ToString(), campfireState.ToString());

		portraitExtraWidth = portraitMaxWidth - (heroImageWidth * SelectHeroes.maxHeroes);

		tm = FindObjectOfType<TitleManager>();

		//Turn Your Journey game title off since we need the screen real estate
		tm.gameTitle.SetActive(false);
		tm.gameTitleFlash.SetActive(false);

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

		//Show/Hide Skill Selection Form based on CampfireState
		if(campfireState == CampfireState.SETUP)
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

		SetImages();
		OnHeroSelect(0);

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
		skillDropdown.interactable = interact;

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

	public void PopulateItemDropdown(TMP_Dropdown dropdown, List<Item> itemList, Slot slot, int tier, int hand=0)
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
			//TODO Grab just the first available of each item type?
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

	public void OnRoleSelect()
    {
		int index = roleDropdown.GetComponent<TMP_Dropdown>().value;
		characterSheets[selectedHero].role = roleList[index].role;
	}

	public void OnItemSelect(string slotHand)
    {
		if(slotHand == "armor")
        {
			int index = armorDropdown.GetComponent<TMP_Dropdown>().value;
			characterSheets[selectedHero].armorId = armorList[index].id;
		}
		else if(slotHand == "hand1")
        {
			int index = hand1Dropdown.GetComponent<TMP_Dropdown>().value;
			characterSheets[selectedHero].hand1Id = hand1List[index].id;
			hand1Handed = Items.FromID(hand1List[index].id).handed;
			PopulateItemDropdown(hand2Dropdown, hand2List, Slot.HAND, 1, 2); //Repopulate to hide/reveal one- or two- handed weapons in hand2 based on hand1 selection
		}
		else if (slotHand == "hand2")
		{
			int index = hand2Dropdown.GetComponent<TMP_Dropdown>().value;
			characterSheets[selectedHero].hand2Id = hand2List[index].id;
			hand2Handed = Items.FromID(hand2List[index].id).handed;
			PopulateItemDropdown(hand1Dropdown, hand1List, Slot.HAND, 1, 1); //Repopulate to hide/reveal one- or two- handed weapons in hand1 based on hand2 selection
		}
		else if (slotHand == "trinket")
		{
			int index = trinketDropdown.GetComponent<TMP_Dropdown>().value;
			characterSheets[selectedHero].trinketId = trinketList[index].id;
		}
		else if (slotHand == "mount")
		{
			int index = mountDropdown.GetComponent<TMP_Dropdown>().value;
			characterSheets[selectedHero].mountId = mountList[index].id;
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
		StartGame();
	}

}
