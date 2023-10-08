using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class GameState
{
	public string coverImage;
	public CampaignState campaignState;
	public PartyState partyState;
	public TriggerState triggerState;
	public ObjectiveState objectiveState;
	public MonsterState monsterState;
	public ChapterState chapterState;
	public TileState tileState;
	public InteractionState interactionState;
	public CamState camState;

	public void SaveState( Engine engine, int saveIndex )
	{
		if ( saveIndex == -1 )//not saving, bug out
		{
			Debug.Log( "SaveState::NOT saving" );
			return;
		}

		SaveState( engine, GetFullSavePath( "SAVE" + saveIndex + ".sav" ) );
	}

	public void SaveStateTemp( Engine engine )
	{
		SaveState( engine, GetFullSavePath( "TEMP.sav" ) );
	}

	public void SaveState( Engine engine, string fullPath )
	{
		//TODO return a bool for success?
		campaignState = CampaignState.GetState();
		coverImage = campaignState?.campaign?.coverImage ?? Bootstrap.gameStarter.coverImage; //go with the campaign cover image if available, otherwise whatever's in Bootstrap which could be a scenario cover image
		partyState = PartyState.GetState( engine );
		triggerState = engine.triggerManager.GetState();
		objectiveState = engine.objectiveManager.GetState();
		monsterState = GlowEngine.FindObjectOfType<MonsterManager>().GetState();
		chapterState = engine.chapterManager.GetState();
		tileState = engine.tileManager.GetState();
		interactionState = engine.interactionManager.GetState();
		camState = GlowEngine.FindObjectOfType<CamControl>().GetState();

		//string basePath = Path.Combine( Environment.ExpandEnvironmentVariables( "%userprofile%" ), "Documents", "Your Journey", "Saves" );
		string basePath = GetFullSavePath();
		if ( basePath is null )
			return;

		//old way: string output = JsonConvert.SerializeObject( this, Formatting.Indented, new Vector3Converter() );
		//new way to avoid circular references related to token rotation?
		List<JsonConverter> jsonConverters = new List<JsonConverter>();
		jsonConverters.Add(new Vector3Converter());
		jsonConverters.Add(new QuaternionConverter());
		string output = JsonConvert.SerializeObject(this, Formatting.Indented, 
			new JsonSerializerSettings()
			{
				//PreserveReferencesHandling = PreserveReferencesHandling.Objects,
				//ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Converters = jsonConverters
			});
		//string outpath = Path.Combine( basePath, "SAVE" + saveIndex + ".sav" );
		Debug.Log( "SaveState::SAVING TO: " + fullPath );

		try
		{
			using ( var stream = File.CreateText( fullPath ) )
			{
				stream.Write( output );
			}
		}
		catch
		{
			Debug.Log( "Could not save the state" );
		}
	}

	/// <summary>
	/// saves the game state with no scenario state data (ie: only the campaign state is saved), use for NEW campaigns and saving a campaign when a scenario was just finished (flushes the unneeded scenario data)
	/// </summary>
	public bool SaveCampaignState( int saveIndex, CampaignState campaignState )
	{
		if ( saveIndex == -1 )
		{
			Debug.Log( "SaveCampaignState() ERROR::saveIndex is -1" );
			return false;
		}

		this.campaignState = campaignState;

		string fullPath = GetFullSavePath( "SAVE" + saveIndex + ".sav" );
		string output = JsonConvert.SerializeObject( this, Formatting.Indented, new Vector3Converter() );
		try
		{
			using ( var stream = File.CreateText( fullPath ) )
			{
				stream.Write( output );
			}
			Debug.Log( "SaveCampaignState()::SLOT " + saveIndex );
			Debug.Log( fullPath );
			return true;
		}
		catch
		{
			Debug.Log( "Could not save the state" );
			return false;
		}
	}

	public static GameState LoadState( int saveIndex )
	{
		if ( saveIndex < 0 )
		{
			Debug.Log( "LoadState::saveIndex not valid" );
			return null;
		}
		return LoadState( GetFullSavePath( "SAVE" + saveIndex + ".sav" ) );
	}

	public static GameState LoadStateTemp( Scenario s )
	{
		Debug.Log( "Loading TEMP state" );
		return LoadState( GetFullSavePath( "TEMP.sav" ), s );
	}
	/// <summary>
	/// expects the FULL PATH+FILENAME
	/// </summary>
	public static GameState LoadState( string filename, Scenario s = null )
	{
		//string basePath = Path.Combine( Environment.ExpandEnvironmentVariables( "%userprofile%" ), "Documents", "Your Journey", "Saves" );
		//string inpath = Path.Combine( basePath, filename );

		try
		{
			string json = "";
			using ( StreamReader sr = new StreamReader( filename ) )
			{
				json = sr.ReadToEnd();
			}

			var fm = JsonConvert.DeserializeObject<GameState>( json, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate } );

			//s is not null only when quickloading - make sure quickloading into same version of scenario as was quick saved
			if ( s != null && fm.partyState.scenarioGUID != s.scenarioGUID )
				return null;

			return fm;
		}
		catch ( Exception e )
		{
			Debug.Log( "CRITICAL ERROR: LoadState::" + filename );
			Debug.Log( e.Message );
			return null;
		}
	}

	public static IEnumerable<StateItem> GetSaveItems()
	{
		string basePath = GetFullSavePath();

		List<StateItem> items = new List<StateItem>();
		DirectoryInfo di = new DirectoryInfo( basePath );
		FileInfo[] files = di.GetFiles();

		//exclude the temp save file
		files = ( from f in files
							where f.Name != "TEMP.sav" && f.Extension == ".sav"
							select f ).ToArray();
		for ( int i = 0; i < 6; i++ )
		{
			var fi = ( from f in files
								 where f.Name == "SAVE" + i + ".sav"
								 select f ).FirstOr( null );
			//Debug.Log( fi.FullName );
			//Debug.Log( fi.Name );
			if ( fi == null )
			{
				items.Add( null );
				continue;
			}
			GameState state = LoadState( fi.FullName );
			//it's a standalone scenario
			if ( state != null && state.campaignState == null )
			{
				items.Add( new StateItem()
				{
					gameName = state.partyState.gameName,
					gameDate = state.partyState.gameDate,
					coverImage = state.coverImage,
					stateGUID = state.partyState.scenarioGUID,
					scenarioFilename = state.partyState.scenarioFileName,
					fileVersion = state.partyState.fileVersion,
					fullSavePath = fi.FullName,
					heroes = state.partyState.heroes.Aggregate( ( acc, cur ) => acc + ", " + cur ),
					heroArray = state.partyState.heroes,
					heroIndexArray = state.partyState.heroesIndex,
					projectType = ProjectType.Standalone,
					campaignState = null
				} );
			}
			//it's a campaign
			else if ( state != null && state.campaignState != null )
			{
				string path = GetFullSavePath( state.campaignState.campaign.campaignGUID.ToString() );
				path = Path.Combine( path, state.campaignState.campaign.campaignGUID.ToString() + ".json" );

				items.Add( new StateItem()
				{
					gameName = state.campaignState.gameName,
					coverImage = state.coverImage,
					heroes = state.campaignState.heroes.Aggregate( ( acc, cur ) => acc + ", " + cur ),
					heroIndexArray = state.campaignState.heroesIndex,
					projectType = ProjectType.Campaign,
					campaignState = state.campaignState,
					stateGUID = state.campaignState.campaign.campaignGUID,
					scenarioFilename = path,
					fileVersion = state.campaignState.campaign.fileVersion,
					gameDate = state.campaignState.gameDate,
					fullSavePath = fi.FullName
				} );
			}
		}
		//Debug.Log( "GetSaveItems::FOUND " + files.Count() );
		return items;
	}

	/// <summary>
	/// returns full save folder path + filename or just the save folder path if filename isn't given, checks+creates Documents/Save folder
	/// </summary>
	public static string GetFullSavePath( string filename = "" )
	{
		string mydocs = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
		string basePath = Path.Combine( mydocs, "Your Journey", "Saves" );

		if ( !Directory.Exists( basePath ) )
		{
			var di = Directory.CreateDirectory( basePath );
			if ( di == null )
			{
				Debug.Log( "Could not create the Scenario save folder.\r\nTried to create: " + basePath );
				return null;
			}
		}

		if ( !string.IsNullOrEmpty( filename ) )
			basePath = Path.Combine( mydocs, "Your Journey", "Saves", filename );

		return basePath;
	}
}

