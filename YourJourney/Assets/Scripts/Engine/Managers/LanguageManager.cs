using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour
{
	//The LanguageManager is added to the Scenes/title/Canvas object
	[System.Serializable]
	public class TranslationFileEntry : IComparable<TranslationFileEntry>
	{
		public string languageCode;
		public string languageName;
		public TextAsset internalFile;
		public string externalFile;

        public int CompareTo(TranslationFileEntry other)
        {
            return String.Compare(languageName, other.languageName, StringComparison.OrdinalIgnoreCase);
        }
    }

	public List<TranslationFileEntry> defaultLanguageFiles;

	public static List<TranslationFileEntry> staticLanguageFiles;

	public static List<TranslationFileEntry> languageFiles = new List<TranslationFileEntry>(); //list of default language translations plus language files in the Languages folder

	public static Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>();

	public static Dictionary<string, string> scenarioLanguages = new Dictionary<string, string>();
	public static Dictionary<string, Dictionary<string, string>> scenarioTranslations = new Dictionary<string, Dictionary<string, string>>();

	public static string currentLanguage = "English";
	public static string currentLanguageCode = "en";

	public static List<Action> translationUpdateSubscribers = new List<Action>();

	public void Awake()
    {
		staticLanguageFiles = defaultLanguageFiles;
    }

	public static void UpdateCurrentLanguage(string languageName)
    {
		currentLanguage = languageName;
		TranslationFileEntry entry = languageFiles.FirstOrDefault(it => it.languageName == languageName);
		if (entry == null) { return; }
		currentLanguageCode = entry.languageCode;
		//Debug.Log("Current language switched to " + currentLanguage + " / " + currentLanguageCode);
	}

	public static List<TranslationFileEntry> DiscoverLanguageFiles()
	{
		languageFiles.Clear();

		//First add all the default translation files
		foreach(var file in staticLanguageFiles)
        {
			languageFiles.Add(file);
        }

		//Next look for all custom translations in the Languages folder
		string languagesPath = Path.Combine(FileManager.BasePath(true), "Languages");
		if (!Directory.Exists(languagesPath))
		{
			Directory.CreateDirectory(languagesPath);
		}

		//...and add the external custom file to the existing default file, otherwise create a new entry for it
		string[] files = Directory.GetFiles(languagesPath, "*.json");
		for (int i = 0; i<files.Length; i++)
		{
			string fileName = new DirectoryInfo(files[i]).Name;
			string[] langPair = FilenameWithoutExtension(fileName).Split(new char[]{'_'}, 2);
			if(langPair != null && langPair.Length == 2)
			{
				string langName = langPair[0];
				string langCode = langPair[1];
				TranslationFileEntry entry = languageFiles.FirstOrDefault(it => it.languageCode == langCode);
				if(entry != null)
                {
					entry.externalFile = fileName;
					entry.languageName = langName;
                }
				else
                {
					entry = new TranslationFileEntry();
					entry.externalFile = fileName;
					entry.languageCode = langCode;
					entry.languageName = langName;
					languageFiles.Add(entry);
                }
			}
		}

		//Finally add any scenario translation languages from the current scenario
		if(scenarioLanguages != null)
        {
			foreach(var pair in scenarioLanguages)
            {
				string langCode = pair.Key;
				string langName = pair.Value;
				TranslationFileEntry entry = languageFiles.FirstOrDefault(it => it.languageCode == langCode);
				if (entry != null)
				{
					if (String.IsNullOrEmpty(entry.languageName)) { entry.languageName = langName; }
				}
				else
				{
					entry = new TranslationFileEntry();
					entry.languageCode = langCode;
					entry.languageName = langName;
					languageFiles.Add(entry);
				}
			}
		}

		languageFiles.Sort();

		//foreach(var file in languageFiles)
        //{
		//	Debug.Log(file.languageName + ":" + file.languageCode + ":" + (file.internalFile == null ? "null" : "exists") + ":" + file.externalFile);
        //}

		return languageFiles;
	}

	public static string FilenameWithoutExtension(string filename)
    {
		return Path.GetFileNameWithoutExtension(filename);
    }

	public static void LoadLanguage(string languageName)
	{
		//Debug.Log("LoadLanguage: " + languageName);
		TranslationFileEntry entry = languageFiles.FirstOrDefault(it => it.languageName == languageName);
		//Debug.Log("LoadLanguage entry " + (entry == null ? "null" : entry.languageCode));
		if(entry == null) { return; }

		string languageCode = entry.languageCode;

		translations.Remove(languageCode);

		if(entry.internalFile != null)
        {
			//Debug.Log("Internal file exists");
			var json = entry.internalFile.text;
			var keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
			translations.Add(languageCode, keyValuePairs);
		}

		//Load the external file if it exists
		if (entry.externalFile != null)
		{
			string languagePath = Path.Combine(FileManager.BasePath(false), "Languages", entry.externalFile);
			//Debug.Log("Load external translations from: " + languagePath);
			if (File.Exists(languagePath))
			{
				//Debug.Log(entry.externalFile + " exists");
				var json = File.ReadAllText(languagePath);
				var keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
				//If a default translation exists, overwrite it with the external files values if they exist
				if (translations.ContainsKey(languageCode))
				{
					Dictionary<string, string> dict = translations[languageCode];
					foreach(var keyValue in keyValuePairs)
                    {
						if(dict.ContainsKey(keyValue.Key))
                        {
							dict.Remove(keyValue.Key);
                        }
						dict.Add(keyValue.Key, keyValue.Value);
					}
				}
				//If no default translation exists, just add the keys from the external file
				else
				{
					translations.Add(languageCode, keyValuePairs);
				}
			}
		}
	}

	public static void AssignScenarioTranslations(ObservableCollection<Translation> newTranslations)
    {
		scenarioLanguages.Clear();
		scenarioTranslations.Clear();
		foreach (var translation in newTranslations)
        {
			string langCode = translation.dataName;
			string langName = translation.langName;

			scenarioLanguages.Add(langCode, langName);

			Dictionary<string, string> dict = new Dictionary<string, string>();
			scenarioTranslations.Add(langCode, dict);
			foreach(var item in translation.translationItems)
            {
				dict.Add(item.key, item.text);
            }
        }
    }

	//Interpret is used to give the translation string for Scenario text
	public static string Interpret(string key, string defval)
    {
		if(scenarioTranslations.ContainsKey(currentLanguageCode))
        {
			var keyval = scenarioTranslations[currentLanguageCode];
			if(keyval.ContainsKey(key))
            {
				return keyval[key];
            }
        }

		if(defval != null)
        {
			return defval;
        }
		return key;
    }


	//Translate is used to give the translation string for Your Journey app text
	public static string Translate(string key, string defval=null)
    {
		if(translations.ContainsKey(currentLanguageCode))
        {
			var keyval = translations[currentLanguageCode];
			if(keyval.ContainsKey(key))
            {
				return keyval[key];
            }
        }

		if(defval != null)
		{
			return defval;
		}
		return key;
    }

	public static string Translate(string key, List<string> values)
    {
		string translation = Translate(key);
		if (values != null && values.Count > 0)
		{
			translation = string.Format(translation, values.ToArray());
		}
		return translation;
	}

	public static string Translate(string key, string defval, List<string> values)
	{
		string translation = Translate(key, defval);
		if (values != null && values.Count > 0)
		{
			translation = string.Format(translation, values.ToArray());
		}
		return translation;
	}

	public static void AddSubscriber(Action onUpdateTranslation)
    {
		if (!translationUpdateSubscribers.Contains(onUpdateTranslation))
		{
			translationUpdateSubscribers.Add(onUpdateTranslation);
		}
    }

	public static void RemoveSubscriber(Action onUpateTranslation)
    {
		translationUpdateSubscribers.Remove(onUpateTranslation);
    }

	public static void ClearSubscribers()
    {
		translationUpdateSubscribers.Clear();
    }

	public static void CallSubscribers()
    {
		foreach(Action onUpdateTranslation in translationUpdateSubscribers)
        {
			if(onUpdateTranslation != null)
			{
				onUpdateTranslation();
			}
		}
    }
}
