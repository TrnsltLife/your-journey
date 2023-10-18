using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LanguageManager;

public class StatTestPanel : MonoBehaviour
{
	public TextMeshProUGUI mainText, abilityText, dummy;
	public Text counterText;
	public GameObject btn1, btn2, continueBtn, submitBtn;
	public CanvasGroup overlay;
	public GameObject progressRoot;

	CanvasGroup group;
	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action<InteractionResult> buttonActions;
	Transform root;
	int value;
	StatTestInteraction statTestInteraction;

	private void CalculatePanelPosition()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		gameObject.SetActive(false);
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

	public void Show( StatTestInteraction testInteraction, Action<InteractionResult> actions )
	{
		CalculatePanelPosition();
		FindObjectOfType<TileManager>().ToggleInput( true );

		statTestInteraction = testInteraction;

		btn1.SetActive( testInteraction.passFail || !testInteraction.isCumulative );
		btn2.SetActive( testInteraction.passFail || !testInteraction.isCumulative );
		progressRoot.SetActive( !testInteraction.passFail && testInteraction.isCumulative );
		continueBtn.SetActive( false );

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonActions = actions;


		if (testInteraction.isCumulative && !testInteraction.passFail)
		{
			abilityText.text = "Test " + AbilityUtility.ColoredText(testInteraction.testAttribute, 42) + " " + Translate("stat." + testInteraction.testAttribute.ToString(), testInteraction.testAttribute.ToString());
			string ability1 = AbilityUtility.ColoredText(testInteraction.testAttribute, 42) + " " + Translate("stat." + testInteraction.testAttribute.ToString(), testInteraction.testAttribute.ToString());
			if (!testInteraction.noAlternate)//use alternate test
			{
				//abilityText.text += " or " + AbilityUtility.ColoredText(testInteraction.altTestAttribute, 42) + " " + testInteraction.altTestAttribute.ToString();
				string ability2 = AbilityUtility.ColoredText(testInteraction.altTestAttribute, 42) + " " + Translate("stat." + testInteraction.altTestAttribute.ToString(), testInteraction.altTestAttribute.ToString());
				abilityText.text = Translate("test.text.TestStatOrStat", "Test {0} or {1}; {2}.", new List<string> { ability1, ability2 });
			}
			else
            {
				abilityText.text = Translate("test.text.TestStat", "Test {0}; {1}.", new List<string> { ability1 } );
            }
			//abilityText.text += ".";
		}
		else
		{
			//abilityText.text = "Test " + AbilityUtility.ColoredText(testInteraction.testAttribute, 42) + " " + testInteraction.testAttribute.ToString();
			string ability1 = AbilityUtility.ColoredText(testInteraction.testAttribute, 42) + " " + Translate("stat." + testInteraction.testAttribute.ToString(), testInteraction.testAttribute.ToString());
			if (!testInteraction.noAlternate)
			{
				//abilityText.text += " or " + AbilityUtility.ColoredText(testInteraction.altTestAttribute, 42) + " " + testInteraction.altTestAttribute.ToString();
				string ability2 = AbilityUtility.ColoredText(testInteraction.altTestAttribute, 42) + " " + Translate("stat." + testInteraction.altTestAttribute.ToString(), testInteraction.altTestAttribute.ToString());
				abilityText.text = Translate("test.text.TestStatOrStatValue", "Test {0} or {1}; {2}.", new List<string> { ability1, ability2, testInteraction.successValue.ToString() });
			}
			else
			{
				abilityText.text = Translate("test.text.TestStatValue", "Test {0}; {1}.", new List<string> { ability1, testInteraction.successValue.ToString() });
			}
			//abilityText.text += "; " + testInteraction.successValue + ".";
		}


		//if it's cumulative (and not simple pass/fail) and already started, show progress text
		if ((testInteraction.isCumulative && !testInteraction.passFail) && testInteraction.accumulatedValue >= 0)
		{
			//TODO At some point we might add a progressEventBookData to go here in addition to the result progressBookData that's in InteractionManager. Need to update the editor first.
			string progressText = Interpret(testInteraction.TranslationKey("eventText"), testInteraction.eventBookData.pages[0]);
			SetText(progressText);
			Scenario.Chronicle(progressText + "\n\n" + abilityText.text);
		}
		else//otherwise show normal event text
		{
			string eventText = Interpret(testInteraction.TranslationKey("eventText"), testInteraction.eventBookData.pages[0]);
			SetText(eventText);
			Scenario.Chronicle(eventText + "\n\n" + abilityText.text);
		}

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		//acc value starts at -1, so set it to minimum of 0 to show the event has started
		testInteraction.accumulatedValue = Math.Max( 0, testInteraction.accumulatedValue );

		counterText.text = value.ToString();

		group.DOFade( 1, .5f );
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

	void SetText( string t )
	{
		mainText.text = t;
		dummy.text = t;

		int hmax = 525;
		if (statTestInteraction.isCumulative && !statTestInteraction.passFail)
			hmax = 410;

		float preferredHeight = dummy.preferredHeight; //Dummy text (which must be active) is used to find the correct preferredHeight so it can then be set on the mainText which is in a scroll view viewport
		dummy.text = ""; //After we have the height we clear dummy.text so it doesn't show up anymore

		var dialogHeight = Math.Min(hmax, 30 + preferredHeight + 90);

		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dialogHeight);
	}

	public void OnAdd()
	{
		value++;
		counterText.text = value.ToString();
	}

	public void OnMinus()
	{
		value = Math.Max( 0, value - 1 );
		counterText.text = value.ToString();
	}

	public void OnSubmit()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		continueBtn.SetActive( false );

		Scenario.ChroniclePS("\n<font=\"Icon\">O</font>[" + submitBtn.GetComponentInChildren<TextMeshProUGUI>().text + " " + value.ToString() + "]");

		//use btn4 = true to signify this as a cumulative result
		buttonActions?.Invoke( new InteractionResult() { btn4 = statTestInteraction.isCumulative, value = value } );
		Hide();
	}

	public void OnSuccess()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		continueBtn.SetActive( false );

		int v = value;
		if ( statTestInteraction.passFail )
			v = 1;

		Scenario.ChroniclePS("\n<font=\"Icon\">O</font>[" + btn1.GetComponentInChildren<TextMeshProUGUI>().text + "]");

		buttonActions?.Invoke( new InteractionResult() { btn4 = statTestInteraction.isCumulative, success = true, value = v } );
		Hide();
	}

	public void OnFail()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		continueBtn.SetActive( false );

		int v = value;
		if ( statTestInteraction.passFail )
			v = -1;

		Scenario.ChroniclePS("<font=\"Icon\">O</font>[" + btn2.GetComponentInChildren<TextMeshProUGUI>().text + "]");

		buttonActions?.Invoke( new InteractionResult() { btn4 = statTestInteraction.isCumulative, success = false, value = v } );
		Hide();
	}

	public void OnContinue()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		continueBtn.SetActive( false );

		buttonActions?.Invoke( new InteractionResult() { } );
		Hide();
	}
}
