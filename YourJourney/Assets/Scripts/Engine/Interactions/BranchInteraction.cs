public class BranchInteraction : InteractionBase
{
	public string triggerTest;
	public string triggerIsSet;
	public string triggerNotSet;
	public string triggerIsSetTrigger;
	public string triggerNotSetTrigger;
	public bool branchTestEvent;

	public override InteractionType interactionType { get { return InteractionType.Branch; } set { } }

	public override string TranslationKey(string suffix) { return "event.story-branch." + dataName + "." + suffix; }

	public void Resolve( InteractionManager im )
	{
		if ( branchTestEvent )//if it's an EVENT to activate
		{
			//if the trigger test HAS BEEN FIRED...
			if ( im.engine.triggerManager.IsTriggered( triggerTest ) )
				im.TryFireEventByName( triggerIsSet );
			else//otherwise...
				im.TryFireEventByName( triggerNotSet );
		}
		else//otherwise it's a TRIGGER, so fire it
		{
			//if the trigger test HAS BEEN FIRED...
			if ( im.engine.triggerManager.IsTriggered( triggerTest ) )
				im.engine.triggerManager.FireTrigger( triggerIsSetTrigger );
			else//otherwise...
				im.engine.triggerManager.FireTrigger( triggerNotSetTrigger );
		}
	}
}