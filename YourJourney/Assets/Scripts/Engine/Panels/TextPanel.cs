using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Text.RegularExpressions;
using TMPro;
using static LanguageManager;

public class TextPanel : MonoBehaviour
{
	public TextMeshProUGUI mainText, btn1Text, btn2Text, btn2ActionText, btnSingleText, dummy;
	public GameObject btn1, btn2;
	public GameObject buttonSingle;
	public GameObject actionIcon;
	public CanvasGroup overlay;

	CanvasGroup group;

	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action btnSingleAction;
	Action<InteractionResult> buttonActions;
	Transform root;

	private void CalculatePanelPosition()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		gameObject.SetActive(false);
		sp = transform.position;
		ap = rect.anchoredPosition;
	}

	void Awake()
	{
		CalculatePanelPosition();
		root = transform.parent;
		mainText.alignment = TextAlignmentOptions.Top; //We set this here instead of the editor to make it easier to see mainText and dummy are lined up with each other in the editor
		dummy.alignment = TextAlignmentOptions.Top;
	}

	void Show( string t, string btn1, string btn2, ButtonIcon icon = ButtonIcon.None, Action<InteractionResult> actions = null )
	{
		CalculatePanelPosition();
		FindObjectOfType<TileManager>().ToggleInput( true );

		this.btn1.SetActive( true );
		this.btn2.SetActive( true );
		buttonSingle.SetActive( false );

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		btn1Text.text = btn1;
		btn2Text.text = btn2;
		buttonActions = actions;

		actionIcon.SetActive( false );
		btn2ActionText.text = "";
		switch ( icon )
		{
			//If a ButtonIcon is enabled, disable the full-width btn2Text object and enable the indented btn2ActionText while also setting its text value
			case ButtonIcon.Action:
				actionIcon.SetActive( true );
				btn2Text.text = "";
				btn2ActionText.text = btn2;
				break;
		}
		SetText( t );

		Scenario.Chronicle(t + "\n[" + btn1 + "] [" +
				((ButtonIcon.Action  == icon) ? "<font=\"Icon\">I</font>" : "") +
				btn2 + "]"
			);

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		group.DOFade( 1, .5f );
	}

	public void ShowCustom( string t, string btn1, string btn2, Action<InteractionResult> actions = null )
	{
		Show( t, btn1, btn2, ButtonIcon.None, actions );
	}

	public void ShowYesNo( string s, Action<InteractionResult> actions = null )
	{
		Show( s, Translate("dialog.button.Yes", "Yes"), Translate("dialog.button.No", "No"), ButtonIcon.None, actions );
	}

	public void ShowOkContinue( string s, ButtonIcon icon, Action action = null )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		btn1.SetActive( false );
		btn2.SetActive( false );
		buttonSingle.SetActive( true );

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );
		gameObject.SetActive( true );

		btnSingleText.text = Translate("dialog.button." + icon.ToString(), icon.ToString());
		btnSingleAction = action;
		SetText( s );

		Scenario.Chronicle(s + "\n[" + btnSingleText.text + "]");

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		group.DOFade( 1, .5f );
	}

	/// <summary>
	/// Shows a dialog when QUERYING a token
	/// </summary>
	public void ShowQueryInteraction( IInteraction it, string btnName, Action<InteractionResult> actions )
	{
		Show( Interpret(it.TranslationKey("flavorText"), it.textBookData.pages[0]), Translate("dialog.button.Cancel", "Cancel"), btnName, ButtonIcon.Action, actions );
	}

	/// <summary>
	/// ask whether to explore the tile after clicking Explore token
	/// </summary>
	public void ShowQueryExploration( Action<InteractionResult> actions )
	{
		Show( Translate("dialog.text.ExploreTile", "Explore this tile?"), Translate("dialog.button.Yes", "Yes"), Translate("dialog.button.No", "No"), ButtonIcon.None, actions );
	}

	/// <summary>
	/// Shows dialog with an interaction
	/// </summary>
	public void ShowTextInteraction( IInteraction it, Action actions )
	{
		ShowOkContinue( Interpret(it.TranslationKey("eventText"), it.eventBookData.pages[0]), ButtonIcon.Continue, actions );
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

	/// <summary>
	/// JUST remove the box, don't fade overlay, don't toggle input
	/// </summary>
	public void RemoveBox()
	{
		group.DOFade( 0, .25f ).OnComplete( () =>
		{
			Destroy( root.gameObject );
		} );
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

	public void OnBtn1()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		buttonSingle.SetActive( false );

		buttonActions?.Invoke( new InteractionResult() { btn1 = true } );
		Hide();
	}

	public void OnBtn2()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		buttonSingle.SetActive( false );

		buttonActions?.Invoke( new InteractionResult() { btn2 = true } );
		Hide();
	}

	public void OnBtnSingle()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		buttonSingle.SetActive( false );

		btnSingleAction?.Invoke();
		Hide();
	}
}
