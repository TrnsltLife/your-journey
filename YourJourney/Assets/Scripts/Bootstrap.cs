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
	public static readonly string AppVersion = "0.37";
	public static readonly string FormatVersion = "1.35";

	//REQUIRED for playing ANY scenario, bootstraps the scenario
	public static GameStarter gameStarter;
	//REQUIRED for campaign scenarios, otherwise it's null
	public static CampaignState campaignState;

	//global state properties
	//this data is Reset for new games or restored from game state
	public static int[] lastStandCounter;
	public static bool[] isDead;
	public static int[] corruptionCounter = new int[6];
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
			string basePath = FileManager.BasePath(false);
            basePath = Path.Combine(basePath, campaignState.campaign.campaignGUID.ToString(), gameStarter.scenarioFileName );
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
		Debug.Log("Bootstrap.ResetVars()");
		foreach ( string s in gameStarter.heroes )
			Debug.Log( "Hero:" + s );
		isDead = new bool[6];
		isDead.Fill( false );
		lastStandCounter = new int[6];
		lastStandCounter.Fill( 1 );
		loreCount = xpCount = 0;
		returnToCampaign = false;
	}

	public static void ResetCorruption()
    {
		Debug.Log("Bootstrap.ResetCorruption()");
		corruptionCounter = new int[6];
		corruptionCounter.Fill(0);
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
    public static string GetTileTexturePack()
    {
        return PlayerPrefs.GetString("tileTexturePack", SettingsDialog.defaultTileTexturePack);
    }

    public static Settings LoadSettings()
    {
        Settings settings = new Settings
        {
            music = PlayerPrefs.GetInt("music", 1),
            vignette = PlayerPrefs.GetInt("vignette", 1),
            color = PlayerPrefs.GetInt("color", 1),
            width = PlayerPrefs.GetInt("width", Screen.currentResolution.width),
            height = PlayerPrefs.GetInt("height", Screen.currentResolution.height),
            fullscreen = PlayerPrefs.GetInt("fullscreen", 1),
            skinpack = PlayerPrefs.GetString("skinpack", SettingsDialog.defaultSkinpack),
            language = PlayerPrefs.GetString("language", SettingsDialog.defaultLanguage),
            tileTexturePack = PlayerPrefs.GetString("tileTexturePack", SettingsDialog.defaultTileTexturePack)
        };

        // keep your existing side effects
        LanguageManager.DiscoverLanguageFiles();
        LanguageManager.UpdateCurrentLanguage(settings.language);

        return settings;
    }

    public static void SaveSettings(Settings settings)
    {
        PlayerPrefs.SetInt("music", settings.music);
        PlayerPrefs.SetInt("vignette", settings.vignette);
        PlayerPrefs.SetInt("color", settings.color);
        PlayerPrefs.SetInt("width", settings.width);
        PlayerPrefs.SetInt("height", settings.height);
        PlayerPrefs.SetInt("fullscreen", settings.fullscreen);

        PlayerPrefs.SetString("skinpack", settings.skinpack ?? "");
        PlayerPrefs.SetString("language", settings.language ?? "");
        PlayerPrefs.SetString("tileTexturePack", settings.tileTexturePack ?? "");

        PlayerPrefs.Save(); // ensures it’s flushed to disk
    }
}
