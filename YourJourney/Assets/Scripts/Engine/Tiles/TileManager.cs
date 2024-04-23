using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LanguageManager;

public class TileManager : MonoBehaviour
{
	public GameObject[] tilePrefabs;
	public GameObject[] tilePrefabsB;
	public GameObject searchTokenPrefab, darkTokenPrefab, threatTokenPrefab, difficultGroundTokenPrefab, fortifiedTokenPrefab, noneTokenPrefab, startTokenPrefab;
	public GameObject humanTokenPrefab, elfTokenPrefab, dwarfTokenPrefab, hobbitTokenPrefab;
	public GameObject barrelsTokenPrefab, barricadeTokenPrefab, chestTokenPrefab, elevationTokenPrefab, logTokenPrefab, tableTokenPrefab;
	public GameObject boulderTokenPrefab, bushTokenPrefab, firePitTokenPrefab, rubbleTokenPrefab, statueTokenPrefab, webTokenPrefab;
	public GameObject fountainTokenPrefab, mistTokenPrefab, pitTokenPrefab, pondTokenPrefab;
	public GameObject fenceTokenPrefab, streamTokenPrefab, trenchTokenPrefab, wallTokenPrefab;
	public GameObject fogPrefab;
	public PartyPanel partyPanel;
	public ChroniclePanel chroniclePanel;
	public SettingsDialog settingsDialog;

	CombatPanel combatPanel;
	ProvokeMessage provokePanel;
	InteractionManager interactionManager;
	bool disableInput = false;
	Camera theCamera;

	List<TileGroup> tileGroupList = new List<TileGroup>();

	void Awake()
	{
		theCamera = Camera.main;
		combatPanel = FindObjectOfType<CombatPanel>();
		provokePanel = FindObjectOfType<ProvokeMessage>();
		interactionManager = FindObjectOfType<InteractionManager>();
	}

	//take an id (101) and return its prefab
	public GameObject GetPrefab( string side, int id )
	{
		//if ( id == 100 )
		//	return ATiles[0].gameObject;
		//else if ( id == 200 )
		//	return ATiles[1].gameObject;

		return side == "A" ? getATile( id ) : getBTile( id );
	}

	GameObject getATile( int id )
	{
		switch ( id )
		{
			//Original JiME Tiles
			case 100:
				return tilePrefabs[0];
			case 101:
				return tilePrefabs[1];
			case 200:
				return tilePrefabs[2];
			case 201:
				return tilePrefabs[3];
			case 202:
				return tilePrefabs[4];
			case 203:
				return tilePrefabs[5];
			case 204:
				return tilePrefabs[6];
			case 205:
				return tilePrefabs[7];
			case 206:
				return tilePrefabs[8];
			case 207:
				return tilePrefabs[9];
			case 208:
				return tilePrefabs[10];
			case 209:
				return tilePrefabs[11];
			case 300:
				return tilePrefabs[12];
			case 301:
				return tilePrefabs[13];
			case 302:
				return tilePrefabs[14];
			case 303:
				return tilePrefabs[15];
			case 304:
				return tilePrefabs[16];
			case 305:
				return tilePrefabs[17];
			case 306:
				return tilePrefabs[18];
			case 307:
				return tilePrefabs[19];
			case 308:
				return tilePrefabs[20];
			case 400:
				return tilePrefabs[21];

			//Shadowed Paths Expansion Tiles
			case 102:
				return tilePrefabs[22];
			case 210:
				return tilePrefabs[23];
			case 211:
				return tilePrefabs[24];
			case 212:
				return tilePrefabs[25];
			case 213:
				return tilePrefabs[26];
			case 214:
				return tilePrefabs[27];
			case 215:
				return tilePrefabs[28];
			case 216:
				return tilePrefabs[29];
			case 217:
				return tilePrefabs[30];
			case 218:
				return tilePrefabs[31];
			case 219:
				return tilePrefabs[32];
			case 220:
				return tilePrefabs[33];
			case 221:
				return tilePrefabs[34];
			case 309:
				return tilePrefabs[35];
			case 310:
				return tilePrefabs[36];
			case 311:
				return tilePrefabs[37];
			case 312:
				return tilePrefabs[38];
			case 313:
				return tilePrefabs[39];
			case 401:
				return tilePrefabs[40];
			case 402:
				return tilePrefabs[41];

			//Spreading War Expansion Tiles
			case 103:
				return tilePrefabs[42];
			case 104:
				return tilePrefabs[43];
			case 222:
				return tilePrefabs[44];
			case 223:
				return tilePrefabs[45];
			case 224:
				return tilePrefabs[46];
			case 225:
				return tilePrefabs[47];
			case 226:
				return tilePrefabs[48];
			case 227:
				return tilePrefabs[49];
			case 314:
				return tilePrefabs[50];
			case 315:
				return tilePrefabs[51];
			case 316:
				return tilePrefabs[52];
			case 317:
				return tilePrefabs[53];
			case 318:
				return tilePrefabs[54];
			case 319:
				return tilePrefabs[55];
			case 320:
				return tilePrefabs[56];
			case 403:
				return tilePrefabs[57];
			case 404:
				return tilePrefabs[58];
			case 500:
				return tilePrefabs[59];

			//Battle Tiles
			case 998:
				return tilePrefabs[60];
			case 999:
				return tilePrefabs[61];

			//Default
			default:
				return tilePrefabs[0];
		}
	}

