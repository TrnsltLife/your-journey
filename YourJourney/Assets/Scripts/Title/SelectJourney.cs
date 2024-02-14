using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class SelectJourney : MonoBehaviour
{
	public SelectHeroes selectHeroes;
	public SelectSaveSlot selectSaveSlot;
	public List<FileItemButton> fileItemButtons = new List<FileItemButton>();
	public Image finalFader;
	public TextMeshProUGUI nameText, collectionsText, scenarioVersionText, versionText, fileText, appVersion, engineVersion;
	TextTranslation appVersionTranslation, engineVersionTranslation, versionTextTranslation, scenarioVersionTextTranslation;
	ProjectItem[] projectItems;
	public GameObject fileItemPrefab, warningPanel;
	public RectTransform itemContainer;
	public Button nextButton, cancelButton;
	public GameObject campaignWarning;
	public int selectedIndex = -1;

	TitleMetaData titleMetaData;
	TitleManager tm;

	public void ActivateScreen( TitleMetaData metaData )
	{
		LanguageManager.AddSubscriber(onUpdateTranslation);

		titleMetaData = metaData;

		tm = FindObjectOfType<TitleManager>();
		tm.ClearScenarioImage();

		gameObject.SetActive( true );
		warningPanel.SetActive( false );
		cancelButton.interactable = true;

		for ( int i = 0; i < fileItemButtons.Count; i++ )
			fileItemButtons[i].ResetColor();

		appVersionTranslation = appVersion.gameObject.GetComponent<TextTranslation>();
		appVersionTranslation.Change("journey.text.AppVersion", "App Version: " + Bootstrap.AppVersion, new List<string>{ Bootstrap.AppVersion });
		engineVersionTranslation = engineVersion.gameObject.GetComponent<TextTranslation>();
		engineVersionTranslation.Change("journey.text.FormatVersion", "Scenario Format Version: " + Bootstrap.FormatVersion, new List<string>{Bootstrap.FormatVersion});
		versionTextTranslation = versionText.gameObject.GetComponent<TextTranslation>();
		scenarioVersionTextTranslation = scenarioVersionText.gameObject.GetComponent<TextTranslation>();

		//appVersion.text = "App Version: " + Bootstrap.AppVersion;
		//engineVersion.text = "Scenario Format Version: " + Bootstrap.FormatVersion;
		nameText.text = "";
		scenarioVersionText.text = "";
		fileText.text = "";
		versionText.text = "";
		collectionsText.text = "";

		finalFader.DOFade( 0, .5f );
	}

	public void AddScenarioPrefabs()
	{
		var scenarios = FileManager.GetProjects().ToArray();
		var campaigns = FileManager.GetCampaigns().ToArray();
		projectItems = campaigns.Concat(scenarios).ToArray();

		for (int i = 0; i < projectItems.Length; i++)
		{
			var go = Instantiate(fileItemPrefab, itemContainer).GetComponent<FileItemButton>();
			go.transform.localPosition = new Vector3(0, (-110 * i));

			go.Init( i, projectItems[i].Translated("scenario.scenarioName", projectItems[i].Title),
				string.Join(" ", projectItems[i].collections.Select(c => Collection.FromID(c).FontCharacter)), 
				projectItems[i].projectType, ( index ) => OnSelectQuest( index ) );
			fileItemButtons.Add( go );
		}
		itemContainer.sizeDelta = new Vector2( 772, fileItemButtons.Count * 110 );
	}

	public void UpdateScenarioPrefabsAndSelectedQuest()
	{
		//Update the titles on the buttons
		for (int i = 0; i < projectItems.Length; i++)
		{
			FileItemButton go = fileItemButtons[i];
			go.title.text = projectItems[i].Translated("scenario.scenarioName", projectItems[i].Title);
		}

		//Update the title in the panel for the selected quest
		if(selectedIndex >= 0)
        {
			nameText.text = projectItems[selectedIndex].Translated("scenario.scenarioName", projectItems[selectedIndex].Title);
		}
	}

	public void onUpdateTranslation()
    {
		UpdateScenarioPrefabsAndSelectedQuest();
    }

	public void OnSelectQuest( int index )
	{
		selectedIndex = index;
		warningPanel.SetActive( false );
		campaignWarning.SetActive( false );

		for ( int i = 0; i < fileItemButtons.Count; i++ )
		{
			if ( i != index )
				fileItemButtons[i].ResetColor();
		}

		//fill in file info
		nameText.text = projectItems[index].Translated("scenario.scenarioName", projectItems[index].Title);
		if ( projectItems[index].projectType == ProjectType.Standalone )
			fileText.text = projectItems[index].fileName;
		else
			fileText.text = projectItems[index].campaignDescription;
		collectionsText.text = string.Join(" ", projectItems[index].collections.Select(c => Collection.FromID(c).FontCharacter));
		//projectItems[index].collections;
		//versionText.text = "File Version: " + projectItems[index].fileVersion;
		versionTextTranslation.Change("journey.text.FileVersion", "File Version: " + projectItems[index].fileVersion, new List<string> { projectItems[index].fileVersion });
		scenarioVersionTextTranslation.Change("journey.text.ScenarioVersion", "Scenario Version: " + projectItems[index].scenarioVersion, new List<string> { projectItems[index].scenarioVersion });

		//check version
		if ( projectItems[index].fileVersion != Bootstrap.FormatVersion )
			warningPanel.SetActive( true );

        //Set cover image
        tm.LoadScenarioImage(projectItems[index].coverImage);

		//check if it's a campaign without a save slot
		if ( projectItems[index].projectType == ProjectType.Campaign && titleMetaData.saveStateIndex == -1 )
		{
			campaignWarning.SetActive( true );
			return;
		}

		nextButton.interactable = true;
		titleMetaData.projectItem = projectItems[index];
		//Debug.Log( selectedJourney.fileName );
	}

	public void OnNext()
	{
		nextButton.interactable = false;
		cancelButton.interactable = false;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			selectHeroes.ActivateScreen( titleMetaData );
		} );
	}

	public void OnCancel()
	{
		nextButton.interactable = false;
		cancelButton.interactable = false;
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			tm.ClearScenarioImage();
			gameObject.SetActive( false );
			selectSaveSlot.ActivateScreen( titleMetaData );
		} );
	}
}
