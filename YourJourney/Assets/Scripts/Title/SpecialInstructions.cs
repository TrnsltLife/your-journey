using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialInstructions : MonoBehaviour
{
	public SelectHeroes selectHeroes;
	public Image finalFader;
	public Button beginButton, cancelButton, backButton, increaseLoreButton, decreaseLoreButton, increaseXPButton, decreaseXPButton;
	public TextMeshProUGUI loreText, xpText, instructions;
	TextTranslation instructionsTranslation;
	public AudioSource music;

	RectTransform itemContainer;
	TitleMetaData titleMetaData;
	Scenario s;

	public void ActivateScreen( TitleMetaData metaData )
	{
		LanguageManager.AddSubscriber(onUpdateTranslation);

		titleMetaData = metaData;
		gameObject.SetActive( true );
		//itemContainer = instructions.rectTransform;

		loreText.text = "";
		xpText.text = "";
		instructions.text = "";
		instructionsTranslation = instructions.GetComponent<TextTranslation>();

		finalFader.DOFade( 0, .5f ).OnComplete( () =>
		{
			beginButton.interactable = true;
			backButton.interactable = true;
			cancelButton.interactable = true;
			s = Bootstrap.LoadScenarioFromFilename( titleMetaData.projectItem.fileName );
			if ( s != null )
			{
				UpdateInstructions();

				loreText.text = s.loreStartValue.ToString();
				xpText.text = s.xpStartValue.ToString();

				if (s.projectType == ProjectType.Standalone)
				{
					//Set buttons visible
					increaseLoreButton.gameObject.SetActive(true);
					decreaseLoreButton.gameObject.SetActive(true);
					increaseXPButton.gameObject.SetActive(true);
					decreaseXPButton.gameObject.SetActive(true);
					OnAdjustLore(0); //This will set the buttons interactable
					OnAdjustXP(0); //This will set the buttons interactable
				}
			}
			else
			{
				//SetText( "There was a problem loading the Scenario." );
				instructionsTranslation.TranslationEnabled(true);
				instructionsTranslation.Change("story.text.ErrorLoading", "There was a problem loading the Scenario.");
				beginButton.interactable = false;
			}
		} );
	}

	void UpdateInstructions()
    {
		if (s != null)
		{
			if (!string.IsNullOrEmpty(s.specialInstructions))
			{
				instructionsTranslation.TranslationEnabled(false); //Don't let text be overwritten by translation if the language changes
				SetText(titleMetaData.projectItem.Translated("scenario.instructions", s.specialInstructions));
			}
			else
			{
				//SetText("There are no special instructions for this Scenario.");
				instructionsTranslation.TranslationEnabled(true);
				instructionsTranslation.Change("story.text.NoStoryDescription", "There are no special instructions for this Scenario.");
			}
		}
	}

	void SetText( string t )
	{
		instructions.text = t;
	}

	public void onUpdateTranslation()
    {
		UpdateInstructions();
    }

	public void OnBegin()
	{
		//bootstrap into the scenario
		GameStarter gameStarter = new GameStarter();
		gameStarter.gameName = titleMetaData.gameName;
		gameStarter.saveStateIndex = titleMetaData.saveStateIndex;
		gameStarter.scenarioFileName = titleMetaData.projectItem.fileName;
        gameStarter.heroes = titleMetaData.selectedHeroes;
		gameStarter.heroesIndex = titleMetaData.selectedHeroesIndex;
		gameStarter.characterSheets = titleMetaData.characterSheets;
        gameStarter.difficulty = titleMetaData.difficulty;
		gameStarter.isNewGame = true;
		gameStarter.loreStartValue = s.loreStartValue;
		gameStarter.xpStartValue = s.xpStartValue;
		gameStarter.coverImage = s.coverImage;

		Bootstrap.gameStarter = gameStarter;
		Bootstrap.campaignState = null;

		DOTween.To( () => music.volume, setter => music.volume = setter, 0f, .5f );
		//gameObject.SetActive( false );
		gameObject.SetActive( false ); //hide the SpecialInstructions form but leave scenarioOverlay with coverImage showing
		TitleManager tm = FindObjectOfType<TitleManager>();
		tm.LoadScenario();
	}

	public void OnBack()
	{
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			selectHeroes.ActivateScreen( titleMetaData );
			gameObject.SetActive( false );
		} );
	}

	public void OnCancel()
	{
		beginButton.interactable = false;
		backButton.interactable = false;
		cancelButton.interactable = false;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			FindObjectOfType<TitleManager>().ResetScreen();
		} );
	}

	public void OnAdjustLore(int amount)
	{
		Debug.Log("Adjust Lore " + amount);

		int minLore = 0;
		int maxLore = 105;

		s.loreStartValue += amount;
		if(s.loreStartValue < minLore) { s.loreStartValue = minLore; }
		else if(s.loreStartValue > maxLore) { s.loreStartValue = maxLore; }

		loreText.text = s.loreStartValue.ToString();

		decreaseLoreButton.interactable = s.loreStartValue > minLore;
		increaseLoreButton.interactable = s.loreStartValue < maxLore;
	}

	public void OnAdjustXP(int amount)
	{
		Debug.Log("Adjust XP " + amount);

		int minXP = 0;
		int maxXP = 105;

		s.xpStartValue += amount;
		if (s.xpStartValue < minXP) { s.xpStartValue = minXP; }
		else if (s.xpStartValue > maxXP) { s.xpStartValue = maxXP; }

		xpText.text = s.xpStartValue.ToString();

		decreaseXPButton.interactable = s.xpStartValue > minXP;
		increaseXPButton.interactable = s.xpStartValue < maxXP;
	}
}