	GameObject getBTile( int id )
	{
		switch ( id )
		{
			//Original JiME Tiles
			case 100:
				return tilePrefabsB[0];
			case 101:
				return tilePrefabsB[1];
			case 200:
				return tilePrefabsB[2];
			case 201:
				return tilePrefabsB[3];
			case 202:
				return tilePrefabsB[4];
			case 203:
				return tilePrefabsB[5];
			case 204:
				return tilePrefabsB[6];
			case 205:
				return tilePrefabsB[7];
			case 206:
				return tilePrefabsB[8];
			case 207:
				return tilePrefabsB[9];
			case 208:
				return tilePrefabsB[10];
			case 209:
				return tilePrefabsB[11];
			case 300:
				return tilePrefabsB[12];
			case 301:
				return tilePrefabsB[13];
			case 302:
				return tilePrefabsB[14];
			case 303:
				return tilePrefabsB[15];
			case 304:
				return tilePrefabsB[16];
			case 305:
				return tilePrefabsB[17];
			case 306:
				return tilePrefabsB[18];
			case 307:
				return tilePrefabsB[19];
			case 308:
				return tilePrefabsB[20];
			case 400:
				return tilePrefabsB[21];

			//Shadowed Paths Expansion Tiles
			case 102:
				return tilePrefabsB[22];
			case 210:
				return tilePrefabsB[23];
			case 211:
				return tilePrefabsB[24];
			case 212:
				return tilePrefabsB[25];
			case 213:
				return tilePrefabsB[26];
			case 214:
				return tilePrefabsB[27];
			case 215:
				return tilePrefabsB[28];
			case 216:
				return tilePrefabsB[29];
			case 217:
				return tilePrefabsB[30];
			case 218:
				return tilePrefabsB[31];
			case 219:
				return tilePrefabsB[32];
			case 220:
				return tilePrefabsB[33];
			case 221:
				return tilePrefabsB[34];
			case 309:
				return tilePrefabsB[35];
			case 310:
				return tilePrefabsB[36];
			case 311:
				return tilePrefabsB[37];
			case 312:
				return tilePrefabsB[38];
			case 313:
				return tilePrefabsB[39];
			case 401:
				return tilePrefabsB[40];
			case 402:
				return tilePrefabsB[41];

			//Spreading War Expansion Tiles
			case 103:
				return tilePrefabsB[42];
			case 104:
				return tilePrefabsB[43];
			case 222:
				return tilePrefabsB[44];
			case 223:
				return tilePrefabsB[45];
			case 224:
				return tilePrefabsB[46];
			case 225:
				return tilePrefabsB[47];
			case 226:
				return tilePrefabsB[48];
			case 227:
				return tilePrefabsB[49];
			case 314:
				return tilePrefabsB[50];
			case 315:
				return tilePrefabsB[51];
			case 316:
				return tilePrefabsB[52];
			case 317:
				return tilePrefabsB[53];
			case 318:
				return tilePrefabsB[54];
			case 319:
				return tilePrefabsB[55];
			case 320:
				return tilePrefabsB[56];
			case 403:
				return tilePrefabsB[57];
			case 404:
				return tilePrefabsB[58];
			case 500:
				return tilePrefabsB[59];

			//Battle Tiles
			case 998:
				return tilePrefabsB[60];
			case 999:
				return tilePrefabsB[61];

			//Default
			default:
				return tilePrefabsB[0];
		}
	}

	public TileGroup[] GetAllTileGroups()
	{
		return tileGroupList.ToArray();
	}

