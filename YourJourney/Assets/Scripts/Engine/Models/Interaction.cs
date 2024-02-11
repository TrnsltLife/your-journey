using System;
using System.Collections.ObjectModel;

//Interaction as it is imported from .jime editor file
public class Interaction
{
	public string dataName;
	public Guid GUID;
	public bool isEmpty;
	public InteractionType interactionType;
	public string triggerName;
	public string triggerAfterName { get; set; }
	public bool isRandom;
	public bool isTokenInteraction;
	public bool isFromThreatThreshold;
	public TextBookData eventBookData;
	public string tokenInteractionText;

	//Attribute Test
	public Ability testAttribute;
	public bool isCumulative;
	public bool passFail;
	public int successValue;
	public string successTrigger;
	public string failTrigger;
	public TextBookData textBookData;
	public TextBookData passBookData;
	public TextBookData failBookData;
	public TextBookData progressBookData;

	//Monster
	public bool isReuseable;
	public ObservableCollection<Monster> monsterCollection;

	//Decision
	public string choice1;
	public string choice2;
	public string choice3;
	public bool isThreeChoices { get; set; }
	public string choice1Trigger;
	public string choice2Trigger;
	public string choice3Trigger;

	//Story branch
	public string triggerTest;
	public string triggerIsSet;
	public string triggerNotSet;
	public string triggerIsSetTrigger;
	public string triggerNotSetTrigger;
	public bool branchTestEvent;
}