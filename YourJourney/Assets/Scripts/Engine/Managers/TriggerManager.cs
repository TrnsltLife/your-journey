using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerManager : MonoBehaviour
{
	public Engine engine;
	public Dictionary<string, bool> firedTriggersList = new Dictionary<string, bool>();
	[HideInInspector]
	public bool busyTriggering = false;

	Queue<string> queue = new Queue<string>();
	string endTriggerGUID, resolutionName;
	Dictionary<string, bool> campaignTriggers = new Dictionary<string, bool>();

	public void InitCampaignTriggers()
	{
		if ( Bootstrap.campaignState == null )
			return;

		foreach ( var t in Bootstrap.campaignState.campaign.triggerCollection )
			campaignTriggers.Add( t.dataName, false );
	}

	/// <summary>
	/// Takes a trigger name or event name to fire
	/// </summary>
	public void FireTrigger( string name )
	{
		if ( !string.IsNullOrEmpty( name )
			&& name.ToLower() != "none"
			&& !firedTriggersList.ContainsKey( name ) )
		{
			if ( queue.Count == 0 )
			{
				Debug.Log( "FIRST ENQUEUE: " + name );
				queue.Enqueue( name );
				StartCoroutine( TriggerChain() );
			}
			else
			{
				Debug.Log( "ENQUEUE: " + name );
				queue.Enqueue( name );
			}
		}
		//else
		//	Debug.Log( "FireTrigger::NO TRIGGER/NONE" );
	}

	public void TriggerEndGame( string rname )
	{
		endTriggerGUID = Guid.NewGuid().ToString();
		resolutionName = rname;

		FireTrigger( endTriggerGUID );
	}

	IEnumerator TriggerChain()
	{
		Debug.Log( "*******************TriggerChain STARTED" );

		while ( queue.Count > 0 )
		{
			busyTriggering = true;
			//wait until engine ready to handle next trigger
			yield return WaitUntilFinished();
			string name = queue.Peek();
			Trigger trigger;
			//check normal Triggers
			if ( engine.scenario.triggersObserver.Any( x => x.dataName == name ) )
				trigger = engine.scenario.triggersObserver.Where( x => x.dataName == name ).First();
			//check campaign Triggers
			else if ( campaignTriggers.Any( x => x.Key == name ) )
			{
				trigger = new Trigger()
				{
					dataName = name,
					isMultiTrigger = true,
					isCampaignTrigger = true
				};
			}
			//handle end scenario Trigger
			else if ( name == endTriggerGUID )
			{
				trigger = new Trigger()
				{
					dataName = endTriggerGUID,
					triggerName = resolutionName
				};
			}
			//handle no Trigger with name found
			else
				trigger = new Trigger() { isMultiTrigger = false };

			Debug.Log( "FireTrigger::" + name );
			if ( !trigger.isMultiTrigger && firedTriggersList.ContainsKey( name ) )
			{
				Debug.Log( "WARNING: Trigger has already been fired: " + name );
				queue.Dequeue();
				continue;
			}

			//sanity check - add trigger to fired list
			if ( !firedTriggersList.ContainsKey( name ) )
				firedTriggersList.Add( name, true );
			//add campaign trigger to campaignstate trigger fired list
			if ( Bootstrap.campaignState != null
				&& trigger.isCampaignTrigger
				&& !Bootstrap.campaignState.campaignTriggerState.Contains( name ) )
				Bootstrap.campaignState.campaignTriggerState.Add( name );

			//first, check conditional events listening for triggers to fire
			foreach ( ConditionalInteraction con in engine.interactionManager.interactions.Where( x => x.interactionType == InteractionType.Conditional ) )
			{
				if ( con.triggerList.Count > 0 )
				{
					bool containsAll = con.triggerList.All( x => firedTriggersList.Keys.Contains( x ) );
					if (containsAll)
					{
						FireTrigger(con.finishedTrigger);
					}
					else if (con.triggersRequiredCount > 0)
					{
						bool containsEnough = con.triggerList.Count(x => firedTriggersList.Keys.Contains(x)) >= con.triggersRequiredCount;
						if (containsEnough)
						{
							FireTrigger(con.finishedTrigger);
						}
					}

				}
			}

			//trigger Token
			if ( engine.tileManager.TryTriggerToken( name ) )
			{
				if ( !trigger.isMultiTrigger )
				{
					queue.Dequeue();
					continue;
				}
			}
			yield return WaitUntilFinished();

			//trigger Objective complete
			if ( engine.objectiveManager.TryCompleteObjective( name ) )
			{
				if ( !trigger.isMultiTrigger )
				{
					queue.Dequeue();
					continue;
				}
			}
			yield return WaitUntilFinished();

			//trigger Objective start
			if ( engine.objectiveManager.TrySetObjective( name ) )
			{
				if ( !trigger.isMultiTrigger )
				{
					queue.Dequeue();
					continue;
				}
			}
			yield return WaitUntilFinished();

			//trigger Event by trigger
			if ( engine.interactionManager.TryFireEventByTrigger( name ) )
			{
				if ( !trigger.isMultiTrigger )
				{
					queue.Dequeue();
					continue;
				}
			}
			yield return WaitUntilFinished();

			//trigger Event by event name
			if ( engine.interactionManager.TryFireEventByName( name ) )
			{
				if ( !trigger.isMultiTrigger )
				{
					queue.Dequeue();
					continue;
				}
			}
			yield return WaitUntilFinished();

			//trigger Chapter
			if ( engine.chapterManager.TriggerChapterByTrigger( name ) )
			{
				if ( !trigger.isMultiTrigger )
				{
					queue.Dequeue();
					continue;
				}
			}
			yield return WaitUntilFinished();

			//trigger end scenario
			if ( engine.interactionManager.TryFireEndScenario( name ) )
			{
				if ( !trigger.isMultiTrigger )
				{
					queue.Dequeue();
					continue;
				}
			}
			yield return WaitUntilFinished();

			if ( trigger.dataName == endTriggerGUID && !string.IsNullOrEmpty(endTriggerGUID))
			{
				Debug.Log( "Triggering End Scenario" );
				engine.EndScenario( trigger.triggerName );
				queue.Dequeue();
				continue;
			}
			yield return WaitUntilFinished();

			string n = queue.Dequeue();
			Debug.Log( "Multi done/Nothing listening to: " + n );
		}
		Debug.Log( "******************TriggerChain ENDED" );
		busyTriggering = false;

		//}
		//else
		//	Debug.Log( "FireTrigger::NO TRIGGER/NONE" );
	}

	/// <summary>
	/// wait until engine is ready to handle next trigger (no panels shown)
	/// </summary>
	IEnumerator WaitUntilFinished()
	{
		//Debug.Log( "***WAITING..." );
		var im = FindObjectOfType<InteractionManager>();
		//wait for all UI screens to go away - signifies all triggers finished
		//poll IManager uiRoot for children (panels)
		while ( im.PanelShowing )
			yield return null;

		//if ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase )
		//{
		//	Debug.Log( "Waiting for Shadow Phase to finish..." );
		//	while ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase )
		//		yield return null;
		//}
		//Debug.Log( "***DONE WAITING..." );
	}

	/// <summary>
	/// returns whether or not the given Trigger has fired
	/// </summary>
	public bool IsTriggered( string name )
	{
		return firedTriggersList.TryGetValue( name, out bool v ) ? v : false;
	}

	public TriggerState GetState()
	{
		return new TriggerState()
		{
			firedTriggersList = firedTriggersList
		};
	}

	public void SetState( TriggerState triggerState )
	{
		firedTriggersList = triggerState.firedTriggersList;
	}
}