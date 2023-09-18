using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static LanguageManager;

public class ChroniclePanel : MonoBehaviour
{
	public CanvasGroup overlay;
	public PartyPanel partyPanel;
	public CanvasGroup canvasGroup;
	public TextMeshProUGUI chronicleText;

	//CanvasGroup group;
	RectTransform rect;
	Vector3 sp;
	Vector2 ap;

	private void CalculatePanelPosition()
	{
		rect = canvasGroup.gameObject.GetComponent<RectTransform>();
		sp = canvasGroup.gameObject.transform.position;
	}

	void Awake()
	{
		CalculatePanelPosition();
	}

	public void Show(List<string> chronicle)
	{
		CalculatePanelPosition();

		FindObjectOfType<TileManager>().ToggleInput( true );

		gameObject.SetActive( true );

		canvasGroup.alpha = 0;
		canvasGroup.gameObject.SetActive(true);
		canvasGroup.DOFade(1, .5f);

		//rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		canvasGroup.gameObject.transform.DOMoveY(sp.y, .75f);

		SetText(string.Join("\n<align=center><font=\"Icon\"><b>L  L  L</b></font></align>\n", chronicle));
	}

	void SetText(string t)
	{
		chronicleText.text = t;
	}

	public void ToggleVisible( bool visible )
	{
		gameObject.SetActive( visible );
	}

	public void Hide()
	{
		canvasGroup.DOFade( 0, .25f );
		overlay.DOFade( 0, .25f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			partyPanel.ToggleVisible(true);
		} );
	}

	public void OnClose()
	{
		Hide();
	}
}
