using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using static LanguageManager;

public class ShadowPhaseManager : MonoBehaviour
{
	public PhaseNotification phaseNotification;
	public EndTurnButton endTurnButton;
	public Transform shadowWisp;

	[HideInInspector]
	public bool doingShadowPhase = false;
	[HideInInspector]
	public bool allowAttacks { get; set; } = false;//allow clicking button
	[HideInInspector]
	public Guid allowedMonsterGUID;
	MeshRenderer wispRenderer;
	float alphaValue;
	bool doInterrupt = false;

	private void Awake()
	{
		wispRenderer = shadowWisp.GetComponent<MeshRenderer>();
	}

	private void Update()
	{
		float ypos = GlowEngine.SineAnimation( -.2f, -.08f, .5f );
		float zpos = GlowEngine.SineAnimation( .35f, .43f, .25f );
		float zanim = GlowEngine.SineAnimation( -10f, 10f, .4f );
		float xanim = GlowEngine.SineAnimation( 0f, 28f, .6f );
		float sxanim = GlowEngine.SineAnimation( .73f, 1f, .3f );
		float syanim = GlowEngine.SineAnimation( .32f, 0.45728f, .6f );
		shadowWisp.localPosition = new Vector3( 0, ypos, zpos );
		shadowWisp.localScale = new Vector3( sxanim, syanim, 1 );
		shadowWisp.localRotation = Quaternion.Euler( xanim, 0, zanim );
	}

	public void NotifyInterrupt()
	{
		doInterrupt = true;
	}

	public void EndTurn()
	{
		//if in shadow phase, provoke mode, or other UI showing, we're busy, so bug out
		if ( doingShadowPhase
			|| FindObjectOfType<InteractionManager>().PanelShowing
			|| FindObjectOfType<ProvokeMessage>().provokeMode )
			return;

		FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowYesNo( Translate("dialog.text.BeginShadowPhase", "End your turn and begin the Shadow Phase?"), result =>
		{
			if ( result.btn1 )
				DoEndTurn();
		} );
	}

	void DoEndTurn()
	{
		Debug.Log( "***STARTED SHADOW PHASE" );
		doInterrupt = false;
		var objs = FindObjectsOfType<SpawnMarker>();
		foreach ( var ob in objs )
		{
			if ( ob.name.Contains( "SPAWNMARKER" ) )
				Destroy( ob.gameObject );
			if ( ob.name == "STARTMARKER" )
				ob.gameObject.SetActive( false );
			if (ob.name.StartsWith("Start Token"))
				ob.gameObject.SetActive(false);
		}

		alphaValue = 0;
		DOTween.To( () => alphaValue, x =>
		{
			alphaValue = x;
			wispRenderer.material.SetColor( "_Color", new Color( 0, 0, 0, alphaValue ) );
		}, 1, 4f );

		allowAttacks = false;

		//Check for dead heroes
		for(int i=0; i<Bootstrap.isDead.Length; i++)
        {
			if(Bootstrap.isDead[i])
            {
				//Fail the mission
				string lastStandFailedResolution = Engine.currentScenario.lastStandFailedResolution;

				Engine engine = Engine.FindEngine();

				if(!string.IsNullOrEmpty(lastStandFailedResolution) && lastStandFailedResolution.ToLower() != "none")
                {
					Debug.Log("DoEndTurn -> TriggerEndGame lastStandFailedResolution: " + lastStandFailedResolution);
					//engine.triggerManager.TriggerEndGame(lastStandFailedResolution);
					engine.triggerManager.FireTrigger(lastStandFailedResolution);
					return;
				}
				else
                {
					Debug.Log("DoEndTurn -> EndScenario, lastStandFailedResolution: " + lastStandFailedResolution);
					engine.EndScenario(lastStandFailedResolution);
					return;
				}
			}
		}

		doingShadowPhase = true;
		//go thru each monster, ask if it can move+attack random hero
		//attack OR no target
		//if no attack, make it move towards nearest
		//advance threat
		//trigger threat if threshold reached

		//TODO?? - disable group button
		StartCoroutine( EndTurnSequence() );
	}

	IEnumerator EndTurnSequence()
	{
		Debug.Log( "***STARTED COROUTINE" );
		FindObjectOfType<TileManager>().ToggleInput( true );
		//SHADOW PHASE announcement
		phaseNotification.Show( Translate("dialog.text.ShadowPhase", "Shadow Phase") );
		yield return new WaitForSeconds( 3 );

		//INSPIRATION STEP
		yield return InspirationStep();

		//ENEMY ACTIVATION STEP
		yield return EnemyStep();

		//DARKNESS STEP
		yield return DarknessStep();

		//THREAT STEP
		yield return ThreatStep();

		//FINISH UP
		yield return FinishShadowPhase();

		//finally end shadow phase
		doingShadowPhase = false;
		Debug.Log( "***ENDED COROUTINE" );

		//SAVE PROGRESS
		GameState gs = new GameState();
		gs.SaveState( FindObjectOfType<Engine>(), Bootstrap.gameStarter.saveStateIndex );
	}

