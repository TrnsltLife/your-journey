using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using static TileTextureManager;

public class TileTextureManager : MonoBehaviour
{
    //The TileTextureManager (Script) is added as one of the script components on the /Scenes/gameboard/Engine object

    public class TileAndSide
	{
		public int index;
		public string side;
		override public string ToString()
		{
			return index.ToString() + side.ToString();
		}
		public static TileAndSide FromLabel(string label)
		{
            Match m = new Regex(@"([1-5][0-9]{2})([AB]).*", RegexOptions.IgnoreCase).Match(label);
            if (!m.Success) { return null; }
            int index = int.Parse(m.Groups[1].Value);
            string side = m.Groups[2].Value;
			return new TileAndSide() { index = index, side = side };
        }
	}

	public class TileSideAndPrefab
	{
		public int index;
		public string side;
		public GameObject prefab;
		public string Label()
        {
            return index.ToString() + side;
        }
    }

    [System.Serializable]
	public class TileTexturePackEntry
	{
		public string name;
		public List<TileTextureEntry> tileTextureEntries;
	}


	[System.Serializable]
	public class TileTextureEntry
	{
		public string name;
		public Texture texture;
	}

	public static Dictionary<string, TileTextureEntry> defaultTileTextures;

	public static int tileCount = 124; //120 hex tiles + 4 battle tiles
    public static List<String> tileTexturePackDirectories = new List<String>(); //list of directories that contain tile texture packs

	public static TileManager tileManager = null;

    //monsterSkinFileNames:
    //The string values indicate the filename for the corresponding tile texture image
    public static string[] tileTextureLabels = new string[] {"100A", "100B", "101A", "101B", "102A-SP", "102B-SP", "103A-SW", "103B-SW", "104A-SW", "104B-SW",
		"200A", "200B", "201A", "201B", "202A", "202B", "203A", "203B", "204A", "204B", "205A", "205B",
		"206A", "206B", "207A", "207B", "208A", "208B", "209A", "209B", "210A-SP", "210B-SP",
		"211A-SP", "211B-SP", "212A-SP", "212B-SP", "213A-SP", "213B-SP", "214A-SP", "214B-SP", "215A-SP", "215B-SP",
		"216A-SP", "216B-SP", "217A-SP", "217B-SP", "218A-SP", "218B-SP", "219A-SP", "219B-SP", "220A-SP", "220B-SP",
		"221A-SP", "221B-SP", "222A-SW", "222B-SW", "223A-SW", "223B-SW", "224A-SW", "224B-SW", "225A-SW", "225B-SW",
		"226A-SW", "226B-SW", "227A-SW", "227B-SW",
		"300A", "300B", "301A", "301B", "302A", "302B", "303A", "303B", "304A", "304B", "305A", "305B",
		"306A", "306B", "307A", "307B", "308A", "308B", "309A-SP", "309B-SP", "310A-SP", "310B-SP",
		"311A-SP", "311B-SP", "312A-SP", "312B-SP", "313A-SP", "313B-SP", "314A-SW", "314B-SW", "315A-SW", "315B-SW",
		"316A-SW", "316B-SW", "317A-SW", "317B-SW", "318A-SW", "318B-SW", "319A-SW", "319B-SW", "320A-SW", "320B-SW",
		"400A", "400B", "401A-SP", "401B-SP", "402A-SP", "402B-SP", "403A-SW", "403B-SW", "404A-SW", "404B-SW",
		"500A-SW", "500B-SW",
		"998A", "998B", "999A", "999B"};


	public void Awake()
    {
        //Debug.Log("SkinsManager Awake!");
        tileManager = FindObjectOfType<TileManager>();
        LoadDefaultTileTextures();
		foreach(var tileTextureEntry in defaultTileTextures)
        {
            //Debug.Log(tileTexturerEntry.name);
        }
    }

	//The tileTextures array has one index for each tile name and the corresponding texture
	public static Dictionary<string, Texture> tileTextures = new Dictionary<string, Texture>();


	public static void LoadDefaultTileTextures()
	{
        defaultTileTextures = new Dictionary<string, TileTextureEntry>();
		Debug.Log("Loading static builtin tile texture packs");
        foreach (var label in tileTextureLabels)
		{
			Debug.Log("Load default tile texture: " + label);
            TileAndSide tileAndSide = TileAndSide.FromLabel(label);
			if(tileAndSide != null)
			{
				GameObject tilePrefab = tileManager.GetPrefab(tileAndSide.side, tileAndSide.index);
				Debug.Log("tilePrefab: " + tilePrefab.name);
                Transform tileTransform = tilePrefab.transform.Find("tile");
                Debug.Log("tileTransform: " + tileTransform.name);
                MeshRenderer renderer = tileTransform.GetComponent<MeshRenderer>();
                Debug.Log("renderer: " + renderer.name);
                Material originalMaterial = renderer.sharedMaterial;
                Debug.Log("originalMaterial: " + originalMaterial.name);
                Texture originalTexture = originalMaterial.mainTexture; //TODO is this OK or do I need to clone it
                Debug.Log("originalTexture: " + originalTexture.name);
                if (tilePrefab != null)
				{
                    defaultTileTextures.Add(tileAndSide.ToString(),
						new TileTextureEntry() { 
							name = label,
							texture = originalTexture
						}
					);
				}
            }
        }
	}

