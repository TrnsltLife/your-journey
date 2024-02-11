public class DialogInteraction : InteractionBase
{
	public string choice1, choice2, choice3, c1Trigger, c2Trigger, c3Trigger, persistentText, c1Text, c2Text, c3Text;

	public bool c1Used = false, c2Used = false, c3Used = false, hasActivated = false, isDone = false;

	public override InteractionType interactionType { get { return InteractionType.Dialog; } set { } }

	public override string TranslationKey(string suffix) { return "event.dialog." + dataName + "." + suffix; }

}

