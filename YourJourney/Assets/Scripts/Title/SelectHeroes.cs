using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

public class SelectHeroes : MonoBehaviour
{
	public CampaignScreen campaignScreen;
	public SelectJourney selectJourney;
	public SpecialInstructions specialInstructions;
	public Image finalFader;
	public Button[] heroButtons;
	public Button beginButton, backButton, leftScrollButton, rightScrollButton;
	public TextMeshProUGUI[] heroNameText;
	public TextMeshProUGUI[] heroCollectionText;
	public TextMeshProUGUI diffText;
	public TextTranslation diffTextTranslation;

	public Sprite heroImageBlank;
	public Sprite[] heroImage;
	public string[] heroName;
	public string[] heroCollection;
	int lineupOffset = 0;
	int lineupTotal = 0;
	int lineupSize = 6;
	public static readonly int maxHeroes = 5;
	int heroCount = 0;


	bool[] selectedHeroes;
	string tempName;
	bool isChangingName = false;
	int nameIndex = -1;
	TitleMetaData titleMetaData;

	public void ActivateScreen( TitleMetaData metaData )
	{
		titleMetaData = metaData;
		gameObject.SetActive( true );
		lineupTotal = heroImage.Length;
		if (heroCount == 0)
		{
			selectedHeroes = new bool[heroImage.Length].Fill(false);
		}
		//diffText.text = "Normal";
		diffTextTranslation = diffText.GetComponent<TextTranslation>();
		ResetHeros();

		titleMetaData.difficulty = Difficulty.Normal;
		//diffText.text = titleMetaData.difficulty.ToString();
		diffTextTranslation.Change("heroes.button." + titleMetaData.difficulty.ToString(), titleMetaData.difficulty.ToString());


		finalFader.DOFade( 0, .5f ).OnComplete( () =>
		{
			backButton.interactable = true;
		} );
	}

	public void OnHeroSelect( int index )
	{
		int lineupIndex = lineupOffset + index;
		isChangingName = false;
		if ( nameIndex != -1 )
		{
			heroNameText[nameIndex].color = Color.white;
			heroNameText[nameIndex].text = tempName;
			heroName[lineupOffset + nameIndex] = tempName;
		}

		if (heroCount < maxHeroes || selectedHeroes[lineupIndex]) //Process the click if there are still hero slots left or if we're deselecting
		{
			ColorBlock cb = heroButtons[index].colors;
			selectedHeroes[lineupIndex] = !selectedHeroes[lineupIndex];
			heroCount += selectedHeroes[lineupIndex] ? 1 : -1;
			heroButtons[index].colors = new ColorBlock()
			{
				normalColor = selectedHeroes[lineupIndex] ? new Color(1, 167f / 255f, 124f / 255f, 1) : new Color(1, 1, 1, 1),
				pressedColor = cb.pressedColor,
				selectedColor = selectedHeroes[index] ? new Color(1, 1, 1, 1) : new Color(1, 167f / 255f, 124f / 255f, 1),
				colorMultiplier = cb.colorMultiplier,
				disabledColor = cb.disabledColor,
				fadeDuration = cb.fadeDuration,
				highlightedColor = cb.highlightedColor
			};
		}
		heroButtons[index].enabled = false;
		heroButtons[index].enabled = true;

		//ResetHeros();

		beginButton.interactable = selectedHeroes.Any( b => b );
	}

	public void OnHeroScroll( int direction )
    {
		bool updated = false;
		if (direction == -1)
		{
			if (lineupOffset > 0)
			{
				lineupOffset -= lineupSize;
				if (lineupOffset < 0) { lineupOffset = 0; }
				updated = true;
			}
		}
		else
        {
			//Check if we can scroll right
			if (lineupOffset + lineupSize < lineupTotal)
			{
				lineupOffset += lineupSize;
				updated = true;
			}
		}

		if(updated)
        {
			for(int i=0, j=lineupOffset; i<lineupSize; i++, j++)
            {
				if (j < lineupTotal)
				{
					heroButtons[i].GetComponent<Image>().sprite = heroImage[j];
					heroCollectionText[i].text = heroCollection[j];
					heroNameText[i].text = heroName[j];
					heroButtons[i].interactable = true;
				}
				else
                {
					heroButtons[i].GetComponent<Image>().sprite = heroImageBlank;
					heroCollectionText[i].text = "";
					heroNameText[i].text = "";
					heroButtons[i].interactable = false;
                }
			}
        }

		leftScrollButton.interactable = lineupOffset > 0;
		rightScrollButton.interactable = (lineupOffset + lineupSize >= lineupTotal) ? false : true;

		ResetHeros();
	}

