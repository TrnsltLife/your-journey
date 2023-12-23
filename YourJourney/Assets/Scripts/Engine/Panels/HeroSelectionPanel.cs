using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LanguageManager;

public class HeroSelectionPanel : MonoBehaviour
{
	public TextMeshProUGUI mainText, selectionText, dummy;
	public GameObject submitBtn;
	public CanvasGroup overlay;

	CanvasGroup group;
	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action<InteractionResult> buttonActions;
	Transform root;

	public Button[] heroButtons;
	public Image[] heroImage;
	public Image portraitBackground;
	public float heroImageWidth;
	public float portraitMaxWidth;
	private float portraitExtraWidth = 0f;
	int selectedHero = -1;

	ItemInteraction itemInteraction;
	Item giveItem;
	List<Item> giveItems;
	int missingItems;

	TitleInteraction titleInteraction;
	Title giveTitle;
	List<Title> giveTitles;
	int missingTitles;

	int corruption;

	Action<InteractionResult> originalAction;

	private void CalculatePanelPosition()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		gameObject.SetActive(false);
		sp = transform.position;
		ap = rect.anchoredPosition;
	}

	private void Awake()
	{
		CalculatePanelPosition();
		root = transform.parent;
		mainText.alignment = TextAlignmentOptions.Top; //We set this here instead of the editor to make it easier to see mainText and dummy are lined up with each other in the editor
		dummy.alignment = TextAlignmentOptions.Top;
	}

	public void Show(ItemInteraction ii, List<Item> giveItems, int missingItems, Item item, Action<InteractionResult> originalAction, Action<InteractionResult> actions)
    {
		itemInteraction = ii;
		this.giveItem = item;
		this.giveItems = giveItems;
		this.missingItems = missingItems;
		this.originalAction = originalAction;

		Show(actions);
    }

	public void Show(TitleInteraction ti, List<Title> giveTitles, int missingTitles, Title title, Action<InteractionResult> originalAction, Action<InteractionResult> actions)
	{
		titleInteraction = ti;
		this.giveTitle = title;
		this.giveTitles = giveTitles;
		this.missingTitles = missingTitles;
		this.originalAction = originalAction;

		Show(actions);
	}

	public void Show(ItemInteraction ii,int corruption, Action<InteractionResult> originalAction, Action<InteractionResult> actions)
	{
		this.corruption = corruption;
	}

	public void Show(Action<InteractionResult> actions )
	{
		CalculatePanelPosition();
		FindObjectOfType<TileManager>().ToggleInput( true );

		if (giveItem != null)
		{
			SetText(giveItem);
		}
		else if(giveTitle != null)
        {
			SetText(giveTitle);
        }
		else
        {
			SetText(corruption);
        }

		SetImages();

		UpdateSubmitButtonInteractability();

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonActions = actions;

		//rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		//transform.DOMoveY( sp.y, .75f );

		portraitExtraWidth = portraitMaxWidth - (heroImageWidth * SelectHeroes.maxHeroes);

		group.DOFade( 1, .5f );
	}

	public void Hide()
	{
		group.DOFade( 0, .25f );
		overlay.DOFade( 0, .25f ).OnComplete( () =>
		{
			FindObjectOfType<TileManager>().ToggleInput( false );
			Destroy( root.gameObject );
		} );
	}

	void SetImages()
    {
		for (int i = 0; i < SelectHeroes.maxHeroes; i++)
		{
			//heroItems[i].gameObject.SetActive(true);
			//heroNameText.text = Bootstrap.gameStarter.heroes[i];

			if (i < Bootstrap.gameStarter.heroes.Length)
			{
				heroImage[i].gameObject.SetActive(true);
				heroButtons[i].gameObject.SetActive(true);

				//Load portrait image based on the portrait index, e.g p0.png or p63.png
				Sprite portraitSprite = Resources.Load<Sprite>("Images/Portraits/p" + Bootstrap.gameStarter.heroesIndex[i]);
				heroImage[i].GetComponent<Image>().sprite = portraitSprite;
			}
			else
            {
				heroImage[i].gameObject.SetActive(false);
				heroButtons[i].gameObject.SetActive(false);
			}

			//Set the size of the background image behind the hero images so it fits however many heroes we have
			float portraitBackgroundWidth = portraitExtraWidth + (heroImageWidth * Bootstrap.gameStarter.heroes.Length);
			if (portraitBackgroundWidth > portraitMaxWidth) { portraitBackgroundWidth = portraitMaxWidth; }
			portraitBackground.rectTransform.sizeDelta = new Vector2(portraitBackgroundWidth, portraitBackground.rectTransform.sizeDelta.y);
        }
    }

    void SetText(Item item)
    {
        string text = "";
		string text2 = "";

		//TODO Translate
        string icon = "t"; //Start out with "t" for TRINKET
        string iconText = "<font=\"Icon\">" + icon + "</font> ";
        string itemName = Translate("item." + item.seriesName + "." + item.tier + "." + item.dataName, item.dataName);
        text = Translate("heroSelection.text.HeroGainsTrinket", "One hero gains the {0} {1} trinket.", new List<string> { iconText, itemName });
		text2 = Translate("heroSelection.text.ChooseWhoGainsTrinket", "Choose a hero to gain the {0} {1} trinket.", new List<string> { iconText, itemName });
		if (item.slotId == Slot.MOUNT)
        {
            icon = "m";
            text = Translate("heroSelection.text.HeroGainsMount", "One hero gains the {0} {1} mount.", new List<string> { iconText, itemName });
			text2 = Translate("heroSelection.text.ChooseWhoGainsMount", "Choose a hero to gain the {0} {1} mount.", new List<string> { iconText, itemName });
		}

		text += "\n\n";

        if (item.slotId == Slot.TRINKET)
        {
            text += Translate("heroSelection.text.TrinketText", "That hero may equip it immediately even if other trinkets are already equipped. Add the full number of depletion tokens to the trinket.");
        }
        else //MOUNT
        {
            text += Translate("heroSelection.text.MountText", "That hero may equip it immediately even if other mounts are already equipped.");
        }

        mainText.text = text;
        dummy.text = text;

		selectionText.text = text2;

		Scenario.Chronicle(text);
		Scenario.ChroniclePS("\n" + text2);

		float preferredHeight = dummy.preferredHeight; //Dummy text (which must be active) is used to find the correct preferredHeight so it can then be set on the mainText which is in a scroll view viewport
        dummy.text = ""; //After we have the height we clear dummy.text so it doesn't show up anymore

        //TODO Fix later?
        //These were to scale the text box to the size of the text, but they scale the whole panel. Since the portrait background, hero portraits, and select button are in that panel, it was messing up the alignment.
        //var dialogHeight = Math.Min(525, 30 + preferredHeight + 90);
        //rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dialogHeight);
    }

	void SetText(Title title)
	{
		string text = "";
		string text2 = "";

		//TODO Translate
		string titleName = Translate("title." + title.id, "Title " + title.id);
		//Choose a hero to gain the "Fire-Giver" title and immediately prepare it.
		text = Translate("heroSelection.text.HeroGainsTitle", "One hero gains the {0} title and prepares it immediately.", new List<string> { titleName, title.id.ToString() });
		text2 = Translate("heroSelection.text.ChooseWhoGainsTitle", "Choose a hero to gain the {0} title (Title {1}).", new List<string> { titleName, title.id.ToString() });

		mainText.text = text;
		dummy.text = text;

		selectionText.text = text2;

		Scenario.Chronicle(text);
		Scenario.ChroniclePS("\n" + text2);

		float preferredHeight = dummy.preferredHeight; //Dummy text (which must be active) is used to find the correct preferredHeight so it can then be set on the mainText which is in a scroll view viewport
		dummy.text = ""; //After we have the height we clear dummy.text so it doesn't show up anymore

		//TODO Fix later?
		//These were to scale the text box to the size of the text, but they scale the whole panel. Since the portrait background, hero portraits, and select button are in that panel, it was messing up the alignment.
		//var dialogHeight = Math.Min(525, 30 + preferredHeight + 90);
		//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dialogHeight);
	}

	void SetText(int corruption)
	{
		string text = "";
		string text2 = "";

		//TODO Translate
		if (corruption > 0)
		{
			text = "One hero to gains " + corruption + " corruption token.\n\n";
			text += "If the hero would gain a 4th corruption token, the hero must successfully perform a Last Stand or else perish.";

			text2 = "Choose a hero to gain " + corruption + " corruption token.";
		}
		else if (corruption < 0)
		{
			text = "One hero removes " + corruption + " corruption token.";

			text2 = "Choose a hero to remove " + corruption + " corruption token.";
		}

		mainText.text = text;
		dummy.text = text;

		selectionText.text = text2;

		Scenario.Chronicle(text);
		Scenario.ChroniclePS("\n" + text2);

		float preferredHeight = dummy.preferredHeight; //Dummy text (which must be active) is used to find the correct preferredHeight so it can then be set on the mainText which is in a scroll view viewport
		dummy.text = ""; //After we have the height we clear dummy.text so it doesn't show up anymore

		//TODO Fix later?
		//These were to scale the text box to the size of the text, but they scale the whole panel. Since the portrait background, hero portraits, and select button are in that panel, it was messing up the alignment.
		//var dialogHeight = Math.Min(525, 30 + preferredHeight + 90);
		//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dialogHeight);
	}

	public void OnHeroSelect(int index)
	{
		if (index >= Bootstrap.gameStarter.heroes.Length) { return; }

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

		UpdateSubmitButtonInteractability();
	}

	void UpdateSubmitButtonInteractability()
    {
		submitBtn.GetComponent<Button>().interactable = selectedHero > -1;
	}

	void ResetHeroes()
	{
		for (int i = 0; i < Bootstrap.gameStarter.heroes.Length; i++)
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


	public void OnSubmit()
	{
		//Record the hero's name in the Chronicle
		Scenario.ChroniclePS("\n<font=\"Icon\">O</font>[" + Bootstrap.gameStarter.heroes[selectedHero] + "]");

		//"Return" the selected hero to the InteractionManager
		buttonActions?.Invoke( new InteractionResult() { value = selectedHero } );
		Hide();

		if(giveItem != null)
        {
			//Call back to InteractionManager.ItemFollowup to trigger the HeroSelectionPanel for the next item, or do final lore/xp/threat rewards, or do a fallbackTrigger
			FindObjectOfType<InteractionManager>().ItemFollowup(itemInteraction, giveItems, missingItems, originalAction);
		}
		else if(giveTitle != null)
        {
			//Call back to InteractionManager.TitleFollowup to trigger the HeroSelectionPanel for the next title, or do final lore/xp/threat rewards, or do a fallbackTrigger
			FindObjectOfType<InteractionManager>().TitleFollowup(titleInteraction, giveTitles, missingTitles, originalAction);
		}
	}
}
