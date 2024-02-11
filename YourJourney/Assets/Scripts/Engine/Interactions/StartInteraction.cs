public class StartInteraction : InteractionBase
{
	public override InteractionType interactionType { get { return InteractionType.Start; } set { } }

	public override string TranslationKey(string suffix) { return "event.start." + dataName + "." + suffix; }
}