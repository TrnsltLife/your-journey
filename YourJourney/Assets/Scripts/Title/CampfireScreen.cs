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
	//public TextTranslation remainingPointsTextTranslation, skillsTranslationText, roleItemsTranslationText;

	TitleManager tm;
	TitleMetaData titleMetaData;

	public void ActivateScreen( TitleMetaData metaData )
	{
		titleMetaData = metaData;

		tm = FindObjectOfType<TitleManager>();

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
