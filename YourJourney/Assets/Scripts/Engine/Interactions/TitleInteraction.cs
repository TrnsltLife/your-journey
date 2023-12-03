using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TitleInteraction : InteractionBase
{
	public int loreFallback, xpFallback, threatFallback;
	public string fallbackTrigger;
	public int randomizedTitlesCount;
	public List<int> titleList;

	public override InteractionType interactionType { get { return InteractionType.Title; } set { } }

	public override string TranslationKey(string suffix) { return "event.title." + dataName + "." + suffix; }
}
