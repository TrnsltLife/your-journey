using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using System;
using static LanguageManager;
using System.Collections.Generic;

public class CombatPanel : MonoBehaviour
{
	public RectTransform healthMeter;
	public Sprite selectedButton, unselectedButton;
	public GameObject large, armored, bloodthirsty, eliteBG, rangedIcon;
	public Text monsterName, damageText;
	public CanvasGroup canvasGroup;
	public MonsterItem[] monsterItems;
	public Button applyButton;
	public Button[] modifierButtons;
	public GameObject[] monsterImages;
	public GameObject[] monsterModifierLabels;
	public Image bannerIcon;

	CombatModify modifier;
	Monster monster = null;
	Guid monsterGUID;
	bool pierceSelected, lethalSelected, stunSelected, smiteSelected, sunderSelected, cleaveSelected;
	int damage;

	//pierce=ignore armor
	//smite=ignore sorcery
	//sunder=permanently reduce armor by 1 (before hits applied)
	//cleave=each enemy in group suffers full hits
	//lethal=if this attack reduces enemy health to at least half, kill it
	//stun=this attack exhausts the group.  if it's elite, also cannot counterattack

	void Start()
	{
		gameObject.SetActive( false );

		pierceSelected = lethalSelected = stunSelected = smiteSelected = sunderSelected = cleaveSelected = false;
	}

	/*
	public void UpdateSkins()
    {
		for(int i = 0; i<monsterImages.Length; i++)
        {
			UpdateSkin(i);
		}
	}
	*/

	public void UpdateSkin(int monsterIndex, int skinVariant = 0)
    {
		monsterImages[monsterIndex].GetComponent<Image>().overrideSprite = SkinsManager.SkinVariant(monsterIndex, skinVariant); //Set monster skin override based on current value in SkinsManager.monsterSkins array
	}

	public void SetBanner(Sprite sprite)
	{
		if (sprite != null)
		{
			bannerIcon.sprite = sprite;
			bannerIcon.gameObject.SetActive(true);
		}
		else
        {
			bannerIcon.gameObject.SetActive(false);
		}
	}

	public bool Show( Monster monster, Sprite bannerSprite, int skinVariant = 0 )
	{
		Debug.Log("CombatPanel.Show(" + monster.dataName + ")");
		UpdateSkin((int)monster.monsterType, skinVariant);
		SetBanner(bannerSprite);

		//remove any spawn markers, since player will have seen them by now
		var objs = FindObjectsOfType<SpawnMarker>();
		foreach ( var ob in objs )
		{
			if ( ob.name.Contains( "SPAWNMARKER" ) )
				Destroy( ob.gameObject );
		}

		//set the appropriate monster image
		for (int i = 0; i < monsterImages.Length; i++)
		{
			monsterImages[i].SetActive(false);
		}
		monsterImages[(int)monster.monsterType].SetActive( true );

		if ( monsterGUID == monster.GUID && gameObject.activeSelf )
		{
			Hide();
			return false;
		}
		else// if ( this.monster == null )
		{
			monsterGUID = monster.GUID;
			gameObject.SetActive( true );
			canvasGroup.alpha = 0;
			canvasGroup.DOFade( 1, .5f );
		}

		//Ranged
		rangedIcon.SetActive(monster.isRanged);

		FindObjectOfType<TileManager>().ToggleInput( true );

		pierceSelected = lethalSelected = stunSelected = smiteSelected = sunderSelected = cleaveSelected = false;

		foreach ( var btn in modifierButtons )
			btn.GetComponent<Image>().sprite = unselectedButton;

		//populate data
		eliteBG.SetActive( monster.isElite );
		applyButton.interactable = true;
		damage = 0;
		damageText.text = damage.ToString();
		this.monster = monster;
		foreach ( var m in monsterItems )
			m.Hide();
		for ( int i = 0; i < monster.count; i++ )
			monsterItems[i].Show( monster, i );
		//monsterName.text = monster.dataName;
		monsterName.text = Monster.MonsterNameObject(monster, monster.count);
		ShowModifierLabels(monster);
		//large.SetActive( monster.isLarge );
		//armored.SetActive( monster.isArmored );
		//bloodthirsty.SetActive( monster.isBloodThirsty );

		return true;
	}