	IEnumerator InspirationStep()
	{
		Debug.Log( "***INSPIRATION STEP" );
		if ( Bootstrap.gameStarter.difficulty == Difficulty.Adventure )
		{
			var im = FindObjectOfType<InteractionManager>();
			bool waiting = true;
			im.GetNewTextPanel().ShowOkContinue( Translate("dialog.text.FellowshipPrepared", "Your fellowship is prepared to face the shadow.")
				+ "\r\n\r\n"
				+ Translate("dialog.text.EachGainInspiration", "Each hero gains 1 inspiration."),
					ButtonIcon.Continue, () => waiting = false );

			while ( waiting )
				yield return null;
		}
	}

	IEnumerator EnemyStep()
	{
		Debug.Log( "***ENEMY ACTIVATION STEP" );
		var im = FindObjectOfType<InteractionManager>();
		var fm = FindObjectOfType<FightManager>();
		var mm = FindObjectOfType<MonsterManager>();
		Monster[] monsters = mm.monsterList.ToArray();
		bool waiting = true;

		//foreach ( var monster in monsters )
		for ( int i = 0; i < monsters.Length; i++ )
		{
			mm.UnselectAll();

			if ( monsters[i].isExhausted || monsters[i].isStunned )
				continue;

			Debug.Log( "***MONSTER ACTIVATING" );
			yield return new WaitForSeconds( 1 );

			//yield return fm.MonsterStep( monsters[i] );

			//snip below
			string heroName = Bootstrap.GetRandomHero();
			InteractionResult iResult = null;
			//select monster button group
			mm.SelectMonster( monsters[i], true );
			//ask if it can move and attack
			waiting = true;
			allowAttacks = true;
			allowedMonsterGUID = monsters[i].GUID;
			var tp = im.GetNewTextPanel();

			//Move X: Attack NAME (or closest Hero)
			//buttons: Attack/No Target
			doInterrupt = false;
			int moveA = 0;
			int moveB = 0;
            if (monsters[i].moveA > 0 || monsters[i].moveB > 0)
            {
				moveA = monsters[i].moveA;
				moveB = monsters[i].moveB;
            }
			else
            {
				moveA = monsters[i].movementValue;
				moveB = moveA * 2;
            }
			tp.ShowYesNo( Translate("attack.text.MoveAttack", $"Move {moveA}: Attack {heroName} or closest Hero.", new List<string> { moveA.ToString(), heroName }) +
                "\r\n\r\n" +
                Translate("attack.text.CanAttack", "Can this enemy group attack a target?") +
				"\r\n\r\n" +
				Translate("attack.text.ApplyDamageButton", "If you have a skill to attack or apply damage to this enemy group, do it now by selecting its Enemy Button."), 
				res =>
				{
					waiting = false;
					iResult = res;
				} );
			//wait
			//int startingActive = monsters[i].ActiveMonsterCount;
			while ( waiting )
			{
				if ( monsters[i].ActiveMonsterCount == 0
					|| ( !monsters[i].isElite && monsters[i].isExhausted )
					|| ( monsters[i].isElite && monsters[i].isStunned
					|| doInterrupt )
					/*|| monsters[i].ActiveMonsterCount < startingActive*/ )//something died
				{
					tp.RemoveBox();
					waiting = false;
				}
				yield return null;
			}

			allowAttacks = false;

			//if group was just removed from an interruption, wait until reward and any OnDefeated Events are complete
			yield return new WaitForSeconds( .25f );
			yield return WaitUntilFinished();

			//check if monster group is dead/exhausted and abort this monter's attack if needed
			if ( monsters[i].ActiveMonsterCount == 0
				|| ( !monsters[i].isElite && monsters[i].isExhausted )
				|| ( monsters[i].isElite && monsters[i].isStunned )
				|| doInterrupt )
			{
				tp.RemoveBox();
				waiting = true;
				im.GetNewTextPanel().ShowOkContinue( Translate("attack.text.EnemyCanceled", "This enemy group's activation is canceled."), ButtonIcon.Continue, () => { waiting = false; } );
				while ( waiting )
					yield return null;
			}
			else
			{
				if ( iResult.btn1 )//yes, attack
				{
					Debug.Log( "***YES ATTACK" );
					waiting = true;
					im.GetNewDamagePanel().ShowCombatCounter( monsters[i], () => waiting = false );
					//wait
					while ( waiting )
						yield return null;
					//exhaust the enemy
					mm.ExhaustMonster( monsters[i], true );
				}
				else
				{
					Debug.Log( "***NO ATTACK" );
					waiting = true;
					int remaining = monsters[i].count - monsters[i].deathTally;
					string monsterName = Monster.MonsterNameAttacker(monsters[i], remaining);
					string translateKey = "attack.text.single.MoveTowards";
					if (remaining > 1) { translateKey = "attack.text.plural.MoveTowards"; }
					im.GetNewTextPanel().ShowOkContinue( Translate(translateKey, $"{monsterName} move {moveB} spaces towards {heroName}.", new List<string> { monsterName, moveB.ToString(), heroName }), 
						ButtonIcon.Continue, () => waiting = false );
					//wait
					while ( waiting )
						yield return null;
					//exhaust the enemy
					mm.ExhaustMonster( monsters[i], true );
				}
			}
		}

		mm.UnselectAll();
	}