	/// <summary>
	/// return Transform[] of all visible token positions (for spawning monsters)
	/// </summary>
	public Vector3[] GetAvailableSpawnPositions()
	{
		//get explored tiles
		var explored = from tg in tileGroupList
									 from tile in tg.tileList
									 where tile.isExplored
									 select tile;
		//Debug.Log( "GetAvailableSpawnPositions::explored: " + explored.Count() );
		List<Transform> tkattach = new List<Transform>();
		foreach ( Tile t in explored )
		{
			//get all "token attach" positions
			foreach ( Transform child in t.transform )
				if ( child.name.Contains( "token attach" ) )
					tkattach.Add( child );
		}

		//return all attach positions
		return tkattach.Select( x => new Vector3( x.position.x, .3f, x.position.z ) ).ToArray();
	}

	/// <summary>
	/// Creates a group and places all tiles in Chapter specified
	/// </summary>
	public TileGroup CreateGroupFromChapter( Chapter c )
	{
		Debug.Log( "CreateGroupFromChapter: " + c.dataName );
		//Debug.Log( "CreateGroupFromChapter:dynamic? " + c.isDynamic );
		if ( c.tileObserver.Count == 0 )
			return null;

		TileGroup tg = c.isRandomTiles ? TileGroup.CreateRandomGroup( c ) : TileGroup.CreateFixedGroup( c );
		tg.PruneInternalAnchors();
		tileGroupList.Add( tg );
		return tg;
	}

	/// <summary>
	/// Toggles mouse/touch input with a delay
	/// </summary>
	public void ToggleInput( bool disabled )
	{
		//the delay avoids things being re-enabled on the same frame
		if ( disabled == true )
			disableInput = true;
		else
			GlowTimer.SetTimer( 1, () => { disableInput = false; } );
	}

	//public bool Collision()
	//{
	//	return tileGroupList[0].CollisionCheck();
	//}

	public void RemoveAllTiles()
	{
		foreach ( var tg in tileGroupList )
			tg.RemoveGroup();
		tileGroupList.Clear();
	}

	private void Update()
	{
		if (interactionManager.PanelShowing) return;
		else if (combatPanel.gameObject.activeInHierarchy) return;
		else if (partyPanel.gameObject.activeInHierarchy) return;
		else if (chroniclePanel.gameObject.activeInHierarchy) return;
		else if (settingsDialog.gameObject.activeInHierarchy) return;
		else if (provokePanel.provokeMode) return;

		/*
		if ( interactionManager.PanelShowing
			|| partyPanel.gameObject.activeInHierarchy
			|| chroniclePanel.gameObject.activeInHierarchy
			|| combatPanel.gameObject.activeInHierarchy
			|| settingsDialog.gameObject.activeInHierarchy
			|| provokePanel.provokeMode
			)
			return;
		*/

		if ( !disableInput && Input.GetMouseButtonDown( 0 ) )
		{
			Ray ray = theCamera.ScreenPointToRay( Input.mousePosition );
			foreach ( TileGroup tg in tileGroupList )
				foreach ( Tile t in tg.tileList )
					if ( t.InputUpdate( ray ) )
						return;
		}
	}

	public int UnexploredTileCount()
	{
		var tc = from tg in tileGroupList
						 from tile in tg.tileList
						 where !tile.isExplored && tile.gameObject.activeInHierarchy
						 select tile;
		return tc.Count();
		//int c = 0;
		//foreach ( TileGroup tg in tileGroupList )
		//	foreach ( Tile t in tg.tileList )
		//		if ( !t.isExplored )
		//			c++;
		//return c;
	}

	public int ThreatTokenCount()
	{
		var tc = from tg in tileGroupList
						 from tile in tg.tileList
						 where tile.isExplored
						 select tile;

		int c = 0;
		foreach ( Tile t in tc )
		{
			Transform[] tf = t.GetChildren( "Threat Token" ).Where( x => x.gameObject.activeInHierarchy ).ToArray();
			c += tf.Count( x => x.GetComponent<MetaData>().tokenType == TokenType.Threat );
		}

		//foreach ( TileGroup tg in tileGroupList )
		//	foreach ( Tile t in tg.tileList )
		//	{
		//		Transform[] tf = t.GetChildren( "Token" );
		//		c += tf.Count( x => x.GetComponent<MetaData>().tokenType == TokenType.Threat );
		//	}
		return c;
	}