	public void ShowModifierLabels(Monster monster)
    {
		large.SetActive(false);
		armored.SetActive(false);
		bloodthirsty.SetActive(false);

		for (int i = 0; i < Monster.MAX_MODIFIERS; i++)
        {
			monsterModifierLabels[i].SetActive(false);
        }

		for (int i=0; i<monster.modifierList.Count; i++)
        {
			Text t = monsterModifierLabels[i].GetComponent<Text>();
			Translate("combat.bonus." + monsterModifierLabels[i].name, monsterModifierLabels[i].name);
			t.text = monster.modifierList[i].name;
			monsterModifierLabels[i].SetActive(true);
        }
    }

	public void Hide( bool unselect = false )
	{
		if ( unselect )
			monsterGUID = Guid.NewGuid();
		foreach ( var m in monsterItems )
			m.Hide();
		canvasGroup.DOFade( 0, .5f ).OnComplete( () =>
		{
			FindObjectOfType<TileManager>().ToggleInput( false );
			gameObject.SetActive( false );
		} );
	}

	public void OnSmite( Button btn )
	{
		smiteSelected = !smiteSelected;
		btn.GetComponent<Image>().sprite = smiteSelected ? selectedButton : unselectedButton;

		DoDamage();
	}
	public void OnSunder( Button btn )
	{
		sunderSelected = !sunderSelected;
		btn.GetComponent<Image>().sprite = sunderSelected ? selectedButton : unselectedButton;

		DoDamage();
	}
	public void OnCleave( Button btn )
	{
		cleaveSelected = !cleaveSelected;
		btn.GetComponent<Image>().sprite = cleaveSelected ? selectedButton : unselectedButton;

		DoDamage();
	}
	public void OnPierce( Button btn )
	{
		pierceSelected = !pierceSelected;
		btn.GetComponent<Image>().sprite = pierceSelected ? selectedButton : unselectedButton;

		DoDamage();
	}
	public void OnLethal( Button btn )
	{
		lethalSelected = !lethalSelected;
		btn.GetComponent<Image>().sprite = lethalSelected ? selectedButton : unselectedButton;

		DoDamage();
	}
	public void OnStun( Button btn )
	{
		stunSelected = !stunSelected;
		btn.GetComponent<Image>().sprite = stunSelected ? selectedButton : unselectedButton;
	}

	void DoDamage()
	{
		modifier = new CombatModify() { Pierce = pierceSelected, Smite = smiteSelected, Sunder = sunderSelected, Cleave = cleaveSelected, Lethal = lethalSelected, Stun = stunSelected };

		int r = monsterItems[0].Damage( damage, modifier );
		if ( monster.count > 1 )
		{
			r = monsterItems[1].Damage( r, modifier );
			if ( monster.count > 2 )
				monsterItems[2].Damage( r, modifier );
		}
	}

	public void OnAdd()
	{
		damage++;
		damageText.text = damage.ToString();

		DoDamage();
	}

	public void OnMinus()
	{
		if ( damage == 0 )
			return;

		damage = Mathf.Max( 0, damage - 1 );
		damageText.text = damage.ToString();

		DoDamage();
	}

