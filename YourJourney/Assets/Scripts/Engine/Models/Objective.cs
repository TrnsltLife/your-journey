using System;

public class Objective
{
	public string dataName;
	public Guid GUID;
	public bool isEmpty;
	public string triggerName;
	public string triggeredByName;

	public string eventName;
	public string objectiveReminder;
	public bool skipSummary;
	public string nextTrigger;
	public TextBookData textBookData;
	public int loreReward, xpReward, threatReward;

	public string TranslationKey(string suffix) { return "objective." + dataName + "." + suffix; }
}