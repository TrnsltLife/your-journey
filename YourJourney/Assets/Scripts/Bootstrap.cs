using System;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// this class maintains its state between ALL unity Scenes
/// bootstraps important data for loading a scenario
/// keeps track of persistent game data
/// </summary>
public class Bootstrap
{
	public static readonly string AppVersion = "0.25";
	public static readonly string FormatVersion = "1.13";

	//REQUIRED for playing ANY scenario, bootstraps the scenario
	public static GameStarter gameStarter;
	//REQUIRED for campaign scenarios, otherwise it's null
	public static CampaignState campaignState;

	//global state properties
	//this data is Reset for new games or restored from game state
	public static int[] lastStandCounter;
	public static bool[] isDead;
	public static int loreCount, xpCount;
	//utility data
	public static int PlayerCount { get => gameStarter.heroes.Length; }
	public static bool returnToCampaign = false;// set to true before exiting gameboard screen upon CAMPAIGN scenario completion

	//reset the randomizer on first access
	public static System.Random random = new System.Random();

	/// <summary>
	/// Resets vars and loads scenario using gameStarter
	/// </summary>
	public static Scenario LoadScenario()
	{
		ResetVars();
		Scenario scenario;
		//determine if it's a standalone scenario or one from a campaign
		if ( campaignState == null )
			scenario = FileManager.LoadScenario( FileManager.GetFullPath( gameStarter.scenarioFileName ) );
		else
		{
			string mydocs = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
			string basePath = Path.Combine( mydocs, "Your Journey", campaignState.campaign.campaignGUID.ToString(), gameStarter.scenarioFileName );
			scenario = FileManager.LoadScenario( basePath );
		}
		if ( scenario != null )
		{
			Debug.Log( "LoadLevel()::Loaded: " + gameStarter.scenarioFileName );

			return scenario;
		}
		else
		{
			Debug.Log( "ERROR::LoadLevel(): " + gameStarter.scenarioFileName );
		}

		return null;
	}

	/// <summary>
	/// loads scenario from filename (NOT including path)
	/// </summary>
	public static Scenario LoadScenarioFromFilename( string filename )
	{
		//Debug.Log( "LoadLevel(filename)::" + filename );
		try
		{
			Scenario scenario = FileManager.LoadScenario( FileManager.GetFullPath( filename ) );
			if ( scenario != null )
			{
				//Debug.Log( "LoadLevel(filename)::Loaded: " + filename );
				return scenario;
			}
			else
				return null;
		}
		catch ( Exception e )
		{
			Debug.Log( "LoadLevel(filename)::ERROR: " + filename );
			Debug.Log( "LoadLevel(filename)::ERROR: " + e.Message );
			return null;
		}
	}

	/// <summary>
	/// resets isDead, loreCount, lastStandCounter
	/// </summary>
	public static void ResetVars()
	{
		foreach ( string s in gameStarter.heroes )
			Debug.Log( "Hero:" + s );
		isDead = new bool[5];
		isDead.Fill( false );
		lastStandCounter = new int[5];
		lastStandCounter.Fill( 1 );
		loreCount = xpCount = 0;
		returnToCampaign = false;
	}

	public static Scenario DEBUGLoadLevel()
	{
		gameStarter = new GameStarter();
		gameStarter.heroes = new string[2] { "P1", "P2" };
		gameStarter.scenarioFileName = FileManager.GetProjects().First().fileName;
		Debug.Log( "DEBUGLoadLevel()::Loaded: " + gameStarter.scenarioFileName );
		Scenario scenario = FileManager.LoadScenario( FileManager.GetFullPath( gameStarter.scenarioFileName ) );

		ResetVars();

		//force debug vars
		gameStarter.gameName = "DEBUG game";
		gameStarter.difficulty = Difficulty.Normal;
		gameStarter.saveStateIndex = -1;
		gameStarter.isNewGame = true;
		campaignState = null;

		return scenario;
	}

	public static string GetRandomHero()
	{
		return gameStarter.heroes[UnityEngine.Random.Range( 0, gameStarter.heroes.Length )];
	}

	/// <summary>
	/// saves custom hero name to PlayerPrefs
	/// </summary>
	public static void SaveHeroName( int index, string name )
	{
		PlayerPrefs.SetString( "Hero" + index, name );
	}

	/// <summary>
	/// gets custom hero name from PlayerPrefs
	/// </summary>
	public static string GetHeroName( int index )
	{
		return PlayerPrefs.GetString( "Hero" + index, "Hero" + index );
	}

	public static string GetSkinpack()
    {
		return PlayerPrefs.GetString("skinpack", SettingsDialog.defaultSkinpack);
    }

	public static string GetLanguage()
    {
		return PlayerPrefs.GetString("language", SettingsDialog.defaultLanguage);
    }

	public static Tuple<int, int, int, int, int, int, string, Tuple<string>> LoadSettings()
	{
		int music = PlayerPrefs.GetInt( "music", 1 );
		int vignette = PlayerPrefs.GetInt( "vignette", 1 );
		int color = PlayerPrefs.GetInt( "color", 1 );
		int width = PlayerPrefs.GetInt("width", Screen.currentResolution.width);
		int height = PlayerPrefs.GetInt("height", Screen.currentResolution.height);
		int fullscreen = PlayerPrefs.GetInt("fullscreen", 1);
		string skinpack = PlayerPrefs.GetString("skinpack", SettingsDialog.defaultSkinpack);
		string language = PlayerPrefs.GetString("language", SettingsDialog.defaultLanguage);

		LanguageManager.DiscoverLanguageFiles();
		LanguageManager.UpdateCurrentLanguage(language);

		return new Tuple<int, int, int, int, int, int, string, Tuple<string>>( music, vignette, color, width, height, fullscreen, skinpack, new Tuple<string>(language) );
	}

	public static void SaveSettings( Tuple<int, int, int, int, int, int, string, Tuple<string>> prefs )
	{
		PlayerPrefs.SetInt( "music", prefs.Item1 );
		PlayerPrefs.SetInt( "vignette", prefs.Item2 );
		PlayerPrefs.SetInt( "color", prefs.Item3 );
		PlayerPrefs.SetInt( "width", prefs.Item4 );
		PlayerPrefs.SetInt ("height", prefs.Item5 );
		PlayerPrefs.SetInt("fullscreen", prefs.Item6);
		PlayerPrefs.SetString("skinpack", prefs.Item7);
		PlayerPrefs.SetString("language", prefs.Rest.Item1);
	}
}