///STATE OBJECTS

public class CampaignState
{
	public Campaign campaign;
	//public CampaignStatus campaignStatus;//in menus or playing scenario
	public ScenarioStatus[] scenarioStatus;//success, failure, not played
	public int[] scenarioXP, scenarioLore;
	public int scenarioPlayingIndex;//currently PLAYING scenario (ie: replays)
	public int currentScenarioIndex;//the current scenario in the campaign
	public string gameDate;

	//this data has to be set before starting a new campaign
	public int saveStateIndex;
	public string[] heroes;
	public int[] heroesIndex;
	public string gameName;
	public Difficulty difficulty;
	//list of FIRED campaign triggers
	public List<string> campaignTriggerState = new List<string>();

	public CampaignState()
	{
		//empty ctor for json deserialization
	}

	public CampaignState( Campaign c )
	{
		campaign = c;
		//campaignStatus = CampaignStatus.InMenus;
		scenarioStatus = new ScenarioStatus[campaign.scenarioCollection.Count];
		scenarioXP = new int[campaign.scenarioCollection.Count];
		scenarioLore = new int[campaign.scenarioCollection.Count];
		gameDate = DateTime.Today.ToShortDateString();
		saveStateIndex = -1;
		scenarioPlayingIndex = 0;
		currentScenarioIndex = 0;

		scenarioStatus.Fill( ScenarioStatus.NotPlayed );
		scenarioXP.Fill( 0 );
		scenarioLore.Fill( 0 );
	}

