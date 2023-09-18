using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LanguageManager;

public class DialogPanel : MonoBehaviour
{
	public TextMeshProUGUI mainText, btn1Text, btn2Text, btn3Text, cancelText, dummy;
	public Button btn1, btn2, btn3, cancelBtn;
	public CanvasGroup overlay;

	CanvasGroup group;

	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action<InteractionResult> buttonActions;
	Transform root;

	DialogInteraction dialogInteraction;

	private void CalculatePanelPosition()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		//gameObject.SetActive( false );
		sp = transform.position;
		ap = rect.anchoredPosition;
	}

	private void Awake()
	{
		CalculatePanelPosition();
		root = transform.parent;
		mainText.alignment = TextAlignmentOptions.Top; //We set this here instead of the editor to make it easier to see mainText and dummy are lined up with each other in the editor
		dummy.alignment = TextAlignmentOptions.Top;
	}

	public void Show( DialogInteraction di, Action<InteractionResult> actions = null )
	{
		CalculatePanelPosition();
		gameObject.SetActive( true );
		FindObjectOfType<TileManager>().ToggleInput( true );

		btn1.gameObject.SetActive( !string.IsNullOrEmpty( di.choice1 ) );
		btn2.gameObject.SetActive( !string.IsNullOrEmpty( di.choice2 ) );
		btn3.gameObject.SetActive( !string.IsNullOrEmpty( di.choice3 ) );
		cancelBtn.gameObject.SetActive( true );

		if ( !btn1.gameObject.activeInHierarchy )
			di.c1Used = true;
		if ( !btn2.gameObject.activeInHierarchy )
			di.c2Used = true;
		if ( !btn3.gameObject.activeInHierarchy )
			di.c3Used = true;

		btn1.interactable = !di.c1Used;
		btn2.interactable = !di.c2Used;
		btn3.interactable = !di.c3Used;

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		group.alpha = 0;
		btn1Text.text = Interpret(di.TranslationKey("choice1"), di.choice1);
		btn2Text.text = Interpret(di.TranslationKey("choice2"), di.choice2);
		btn3Text.text = Interpret(di.TranslationKey("choice3"), di.choice3);
		buttonActions = actions;

		if ( !di.isDone )
			SetText( Interpret(di.TranslationKey("eventText"), di.eventBookData.pages[0]) );
		else
			SetText( Interpret(di.TranslationKey("persistentText"), di.persistentText) );


		Scenario.Chronicle(mainText.text + "\n[" + btn1Text.text + "] [" + btn2Text.text + "] [" + btn3Text.text + "]");

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		group.DOFade( 1, .5f );

		dialogInteraction = di;
	}

	void SetText( string t )
	{
		mainText.text = t;
		dummy.text = t;

		float preferredHeight = dummy.preferredHeight; //Dummy text (which must be active) is used to find the correct preferredHeight so it can then be set on the mainText which is in a scroll view viewport
		dummy.text = ""; //After we have the height we clear dummy.text so it doesn't show up anymore

		var dialogHeight = Math.Min(525, 30 + preferredHeight + 30);

		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dialogHeight);
	}

	public void Hide()
	{
		group.DOFade( 0, .25f );
		overlay.DOFade( 0, .25f ).OnComplete( () =>
		{
			FindObjectOfType<TileManager>().ToggleInput( false );
			Destroy( root.gameObject );
		} );
	}

	void DisableButtons()
	{
		btn1.gameObject.SetActive( false );
		btn2.gameObject.SetActive( false );
		btn3.gameObject.SetActive( false );
		cancelBtn.gameObject.SetActive( false );
	}

	public void OnBtn1()
	{
		DisableButtons();

		dialogInteraction.c1Used = true;
		if ( dialogInteraction.c1Used && dialogInteraction.c2Used && dialogInteraction.c3Used )
			dialogInteraction.isDone = true;

		buttonActions?.Invoke( new InteractionResult() { btn1 = true, removeToken = false } );
		Hide();
	}

	public void OnBtn2()
	{
		DisableButtons();

		dialogInteraction.c2Used = true;
		if ( dialogInteraction.c1Used && dialogInteraction.c2Used && dialogInteraction.c3Used )
			dialogInteraction.isDone = true;

		buttonActions?.Invoke( new InteractionResult() { btn2 = true, removeToken = false } );
		Hide();
	}

	public void OnBtn3()
	{
		DisableButtons();

		dialogInteraction.c3Used = true;
		if ( dialogInteraction.c1Used && dialogInteraction.c2Used && dialogInteraction.c3Used )
			dialogInteraction.isDone = true;

		buttonActions?.Invoke( new InteractionResult() { btn3 = true, removeToken = false } );
		Hide();
	}

	public void OnCancel()
	{
		DisableButtons();

		buttonActions?.Invoke( new InteractionResult() { canceled = true, removeToken = false } );
		Hide();
	}
}