    public static void RestoreOriginalTileTextures()
	{
        foreach (var tileTextureKV in defaultTileTextures)
		{
			Texture originalTexture = tileTextureKV.Value.texture;
			TileAndSide tileAndSide = TileAndSide.FromLabel(tileTextureKV.Value.name);

			if (originalTexture != null)
			{
				GameObject tilePrefab = tileManager.GetPrefab(tileAndSide.side, tileAndSide.index);
				Transform tileTransform = tilePrefab.transform.Find("tile");
				MeshRenderer renderer = tileTransform.GetComponent<MeshRenderer>();
				Material originalMaterial = renderer.sharedMaterial;
				originalMaterial.mainTexture = originalTexture;
			}
		}
	}

	public static void ApplyTileTextures()
	{
        foreach (var tileTextureKV in tileTextures)
        {
            Texture customTexture = tileTextureKV.Value;
            TileAndSide tileAndSide = TileAndSide.FromLabel(tileTextureKV.Key);

            if (customTexture != null)
            {
                GameObject tilePrefab = tileManager.GetPrefab(tileAndSide.side, tileAndSide.index);
                Transform tileTransform = tilePrefab.transform.Find("tile");
                MeshRenderer renderer = tileTransform.GetComponent<MeshRenderer>();
                Material originalMaterial = renderer.sharedMaterial;
                originalMaterial.mainTexture = customTexture;
            }
        }
    }

	public static Texture TileTexture(int tileIndex, string tileSide)
    {
		return tileTextures[tileIndex.ToString() + tileSide];
    }

	public static List<String> LoadTileTexturePackDirectories()
	{
		tileTexturePackDirectories.Clear();
		string tileTexturesPath = Path.Combine(FileManager.BasePath(true), "Tiles");
		if (!Directory.Exists(tileTexturesPath))
		{
			Directory.CreateDirectory(tileTexturesPath);
		}

		string[] tileTexturePacks = Directory.GetDirectories(tileTexturesPath);
		for(int i = 0; i< tileTexturePacks.Length; i++)
		{
            tileTexturePacks[i] = new DirectoryInfo(tileTexturePacks[i]).Name;
		}

        tileTexturePackDirectories.Clear();
        tileTexturePackDirectories.AddRange(tileTexturePacks);
		return tileTexturePackDirectories;
	}

	public static void LoadTileTextures(string tileTexturePackName)
	{
        RestoreOriginalTileTextures();

		//Built-in skinpack - see the Scenes/gameboard/Engine object
		if (tileTexturePackName.StartsWith("*"))
        {
			/*
			foreach(var builtin in staticBuiltinTileTexturePacks)
            {
				if(tileTexturePackName == "*" + builtin.name + "*")
                {
					foreach(var texture2D in builtin.texture2Ds)
                    {
						string name = texture2D.name;
                        TileSideAndPrefab tileSideAndPrefab = FindTilePrefab(name);
                        if (tileSideAndPrefab != null && tileSideAndPrefab.prefab != null)
                        {
                            tileTextures[tileSideAndPrefab.Label()] = null;
                        }
                    }
                }
            }
			return;
			*/
        }

		//Tile texture pack in an external directory
		string tileTexturePackPath = Path.Combine(FileManager.BasePath(false), "Tiles", tileTexturePackName);
		if(Directory.Exists(tileTexturePackPath))
        {
			tileTextures.Clear();
			foreach(var filepath in Directory.GetFiles(tileTexturePackPath))
            {
				string name = Path.GetFileNameWithoutExtension(filepath);
                TileSideAndPrefab tileSideAndPrefab = FindTilePrefab(name);
				if(tileSideAndPrefab != null && tileSideAndPrefab.prefab != null)
                {
					tileTextures[tileSideAndPrefab.Label()] = LoadTileTexture(filepath);
                }
            }
			ApplyTileTextures();
        }
	}

	public static TileSideAndPrefab FindTilePrefab(string filename)
    {
		for(int i = 0; i< tileTextureLabels.Length; i++)
        {
            if (filename.StartsWith(tileTextureLabels[i]))
			{
				TileAndSide tileAndSide = TileAndSide.FromLabel(filename);
                GameObject prefab = tileManager.GetPrefab(tileAndSide.side, tileAndSide.index);
				TileSideAndPrefab tileSideAndPrefab = new TileSideAndPrefab() { index = tileAndSide.index, side = tileAndSide.side, prefab = prefab };
                return tileSideAndPrefab;
			}
        }
		return null;
    }

	private static Texture2D LoadTileTexture(string filepath)
	{
		if (string.IsNullOrEmpty(filepath)) return null;
		if (File.Exists(filepath))
		{
			int textureWidth = 1024, textureHeight = 1024;
			byte[] bytes = File.ReadAllBytes(filepath);
			Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
			texture.LoadImage(bytes);
			return texture;
		}
		return null;
	}
}