	void ResetHeros()
	{
		for ( int i=0, j=lineupOffset; i < lineupSize && j < lineupTotal; i++, j++ )
		{
			heroNameText[i].color = Color.white;
			string savedName = Bootstrap.GetHeroName(j);
			heroNameText[i].text = String.IsNullOrWhiteSpace(savedName) ? savedName : heroName[j];
			heroName[j] = heroNameText[i].text;
			ColorBlock cb = heroButtons[i].colors;
			//if ( selectedHeroes[j] )
			//{
				heroButtons[i].colors = new ColorBlock()
				{
					normalColor = selectedHeroes[j] ? new Color(1, 167f / 255f, 124f / 255f, 1) : new Color(1, 1, 1, 1),
					pressedColor = cb.pressedColor,
					selectedColor = selectedHeroes[j] ? new Color(1, 1, 1, 1) : new Color( 1, 167f / 255f, 124f / 255f, 1 ),
					colorMultiplier = cb.colorMultiplier,
					disabledColor = cb.disabledColor,
					fadeDuration = cb.fadeDuration,
					highlightedColor = cb.highlightedColor
				};
			//}
			heroButtons[i].enabled = false;
			heroButtons[i].enabled = true;
		}
	}

	public void OnDifficulty()
	{
		if ( titleMetaData.difficulty == Difficulty.Adventure )
			titleMetaData.difficulty = Difficulty.Normal;
		else if ( titleMetaData.difficulty == Difficulty.Normal )
			titleMetaData.difficulty = Difficulty.Hard;
		else if ( titleMetaData.difficulty == Difficulty.Hard )
			titleMetaData.difficulty = Difficulty.Adventure;
		//diffText.text = titleMetaData.difficulty.ToString();
		diffTextTranslation.Change("heroes.button." + titleMetaData.difficulty.ToString(), titleMetaData.difficulty.ToString());
	}

	public void OnChangeNameClick( int index )
	{
		isChangingName = true;
		nameIndex = index;
		heroNameText[index].color = Color.green;
		tempName = heroNameText[nameIndex].text;
		heroNameText[nameIndex].text = "";
	}

