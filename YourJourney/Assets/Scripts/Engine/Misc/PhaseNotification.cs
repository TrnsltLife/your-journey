using UnityEngine;
using DG.Tweening;
using TMPro;

public class PhaseNotification : MonoBehaviour
{
	public GameObject rootObject;
	public CanvasGroup cg;
	public TextMeshProUGUI msgText;

	public void Show( string msg )
	{
		Scenario.Chronicle(msg);
		rootObject.gameObject.SetActive( true );
		msgText.text = msg;
		cg.DOFade( 1, .5f );
		GlowTimer.SetTimer( 2, () => Hide() );
	}

	void Hide()
	{
		cg.DOFade( 0, .5f ).OnComplete( () => rootObject.gameObject.SetActive( false ) );
	}
}
