using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkinsManager : MonoBehaviour
{
	//The SkinsManager is added to the /Scenes/gameboard/Engine object
	[System.Serializable]
	public class SkinpackEntry
	{
		public string name;
		public List<SkinpackSpriteEntry> sprites;
	}


	[System.Serializable]
	public class SkinpackSpriteEntry
	{
		public string name;
		public Sprite sprite;
	}

	public List<SkinpackEntry> builtinSkinpacks;
	public static List<SkinpackEntry> staticBuiltinSkinpacks;


	public static int monsterCount = 30;
	public static List<String> skinpackDirectories = new List<String>(); //list of directories that contain skin packs

	//monsterSkinFileNames:
	//The index in this array corresponds to the index in MonsterButton.monsters and CombatPanel.
	//The string values indicate the filename for the corresponding monster image
	public static string[] monsterSkinFileNames = new string[] { "ruffian", "goblin-scout", "orc-hunter", "orc-marauder", "hungry-varg", "hill-troll", "wight",
		"atari", "gargletarg", "chartooth",
		"giant-spider", "pit-goblin", "orc-taskmaster", "shadowman", "anonymous-thing", "cave-troll", "balerock", "spawn-of-uglygiant",
		"supplicant-of-more-goth", "ursula", "oliver",
		"foul-beast", "varg-rider", "siege-engine", "war-elephant", "soldier", "high-orc-warrior",
		"lord-javelin", "lich-king", "endris" };


	public void Awake()
    {
		Debug.Log("SkinsManager Awake!");
		staticBuiltinSkinpacks = builtinSkinpacks;
		foreach(var skinpack in staticBuiltinSkinpacks)
        {
			Debug.Log(skinpack.name + " " + skinpack.sprites.Count);
        }
    }

	//The monsterSkins array has one index for each monster type; the array value is a list of sprites, one for each matching image for that monster type in the directory, e.g. giant-spider-0, giant-spider-1, giant-spider-2
	public static List<Sprite>[] monsterSkins = new List<Sprite>[monsterCount]; 


	public static void RestoreOriginalSkins()
	{
		Debug.Log("SkinsManager.RestoreOriginalSkins()");
		for (int i = 0; i < monsterSkins.Length; i++)
		{
			if (monsterSkins[i] == null) 
			{ 
				monsterSkins[i] = new List<Sprite>(); 
			}
			else
			{
				monsterSkins[i].Clear();
			}
		}
	}

	public static Sprite SkinVariant(int monsterIndex, int skinVariant = 0)
    {
		if(monsterIndex < 0 || monsterIndex >= monsterSkins.Length) { return null; }
		List<Sprite> spriteList = monsterSkins[monsterIndex];
		if(spriteList == null || spriteList.Count == 0) { return null; }
		if(skinVariant < 0 || skinVariant >= spriteList.Count) { skinVariant = 0; }
		return spriteList[skinVariant];
    }

	public static int RandomSkinVariantIndex(int monsterIndex)
    {
		if (monsterIndex < 0 || monsterIndex >= monsterSkins.Length) { return -1; }
		List<Sprite> spriteList = monsterSkins[monsterIndex];
		if (spriteList.Count == 0) { return -1; }
		return Bootstrap.random.Next(spriteList.Count);
	}

	public static List<String> LoadSkinpackDirectories()
	{
		skinpackDirectories.Clear();
		string skinsPath = Path.Combine(FileManager.BasePath(true), "Skins");
		if (!Directory.Exists(skinsPath))
		{
			Directory.CreateDirectory(skinsPath);
		}

		string[] skinpacks = Directory.GetDirectories(skinsPath);
		for(int i = 0; i<skinpacks.Length; i++)
		{
			skinpacks[i] = new DirectoryInfo(skinpacks[i]).Name;
		}

		skinpackDirectories.Clear();
		skinpackDirectories.AddRange(skinpacks);
		return skinpackDirectories;
	}

	public static void LoadSkins(string skinpackName)
	{
		RestoreOriginalSkins();

		//Built-in skinpack - see the Scenes/gameboard/Engine object
		if (skinpackName.StartsWith("*"))
        {
			foreach(var builtin in staticBuiltinSkinpacks)
            {
				if(skinpackName == "*" + builtin.name + "*")
                {
					foreach(var skin in builtin.sprites)
                    {
						string name = skin.name;
						int monsterIndex = FindMonsterIndexOrNegativeOne(name);
						if(monsterIndex > -1)
                        {
							monsterSkins[monsterIndex].Add(skin.sprite);
                        }
                    }
                }
            }
			return;
        }

		//Skinpack in an external directory
		string skinpackPath = Path.Combine(FileManager.BasePath(false), "Skins", skinpackName);
		if(Directory.Exists(skinpackPath))
        {
			foreach(var filepath in Directory.GetFiles(skinpackPath))
            {
				string name = Path.GetFileNameWithoutExtension(filepath);
				int monsterIndex = FindMonsterIndexOrNegativeOne(name);
				if(monsterIndex > -1)
                {
					monsterSkins[monsterIndex].Add(LoadMonsterSprite(filepath));
                }
            }
        }
	}

	public static int FindMonsterIndexOrNegativeOne(string filename)
    {
		for(int i = 0; i<monsterSkinFileNames.Length; i++)
        {
            if (filename.StartsWith(monsterSkinFileNames[i])) { return i; }
        }
		return -1;
    }

	private static Sprite LoadMonsterSprite(string filepath)
	{
		if (string.IsNullOrEmpty(filepath)) return null;
		if (File.Exists(filepath))
		{
			int spriteWidth = 420, spriteHeight = 420;
			byte[] bytes = File.ReadAllBytes(filepath);
			Texture2D texture = new Texture2D(spriteWidth, spriteHeight, TextureFormat.ARGB32, false);
			texture.LoadImage(bytes);
			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			return sprite;
		}
		return null;
	}
}