	public bool TryTriggerToken( string name )
	{
		//Debug.Log( "TryTriggerToken: " + name );
		//this method acts on ALL tiles on ALL VISIBLE chapters on the board

		//select VISIBLE tile(s) that have a token Triggered By 'name'
		var tiles = from tg in tileGroupList
								from t in tg.tileList.Where( x => x.HasTriggeredToken( name ) && x.gameObject.activeInHierarchy )
								select t;

		//enqueue it for later chapters to check when they get explored
		FindObjectOfType<ChapterManager>().EnqueueTokenTrigger( name );

		//there are tiles on the table with matching tokens, weed out explored tiles
		var explored = tiles.Where( x => x.isExplored );
		var unexplored = tiles.Where( x => !x.isExplored );

		//iterate the tiles and either reveal the token or queue it to show when its tile gets explored
		if ( explored.Count() > 0 )
		{
			//Debug.Log( "TryTriggerToken() FOUND: " + name );
			//Debug.Log( "Found " + explored.Count() + " matching EXPLORED tiles" );
			List<Tuple<int, string, Vector3[], string[]>> tokpos = new List<Tuple<int, string, Vector3[], string[]>>();
			foreach ( Tile t in explored )
			{
				Tuple<Vector3[], string[]> vectorAndName = t.RevealTriggeredTokens(name);
				tokpos.Add( new Tuple<int, string, Vector3[], string[]>( t.baseTile.idNumber, t.baseTile.tileSide,  vectorAndName.Item1, vectorAndName.Item2) );
			}
			StartCoroutine( TokenPlacementPrompt( tokpos ) );
		}

		//if ( unexplored.Count() > 0 )
		//	Debug.Log( "Found " + unexplored.Count() + " matching UNEXPLORED tiles" );
		//mark the rest to trigger later when the tiles get explored
		foreach ( Tile t in unexplored )
			t.EnqueueTokenTrigger( name );

		if ( tiles.Count() > 0 )
			return true;
		else
			return false;
	}

	IEnumerator TokenPlacementPrompt( IEnumerable<Tuple<int, string, Vector3[], string[]>> explored )
	{
		//Debug.Log( "**START TokenPlacementPrompt" );

		foreach ( Tuple<int, string, Vector3[], string[]> t in explored )//each tile...
		{
			bool waiting = true;
			//Debug.Log( $"Tokens in tile {t.Item1}: {t.Item3.Length}" );

			//foreach ( Vector3 v in t.Item3 )//each token...
			for(int i=0; i<t.Item3.Length; i++)
			{
				Vector3 v = t.Item3[i];
				string tokenName = Translate("interaction." + t.Item4[i], t.Item4[i]);
				FindObjectOfType<CamControl>().MoveTo( v );
				TextPanel p = FindObjectOfType<InteractionManager>().GetNewTextPanel();

				string s = "";
				if (t.Item1 == 998 || t.Item1 == 999) //SquareTile
				{
					s += Translate(t.Item2 == "A" ? "dialog.text.TheBattleTileGrass" : "TheBattleTileDirt", 
						"the Battle Map Tile " + (t.Item2 == "A" ? " (Grass)" : " (Dirt)"));
				}
				else //HexTile
				{
					s += Translate("dialog.text.Tile", "Tile") + " " + t.Item1 + " " + t.Item2
						+ " <font=\"Icon\">" + Collection.FromTileNumber(t.Item1).FontCharacter + "</font>"; //Add the Collection symbol.
				}

				p.ShowOkContinue( Translate("dialog.text.PlaceToken", "Place the indicated " + tokenName + " Token on " + s + ".",
					new List<string> {tokenName, s}), 
					ButtonIcon.Continue, () => waiting = false );
				while ( waiting )
					yield return null;
			}
		}

		//Debug.Log( "**END TokenPlacementPrompt" );
	}

