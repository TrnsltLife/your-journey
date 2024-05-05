using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CorruptionInteraction : InteractionBase
{
	public int corruption;
	public CorruptionTarget corruptionTarget;

	public override InteractionType interactionType { get { return InteractionType.Corruption; } set { } }

	public override string TranslationKey(string suffix) { return "event.corruption." + dataName + "." + suffix; }
}
