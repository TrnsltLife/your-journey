using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static LanguageManager;

public class MonsterManager : MonoBehaviour
{
	public CombatPanel combatPanel;
	public GameObject monsterButtonPrefab;
	public Transform bar, buttonAttach;
	public Button leftB, rightB;
	public Canvas canvas;
	public Sprite[] banners, eliteBanners;
	public RectTransform sbRect;

	Queue<Sprite> bannerQueue, eliteBannerQueue;
	int scrollOffset = -517;
	bool scrollReady = true;
	[HideInInspector]
	public RectTransform attachRect;
	public GameObject sizebar;
	[HideInInspector]
	public float scalar;
	[HideInInspector]
	public List<Monster> monsterList = new List<Monster>();

	private void Start()
	{
		attachRect = buttonAttach.GetComponent<RectTransform>();
		scalar = canvas.scaleFactor;
		LoadBanners();
	}

	public void UpdateSkins()
	{
		foreach (Transform child in buttonAttach)
		{
			var mb = child.GetComponent<MonsterButton>();
			mb.UpdateSkin();
		}
	}

	/// <summary>
	/// Add from saved game
	/// </summary>
	public void AddMonsterGroup( Monster m, ThreatInteraction interaction )
	{
		//check if monster can be spawned in this Difficulty mode
		if ( !m.IsValid() )
			return;

		bar.DOLocalMoveY( 50, .75f ).SetEase( Ease.InOutCubic );

		//apply elite modifier bonuses
		if ( m.isArmored )
			m.shieldValue += 1;
		if ( m.isLarge )
			m.health += 2;
		if ( m.isBloodThirsty )
			m.damage += 1;

		m.interaction = interaction;
		m.currentHealth = new int[3] { m.health, m.health, m.health };
		GameObject go = Instantiate( monsterButtonPrefab, buttonAttach );

		go.transform.localPosition = new Vector3( ( 175 * monsterList.Count ), 25, 0 );
		go.GetComponent<MonsterButton>().monster = m;
		go.GetComponent<MonsterButton>().AddToBar( m.isElite, this );

		monsterList.Add( m );

		AddBanners(go, m);

		scrollReady = false;
		foreach ( Transform child in buttonAttach )
		{
			if ( child.transform.position.x > sbRect.position.x + ( 1000f * scalar ) )//off the edge
				scrollOffset -= 175;
		}
		buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad ).OnComplete( () => { scrollReady = true; } );
	}

	private void Update()
	{
		scalar = canvas.scaleFactor;
		attachRect = buttonAttach.GetComponent<RectTransform>();
		sbRect = sizebar.GetComponent<RectTransform>();
		float maxX = sbRect.position.x + ( 1000f * scalar );

		leftB.interactable = GetFirstButtonPos() < sbRect.position.x;//130;
		rightB.interactable = GetLastButtonPos() > maxX;//402;
	}

	public void OnScrollLeft()
	{
		//leftB.interactable = GetLastButtonPos() != -517;

		if ( scrollOffset == -517 )
			return;

		if ( scrollReady )
		{
			scrollReady = false;
			scrollOffset += 175;
			buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad ).OnComplete( () => { scrollReady = true; } );
		}
	}

	public void OnScrollRight()
	{
		//rightB.interactable = GetLastButtonPos() > 402;

		if ( GetLastButtonPos() <= buttonAttach.position.x + ( 1000f * scalar ) ) //402 )
			return;

		if ( scrollReady )
		{
			scrollReady = false;
			scrollOffset -= 175;
			buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad ).OnComplete( () => { scrollReady = true; } );
		}
	}

	float GetFirstButtonPos()
	{
		foreach ( Transform child in buttonAttach )
			return child.transform.position.x;

		return 10000;//-517;
	}

	float GetLastButtonPos()
	{
		List<MonsterButton> buttons = new List<MonsterButton>();

		foreach ( Transform child in buttonAttach )
			buttons.Add( child.GetComponent<MonsterButton>() );

		if ( buttons.Count == 0 )
			return 0;//-517;

		return buttons[buttons.Count - 1].transform.position.x;
	}

	/// <summary>
	/// removes monster group, shows reward, and fires its Trigger After Event
	/// </summary>
	public void RemoveMonster( Monster m )
	{
		monsterList.Remove( m );

		//remove the prefab button from the screen and rearrange buttons
		int c = 0;

		foreach ( Transform child in buttonAttach )
		{
			var mb = child.GetComponent<MonsterButton>();

			if ( mb.monster == m )
			{
				//add any lore unique to this monster
				FindObjectOfType<LorePanel>().AddReward( mb.monster.loreReward );

				mb.markRemove = true;
				mb.cg.DOFade( 0, .5f );
				child.DOLocalMoveY( 0, .5f ).OnComplete( () =>
				{
					Sprite b = mb.Remove();
					if ( b != null && !m.isElite )
						bannerQueue.Enqueue( b );
					else if ( b != null )
						eliteBannerQueue.Enqueue( b );
				} );
			}
			else
				mb.Regroup( c++ * 175 );
		}

		if ( monsterList.Count == 0 )
			bar.DOLocalMoveY( -45, .5f ).SetEase( Ease.InOutCubic );
		else
		{
			scrollReady = false;
			scrollOffset = -517;
			buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad ).OnComplete( () => { scrollReady = true; } );
		}

		//show reward on group defeated
		//then fire Defeated trigger only if ALL groups in the Event are defeated
		if ( m.interaction != null )
		{
			ThreatInteraction ti = m.interaction;
			int foo = monsterList.Count( x => x.interaction.GUID == m.interaction.GUID );

			string monsterName = Monster.MonsterNameObject(m, m.deadCount);
			//Debug.Log("Get name for " + m.dataName + "/" + m.enumName + "/" + m.count + "/" + m.deadCount + "/" + m.deathTally + " => " + monsterName);
			string monsterText = Translate("dialog.text.RemoveMonsters", $"Remove {m.deadCount} {m.dataName}(s) from the board.", new List<string> { m.deadCount.ToString(), monsterName });
			string inspirationText = Translate("dialog.text.YouNearbyInspiration", "You or a nearby Hero gain 1 Inspiration.");

			FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( monsterText + "\r\n\r\n" + inspirationText, ButtonIcon.Continue, () =>
			{
				if ( foo == 0 )
				{
					string trigger = ti.triggerDefeatedName;
					FindObjectOfType<TriggerManager>().FireTrigger( trigger );
					FindObjectOfType<LorePanel>().AddReward( ti.loreReward, ti.xpReward, ti.threatReward );
				}
			} );
		}
	}

	public bool ShowCombatPanel( Monster m, Sprite bannerSprite, int skinVariant = 0 )
	{
		foreach ( Transform child in buttonAttach )
		{
			child.GetComponent<MonsterButton>().ToggleSelect( false );
		}
		//selectedMonster = m;
		return combatPanel.Show( m, bannerSprite, skinVariant );
	}

	public void UnselectAll()
	{
		foreach ( Transform child in buttonAttach )
		{
			//if ( child.name.Contains( "MonsterButton" ) )
			{
				child.GetComponent<MonsterButton>().ToggleSelect( false );
			}
		}
	}

	public void SelectMonster( Monster m, bool select )
	{
		foreach ( Transform child in buttonAttach )
		{
			//if ( child.name.Contains( "MonsterButton" ) )
			{
				if ( child.GetComponent<MonsterButton>().monster.GUID == m.GUID )
					child.GetComponent<MonsterButton>().ToggleSelect( select );
			}
		}
	}

	public void ExhaustMonster( Monster m, bool exhaust )
	{
		foreach ( Transform child in buttonAttach )
		{
			if ( child.GetComponent<MonsterButton>().monster.GUID == m.GUID )
			{
				child.GetComponent<MonsterButton>().ToggleExhausted( exhaust );
				return;
			}
		}
		Debug.Log( "ExhaustMonster::no monster found with this GUID" );
	}

	public void ReadyAll()
	{
		foreach ( Transform child in buttonAttach )
		{
			//if ( child.name.Contains( "MonsterButton" ) )
			child.GetComponent<MonsterButton>().ToggleExhausted( false );
		}
	}

	private void RemoveMonsterButtons()
	{
		monsterList.Clear();
		foreach ( Transform child in buttonAttach )
		{
			var mb = child.GetComponent<MonsterButton>();
			mb.RemoveNow();
		}
	}

	public void SetState( MonsterState monsterState )
	{
		RemoveMonsterButtons();
		bannerQueue.Clear();
		eliteBannerQueue.Clear();
		LoadBanners();

		if ( monsterState.monsterList.Count > 0 )
			bar.DOLocalMoveY( 50, .75f ).SetEase( Ease.InOutCubic );
		else
			bar.DOLocalMoveY( -45, .5f ).SetEase( Ease.InOutCubic );

		foreach ( SingleMonsterState sms in monsterState.monsterList )
		{
			Monster m = sms.monster;
			m.interaction = FindObjectOfType<InteractionManager>().allInteractions.Where( x => x.GUID == sms.eventGUID ).First() as ThreatInteraction;

			GameObject go = Instantiate( monsterButtonPrefab, buttonAttach );

			go.transform.localPosition = new Vector3( ( 175 * monsterList.Count ), 25, 0 );
			go.GetComponent<MonsterButton>().monster = m;
			go.GetComponent<MonsterButton>().AddToBar( m.isElite, this );

			//restore exhausted state
			go.GetComponent<MonsterButton>().ToggleExhausted( m.isExhausted );

			monsterList.Add( m );

			AddBanners(go, m);

			scrollReady = false;
			foreach ( Transform child in buttonAttach )
			{
				if ( child.transform.position.x > sbRect.position.x + ( 1000f * scalar ) )//off the edge
					scrollOffset -= 175;
			}
			//buttonAttach.DOLocalMoveX( scrollOffset, .5f ).SetEase( Ease.InOutQuad ).OnComplete( () => { scrollReady = true; } );
		}

		scrollOffset = -517;
		buttonAttach.localPosition = buttonAttach.localPosition.X( -517 );
	}

	public void LoadBanners()
	{
		//Load the banners and eliteBanners that are attached in unity into the queues used in this class
		bannerQueue = new Queue<Sprite>(banners);
		eliteBannerQueue = new Queue<Sprite>(eliteBanners);
	}

	public void AddBanners(GameObject go, Monster m)
	{
		//Elite enemies: always add a banner (if there is a banner available)
		if ( m.isElite )
		{
			 if ( eliteBannerQueue.Count > 0 )
			 {
				go.GetComponent<MonsterButton>().SetBanner( eliteBannerQueue.Dequeue() );
				m.hasBanner = true;
			 }
		}
		//Normal enemies: add a banner if there are any other enemies of the same type already on the board that don't have a banner
		else
		{
			var mc = from monster in monsterList
							where monster.monsterType == m.monsterType 
							&& monster.isElite == m.isElite
							&& monster.hasBanner == false
							select monster;
			if ( mc.Count() > 1 && bannerQueue.Count > 0 )
			{
				go.GetComponent<MonsterButton>().SetBanner( bannerQueue.Dequeue() );
				m.hasBanner = true;
			}
		}
	}

	public MonsterState GetState()
	{
		List<SingleMonsterState> sms = new List<SingleMonsterState>();
		foreach ( Monster m in monsterList )
			sms.Add( new SingleMonsterState()
			{
				monster = m,
				eventGUID = m.interaction.GUID,
				//collectionCount = m.interaction.totalMonsterCount
			} );
		return new MonsterState()
		{
			monsterList = sms
		};
	}
}
