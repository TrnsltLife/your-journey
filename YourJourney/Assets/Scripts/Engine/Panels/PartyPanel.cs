using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static LanguageManager;

public class PartyPanel : MonoBehaviour
{
	public CanvasGroup overlay;
	public Text loreText, xpText, diffText;
	TextTranslation diffTextTranslation;
	public HeroItem[] heroItems;

	CanvasGroup group;
	Color[] testColors;
	RectTransform rect;
	Vector3 sp;
	Vector2 ap;

	private void CalculatePanelPosition()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		sp = transform.position;
		ap = rect.anchoredPosition;
	}

	void Awake()
	{
		CalculatePanelPosition();
		testColors = new Color[5];
		//mit/agi/wis/spi/wit
		testColors[0] = Color.red;
		testColors[1] = Color.green;
		testColors[2] = Color.HSVToRGB( 300f / 360f, 1, .5f );
		testColors[3] = Color.HSVToRGB( 207f / 360f, .61f, .71f );
		testColors[4] = Color.yellow;
		diffTextTranslation = diffText.GetComponent<TextTranslation>();
		diffTextTranslation.Change("heroes.button." + Bootstrap.gameStarter.difficulty.ToString(), Bootstrap.gameStarter.difficulty.ToString());
	}

	public void Show()
	{
		CalculatePanelPosition();
		if ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase
	|| FindObjectOfType<ProvokeMessage>().provokeMode )
			return;

		FindObjectOfType<TileManager>().ToggleInput( true );

		gameObject.SetActive( true );

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		diffText.text = Bootstrap.gameStarter.difficulty.ToString();
		int totalLore = Bootstrap.loreCount + Bootstrap.gameStarter.loreStartValue;
		int totalXP = Bootstrap.xpCount + Bootstrap.gameStarter.xpStartValue;
		loreText.text = Translate("party.text.Lore", "Lore: " + totalLore.ToString(), new List<string> { totalLore.ToString() });
		xpText.text = Translate("party.text.XP", "XP: " + totalXP.ToString(), new List<string> { totalXP.ToString() });

		foreach ( HeroItem go in heroItems )
		{
			go.gameObject.SetActive( false );
			go.pPanel = this;
		}

		for ( int i = 0; i < Bootstrap.gameStarter.heroes.Length; i++ )
		{
			heroItems[i].gameObject.SetActive( true );
			heroItems[i].UpdateUI();
		}

		group.DOFade( 1, .5f );
	}

	public void ToggleVisible( bool visible )
	{
		gameObject.SetActive( visible );
	}

	public void Hide()
	{
		group.DOFade( 0, .25f );
		overlay.DOFade( 0, .25f ).OnComplete( () =>
		{
			FindObjectOfType<TileManager>().ToggleInput( false );
			gameObject.SetActive( false );
		} );
	}

	public void OnClose()
	{
		Hide();
	}

	public void OnDifficulty()
	{
		if ( Bootstrap.gameStarter.difficulty == Difficulty.Adventure )
			Bootstrap.gameStarter.difficulty = Difficulty.Normal;
		else if ( Bootstrap.gameStarter.difficulty == Difficulty.Normal )
			Bootstrap.gameStarter.difficulty = Difficulty.Hard;
		else if ( Bootstrap.gameStarter.difficulty == Difficulty.Hard )
			Bootstrap.gameStarter.difficulty = Difficulty.Adventure;
		//diffText.text = Bootstrap.gameStarter.difficulty.ToString();
		diffTextTranslation.Change("heroes.button." + Bootstrap.gameStarter.difficulty.ToString(), Bootstrap.gameStarter.difficulty.ToString());

		//set campaign state difficulty, if it exists
		if ( Bootstrap.campaignState != null )
			Bootstrap.campaignState.difficulty = Bootstrap.gameStarter.difficulty;
	}
}
