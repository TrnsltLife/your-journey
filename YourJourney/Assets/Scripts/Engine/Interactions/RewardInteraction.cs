public class RewardInteraction : InteractionBase
{
	public int rewardLore, rewardXP, rewardThreat;

	public override InteractionType interactionType { get { return InteractionType.Reward; } set { } }

	public override string TranslationKey(string suffix) { return "event.reward." + dataName + "." + suffix; }

}