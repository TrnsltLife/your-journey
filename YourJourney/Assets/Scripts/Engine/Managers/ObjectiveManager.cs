using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;
using static LanguageManager;

public class ObjectiveManager : MonoBehaviour
{
	public TextMeshProUGUI objectiveText;

	List<Objective> objectiveList;
	[HideInInspector]
	public Objective currentObjective;

	public void Init( Scenario s )
	{
		objectiveList = new List<Objective>( s.objectiveObserver );
		currentObjective = null;
		//Debug.Log( $"Objective Manager: {objectiveList.Count} Objectives Found" );

		LanguageManager.AddSubscriber(onUpdateTranslation);
		onUpdateTranslation();
	}

	public void onUpdateTranslation()
	{
		if (currentObjective != null)
		{
			objectiveText.text = Interpret(currentObjective.TranslationKey("reminder"), currentObjective.objectiveReminder);
		}
		else
        {
			objectiveText.text = Translate("objective.NoObjective", "No Objective");
		}
	}

	/// <summary>
	/// sets the current objective by a TRIGGER, shows text box story, updates reminder text
	/// </summary>
	public bool TrySetObjective( string name, Action followupAction = null )
	{
		//Debug.Log( $"TrySetObjective: {name}" );
		if ( objectiveList.Any( x => x.triggeredByName == name ) )
		{
			currentObjective = objectiveList.Where( x => x.triggeredByName == name ).First();
			Debug.Log( "TrySetObjective() FOUND: " + currentObjective.dataName );
			//set reminder text
			objectiveText.text = Scenario.Chronicle(Interpret(currentObjective.TranslationKey("reminder"), currentObjective.objectiveReminder));

			//only show summary if not skipped
			if ( !currentObjective.skipSummary )
				FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue(Interpret(currentObjective.TranslationKey("summary"), currentObjective.textBookData.pages[0]), ButtonIcon.Continue, () =>
				{
					followupAction?.Invoke();
				} );
			else
				followupAction?.Invoke();
			return true;
		}

		//Debug.Log( "TrySetObjective() NOT FOUND: " + name );
		return false;
	}

	/// <summary>
	/// First Objective is set by its NAME, not a Trigger
	/// </summary>
	public void TrySetFirstObjective( string name, Action followupAction = null )
	{
		Debug.Log( $"TrySetFirstObjective: {name}" );
		if ( objectiveList.Any( x => x.dataName == name ) )
		{
			Debug.Log( "Found Objective" );
			currentObjective = objectiveList.Where( x => x.dataName == name ).First();
			//set reminder text
			objectiveText.text = Scenario.Chronicle(Interpret(currentObjective.TranslationKey("reminder"), currentObjective.objectiveReminder));

			//only show summary if not skipped
			if ( !currentObjective.skipSummary )
				FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue(Interpret(currentObjective.TranslationKey("summary"), currentObjective.textBookData.pages[0]), ButtonIcon.Continue, () =>
				{
					followupAction?.Invoke();
				} );
			else
				followupAction?.Invoke();
			return;
		}

		Debug.Log( "TrySetFirstObjective NOT FOUND: " + name );
	}

	/// <summary>
	/// Quick debug - silently sets objective without showing dialog
	/// </summary>
	public void DebugSetObjective( string name )
	{
		if ( objectiveList.Any( x => x.dataName == name ) )
		{
			currentObjective = objectiveList.Where( x => x.dataName == name ).First();
			//set reminder text
			objectiveText.text = Scenario.Chronicle(Interpret(currentObjective.TranslationKey("reminder"), currentObjective.objectiveReminder));
		}
	}

	public bool Exists( string name )
	{
		return objectiveList.Any( x => x.dataName == name );
	}

	public Objective TryGetObjective( string name )
	{
		if ( objectiveList.Any( x => x.dataName == name ) )
			return objectiveList.Where( x => x.dataName == name ).First();
		else
			return null;
	}

	public bool TryCompleteObjective( string triggername )
	{
		if ( currentObjective == null )
			return false;
		Debug.Log( "TryCompleteObjective: " + triggername);

		//objective is complete, remove it and fire any on complete trigger
		//TODO - show completion textbox?  show lore earned?
		if ( currentObjective.triggerName == triggername )
		{
			Debug.Log( "Completed Objective: " + currentObjective.dataName );
			FindObjectOfType<LorePanel>().AddReward( currentObjective.loreReward, currentObjective.xpReward, currentObjective.threatReward );
			string t = currentObjective.nextTrigger;
			currentObjective = null;
			objectiveText.text = Translate("objective.NoObjective", "No Objective");
			FindObjectOfType<TriggerManager>().FireTrigger( t );
			return true;
		}

		Debug.Log( "TryCompleteObjective NOT FOUND: " + triggername);
		return false;
	}

	public void SetState( ObjectiveState objectiveState )
	{
		if ( objectiveState.currentObjective != Guid.Empty )
		{
			currentObjective = objectiveList.Where( x => x.GUID == objectiveState.currentObjective ).First();
			objectiveText.text = Scenario.Chronicle(Interpret(currentObjective.TranslationKey("reminder"), currentObjective.objectiveReminder));
		}
		else
		{
			currentObjective = null;
			objectiveText.text = Scenario.Chronicle(Translate("objective.NoObjective", "No Objective"));
		}
	}

	public ObjectiveState GetState()
	{
		ObjectiveState os = new ObjectiveState();
		if ( currentObjective != null )
			os.currentObjective = currentObjective.GUID;
		else
			os.currentObjective = Guid.Empty;
		return os;
	}
}
