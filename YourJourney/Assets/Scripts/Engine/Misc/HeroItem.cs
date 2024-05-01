using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static LanguageManager;
public class HeroItem : MonoBehaviour
{
	public int heroIndex;
	public Text heroNameText, thresholdText;
	public Button dButton, fButton;
	public Image skullImage;
	public Image portraitImage;
	[HideInInspector]
	public PartyPanel pPanel;

	CanvasGroup cg;

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
	}

	public void OnDamageFinalStand()
	{
		pPanel.ToggleVisible(false);
		FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowYesNo(Translate("stand.text.ConfirmStand", "{0}: Do you wish to perform a Last Stand against {1}?",
			new List<string> { Bootstrap.gameStarter.heroes[heroIndex], "<font=\"Icon\">D</font> " + Translate("damage." + FinalStand.Damage.ToString(), FinalStand.Damage.ToString()) }), res =>
		{
			if (res.btn1)
			{
				FindObjectOfType<InteractionManager>().GetNewDamagePanel().ShowFinalStand(Bootstrap.lastStandCounter[heroIndex], FinalStand.Damage, (pass) =>
				{
					if (!pass)
					{
						Bootstrap.isDead[heroIndex] = true;
						int deadHeroes = Bootstrap.isDead.Where(x => x).Count();
						Engine.FindEngine().triggerManager.FireTrigger("Last Stand Failed " + deadHeroes); //a trigger fired when the Last Stand Failed, e.g. "Last Stand Failed 1"
					}
					else
                    {
						Engine.FindEngine().triggerManager.FireTrigger("Last Stand x" + Bootstrap.lastStandCounter[heroIndex]); //a trigger fired when the Last Stand Succeeded at this level for the first time. The number indicates this hero has succeeded in that many Last Stands, e.g. "Last Stand x2"
					}
					Bootstrap.lastStandCounter[heroIndex]++;
					UpdateUI();
					pPanel.ToggleVisible(false);
				});
			}
			else
            {
				pPanel.ToggleVisible(true);
			}
		});
	}

	public void OnFearFinalStand()
	{
		pPanel.ToggleVisible(false);
		FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowYesNo(Translate("stand.text.ConfirmStand", "{0}: Do you wish to perform a Last Stand against {1}?",
			new List<string> { Bootstrap.gameStarter.heroes[heroIndex], "<font=\"Icon\">F</font> " + Translate("damage." + FinalStand.Fear.ToString(), FinalStand.Fear.ToString()) }), res =>
		{
			if (res.btn1)
			{
				FindObjectOfType<InteractionManager>().GetNewDamagePanel().ShowFinalStand( Bootstrap.lastStandCounter[heroIndex], FinalStand.Fear, ( pass ) =>
				{
					if (!pass)
					{
						Bootstrap.isDead[heroIndex] = true;
						int deadHeroes = Bootstrap.isDead.Where(x => x).Count();
						Engine.FindEngine().triggerManager.FireTrigger("Last Stand Failed " + deadHeroes); //a trigger fired when the Last Stand Failed, e.g. "Last Stand Failed 1"
					}
					else
                    {
						Engine.FindEngine().triggerManager.FireTrigger("Last Stand x" + Bootstrap.lastStandCounter[heroIndex]); //a trigger fired when the Last Stand Succeeded at this level for the first time. The number indicates this hero has succeeded in that many Last Stands, e.g. "Last Stand x2"
					}
					Bootstrap.lastStandCounter[heroIndex]++;
					UpdateUI();
					pPanel.ToggleVisible( false );
				} );
			}
			else
            {
				pPanel.ToggleVisible(true);
			}
		});
	}

	public void UpdateUI()
	{
		if ( heroIndex >= Bootstrap.gameStarter.heroes.Length )
			return;

		//skullImage.gameObject.SetActive( Bootstrap.lastStandCounter[heroIndex] > 1 );
		if ( Bootstrap.lastStandCounter[heroIndex] == 1 )
		{
			skullImage.color = new Color( 1, 1, 1, .05f );
			thresholdText.gameObject.SetActive( false );
		}
		else
		{
			skullImage.color = Color.white;
			thresholdText.gameObject.SetActive( true );
		}

		thresholdText.text = Bootstrap.lastStandCounter[heroIndex].ToString();
		heroNameText.text = Bootstrap.gameStarter.heroes[heroIndex];

		//Load portrait image based on the portrait index, e.g p0.png or p63.png
		Sprite portraitSprite = Resources.Load<Sprite>("Images/Portraits/p" + Bootstrap.gameStarter.heroesIndex[heroIndex]);
		portraitImage.GetComponent<Image>().sprite = portraitSprite;


		dButton.interactable = !Bootstrap.isDead[heroIndex];
		fButton.interactable = !Bootstrap.isDead[heroIndex];
		if ( Bootstrap.isDead[heroIndex] )
		{
			cg.alpha = .25f;
			skullImage.color = Color.red;
		}
		else
		{
			cg.alpha = 1;
		}
	}
}
