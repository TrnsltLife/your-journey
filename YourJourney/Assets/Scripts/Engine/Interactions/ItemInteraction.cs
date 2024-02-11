using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ItemInteraction : InteractionBase
{
	public int loreFallback, xpFallback, threatFallback;
	public string fallbackTrigger;
	public int randomizedItemsCount;
	public List<int> itemList;

	public override InteractionType interactionType { get { return InteractionType.Item; } set { } }

	public override string TranslationKey(string suffix) { return "event.item." + dataName + "." + suffix; }
}