	public static CampaignState GetState()
	{
		return Bootstrap.campaignState;
	}

	public void SetState()
	{
		Bootstrap.campaignState = this;
	}

	/// <summary>
	/// updates the lore/xp with the highest recorded value, advances scenario index if playing current scenario, updates scenario status 
	/// </summary>
	public void UpdateCampaign( int lore, int xp, bool success )
	{
		if ( success )
			scenarioStatus[scenarioPlayingIndex] = ScenarioStatus.Success;
		else
			scenarioStatus[scenarioPlayingIndex] = ScenarioStatus.Failure;

		scenarioLore[scenarioPlayingIndex] = Math.Max( lore, scenarioLore[scenarioPlayingIndex] );
		scenarioXP[scenarioPlayingIndex] = Math.Max( xp, scenarioXP[scenarioPlayingIndex] );

		//only advance current scenario if the current scenario was played
		//do NOT advance current scenario if this was a REPLAY
		if ( currentScenarioIndex == scenarioPlayingIndex )
			currentScenarioIndex = Math.Min( campaign.scenarioCollection.Count - 1, currentScenarioIndex + 1 );
		Debug.Log( "current S index: " + currentScenarioIndex );
		Debug.Log( "lore " + scenarioLore[scenarioPlayingIndex] );
		Debug.Log( "xp " + scenarioXP[scenarioPlayingIndex] );
	}
}

public class PartyState
{
	public string gameName { get; set; }
	public string gameDate { get; set; }
	public int saveStateIndex { get; set; }
	/// <summary>
	/// file NAME only, not the path
	/// </summary>
	public string scenarioFileName { get; set; }
	public string fileVersion { get; set; }
	public Guid scenarioGUID { get; set; }
	public Difficulty difficulty { get; set; }
	public string[] heroes { get; set; }
	public int[] heroesIndex { get; set; }
	public int[] lastStandCounter { get; set; }
	public bool[] isDead { get; set; }
	public int loreCount { get; set; }
	public int xpCount { get; set; }
	public int loreStartValue { get; set; }
	public int xpStartValue { get; set; }
	public int threatThreshold { get; set; }
	public List<string> chronicle { get; set; }
	public Queue<Threat> threatStack { get; set; }
	public List<FogState> fogList { get; set; } = new List<FogState>();

	public static PartyState GetState( Engine engine )
	{
		return new PartyState()
		{
			gameName = Bootstrap.gameStarter.gameName,
			gameDate = DateTime.Today.ToShortDateString(),
			saveStateIndex = Bootstrap.gameStarter.saveStateIndex,
			scenarioFileName = Bootstrap.gameStarter.scenarioFileName,
			scenarioGUID = engine.scenario.scenarioGUID,
			difficulty = Bootstrap.gameStarter.difficulty,
			loreCount = Bootstrap.loreCount,
			xpCount = Bootstrap.xpCount,
			loreStartValue = Bootstrap.gameStarter.loreStartValue,
			xpStartValue = Bootstrap.gameStarter.xpStartValue,
			threatThreshold = (int)engine.endTurnButton.currentThreat,
			threatStack = engine.endTurnButton.threatStack,
			heroes = Bootstrap.gameStarter.heroes,
			heroesIndex = Bootstrap.gameStarter.heroesIndex,
			lastStandCounter = Bootstrap.lastStandCounter,
			isDead = Bootstrap.isDead,
			fogList = engine.GetFogState(),
			fileVersion = engine.scenario.fileVersion,
			chronicle = engine.scenario.chronicle
		};
	}

