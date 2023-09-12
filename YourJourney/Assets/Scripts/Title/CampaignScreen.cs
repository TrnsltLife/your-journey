using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CampaignScreen : MonoBehaviour
{
	public SelectSaveSlot selectSaveSlot;
	public SelectHeroes selectHeroes;
	public StoryBox storyBox;
	public Image finalFader;
	public Button continueButton, backButton, continueReplayButton;
	public GameObject fileItemButtonPrefab;
	public RectTransform itemContainer;
	public GameObject replaybox;
	public TextMeshProUGUI xpLoreText, currentScenarioText, replayText, replayStatusText;
	public TextTranslation replayStatusTextTranslation;

	TitleManager tm;
	TitleMetaData titleMetaData;
	Campaign campaign;
	CampaignState campaignState;
	List<FileItemButton> fileItemButtons = new List<FileItemButton>();
	int selectedIndex = -1;
	string coverImage = null;

	//players can CONTINUE latest scenario or REPLAY any scenario, keeping only the highest xp/lore earned from it

	public void ActivateScreen( TitleMetaData metaData )
	{
		titleMetaData = metaData;
		selectedIndex = -1;
		//when LOADING from state, some title metadata is unset:
		//difficulty, gameName, projectItem, selectedHeroes
		campaign = titleMetaData.campaignState.campaign;
		campaignState = titleMetaData.campaignState;
		replaybox.SetActive( false );
		var xp = campaignState.scenarioXP.Aggregate( ( acc, cur ) => acc + cur );
		var lore = campaignState.scenarioLore.Aggregate( ( acc, cur ) => acc + cur );
		xpLoreText.text = lore + " / " + xp;
		replayText.text = "";
		currentScenarioText.text = campaignState.campaign.scenarioCollection[campaignState.currentScenarioIndex].scenarioName;

		tm = FindObjectOfType<TitleManager>();
		tm.LoadScenarioImage(campaignState.campaign.scenarioCollection[campaignState.currentScenarioIndex].coverImage);

		bool finishedCampaign = !campaignState.scenarioStatus.Any( x => x == ScenarioStatus.NotPlayed );

		for ( int i = 0; i < campaign.scenarioCollection.Count; i++ )
		{
			var go = Instantiate( fileItemButtonPrefab, itemContainer ).GetComponent<FileItemButton>();
			go.transform.localPosition = new Vector3( 0, ( -110 * i ) );
			go.Init( i, campaign.scenarioCollection[i].scenarioName,
				"", //TODO collections?
				ProjectType.Standalone, ( index ) => OnSelectScenario( index ) );
			if ( campaignState.currentScenarioIndex != i || finishedCampaign )
				go.RemoveRing();
			go.SetSuccess( campaignState.scenarioStatus[i] );
			fileItemButtons.Add( go );
		}
		itemContainer.sizeDelta = new Vector2( 772, fileItemButtons.Count * 110 );

		replayStatusTextTranslation = replayStatusText.GetComponent<TextTranslation>();
		CheckCampaignStatus();

		gameObject.SetActive( true );

		finalFader.DOFade( 0, .5f ).OnComplete( () =>
		{
			backButton.interactable = true;
		} );
	}

	void CheckCampaignStatus()
	{
		//check status
		//1 - no replay in progress, no current campaign in progress
		//2 - current campaign in progress
		//3 - replay in progress
		bool finishedCampaign = !campaignState.scenarioStatus.Any( x => x == ScenarioStatus.NotPlayed );

		var gs = GameState.LoadState( campaignState.saveStateIndex );
		if ( gs.partyState == null )//1
		{
			continueReplayButton.interactable = false;
			continueButton.interactable = true;

			//replayStatusText.text = "No game is in progress.";
			//check if campaign is finished
			if (!finishedCampaign)
			{
				replayStatusTextTranslation.Change("campaign.text.NoGameContinue", "  You may continue the Campaign from the current Scenario with the Continue Campaign button.");
				//replayStatusText.text += "  You may continue the Campaign from the current Scenario with the Continue Campaign button.";
			}
			else
			{
				replayStatusTextTranslation.Change("campaign.text.NoGameReplay", "  The campaign has been finished.  Only Scenario Replays are available to play.");
				//replayStatusText.text += "  The campaign has been finished.  Only Scenario Replays are available to play.";
			}
		}
		else//2 or 3 - a game is in progress
		{
			//3 - different scenario than latest
			if ( gs.campaignState.scenarioPlayingIndex != campaignState.currentScenarioIndex )
			{
				//replayStatusText.text = "A Replay is in progress.";
				continueReplayButton.interactable = true;
				if (!finishedCampaign)
				{
					replayStatusTextTranslation.Change("campaign.text.ReplayContinue", "  You may continue the Replay or forfeit it and continue the Campaign from the current Scenario.");
					//replayStatusText.text += "  You may continue the Replay or forfeit it and continue the Campaign from the current Scenario.";
				}
				else
				{
					replayStatusTextTranslation.Change("campaign.text.ReplayFinished", "  You may continue the Replay, but the Campaign has been finished.");
					//replayStatusText.text += "  You may continue the Replay, but the Campaign has been finished.";
				}
			}
			else//3 - state scenario is same as current scenario, could be a replay or current campaign play. Check its scenarioStatus
			{
				if (gs.campaignState.scenarioStatus[gs.campaignState.scenarioPlayingIndex] == ScenarioStatus.NotPlayed)//2 - current campaign state
				{
					replayStatusTextTranslation.Change("campaign.text.GameInProgress", "A game is in progress.  Continue it with the Continue Campaign button.");
					//replayStatusText.text = "A game is in progress.  Continue it with the Continue Campaign button.";
				}
				else//3 - replay
				{
					//replayStatusText.text = "A Replay is in progress.  Continue it with the Continue Replay button.";
					//if (campaignState.scenarioStatus.Any(x => x == ScenarioStatus.NotPlayed))
					//	replayStatusText.text += "  The campaign has been finished.";

					if (campaignState.scenarioStatus.Any(x => x == ScenarioStatus.NotPlayed))
                    {
						replayStatusTextTranslation.Change("campaign.text.ReplayInProgress", "A Replay is in progress.  Continue it with the Continue Replay button.");
					}
                    else 
					{
						replayStatusTextTranslation.Change("campaign.text.ReplayInProgressFinished", "A Replay is in progress.  Continue it with the Continue Replay button." + "  The campaign has been finished.");
					}
				}
			}
		}

		//disable continue campaign button if all scenarios have been played
		if ( campaignState.scenarioStatus.Any( x => x == ScenarioStatus.NotPlayed ) )
			continueButton.interactable = true;
		else
			continueButton.interactable = false;
	}

	public void OnSelectScenario( int index )
	{
		for ( int i = 0; i < fileItemButtons.Count; i++ )
		{
			if ( i != index )
				fileItemButtons[i].ResetColor();
		}

		selectedIndex = index;

		coverImage = campaign.scenarioCollection[selectedIndex].coverImage ?? campaign.coverImage; //scenario image, or campaign image as a fallback
		tm.LoadScenarioImage(coverImage);

		//if selected scenario has been played (fail or success) activated replay option
		if ( campaignState.scenarioStatus[index] != ScenarioStatus.NotPlayed )
		{
			replaybox.SetActive( true );
			replayText.text = campaign.scenarioCollection[index].scenarioName;
		}
		else
		{
			replaybox.SetActive( false );
			replayText.text = "";
			//show haven't played message
		}
	}

	public void StartGame()
    {
		gameObject.SetActive(false); //hide the SpecialInstructions form but leave scenarioOverlay with coverImage showing
		TitleManager tm = FindObjectOfType<TitleManager>();
		tm.LoadScenario();
	}

	public void OnContinueCampaign()
	{
		campaignState.scenarioPlayingIndex = campaignState.currentScenarioIndex;

		int sIndex = campaignState.currentScenarioIndex;

		//bootstrap the game state
		GameStarter gameStarter = new GameStarter();
		gameStarter.gameName = campaignState.gameName;
		gameStarter.saveStateIndex = campaignState.saveStateIndex;
		gameStarter.scenarioFileName = campaignState.campaign.scenarioCollection[sIndex].fileName;
		gameStarter.heroes = campaignState.heroes;
		gameStarter.heroesIndex = campaignState.heroesIndex;
		gameStarter.difficulty = campaignState.difficulty;
		gameStarter.coverImage = coverImage; //refers to the scenario cover image

		Bootstrap.campaignState = campaignState;
		Bootstrap.gameStarter = gameStarter;

		//check for a saved state
		GameState gs = GameState.LoadState( campaignState.saveStateIndex );
		if ( gs.partyState == null )//no saved state
		{
			gameStarter.isNewGame = true;//start scenario fresh
			Debug.Log( "NEW GAME" );
		}
		else//saved state found, is it current scenario or replay?
		{
			if ( gs.partyState.scenarioFileName == gameStarter.scenarioFileName )//it's the current scenario
			{
				gameStarter.isNewGame = false;//otherwise start scenario fresh
				Debug.Log( "CONTINUING SAVED GAME" );
			}
			else//it's just a replay, toast it and start fresh
			{
				gameStarter.isNewGame = true;
				Debug.Log( "NEW GAME" );
			}
		}

		StartGame();
	}

	public void OnReplayScenario()
	{
		campaignState.scenarioPlayingIndex = selectedIndex;

		//bootstrap the game state
		GameStarter gameStarter = new GameStarter();
		gameStarter.gameName = campaignState.gameName;
		gameStarter.saveStateIndex = campaignState.saveStateIndex;
		gameStarter.scenarioFileName = campaignState.campaign.scenarioCollection[selectedIndex].fileName;
		gameStarter.heroes = campaignState.heroes;
		gameStarter.heroesIndex = campaignState.heroesIndex;
		gameStarter.difficulty = campaignState.difficulty;
		gameStarter.isNewGame = true;//start scenario fresh
		gameStarter.coverImage = campaign.coverImage;

		Bootstrap.campaignState = campaignState;
		Bootstrap.gameStarter = gameStarter;

		var scenario = FileManager.LoadScenario( FileManager.GetFullPathWithCampaign( gameStarter.scenarioFileName, campaign.campaignGUID.ToString() ) );

		if ( !string.IsNullOrEmpty( scenario.specialInstructions ) )
		{
			storyBox.Show( scenario.specialInstructions, () =>
			{
				StartGame();
			});
		}
		else
		{
			StartGame();
		}
	}

	public void OnContinueReplay()
	{
		campaignState.scenarioPlayingIndex = selectedIndex;

		var gs = GameState.LoadState( campaignState.saveStateIndex );

		//bootstrap the game state
		GameStarter gameStarter = new GameStarter();
		gameStarter.gameName = campaignState.gameName;
		gameStarter.saveStateIndex = campaignState.saveStateIndex;
		gameStarter.scenarioFileName = gs.partyState.scenarioFileName;
		gameStarter.heroes = campaignState.heroes;
		gameStarter.heroesIndex = campaignState.heroesIndex;
		gameStarter.difficulty = campaignState.difficulty;
		gameStarter.isNewGame = false;
		gameStarter.coverImage = campaign.coverImage;

		Bootstrap.campaignState = campaignState;
		Bootstrap.gameStarter = gameStarter;

		StartGame();
	}

	public void OnBack()
	{
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			tm.ClearScenarioImage();
			gameObject.SetActive( false );

			if ( titleMetaData.skippedToCampaignScreen )
			{
				FindObjectOfType<TitleManager>().ResetScreen();
			}
			else
			{
				if ( titleMetaData.previousScreen == TitleScreen.SelectHeroes )
					selectHeroes.ActivateScreen( titleMetaData );
				else if ( titleMetaData.previousScreen == TitleScreen.SelectSlot )
					selectSaveSlot.ActivateScreen( titleMetaData );
			}
		} );
	}

	public void OnShowStory()
	{
		storyBox.Show( campaignState.campaign.storyText, null );
	}
}
