public class DecisionInteraction : InteractionBase
{
	public string choice1;
	public string choice2;
	public string choice3;
	public bool isThreeChoices;
	public string choice1Trigger;
	public string choice2Trigger;
	public string choice3Trigger;

	public override InteractionType interactionType { get { return InteractionType.Decision; } set { } }

	public override string TranslationKey(string suffix) { return "event.decision." + dataName + "." + suffix; }

}