	public void SetState()
	{
		Bootstrap.gameStarter.gameName = gameName;
		Bootstrap.gameStarter.saveStateIndex = saveStateIndex;
		Bootstrap.gameStarter.scenarioFileName = scenarioFileName;
		Bootstrap.gameStarter.heroes = heroes;
		Bootstrap.gameStarter.heroesIndex = heroesIndex;
		Bootstrap.gameStarter.loreStartValue = loreStartValue;
		Bootstrap.gameStarter.xpStartValue = xpStartValue;

		Bootstrap.gameStarter.difficulty = difficulty;
		Bootstrap.lastStandCounter = lastStandCounter;
		Bootstrap.isDead = isDead;
		Bootstrap.loreCount = loreCount;
		Bootstrap.xpCount = xpCount;

		Engine.currentScenario.chronicle = chronicle;
	}
}

public class InteractionState
{
	public List<StatEventState> statEventStates = new List<StatEventState>();
	public List<DialogEventState> dialogEventStates = new List<DialogEventState>();
	public List<ReplaceEventState> replaceEventStates = new List<ReplaceEventState>();
	public List<TextEventState> textEventStates = new List<TextEventState>();
	public List<Guid> activeTokenGUIDs = new List<Guid>();
}

public class TextEventState
{
	public Guid eventGUID;
	public bool hasActivated;
}

public class StatEventState
{
	public Guid eventGUID;
	public int accumulatedValue;
}

public class DialogEventState
{
	public Guid eventGUID;
	public bool hasActivated;
	public bool c1Used, c2Used, c3Used;
	public bool isDone;
}

public class ReplaceEventState
{
	public Guid eventGUID;
	public bool hasActivated;
	public bool replaceWithHasActivated;
}

public class FogState
{
	public Vector3 globalPosition;
	public string chapterName;
}

public class TriggerState
{
	public Dictionary<string, bool> firedTriggersList = new Dictionary<string, bool>();
}

public class ObjectiveState
{
	public Guid currentObjective;
}

public class MonsterState
{
	public List<SingleMonsterState> monsterList = new List<SingleMonsterState>();
	public List<int> monsterPool = new List<int>();
}

public class SingleMonsterState
{
	public Monster monster;
	public Guid eventGUID;
}

public class ChapterState
{
	public List<string> tokenTriggerQueue = new List<string>();
	public Guid previousGroupGUID;
}

public class TileState//tilemanager
{
	/// <summary>
	/// dynamic chapters that HAVE BEEN ACTIVATED
	/// </summary>
	public List<Guid> activatedDynamicChapters = new List<Guid>();
	public List<TileGroupState> tileGroupStates = new List<TileGroupState>();
}

public class TileGroupState
{
	public Vector3 globalPosition;
	public bool isExplored;
	public Guid guid;
	public List<SingleTileState> tileStates;
}

public class SingleTileState
{
	public bool isActive;
	public Guid tileGUID;
	public bool isExplored;
	public Vector3 globalPosition;
	public Vector3 globalParentPosition;
	public float globalParentYRotation;//Euler Y angle
	public List<string> tokenTriggerList = new List<string>();
	public List<TokenState> tokenStates = new List<TokenState>();
}

public class TokenState
{
	public bool isActive;
	public Vector3 localPosition;
	public float YRotation; //Euler Y angle
	public Guid parentTileGUID;
	public MetaDataJSON metaData;
}

public class CamState
{
	public Vector3 position;
	public float YRotation;//Euler y angle
}

//[JsonConverter( typeof( Vector3Converter ) )]
//public struct V3
//{
//	public float x, y, z;

//	public V3( Vector3 v3 )
//	{
//		x = v3.x;
//		y = v3.y;
//		z = v3.y;
//	}
//}