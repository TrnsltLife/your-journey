public class TextInteraction : InteractionBase
{
	public bool hasActivated = false;
	public string persistentText;

	public override InteractionType interactionType { get { return InteractionType.Text; } set { } }

	public override string TranslationKey(string suffix) { return "event.text." + dataName + "." + suffix; }
}