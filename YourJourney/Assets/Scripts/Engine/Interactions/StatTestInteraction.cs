public class StatTestInteraction : InteractionBase
{
	public Ability testAttribute;
	public Ability altTestAttribute;
	public bool isCumulative;
	public bool passFail { get; set; }
	public int successValue;
	public string successTrigger;
	public string failTrigger;
	public TextBookData passBookData;
	public TextBookData failBookData;
	public TextBookData progressBookData;
	public int rewardLore, rewardXP, rewardThreat, failThreat;
	public bool noAlternate;

	public override InteractionType interactionType { get { return InteractionType.StatTest; } set { } }

	public override string TranslationKey(string suffix) { return "event.stat-test." + dataName + "." + suffix; }


	public int accumulatedValue = -1;

	/// <summary>
	/// Returns true if test is successful, false if not
	/// </summary>
	public bool ResolveCumulative( int amount )
	{
		if ( !passFail )
			accumulatedValue += amount;
		else
		{
			//successValue = 0; //removed because it made Repeatable Simple Pass/Fail tests reduce the successValue to 0 on the second attempt. Unclear why this was ever put here.
			accumulatedValue = amount;
		}

		if ( accumulatedValue >= successValue )
		{
			//engine.triggerManager.FireTrigger( successTrigger );
			return true;
		}
		else
			return false;//still in progress
	}
}