	/// <summary>
	/// Pre-build scenario tile layout
	/// </summary>
	public void BuildHintedScenario()
	{
		ChapterManager cm = FindObjectOfType<ChapterManager>();
		List<TileGroup> TGList = new List<TileGroup>();
		List<TileGroup> pendingTGList = new List<TileGroup>();

		//create Start chapter first
		Chapter first = cm.chapterList.Where( x => x.dataName == "Start" ).First();
		TileGroup ftg = first.tileGroup = CreateGroupFromChapter( first );
		TGList.Add( ftg );

		//build all chapter tilegroups except Start
		foreach ( Chapter c in cm.chapterList.Where( x => x.dataName != "Start" ) )
		{
			//build the tiles in the tg
			TileGroup tg = c.tileGroup = CreateGroupFromChapter( c );
			if ( tg == null )
			{
				Debug.Log( "WARNING::BuildScenario::Chapter has no tiles: " + c.dataName );
				return;
			}

			TGList.Add( tg );
		}

		//connect all blocks (Start is excluded by its nature) with a hinted attach
		foreach ( TileGroup currentTG in TGList.Where( x => x.GetChapter().attachHint != "None" ) )
		{
			//try connecting to requested Block TG, make sure it exists first and it's not itself
			if ( TGList.Any( x => x.GetChapter().dataName == currentTG.GetChapter().attachHint && x.GetChapter().dataName == currentTG.GetChapter().dataName ) )
			{
				Debug.Log( "WARNING: CONNECTING TO SELF: " + currentTG.GetChapter().dataName );
				continue;
			}

			if ( TGList.Any( x => x.GetChapter().dataName == currentTG.GetChapter().attachHint && x.GetChapter().dataName != currentTG.GetChapter().dataName ) )
			{
				TileGroup conTo = TGList.Where( x => x.GetChapter().dataName == currentTG.GetChapter().attachHint ).First();
				bool success = currentTG.AttachTo( conTo );
				if ( !success )
				{
					Debug.Log( "PENDING:" + currentTG.GetChapter().dataName );
					pendingTGList.Add( currentTG );
				}
				else
				{
					Debug.Log( "CONNECTED " + currentTG.GetChapter().dataName + " TO " + conTo.GetChapter().dataName );
				}
			}
		}

		//now do the rest (excluding Start), including any pending TGs that couldn't connect
		var theRest = pendingTGList.Concat( TGList.Where( x => x.GetChapter().attachHint == "None" && x.GetChapter().dataName != "Start" ) );
		foreach ( TileGroup currentTG in theRest )
		{
			foreach ( TileGroup tilegroup in TGList )
			{
				if ( tilegroup == currentTG )//don't connect to self
					continue;
				bool success = currentTG.AttachTo( tilegroup );
				if ( success )
					continue;
			}
		}
	}

	public void BuildScenario()
	{
		Debug.Log("BuildScenario()");
		ChapterManager cm = FindObjectOfType<ChapterManager>();
		List<TileGroup> TGList = new List<TileGroup>();

		//build ALL chapter tilegroups
		foreach ( Chapter c in cm.chapterList )
		{
			Debug.Log("Building Tile Group " + TGList.Count + " of " + cm.chapterList.Count + "...");
			//build the tiles in the tg
			TileGroup tg = c.tileGroup = CreateGroupFromChapter( c );
			if ( tg == null )
			{
				Debug.Log( "WARNING::BuildScenario::Chapter has no tiles: " + c.dataName );
				return;
			}

			TGList.Add( tg );
			Debug.Log("Built");
		}

		//all non-dynamic tiles excluding start
		int nonDynamicNonStart = TGList.Where(x => !x.GetChapter().isDynamic && x.GetChapter().dataName != "Start").Count();
		int outerCount = 1;
		foreach ( TileGroup tg in TGList.Where( x => !x.GetChapter().isDynamic && x.GetChapter().dataName != "Start" ) )
		{
			//try attaching tg to oldest tg already on board
			int nonDynamic = TGList.Where(x => !x.GetChapter().isDynamic && x.GUID != tg.GUID).Count();
			int innerCount = 1;
			foreach ( TileGroup tilegroup in TGList.Where( x => !x.GetChapter().isDynamic && x.GUID != tg.GUID ) )//every non-dynamic
			{
				Debug.Log("Attach TileGroups " + tg.GetChapter().dataName + " " + tg.ToString() + " to " + tilegroup.GetChapter().dataName + " " + tilegroup.ToString());
				Debug.Log("Attach TileGroups outerLoop " + outerCount + " of " + nonDynamicNonStart + ", innerLoop " + innerCount + " of " + nonDynamic);
				bool success = tg.AttachTo( tilegroup );
				if ( success )
				{
					Debug.Log( "***ATTACHING " + tg.GetChapter().dataName + " to " + tilegroup.GetChapter().dataName );
					GameObject fog = Instantiate( fogPrefab, transform );
					FogData fg = fog.GetComponent<FogData>();
					fg.chapterName = tg.GetChapter().dataName;
					fog.transform.position = tg.groupCenter.Y( .5f );
					Debug.Log("Attached");
					break;
				}
				innerCount++;
			}
			outerCount++;
		}
		Debug.Log("BuildScenario Complete");
	}