	public void OnApply()
	{
		string monsterName = Monster.MonsterNameObject(monster, monster.deadCount);
		gameObject.SetActive(false);
		FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowYesNo(Translate("combat.text.ConfirmHits", "Do you want to apply {0} hits to {1}?",
			new List<string> { damage.ToString(), monsterName }), res =>
		{
			if (res.btn1)
			{
				var sp = FindObjectOfType<ShadowPhaseManager>();
				var mm = FindObjectOfType<MonsterManager>();
				var im = FindObjectOfType<InteractionManager>();
				applyButton.interactable = false;
				//how many died this attack
				int deadCount = monsterItems.Where(x => x.isDead && x.activeMonster).Count() - monster.deathTally;

				monster.deadCount = deadCount;//shadow phase reads this number
				monster.deathTally += deadCount;//how many have died total

				for (int i = 0; i < monster.count; i++)
					monsterItems[i].Apply(stunSelected);

				//elite enemies are able to perform counterattacks even	when they are exhausted
				//stunning exhausts a group - if the group is elite it also cannot counterattack this attack

				Hide(true);

				if (deadCount > 0)
				{
					//if we're here from the shadow phase AND ALL monsters died, bug out and let shadow phase continue
					if (sp.doingShadowPhase && monster.deathTally == monster.count)
					{
						mm.RemoveMonster(monster);
						sp.NotifyInterrupt();
					}
					else if (sp.doingShadowPhase)//otherwise at least 1 killed, remove
					{
						MonsterManager.ReturnMonsterToPool(monster.monsterType, monster.deadCount);
						monsterName = Monster.MonsterNameObject(monster, monster.deadCount);
						string monsterText = Translate("dialog.text.RemoveMonsters", $"Remove {monster.deadCount} {monsterName}(s) from the board.", new List<string> { monster.deadCount.ToString(), monsterName });
						im.GetNewTextPanel().ShowOkContinue(monsterText, ButtonIcon.Continue, null);
					}
					else if (!sp.doingShadowPhase)//otherwise not in SP, continue
					{
						if (monster.deathTally < monster.count)
						{
							MonsterManager.ReturnMonsterToPool(monster.monsterType, monster.deadCount);
							monsterName = Monster.MonsterNameObject(monster, monster.deadCount);
							string monsterText = Translate("dialog.text.RemoveMonsters", $"Remove {monster.deadCount} {monster.dataName}(s) from the board.", new List<string> { monster.deadCount.ToString(), monsterName });

							im.GetNewTextPanel().ShowOkContinue(monsterText, ButtonIcon.Continue, () =>
							{
								QueryCounterAttack(monster);
							});
						}
						else
							mm.RemoveMonster(monster);
					}
				}
				else if (!sp.doingShadowPhase)//only counterattack if NOT in sp
					QueryCounterAttack(monster);
			}
			else
            {
				gameObject.SetActive(true);
			}
		});
	}

	void QueryCounterAttack( Monster monster )
	{
		if ( ( !monster.isElite && monster.isExhausted ) || ( monster.isElite && monster.isStunned ) )
		{
			Debug.Log( "Monster group is exhausted, skipping counterattack" );
			FindObjectOfType<MonsterManager>().UnselectAll();
			return;
		}

		FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowYesNo( Translate("attack.text.EnemyCounterattack", "Can the enemy counterattack?") , res =>
		{
			if ( res.btn1 )
			{
				FindObjectOfType<InteractionManager>().GetNewDamagePanel().ShowCombatCounter( monster, () =>
				 {
					 monster.isExhausted = true;
					 FindObjectOfType<MonsterManager>().UnselectAll();
				 } );
			}
			else
				FindObjectOfType<MonsterManager>().UnselectAll();

			//else
			//{
			//	//show monster move textbox

			//	FindObjectOfType<InteractionManager>()
			//	.GetNewTextPanel()
			//	.ShowCustom( "Move " + monster.movementValue + ": Attack " + Bootstrap.GetRandomHero() + " or closest Hero", "Attack", "No Target", r =>
			//		 {
			//			 if ( r.btn1 )
			//			 {
			//				 monster.isExhausted = true;
			//				 FindObjectOfType<InteractionManager>().GetNewDamagePanel().ShowCombatCounter( monster, () =>
			//					{
			//						FindObjectOfType<MonsterManager>().UnselectAll();
			//					} );
			//			 }
			//			 else
			//			 {
			//				 FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( "Move " + monster.movementValue * 2 + " towards closest Hero", ButtonIcon.Continue, () =>
			//						{
			//							FindObjectOfType<MonsterManager>().UnselectAll();
			//						} );
			//			 }
			//		 } );
			//}
		} );
	}
}
