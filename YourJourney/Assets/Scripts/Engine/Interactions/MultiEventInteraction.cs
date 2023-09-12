using System.Collections.Generic;

public class MultiEventInteraction : InteractionBase
{
	public List<string> eventList { get; set; }
	public List<string> triggerList { get; set; }
	public bool usingTriggers { get; set; }
	public bool isSilent { get; set; }

	public override InteractionType interactionType { get { return InteractionType.MultiEvent; } set { } }

	public override string TranslationKey(string suffix) { return "event.multi-event." + dataName + "." + suffix; }

}