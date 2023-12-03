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
	List<Item> giveItems;
	int missingItems;
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
		Show(ii, giveItems, missingItems, item, 0, originalAction, actions);
    }
	public void Show(ItemInteraction ii, List<Item> giveItems, int missingItems, int corruption, Action<InteractionResult> originalAction, Action<InteractionResult> actions)
	{
		Show(ii, giveItems, missingItems, null, corruption, originalAction, actions);
	}

	public void Show(ItemInteraction ii, List<Item> giveItems, int missingItems, Item item, int corruption, Action<InteractionResult> originalAction, Action<InteractionResult> actions )
	{
		itemInteraction = ii;
		this.giveItems = giveItems;
		this.missingItems = missingItems;
		this.originalAction = originalAction;

		CalculatePanelPosition();
		FindObjectOfType<TileManager>().ToggleInput( true );

		SetText(item, corruption);
		SetSelectionText(item, corruption);
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
			portraitBackground.rectTransform.sizeDelta = new Vector2(portraitBackgroundWidth, portraitBackground.rectTransform.sizeDelta.y);
		}
	}

	void SetText(Item item, int corruption)
	{
		string text = "";

		//TODO Translate
		if (item != null)
		{
			string icon = "t"; //Start out with "t" for TRINKET
			if (item.slotId == Slot.MOUNT) { icon = "m"; }
			text = "One hero gains <font=\"Icon\">" + icon + "</font> " + item.dataName + ".\n\n";
			if (item.slotId == Slot.TRINKET)
			{
				text += "That hero may equip it immediately even if other trinkets are already equipped. Add the full number of depletion tokens to the trinket.";
			}
			else
            {
				text += "That hero may equip it immediately even if other mounts are already equipped.";
			}
		}
		else if (corruption > 0)
		{
			text = "One hero to gains " + corruption + " corruption token.\n\n";
			text += "If the hero would gain a 4th corruption token, the hero must successfully perform a Last Stand or else perish.";
		}
		else if (corruption < 0)
		{
			text = "One hero to removes " + corruption + " corruption token.";
		}

		mainText.text = text;
		dummy.text = text;

		Scenario.Chronicle(text);

		float preferredHeight = dummy.preferredHeight; //Dummy text (which must be active) is used to find the correct preferredHeight so it can then be set on the mainText which is in a scroll view viewport
		dummy.text = ""; //After we have the height we clear dummy.text so it doesn't show up anymore

		//var dialogHeight = Math.Min(525, 30 + preferredHeight + 90);

		//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dialogHeight);
	}

	void SetSelectionText(Item item, int corruption)
    {
		//TODO Translate
		if(item != null)
        {
			string icon = "t"; //Start out with "t" for TRINKET
			if(item.slotId == Slot.MOUNT) { icon = "m"; }
			selectionText.text = "Select a hero to gain <font=\"Icon\">" + icon + "</font> " + item.dataName + ".";
        }
		else if(corruption > 0)
        {
			selectionText.text = "Select a hero to gain " + corruption + " corruption token.";
        }
		else if(corruption < 0)
        {
			selectionText.text = "Select a hero to remove " + corruption + " corruption token.";
		}
		Scenario.Chronicle(selectionText.text);
	}

	public void OnHeroSelect(int index)
	{
		if (index >= Bootstrap.campaignState.heroes.Length) { return; }

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
		for (int i = 0; i < Bootstrap.campaignState.heroes.Length; i++)
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
		//TODO Record the hero's name
		Scenario.ChroniclePS("\n<font=\"Icon\">O</font>[" + Bootstrap.gameStarter.heroes[selectedHero] + "]");

		buttonActions?.Invoke( new InteractionResult() { value = selectedHero } );
		Hide();

		//Call back to InteractionManager.ItemFollowup to trigger the HeroSelectionPanel for the next item, or do final lore/xp/threat rewards, or do a fallbackTriiger
		FindObjectOfType<InteractionManager>().ItemFollowup(itemInteraction, giveItems, missingItems, originalAction);
	}
}