	public void OnNext()
	{
		beginButton.interactable = backButton.interactable = false;
		List<CharacterSheet> characterSheets = titleMetaData?.campaignState?.startingCharacterSheets?[0]; //get List<CharacterSheet> at index [0], i.e. the first scenario. May be null.
		if(characterSheets == null) { characterSheets = new List<CharacterSheet>(); }
		//Attempt to preserve any CharacterSheet that had already been filled in on the Campfire screen,
		//so the Role and Item selections remain even if new characters are added or other characters are removed.
		for(int j=0; j<lineupTotal; j++)
        {
			CharacterSheet character = characterSheets.FirstOrDefault(it => it.portraitIndex == j);
			if (selectedHeroes[j])
            {
				if(character == null)
                {
					Hero hero = Heroes.FromID(j);
					int characterIndex = characterSheets.Count;
					character = new CharacterSheet(heroName[j], j) { race = hero.race, sex = hero.sex, maxHanded = hero.maxHanded, role = hero.role,
						armorId = Items.FirstAvailable(hero.armor, 1, characterSheets, characterIndex, 0), 
						hand1Id = Items.FirstAvailable(hero.hand1, 1, characterSheets, characterIndex, 1), 
						hand2Id = Items.FirstAvailable(hero.hand2, 1, characterSheets, characterIndex, 2), 
						mountId = Items.FirstAvailable(hero.mount, 1, characterSheets, characterIndex, 0)
					};
					characterSheets.Add(character);
                }
				else
                {
					character.name = heroName[j];
                }
            }
			else
            {
				if(character != null)
				{
					characterSheets.Remove(character);
				}
			}
		}
		titleMetaData.selectedHeroes = characterSheets.ConvertAll(it => it.name).ToArray();
		titleMetaData.selectedHeroesIndex = characterSheets.ConvertAll(it => it.portraitIndex).ToArray();

		foreach(CharacterSheet cs in characterSheets)
        {
			Debug.Log(cs.name + ": " + Items.FromID(cs.armorId).dataName + " / " + Items.FromID(cs.hand1Id).dataName + " / " + Items.FromID(cs.hand2Id).dataName + " / " + Items.FromID(cs.mountId).dataName);
        }

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			if (titleMetaData.projectItem.projectType == ProjectType.Standalone)
			{
				specialInstructions.ActivateScreen(titleMetaData);
			}
			else
			{
				//create new campaign state and save it
				CampaignState campaignState = new CampaignState(FileManager.LoadCampaign(titleMetaData.projectItem.campaignGUID));
				titleMetaData.campaignState = campaignState;
				titleMetaData.campaignState.heroes = titleMetaData.selectedHeroes;
				titleMetaData.campaignState.heroesIndex = titleMetaData.selectedHeroesIndex;
				titleMetaData.campaignState.startingCharacterSheets[0] = characterSheets;
				titleMetaData.campaignState.currentCharacterSheets[0] = CampaignState.CloneCharacterSheetList(characterSheets);
				titleMetaData.campaignState.gameName = titleMetaData.gameName;
				titleMetaData.campaignState.saveStateIndex = titleMetaData.saveStateIndex;
				titleMetaData.campaignState.difficulty = titleMetaData.difficulty;
				//titleMetaData difficulty is already set
				titleMetaData.previousScreen = TitleScreen.SelectHeroes;

				new GameState().SaveCampaignState(titleMetaData.saveStateIndex, titleMetaData.campaignState);

				campaignScreen.ActivateScreen(titleMetaData);
			}
		} );

		//DOTween.To( () => music.volume, setter => music.volume = setter, 0f, .5f );
		//finalFader.gameObject.SetActive( true );
		//finalFader.DOFade( 1, .5f ).OnComplete( () => SceneManager.LoadScene( "gameboard" ) );
	}

	public void OnBack()
	{
		beginButton.interactable = backButton.interactable = false;
		isChangingName = false;
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			selectJourney.ActivateScreen( titleMetaData );
			gameObject.SetActive( false );
		} );
	}

	private void Update()
	{
		if ( isChangingName )
		{
			if ( Input.GetKeyDown( KeyCode.Escape ) )
			{
				isChangingName = false;
				heroNameText[nameIndex].color = Color.white;
				heroNameText[nameIndex].text = tempName;
				heroName[lineupOffset + nameIndex] = tempName;
				return;
			}

			foreach ( char c in Input.inputString )
			{
				if ( c == '\b' ) // has backspace/delete been pressed?
				{
					if ( heroNameText[nameIndex].text.Length != 0 )
					{
						heroNameText[nameIndex].text = heroNameText[nameIndex].text.Substring( 0, heroNameText[nameIndex].text.Length - 1 );
						heroName[lineupOffset + nameIndex] = heroNameText[nameIndex].text;
					}
				}
				else if ( ( c == '\n' ) || ( c == '\r' ) ) // enter/return
				{
					isChangingName = false;
					heroNameText[nameIndex].color = Color.white;
					if (string.IsNullOrEmpty(heroNameText[nameIndex].text))
					{
						heroNameText[nameIndex].text = tempName;
						heroName[lineupOffset + nameIndex] = tempName;
					}
					else
					{
						Bootstrap.SaveHeroName(lineupOffset + nameIndex, heroNameText[nameIndex].text);
					}
				}
				else
				{
					heroNameText[nameIndex].text += c;
					heroName[lineupOffset + nameIndex] = heroNameText[nameIndex].text;
				}
			}
		}
	}
}
