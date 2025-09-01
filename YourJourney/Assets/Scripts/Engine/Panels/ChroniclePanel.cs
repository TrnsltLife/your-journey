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
	public Button nextRoundButton;
	public Button prevRoundButton;

	//CanvasGroup group;
	RectTransform rect;
	Vector3 sp;
	Vector2 ap;

	int currentRound = 0;
	List<List<string>> chronicles = new List<List<string>>();

	private void CalculatePanelPosition()
	{
		rect = canvasGroup.gameObject.GetComponent<RectTransform>();
		sp = canvasGroup.gameObject.transform.position;
	}

	void Awake()
	{
		CalculatePanelPosition();
	}

	public void Show(List<List<string>> chronicles)
	{
		this.chronicles = chronicles;

		CalculatePanelPosition();

		FindObjectOfType<TileManager>().ToggleInput( true );

		gameObject.SetActive( true );

		canvasGroup.alpha = 0;
		canvasGroup.gameObject.SetActive(true);
		canvasGroup.DOFade(1, .5f);

		//rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		canvasGroup.gameObject.transform.DOMoveY(sp.y, .75f);

		UpdateText();
		UpdateButtons();
	}

	void UpdateText()
	{
        SetText("[" + (currentRound + 1) + "]\n" + string.Join("\n<align=center><font=\"Icon\">L  L  L</font></align>\n", chronicles[currentRound]));
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

	public void OnPrev()
	{
		if (currentRound > 0)
		{
			currentRound--;
            UpdateText();
			UpdateButtons();
        }
    }

	public void OnNext()
	{
		if(currentRound < (chronicles.Count - 1))
		{
			currentRound++;
            UpdateText();
            UpdateButtons();
        }
    }

	public void UpdateButtons()
	{
		if(currentRound == 0)
		{
			prevRoundButton.enabled = false;
		}
		else
		{
			prevRoundButton.enabled = true;
		}

		if(currentRound == (chronicles.Count - 1))
		{
			nextRoundButton.enabled = false;
		}
		else
		{
			nextRoundButton.enabled = true;
		}
	}
}