	IEnumerator DarknessStep()
	{
		Debug.Log( "***DARKNESS STEP" );
		if ( FindObjectOfType<ChapterManager>().IsDarknessVisible() )
		{
			var im = FindObjectOfType<InteractionManager>();
			bool waiting = true;
			im.GetNewDamagePanel().ShowShadowFear( () => waiting = false );

			while ( waiting )
				yield return null;
		}
	}

	IEnumerator ThreatStep()
	{
		Debug.Log( "***THREAT STEP" );
		//add up threat
		//2*hero count + # unexplored tiles + 1 per threat token
		int hc = 2 * Bootstrap.gameStarter.heroes.Length;
		int ut = 0;
		if (Bootstrap.gameStarter.difficulty != Difficulty.Adventure)
		{
			ut = FindObjectOfType<TileManager>().UnexploredTileCount();
		}
		int tt = FindObjectOfType<TileManager>().ThreatTokenCount();
		Debug.Log( "***hero threat: " + hc );
		Debug.Log( "***unexplored threat: " + ut );
		Debug.Log( "***threat token threat: " + tt );
		int threat = hc + ut + tt;

		Threat[] t = endTurnButton.AddThreat( threat );
		//wait for animation
		//if ( t != null )
		//	yield return new WaitForSeconds( 2 );

		bool waiting = true;
		FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( Translate("dialog.text.ThreatIncreases", $"Threat increases by {threat}.", new List<string> { threat.ToString() }), ButtonIcon.Continue, () => waiting = false );

		while ( waiting )
			yield return null;

		if ( t.Length > 0 )
		{
			Debug.Log( "THREATS FOUND: " + t.Length );
			Debug.Log( "***FIRING THREAT TRIGGERS" );
			for ( int i = 0; i < t.Length; i++ )
			{
				FindObjectOfType<TriggerManager>().FireTrigger( t[i].triggerName );
				//wait until all triggers finished
				yield return WaitUntilFinished();
			}
		}
		else
		{
			Debug.Log( "***NO THREAT TRIGGERS FOUND" );
		}
	}

	/// <summary>
	/// shows rally and action phase with appropriate messages
	/// </summary>
	IEnumerator FinishShadowPhase()
	{
		var im = FindObjectOfType<InteractionManager>();
		//RALLY PHASE announcement
		phaseNotification.Show( Translate("dialog.text.RallyPhase", "Rally Phase") );
		yield return new WaitForSeconds( 3 );

		int scoutAmount = Engine.currentScenario.subsequentScout;
		if (scoutAmount > 0)
		{
			bool waiting = true;
			im.GetNewTextPanel().ShowOkContinue(Translate("dialog.text.EachResetScoutX", "Each Hero resets their deck and Scouts {0}.", new List<string> { scoutAmount.ToString() }), ButtonIcon.Continue, () =>
			{
				waiting = false;
			});

			while (waiting)
				yield return null;
		}

		FindObjectOfType<MonsterManager>().ReadyAll();
		//ACTION PHASE announcement
		phaseNotification.Show( Translate("dialog.text.ActionPhase", "Action Phase") );

		DOTween.To( () => alphaValue, x =>
		{
			alphaValue = x;
			wispRenderer.material.SetColor( "_Color", new Color( 0, 0, 0, alphaValue ) );
		}, 0, 4f );

		//wait for action phase animation
		yield return new WaitForSeconds( 3 );
		FindObjectOfType<TileManager>().ToggleInput( false );
		Debug.Log( "***FINISHED SHADOW PHASE" );
	}

	/// <summary>
	/// Wait until all UI gone
	/// </summary>
	IEnumerator WaitUntilFinished()
	{
		Debug.Log( "***WAITING..." );
		var im = FindObjectOfType<InteractionManager>();
		var tm = FindObjectOfType<TriggerManager>();
		//wait for all UI screens to go away - signifies all triggers finished
		//poll IManager uiRoot for children (panels)
		while ( im.PanelShowing || tm.busyTriggering )
			yield return null;
		Debug.Log( "***DONE WAITING..." );

		//finally finish shadow phase
		//yield return FinishShadowPhase();
	}
}
