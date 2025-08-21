using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class NameGameDialog : MonoBehaviour
{
	public CanvasGroup canvasGroup;
	public TextMeshProUGUI gameName;

#if UNITY_ANDROID && !UNITY_EDITOR
	private TouchScreenKeyboard keyboard;
#endif

    RectTransform rect;
	Vector2 ap;
	Vector3 sp;
	Action<string> onYes = null;
	Action onNo = null;
	bool isChangingName;

	private void Awake()
	{
		rect = canvasGroup.gameObject.GetComponent<RectTransform>();
		ap = rect.anchoredPosition;
		sp = canvasGroup.gameObject.transform.position;
	}

	public void Show( Action<string> yes, Action no )
	{
		isChangingName = true;
		rect = canvasGroup.gameObject.GetComponent<RectTransform>();
		ap = rect.anchoredPosition;
		sp = canvasGroup.gameObject.transform.position;

		onYes = yes;
		onNo = no;
		gameName.text = "";

		canvasGroup.alpha = 0;
		canvasGroup.gameObject.SetActive( true );
		canvasGroup.DOFade( 1, .5f );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		canvasGroup.gameObject.transform.DOMoveY( sp.y, .75f );

#if UNITY_ANDROID && !UNITY_EDITOR
		keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, true );
		TouchScreenKeyboard.hideInput = true;
#endif
    }

    public void OnYes()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (keyboard != null )
		{
			gameName.text = keyboard.text;
		}
		keyboard = null;
#endif

        if ( string.IsNullOrEmpty( gameName.text.Trim() ) )
			return;

		isChangingName = false;
		onYes?.Invoke( gameName.text );

		canvasGroup.DOFade( 0, .25f ).OnComplete( () =>
		{
			canvasGroup.gameObject.SetActive( false );
		} );
	}

	public void OnNo()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		keyboard = null;
#endif

        isChangingName = false;
		onNo?.Invoke();

		canvasGroup.DOFade( 0, .25f ).OnComplete( () =>
		{
			canvasGroup.gameObject.SetActive( false );
		} );
	}

	private void Update()
	{
		if ( isChangingName )
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			if (keyboard != null )
			{
				gameName.text = keyboard.text;
			}
#else
            //if ( Input.GetKeyDown( KeyCode.Escape ) )
            //{
            //	isChangingName = false;
            //	gameName.color = Color.white;
            //	gameName.text = tempName;
            //	return;
            //}

            foreach (char c in Input.inputString)
            {
                if (c == '\b') // has backspace/delete been pressed?
                {
                    if (gameName.text.Length != 0)
                    {
                        gameName.text = gameName.text.Substring(0, gameName.text.Length - 1);
                    }
                }
                else if ((c == '\n') || (c == '\r')) // enter/return
                {
                    if (!string.IsNullOrEmpty(gameName.text.Trim()))
                    {
                        //valid string
                        OnYes();
                    }
                }
                else
                {
                    gameName.text += c;
                }
            }
#endif
        }
    }
}
