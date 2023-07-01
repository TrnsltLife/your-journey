using System.Collections.Generic;

public class ConditionalInteraction : InteractionBase
{
	public List<string> triggerList { get; set; }
	public string finishedTrigger { get; set; }

	public override InteractionType interactionType { get { return InteractionType.Conditional; } set { } }

	public override string TranslationKey(string suffix) { return "event.conditional." + dataName + "." + suffix; }
}