	public IEnumerator BuildScenarioCoroutine()
	{
		Debug.Log("BuildScenarioCoroutine()");
		ChapterManager cm = FindObjectOfType<ChapterManager>();
		List<TileGroup> TGList = new List<TileGroup>();
		Engine engine = Engine.FindEngine();

		//build ALL chapter tilegroups
		foreach (Chapter c in cm.chapterList)
		{
			engine.SetLoadingText(Translate("title.text.AMightyThemeUnfolds", "A Mighty Theme Unfolds..."));
			yield return null;
			Debug.Log("Building Tile Group " + TGList.Count + " of " + cm.chapterList.Count + "...");

			//build the tiles in the tg
			TileGroup tg = c.tileGroup = CreateGroupFromChapter(c);
			if (tg == null)
			{
				Debug.Log("WARNING::BuildScenario::Chapter has no tiles: " + c.dataName);
				continue;
			}

			TGList.Add(tg);

			if(tg.GetChapter().dataName == "Start")
            {
				tg.isPlaced = true;
            }
		}

		//Connect all non-dynamic tiles excluding start
		//int nonDynamicNonStart = TGList.Where(x => !x.isPlaced && !x.GetChapter().isDynamic && x.GetChapter().dataName != "Start").Count();
		List<TileGroup> nonDynamicNonStartList = TGList.Where(x => !x.isPlaced && !x.GetChapter().isDynamic && x.GetChapter().dataName != "Start").ToList();
		int outerCount = 1;
		//foreach (TileGroup tg in TGList.Where(x => !x.isPlaced && !x.GetChapter().isDynamic && x.GetChapter().dataName != "Start"))
		foreach (TileGroup tg in nonDynamicNonStartList)
		{
			//try attaching tg to oldest tg already on board - only ones that have already been placed
			//int nonDynamic = TGList.Where(x => x.isPlaced && !x.GetChapter().isDynamic && x.GUID != tg.GUID).Count();
			List<TileGroup> nonDynamicList = TGList.Where(x => x.isPlaced && !x.GetChapter().isDynamic && x.GUID != tg.GUID).ToList();
			int innerCount = 1;
			foreach (TileGroup tilegroup in nonDynamicList)//every non-dynamic
			{
				Debug.Log("Attach TileGroups " + tg.GetChapter().dataName + " " + tg.ToString() + " to " + tilegroup.GetChapter().dataName + " " + tilegroup.ToString());
				Debug.Log("Attach TileGroups outerLoop " + outerCount + " of " + nonDynamicNonStartList.Count() + ", innerLoop " + innerCount + " of " + nonDynamicList.Count());

				//original code
				//bool success = tg.AttachTo(tilegroup);

				//coroutine code
				engine.SetLoadingText(Translate("title.text.SingingTheWorldIntoBeing", "Singing the world into being {{ Cantata {0}, Movement {1} ",
						new List<string> { outerCount.ToString(), innerCount.ToString()}
					));
				if (engine.mapDebug)
				{
					tg.Visible(true);
					tg.Sepia(true);

					tilegroup.Visible(true);
					tilegroup.Sepia(false);
				}


				yield return StartCoroutine(tg.AttachToCoroutine(tilegroup, 0));
				//yield return StartCoroutine(tg.AttachToWithDensityPreferenceCoroutine(tilegroup, TileGroup.DensityPreference.HIGH));
				bool success = tg.attachToCoroutineResult;

				if (engine.mapDebug)
				{
					tg.Sepia(true);
					tilegroup.Sepia(true);
				}

				if (success)
				{
					tg.isPlaced = true;
					Debug.Log("***ATTACHING " + tg.GetChapter().dataName + " to " + tilegroup.GetChapter().dataName);
					GameObject fog = Instantiate(fogPrefab, transform);
					FogData fg = fog.GetComponent<FogData>();
					fg.chapterName = tg.GetChapter().dataName;
					fog.transform.position = tg.groupCenter.Y(.5f);
					Debug.Log("Attached");

					if (engine.mapDebug)
					{
						//Turn a tile group off after it has been placed
						//tg.Visible(false);
						//tilegroup.Visible(false);
						fog.SetActive(false);
					}

					break;
				}
				innerCount++;
				yield return null;
			}
			outerCount++;
		}
		Debug.Log("BuildScenario Complete");

		yield break;
	}


