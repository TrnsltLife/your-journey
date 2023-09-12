using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using static LanguageManager;

public class LorePanel : MonoBehaviour
{
	public Text loreText, xpText, threatText;
	public Transform panelTF;
	public Canvas canvas;

	bool busy = false;
	int loreacc = 0, xpacc = 0, threatacc = 0;
	float scalar;
	private void Start()
	{
		scalar = canvas.scaleFactor;
	}

	public void AddPenalty( int threatamount )
	{
		if ( threatamount == 0 )
			return;

		FindObjectOfType<EndTurnButton>().AddThreatNoTriggering( threatamount );

		if ( busy )//accumulate if already showing
			threatacc += threatamount;
		else
			threatacc = threatamount;

		threatText.text = Translate("reward.text.ThreatIncreased", "Threat Increased By\r\n" + threatacc,
			new List<string> { threatacc.ToString() });

		if ( !busy )
		{
			busy = true;

			var s = DOTween.Sequence();
			s.Append( panelTF.DOMoveX( 1920f * scalar, .25f ) );
			s.Append( panelTF.DOMoveX( ( 1920f * scalar ) + ( 405f * scalar ), .25f ).SetDelay( 4 ) );
			s.OnComplete( () => busy = false );
			s.Play();
		}
	}

	public void AddReward( int loreamount, int xpamount = 0, int threatamount = 0 )
	{
		if ( loreamount == 0 && xpamount == 0 && threatamount == 0 )
			return;

		FindObjectOfType<EndTurnButton>().RemoveThreat( threatamount );

		scalar = canvas.scaleFactor;

		Bootstrap.loreCount += loreamount;
		Bootstrap.xpCount += xpamount;

		if ( busy )//accumulate if already showing
		{
			loreacc += loreamount;
			xpacc += xpamount;
			threatacc += threatamount;
		}
		else
		{
			loreacc = loreamount;
			xpacc = xpamount;
			threatacc = threatamount;
		}
		loreText.text = Translate("reward.text.LoreEarned", "You Have Earned\r\n" + loreacc + " Lore.",
			new List<string> { loreacc.ToString() });
		xpText.text = Translate("reward.text.XPEarned", "You Have Earned\r\n" + xpacc + " XP.",
			new List<string> { xpacc.ToString() });
		threatText.text = Translate("reward.text.ThreatReduced", "Threat Reduced By\r\n" + threatacc,
			new List<string> { threatacc.ToString() });

		if ( !busy )
		{
			busy = true;

			var s = DOTween.Sequence();
			s.Append( panelTF.DOMoveX( 1920f * scalar, .25f ) );
			s.Append( panelTF.DOMoveX( ( 1920f * scalar ) + ( 405f * scalar ), .25f ).SetDelay( 4 ) );
			s.OnComplete( () => busy = false );
			s.Play();
		}
	}
}
