using System;

public class ReplaceTokenInteraction : InteractionBase
{
	public string eventToReplace { get; set; }
	public string replaceWithEvent { get; set; }
	public bool noText { get; set; }
	public Guid replaceWithGUID { get; set; }

	public bool hasActivated = false;

	public override InteractionType interactionType { get { return InteractionType.Replace; } set { } }
	public override string TranslationKey(string suffix) { return "event.replace-token." + dataName + "." + suffix; }
}
