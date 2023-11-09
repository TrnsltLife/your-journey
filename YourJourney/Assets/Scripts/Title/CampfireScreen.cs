using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CampfireScreen : MonoBehaviour
{
	public CampaignScreen campaignScreen;
	public Image finalFader;
	public Button continueButton, backButton;
	public TextMeshProUGUI nameText, remainingPointsNumberText;
	public Button[] heroButtons;
	public Image[] heroImage;
	public Image portraitBackground;
	public float heroImageWidth;
	public float portraitMaxWidth;
	private float portraitExtraWidth = 0f;
	//public TextTranslation remainingPointsTextTranslation, skillsTranslationText, roleItemsTranslationText;

	TitleManager tm;
	TitleMetaData titleMetaData;

	public void ActivateScreen( TitleMetaData metaData )
	{
		portraitExtraWidth = portraitMaxWidth - (heroImageWidth * SelectHeroes.maxHeroes);

		titleMetaData = metaData;

		tm = FindObjectOfType<TitleManager>();

		//Turn Your Journey game title off since we need the screen real estate
		tm.gameTitle.SetActive(false);
		tm.gameTitleFlash.SetActive(false);

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

	public void SetImages()
	{
		for (int i = 0; i < SelectHeroes.maxHeroes; i++)
		{
			if (i < titleMetaData.selectedHeroesCount)
			{
				heroImage[i].gameObject.SetActive(true);
				heroButtons[i].gameObject.SetActive(true);
				heroImage[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Portraits/p" + titleMetaData.selectedHeroesIndex[i]);
			}
			else
            {
				heroImage[i].gameObject.SetActive(false);
				heroButtons[i].gameObject.SetActive(false);
			}
		}

		//Set the size of the background image behind the hero images so it fits however many heroes we have
		float portraitBackgroundWidth = portraitExtraWidth + (heroImageWidth * titleMetaData.selectedHeroesCount);
		portraitBackground.rectTransform.sizeDelta = new Vector2(portraitBackgroundWidth, portraitBackground.rectTransform.sizeDelta.y);
	}

	public void OnHeroSelect(int index)
	{
		Debug.Log("Hero select " + index);

		if(index >= titleMetaData.selectedHeroesCount) { return; }

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

		nameText.text = titleMetaData.selectedHeroes[index];

		//beginButton.interactable = selectedHeroes.Any(b => b);
	}

	void ResetHeroes()
	{
		for (int i = 0; i < titleMetaData.selectedHeroesCount; i++)
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
			campaignScreen.ActivateScreen(titleMetaData);
		} );
	}

	public void OnNext()
	{
		StartGame();
	}

}
