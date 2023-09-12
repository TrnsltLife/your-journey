using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using UnityEngine;

public class Scenario
{
	public Guid scenarioGUID { get; set; }
	public Guid campaignGUID { get; set; }
	public int loreStartValue { get; set; }
	public int xpStartValue { get; set; }
	public string specialInstructions { get; set; }
	public string coverImage { get; set; }
	public string fileVersion { get; set; }
	public string saveDate { get; set; }
	public bool scenarioTypeJourney { get; set; }
	//public string fileName { get; set; }
	public string scenarioName { get; set; }
	/// <summary>
	/// First Objective, if set in the editor
	/// </summary>
	public string objectiveName { get; set; }
	public int threatMax { get; set; }
	public bool threatNotUsed { get; set; }
	public ProjectType projectType { get; set; }
	public int shadowFear { get; set; }
	public Dictionary<string, bool> scenarioEndStatus;

	public TextBookData introBookData { get; set; }
	public int loreReward { get; set; }
	public int xpReward { get; set; }

	public List<IInteraction> interactionObserver { get; set; }
	public ObservableCollection<Trigger> triggersObserver { get; set; }
	public ObservableCollection<Objective> objectiveObserver { get; set; }
	public ObservableCollection<MonsterActivations> activationsObserver { get; set; }
	public ObservableCollection<Translation> translationObserver { get; set; }
	public ObservableCollection<TextBookData> resolutionObserver { get; set; }
	public ObservableCollection<Threat> threatObserver { get; set; }
	public ObservableCollection<Chapter> chapterObserver { get; set; }
	//[JsonConverter(typeof(CollectionConverter))]
	//public ObservableCollection<Collection> collectionObserver { get; set; }
	public ObservableCollection<int> collectionObserver { get; set; }
	public ObservableCollection<int> globalTilePool { get; set; }

	/// <summary>
	/// Load in data from FileManager
	/// </summary>
	public static Scenario CreateInstance( FileManager fm )
	{
		Scenario s = new Scenario();
		s.scenarioGUID = fm.scenarioGUID;
		s.campaignGUID = fm.campaignGUID;
		s.loreStartValue = fm.loreStartValue;
		s.xpStartValue = fm.xpStartValue;
		s.scenarioName = fm.scenarioName;
		s.fileVersion = fm.fileVersion;
		//s.fileName = fm.fileName;

		s.saveDate = fm.saveDate;
		s.projectType = fm.projectType;
		s.objectiveName = fm.objectiveName;
		s.interactionObserver = new List<IInteraction>( fm.interactions );
		s.triggersObserver = new ObservableCollection<Trigger>( fm.triggers );
		s.objectiveObserver = new ObservableCollection<Objective>( fm.objectives );

		if (fm.activations != null)
		{
			s.activationsObserver = new ObservableCollection<MonsterActivations>(fm.activations);
		}
		else
        {
			s.activationsObserver = new ObservableCollection<MonsterActivations>();
        }
		if (fm.translations != null)
		{
			s.translationObserver = new ObservableCollection<Translation>(fm.translations);
		}
		else
        {
			s.translationObserver = new ObservableCollection<Translation>();
        }
		s.resolutionObserver = new ObservableCollection<TextBookData>( fm.resolutions );
		s.threatObserver = new ObservableCollection<Threat>( fm.threats );
		s.chapterObserver = new ObservableCollection<Chapter>( fm.chapters );
		//s.collectionObserver = new ObservableCollection<Collection>(fm.collections);

		if (fm.collections != null)
		{
			s.collectionObserver = new ObservableCollection<int>(fm.collections);
		}
		else
        {
			s.collectionObserver = new ObservableCollection<int>();
			s.collectionObserver.Add(Collection.CORE_SET.ID);
        }
		s.globalTilePool = new ObservableCollection<int>( fm.globalTiles );
		s.scenarioEndStatus = new Dictionary<string, bool>( fm.scenarioEndStatus );
		//s.fileName = fm.fileName;
		s.introBookData = fm.introBookData;
		s.threatMax = fm.threatMax;
		s.threatNotUsed = fm.threatNotUsed;
		s.scenarioTypeJourney = fm.scenarioTypeJourney;
		s.shadowFear = fm.shadowFear;
		s.specialInstructions = fm.specialInstructions;
		s.coverImage = fm.coverImage;

		s.loreReward = fm.loreReward;
		s.xpReward = fm.xpReward;

		return s;
	}

	/// <summary>
	/// Create a stripped down copy of the scenario translation that only contains the scenario.name and scenario.instructions
	/// </summary>
	/// <returns></returns>
	public Dictionary<string, Dictionary<string, string>> TranslationForTitleScreens()
    {
		Dictionary<string, Dictionary<string, string>> langsDict = new Dictionary<string, Dictionary<string, string>>();
		if(translationObserver == null) { return langsDict; }
        foreach(var translation in translationObserver)
		{
			string langCode = translation.dataName;
			Dictionary<string, string> wordsDict = new Dictionary<string, string>();
			foreach(var item in translation.translationItems)
            {
				if(item.key == "scenario.scenarioName" || item.key == "scenario.instructions")
                {
					wordsDict.Add(item.key, item.text);
                }
            }
			langsDict.Add(langCode, wordsDict);
        }
		return langsDict;
    }
}