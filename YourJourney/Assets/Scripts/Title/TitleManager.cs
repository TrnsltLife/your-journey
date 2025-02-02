using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
	public SelectJourney selectJourney;
	public SelectSaveSlot selectSaveSlot;
	public CampaignScreen campaignScreen;
	public Button newButton, loadButton;
	public Image finalFader;
	public AudioSource music;
	public PostProcessVolume volume;
	public SettingsDialog settings;
	public GameObject gameTitle;
	public GameObject gameTitleFlash;
	public GameObject settingsButton;
	public GameObject bannerTop;
	public GameObject bannerBottom;

	public GameObject scenarioOverlay;
	private Sprite scenarioSprite;
	public Vector2 scenarioImageSize = new Vector2(1024, 512);
	public GameObject scenarioOverlayText;
	public TextMeshProUGUI loadingText;
	public TextMeshProUGUI newButtonText;
	public TextMeshProUGUI loadButtonText;

	public void ClearScenarioImage()
    {
		LoadScenarioImage(null);
    }

	public void LoadScenarioImage(string base64Image)
	{
		if (base64Image == null || base64Image.Length == 0)
		{
			scenarioOverlay.GetComponent<Image>().sprite = null;
			Image image = scenarioOverlay.GetComponent<Image>();
			image.color = Color.black;
			scenarioOverlay.SetActive(false);
		}
		else
		{
			byte[] bytes = Convert.FromBase64String(base64Image);
			Texture2D texture = new Texture2D((int)scenarioImageSize.x, (int)scenarioImageSize.y, TextureFormat.RGBA32, false);
			texture.LoadImage(bytes);
			scenarioSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
			scenarioOverlay.GetComponent<Image>().sprite = scenarioSprite;
			Image image = scenarioOverlay.GetComponent<Image>();
			image.color = Color.white;
			scenarioOverlay.SetActive(true);
		}
	}

	public void LoadScenario()
    {
		scenarioOverlay.SetActive(true);
		gameTitle.SetActive(false);
		gameTitleFlash.SetActive(false);
		settingsButton.SetActive(false);
		bannerTop.SetActive(false);
		bannerBottom.SetActive(false);
		scenarioOverlayText.SetActive(true);
		StartCoroutine(LoadScenarioAsync());
	}

	IEnumerator LoadScenarioAsync()
    {
		AsyncOperation operation = SceneManager.LoadSceneAsync("gameboard");

		while(!operation.isDone)
        {
			//loadingText.text = "Loading Scenario... " + ((int)(operation.progress * 100)).ToString() + "%";
			yield return null;
        }
	}

	CanvasGroup newBcg, loadBcg;
	bool skipped = false;

	//new game->select save->select journey (scenario)->select heroes->show pre-game message->start game

	//new game->select save->select journey (campaign)->select heroes->show campaign screen->start game

	//load game->select save->start game OR campaign screen

	private void Start()
	{
		var settings = Bootstrap.LoadSettings();

		LanguageManager.LoadLanguage(LanguageManager.currentLanguage);
		LanguageManager.CallSubscribers();

		Vignette v;
		ColorGrading cg;
		if ( volume.profile.TryGetSettings( out v ) )
			v.active = settings.Item2 == 1;
		if ( volume.profile.TryGetSettings( out cg ) )
			cg.active = settings.Item3 == 1;
		music.enabled = settings.Item1 == 1;

		newBcg = newButton.GetComponent<CanvasGroup>();
		loadBcg = loadButton.GetComponent<CanvasGroup>();
		loadingText = scenarioOverlayText.GetComponent<TextMeshProUGUI>();

		//find campaign packages and unzip them into folders
		FileManager.UnpackCampaigns();

		//load scenarios and campaigns
		selectJourney.AddScenarioPrefabs();

		if ( Bootstrap.returnToCampaign )
		{
			skipped = true;
			finalFader.DOFade( 1, .5f ).OnComplete( () =>
			{
				campaignScreen.ActivateScreen( new TitleMetaData()
				{
					slotMode = 1,
					campaignState = Bootstrap.campaignState,
					skippedToCampaignScreen = true
				} );
			} );
		}
		else
		{
			GlowTimer.SetTimer( 5, () =>
			{
				newButton.transform.DOLocalMoveX( -700, .75f );
				loadButton.transform.DOLocalMoveX( -700, .75f );
				newBcg.interactable = true;
				loadBcg.interactable = true;
			} );
		}
	}

	public void ResetScreen()
	{
		LanguageManager.CallSubscribers();

		finalFader.DOFade( 0, .5f );

		newBcg.DOFade( 1, .25f );
		loadBcg.DOFade( 1, .25f );
		newBcg.blocksRaycasts = true;
		loadBcg.blocksRaycasts = true;
		newBcg.interactable = true;
		loadBcg.interactable = true;

		newButton.transform.DOLocalMoveX( -700, .75f );
		loadButton.transform.DOLocalMoveX( -700, .75f );
		newButton.interactable = true;
		loadButton.interactable = true;
	}

	public void NewGame()
	{
		newBcg.DOFade( 0, .25f );
		loadBcg.DOFade( 0, .25f );
		newBcg.blocksRaycasts = false;
		loadBcg.blocksRaycasts = false;

		newButton.interactable = false;
		loadButton.interactable = false;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			selectSaveSlot.ActivateScreen( new TitleMetaData() { slotMode = 0 } );
		} );
	}

	public void LoadGame()
	{
		newBcg.DOFade( 0, .25f );
		loadBcg.DOFade( 0, .25f );
		newBcg.blocksRaycasts = false;
		loadBcg.blocksRaycasts = false;

		newButton.interactable = false;
		loadButton.interactable = false;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			selectSaveSlot.ActivateScreen( new TitleMetaData() { slotMode = 1 } );
		} );
	}

	public void OnSettings()
	{
		settings.Show( "settings.QuitApp", "Quit App", OnLanguageUpdate );
	}

	public void OnLanguageUpdate(string languageName)
	{
		//Debug.Log("Engine.OnLanguageUpdate(" + languageName + ")");
		LanguageManager.LoadLanguage(languageName);
		LanguageManager.UpdateCurrentLanguage(languageName);
		LanguageManager.CallSubscribers();
	}

	void SkipIntro()
	{
		if ( skipped )
			return;
		skipped = true;
		newButton.transform.DOLocalMoveX( -700, .75f );
		loadButton.transform.DOLocalMoveX( -700, .75f );
		newBcg.interactable = true;
		loadBcg.interactable = true;
	}

	private void Update()
	{
		if ( !skipped && ( Input.anyKeyDown || Input.GetMouseButtonDown( 1 ) ) )
		{
			SkipIntro();
		}
	}
}