	/// <summary>
	/// Pre-build scenario tile layout
	/// </summary>
	public IEnumerator BuildHintedScenarioCoroutine()
	{
		Debug.Log("BuildHintedScenarioCoroutine()");
		ChapterManager cm = FindObjectOfType<ChapterManager>();
		List<TileGroup> TGList = new List<TileGroup>();
		List<TileGroup> pendingTGList = new List<TileGroup>();
		Engine engine = Engine.FindEngine();

		engine.SetLoadingText(Translate("title.text.AMightyThemeUnfolds", "A Mighty Theme Unfolds..."));
		yield return null;

		//create Start chapter first
		Debug.Log("Building Start Tile Group...");
		Chapter first = cm.chapterList.Where(x => x.dataName == "Start").First();
		TileGroup firstTG = first.tileGroup = CreateGroupFromChapter(first);
		firstTG.isPlaced = true;
		TGList.Add(firstTG);

		//build all chapter tilegroups except Start
		foreach (Chapter c in cm.chapterList.Where(x => x.dataName != "Start"))
		{
			engine.SetLoadingText(Translate("title.text.AMightyThemeUnfolds", "A Mighty Theme Unfolds..."));
			yield return null;
			Debug.Log("Building Tile Group " + TGList.Count + " of " + cm.chapterList.Count + "...");

			//build the tiles in the tg
			TileGroup tg = c.tileGroup = CreateGroupFromChapter(c);
			if (tg == null)
			{
				Debug.Log("WARNING::BuildScenario::Chapter has no tiles: " + c.dataName);
				continue;
			}

			TGList.Add(tg);
		}

		//for each block with an attach hint of "Random", assign its attach hint as one of the actual blocks
		/*
		foreach (TileGroup currentTG in TGList.Where(x => x.GetChapter().attachHint == "Random"))
		{
			string[] randomTargets = TGList.Where(tg => tg != currentTG).Select(tg => tg.GetChapter().dataName).ToArray();
			randomTargets = GlowEngine.RandomizeArray(randomTargets.ToArray());
			currentTG.GetChapter().attachHint = randomTargets[0];
		}
		*/

		//for each block with an attach hint of null or "", assign its attach hint as "Start"
		foreach (TileGroup currentTG in TGList.Where(x => x.GetChapter().attachHint == null || x.GetChapter().attachHint == "" || x.GetChapter().attachHint == "None"))
		{
			currentTG.GetChapter().attachHint = "Start";
		}


		List<TileGroup> nonPlacedNonDynamicList = TGList.Where(x => !x.isPlaced && !x.GetChapter().isDynamic).ToList();
		int outerCount = 1;
		int outerTotal = nonPlacedNonDynamicList.Count;
		while (nonPlacedNonDynamicList.Count > 0)
		{
			//Randomize the list so we pick different blocks first each time the game is run
			//nonPlacedNonDynamicList = GlowEngine.RandomizeArray(nonPlacedNonDynamicList.ToArray()).ToList();

			//Grab the first item from the placeable list whose attachHint target is on the board already, or is Random
			TileGroup tg = null;
			foreach (TileGroup candidate in nonPlacedNonDynamicList)
            {
				if(candidate.GetChapter().attachHint == "Random" ||
					TGList.Where(x => x.GetChapter().dataName == candidate.GetChapter().attachHint).Any())
                {
					tg = candidate;
					break;
                }
            }
			//If tg is null at this point, just grab the first unplaced, non-dynamic tile.
			//e.g. if the game creator made 4 blocks all assigned to each other as attachHint,
			//and none of the 4 pointed at the Start block, we'll have to point at least one at Start.
			if(tg == null)
            {
				tg = nonPlacedNonDynamicList[0];
            }

			//Connect one block with a hinted attach that is already present on the board (or Random)
			if(tg != null)
			{
				//List of potential attach blocks
				//First add the block that matches the attachHint
				List<TileGroup> nonDynamicOrderedList = TGList.Where(x => x.isPlaced && 
					x.GetChapter().dataName != tg.GetChapter().dataName && 
					x.GetChapter().dataName == tg.GetChapter().attachHint).ToList();
				int attachTileHint = nonDynamicOrderedList.Count == 1 ? tg.GetChapter().attachTileHint : 0; //If the hinted attach block was found, also use the attachTileHint
				//Then randomize the other potential placed blocks and add them to the list
				nonDynamicOrderedList.AddRange(GlowEngine.RandomizeArray(TGList.Where(x => x.isPlaced &&
					x.GetChapter().dataName != tg.GetChapter().dataName &&
					x.GetChapter().dataName != tg.GetChapter().attachHint).ToArray()).ToList());
				int innerCount = 1;
				foreach (TileGroup tilegroup in nonDynamicOrderedList)
				{
					Debug.Log("Attach TileGroups " + tg.GetChapter().dataName + " " + tg.ToString() + " with attachHint " + tg.GetChapter().attachHint + " to " + tilegroup.GetChapter().dataName + " " + tilegroup.ToString());
					Debug.Log("Attach TileGroups outerLoop " + outerCount + " of " + outerTotal + ", innerLoop " + innerCount + " of " + nonDynamicOrderedList.Count());

					//coroutine code
					engine.SetLoadingText(Translate("title.text.SingingTheWorldIntoBeing", "Singing the world into being {{ Cantata {0}, Movement {1} ",
							new List<string> { outerCount.ToString(), innerCount.ToString() }
						));
					if (engine.mapDebug)
					{
						tg.Visible(true);
						tg.Sepia(true);

						tilegroup.Visible(true);
						tilegroup.Sepia(false);
					}

					bool success = false;
					if(attachTileHint != 0)
                    {
						yield return StartCoroutine(tg.AttachToCoroutine(tilegroup, attachTileHint));
					}
					success = tg.attachToCoroutineResult;
					if(!success)
                    {
						yield return StartCoroutine(tg.AttachToCoroutine(tilegroup, 0));
						success = tg.attachToCoroutineResult;
					}

					if (engine.mapDebug)
					{
						tg.Sepia(true);
						tilegroup.Sepia(true);
					}

					if (success)
					{
						tg.isPlaced = true;
						Debug.Log("***ATTACHING " + tg.GetChapter().dataName + " to " + tilegroup.GetChapter().dataName);
						GameObject fog = Instantiate(fogPrefab, transform);
						FogData fg = fog.GetComponent<FogData>();
						fg.chapterName = tg.GetChapter().dataName;
						fog.transform.position = tg.groupCenter.Y(.5f);
						Debug.Log("Attached");

						if (engine.mapDebug)
						{
							//Turn a tile group off after it has been placed
							//tg.Visible(false);
							//tilegroup.Visible(false);
							fog.SetActive(false);
						}

						break;
					}
					innerCount++;
					attachTileHint = 0; //If it's not the first TileGroup that matches the attachHint, there won't be an attachTileHint so set it to 0.
					yield return null;
				}
				outerCount++;
			}

			nonPlacedNonDynamicList = TGList.Where(x => !x.isPlaced && !x.GetChapter().isDynamic).ToList();
		}

		Debug.Log("BuildScenario Complete");

		yield break;
	}


