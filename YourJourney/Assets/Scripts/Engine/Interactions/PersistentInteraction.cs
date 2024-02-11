public class PersistentInteraction : InteractionBase
{
	public TextBookData alternativeBookData { get; set; }
	public string eventToActivate { get; set; }
	public string alternativeTextTrigger { get; set; }

	public override InteractionType interactionType { get { return InteractionType.Persistent; } set { } }

	public override string TranslationKey(string suffix) { return "event.persistent-token." + dataName + "." + suffix; }

}
