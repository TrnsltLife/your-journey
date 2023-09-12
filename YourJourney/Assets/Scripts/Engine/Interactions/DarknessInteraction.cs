using System;

public class DarknessInteraction : InteractionBase
{
	public override InteractionType interactionType { get { return InteractionType.Darkness; } set { } }

	public override string TranslationKey(string suffix) { return "event.darkness." + dataName + "." + suffix; }

}