	public TileState GetState()
	{
		List<TileGroupState> tgStates = new List<TileGroupState>();
		List<Guid> dynamicChapters = new List<Guid>();

		foreach ( TileGroup tg in tileGroupList )
		{
			TileGroupState tgState = new TileGroupState();

			tgState.globalPosition = tg.containerObject.position;
			tgState.isExplored = tg.isExplored;
			tgState.guid = tg.GUID;
			if ( tg.GetChapter().isDynamic )
				dynamicChapters.Add( tg.GUID );

			List<SingleTileState> singleTileState = new List<SingleTileState>();

			foreach ( Tile t in tg.tileList )
			{
				SingleTileState state = new SingleTileState()
				{
					isActive = t.gameObject.activeInHierarchy,
					tileGUID = t.baseTile.GUID,
					tokenTriggerList = t.tokenTriggerList,
					isExplored = t.isExplored,
					globalPosition = t.transform.position,
					globalParentPosition = t.transform.parent.position,
					globalParentYRotation = t.transform.parent.rotation.eulerAngles.y,
					tokenStates = t.tokenStates
				};
				singleTileState.Add( state );
			}
			tgState.tileStates = singleTileState;
			tgStates.Add( tgState );
		}

		return new TileState()
		{
			activatedDynamicChapters = dynamicChapters,
			tileGroupStates = tgStates
		};
	}

	public void SetState( TileState tileState )
	{
		foreach ( TileGroup tg in tileGroupList )
		{
			TileGroupState tgs = ( from state in tileState.tileGroupStates
														 where state.guid == tg.GUID
														 select state ).FirstOr( null );
			if ( tgs != null )
				tg.SetState( tgs );
		}

		//activate dynamic chapters

	}
}