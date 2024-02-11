using System;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class StoryBox : MonoBehaviour
{
	public CanvasGroup canvasGroup;
	public TextMeshProUGUI storyText;
	//public RectTransform itemContainer;
	//public Image fader;

	RectTransform rect;
	//Vector2 ap;
	Vector3 sp;
	Action action;

	private void Awake()
	{
		rect = canvasGroup.gameObject.GetComponent<RectTransform>();
		sp = canvasGroup.gameObject.transform.position;
	}

	public void Show( string t, Action a )
	{
		rect = canvasGroup.gameObject.GetComponent<RectTransform>();
		sp = canvasGroup.gameObject.transform.position;

		gameObject.SetActive( true );

		action = a;
		canvasGroup.alpha = 0;
		canvasGroup.gameObject.SetActive( true );
		canvasGroup.DOFade( 1, .5f );

		//rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		canvasGroup.gameObject.transform.DOMoveY( sp.y, .75f );

		SetText( t );
	}

	void SetText( string t )
	{
		storyText.text = t;
	}


	public void OnOK()
	{
		action?.Invoke();

		canvasGroup.DOFade( 0, .25f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			canvasGroup.gameObject.SetActive( false );
		} );
	}
}
