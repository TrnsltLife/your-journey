﻿using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using static LanguageManager;

public class TileGroup
{
	public TileManager tileManager;
	public Vector3 groupCenter;
	//0th Tile is the ROOT
	public List<Tile> tileList;
	public bool isExplored { get; set; }
	public Vector3 startPosition { get; set; }
	public bool isPlaced { get; set; }

	//each Tile is a child of containerObject
	public Transform containerObject;

	//List<Vector3> childOffsets;//normalized vectors pointint to child
	Chapter chapter { get; set; }
	//int[] randomTileIndices;//randomly index into chapter tileObserver

	[HideInInspector]
	public System.Guid GUID { get; set; }//same as chapter GUID

	[HideInInspector]
	public bool attachToCoroutineResult { get; set; }

	//ConnectorGrid grid = new ConnectorGrid();

	private static float RandomAngle()
	{
		int[] a = { 0, 60, 120, 180, 240, -60, -120, -180, -240 };
		return a[Random.Range( 0, a.Length )];
	}

	TileGroup( System.Guid guid )
	{
		GUID = guid;//System.Guid.NewGuid();
	}

	public Chapter GetChapter()
	{
		return chapter;
	}

	public bool ExploredAllTiles()
    {
		foreach(Tile tile in tileList)
        {
            if (!tile.isExplored) { return false; }
        }
		return true;
    }

	public static TileGroup CreateFixedGroup( Chapter c )
	{
		TileGroup tg = new TileGroup( c.GUID );
		tg.startPosition = ( -1000f ).ToVector3();
		tg.isExplored = false;

		tg.BuildFixedFromChapter( c );
		return tg;
	}

	public static TileGroup CreateRandomGroup( Chapter c )
	{
		TileGroup tg = new TileGroup( c.GUID );
		tg.startPosition = ( -1000f ).ToVector3();
		tg.isExplored = false;

		tg.BuildRandomFromChapter( c );
		return tg;
	}

	//Build random group from editor Chapter
	void BuildRandomFromChapter( Chapter c )
	{
		Debug.Log( "BuildRandomFromChapter with tileDensityPreference " + c.tileDensityPreference );
		List<Transform> usedPositions = new List<Transform>();
		tileManager = Object.FindObjectOfType<TileManager>();
		chapter = c;
		tileList = new List<Tile>();
		//randomTileIndices = GlowEngine.GenerateRandomNumbers( chapter.randomTilePool.Count );

		//Debug.Log( "(RANDOM)FOUND " + chapter.randomTilePool.Count + " TILES" );
		//Debug.Log( "RANDOM ROOT INDEX: " + randomTileIndices[0] );

		//create the parent container
		containerObject = new GameObject().transform;
		containerObject.name = "TILEGROUP: ";

		Tile previous = null;
		for ( int i = 0; i < c.tileObserver.Count; i++ )
		{
			BaseTile tileroot = (BaseTile)c.tileObserver[i];
			tileroot.vposition = new Vector3();
			tileroot.angle = RandomAngle();

			//create parent object for prefab tile
			GameObject go = new GameObject();
			go.name = tileroot.idNumber.ToString();

			//instantiate the tile prefab
			string side = tileroot.tileSide == "Random" ? ( Random.Range( 1, 101 ) < 50 ? "A" : "B" ) : tileroot.tileSide;
			Tile tile = Object.Instantiate( tileManager.GetPrefab( side, tileroot.idNumber ), go.transform ).GetComponent<Tile>();
			tile.gameObject.SetActive( false );
			tile.baseTile = tileroot;
			tile.tileGroup = this;
			tile.chapter = c;

//Show ball/sphere/marker for anchor and connection points for debugging
//tile.gameObject.SetActive(true);
//tile.RevealAllAnchorConnectorTokens();

			//rotate go object
			tile.transform.parent.localRotation = Quaternion.Euler( 0, tileroot.angle, 0 );
			//set go's parent
			tile.transform.parent.transform.parent = containerObject;
			containerObject.name += " " + tileroot.idNumber.ToString();
			if ( previous != null )
			{
				if(chapter.tileDensityPreference == DensityPreference.FIRST)
                {
					tile.AttachTo( previous, this );
				}
				else
                {
					tile.AttachToWithDensityPreference(previous, this, chapter.tileDensityPreference);
				}
			}
			tileList.Add( tile );
			previous = tile;

//Show ball/sphere/marker for anchor and connection points for debugging
//tile.gameObject.SetActive(true);
//tile.RevealAllAnchorConnectorTokens();
//tile.GenerateConnectorGrid(false);

			//add fixed tokens
			if ( tile.baseTile.tokenList.Count > 0 )
				usedPositions.AddRange( AddFixedToken( tile ) );

			if (tileroot.isStartTile )
			{
				//Check for a Start token. If it exists, it was taken care of by the AddFixedToken and RevealStartToken methods elsewhere in this file.
				Token startToken = tileroot.tokenList.Where(it => it.tokenType == TokenType.Start).FirstOrDefault();
				TokenState startTokenState = tile.tokenStates.Where(it => it.metaData.tokenType == TokenType.Start).FirstOrDefault();

				//Default Starting Position
				if(startTokenState == null)
				{
					startPosition = tile.GetChildren("token attach")[0].position.Y(SpawnMarker.SPAWN_HEIGHT);
				}
				tile.isExplored = true;
			}
		}

		//add random tokens
		if ( c.usesRandomGroups )
			AddRandomTokens( usedPositions );

		GenerateGroupCenter();
	}



	//Build fixed group from editor Chapter
	void BuildFixedFromChapter( Chapter c )
	{
		//Debug.Log( "BuildFixedFromChapter" );
		List<Transform> usedPositions = new List<Transform>();
		tileManager = Object.FindObjectOfType<TileManager>();
		chapter = c;
		tileList = new List<Tile>();

		//Debug.Log( "(FIXED)FOUND " + chapter.tileObserver.Count + " TILES" );

		//create the parent container
		containerObject = new GameObject().transform;
		containerObject.name = "TILEGROUP: ";

		//grid.AllocateTileGroupGridSize();

		int tileMarker = 0;
		for ( int i = 0; i < c.tileObserver.Count; i++ )
		{
			tileMarker++;
			//Debug.Log( ((BaseTile)chapter.tileObserver[i]).idNumber );

			BaseTile bt = chapter.tileObserver[i] as BaseTile;
			containerObject.name += " " + bt.idNumber.ToString();
			GameObject goc = new GameObject();
			goc.name = bt.idNumber.ToString();

			Tile tile = Object.Instantiate( tileManager.GetPrefab( bt.tileSide, bt.idNumber ), goc.transform ).GetComponent<Tile>();
			//Tile tile = tileManager.GetPrefab( h.tileSide, h.idNumber );
			//set its data
			//tile.Init();
			tile.gameObject.SetActive( false );
			//tile.transform.parent = goc.transform;
			//tile.transform.localPosition = Vector3.zero;
			tile.chapter = c;
			tile.baseTile = bt;
			tile.tileGroup = this;

//Show ball/sphere/marker for anchor and connection points for debugging
//tile.gameObject.SetActive(true);
//tile.RevealAllAnchorConnectorTokens();

			//Set the tile position/coordinates
			if ( i > 0 )
			{
				Vector3 convertedSpace = Vector3.zero;
				if (tile.baseTile.tileType == TileType.Hex) { convertedSpace = ConvertHexEditorSpaceToGameSpace(tile); }
				else if (tile.baseTile.tileType == TileType.Square) { convertedSpace = ConvertSquareEditorSpaceToGameSpace(tile); }

				Vector3 tilefix = HexTileOffsetFix(tile);

				//set tile position using goc's position + reflected offset
				tile.SetPosition( tileList[0].transform.parent.transform.position + convertedSpace + tilefix, bt.angle );
				//Debug.Log( "ROOTPOS:" + tile.rootPosition.transform.position );
				//Debug.Log( "ROOT::" + tileList[0].transform.parent.transform.position );
			}
			else //First tile
			{
				Vector3 tilefix = HexTileOffsetFix(tile);

				tile.SetPosition( Vector3.zero, bt.angle );
				tile.transform.position += tilefix;
			}

			tileList.Add( tile );
			//set parent of goc 
			tile.transform.parent.transform.parent = containerObject;

//Show ball/sphere/marker for anchor and connection points for debugging
//tile.gameObject.SetActive(true);
//tile.RevealAllAnchorConnectorTokens();
//ConnectorGrid tileGrid = tile.GenerateConnectorGrid(false);
//grid.CopyTileToTileGroup(tileGrid, tileMarker);

			//add a token, if there is one
			//if ( !c.usesRandomGroups )
			if ( tile.baseTile.tokenList.Count > 0 )
				usedPositions.AddRange( AddFixedToken( tile ) );

			//find starting position if applicable and add the player spawn marker
			if ( bt.isStartTile )
			{
				//Check for a Start token. If it exists, it was taken care of by the AddFixedToken and RevealStartToken methods elsewhere in this file.
				Token startToken = bt.tokenList.Where(it => it.tokenType == TokenType.Start).FirstOrDefault();
				TokenState startTokenState = tile.tokenStates.Where(it => it.metaData.tokenType == TokenType.Start).FirstOrDefault();

				//Default Starting Position
				if (startTokenState == null)
				{
					startPosition = tile.GetChildren("token attach")[0].position.Y(SpawnMarker.SPAWN_HEIGHT);
				}
				tile.isExplored = true;
			}
		}

		//Debug.Log("Display TileGroup:\r\n" + grid.TileGroupToString());


		//add random tokens
		//if ( c.usesRandomGroups && usedPositions.Count > 0 ) // <-- This code was blocking random events from occurring *unless* there was at least one fixed token
		if ( c.usesRandomGroups )
			AddRandomTokens( usedPositions );

		GenerateGroupCenter();
	}

	Vector3 ConvertSquareEditorSpaceToGameSpace(Tile tile)
    {
		//3D distance between tiles in X and Y = 6
		//EDITOR distance between square tile centers X and Y = 425
		//425 / 6 = 70.833333
		float d = Vector3.Distance(tile.baseTile.vposition, tileList[0].baseTile.vposition);
		float scalar = d / 70.833333f;

		Vector3 offset = tile.baseTile.vposition - tileList[0].baseTile.vposition;
		Vector3 n = Vector3.Normalize(offset) * scalar;

		//reflect to account for difference in coordinate systems quadrant (2D to 3D)
		n = Vector3.Reflect(n, new Vector3(0, 0, 1));

		return n;
    }

	Vector3 ConvertHexEditorSpaceToGameSpace(Tile tile)
    {
		//3D distance between tiles in X = 0.75
		//3D distance between tiles in Y = 0.4330127

		//EDITOR distance between hextile centers = 55.425626
		//3D distance between hextile centers = .8660254
		float d = Vector3.Distance(tile.baseTile.vposition, tileList[0].baseTile.vposition);
		float scalar = .8660254f * d;
		scalar = scalar / 55.425626f;

		//get normalized EDITOR vector to first tile in this group
		Vector3 offset = tile.baseTile.vposition - tileList[0].baseTile.vposition;
		//convert normalized EDITOR vector to 3D using distance tween hexes
		Vector3 n = Vector3.Normalize(offset) * scalar;

		//reflect to account for difference in coordinate systems quadrant (2D to 3D)
		n = Vector3.Reflect(n, new Vector3(0, 0, 1));

		return n;
	}

	Vector3 HexTileOffsetFix(Tile tile)
    {
		//fix tile positions that don't have editor root hex at 0,1
		Vector3 tilefix = Vector3.zero;
		//convert the string to vector2
		string[] s = tile.baseTile.hexRoot.Split(',');
		//Debug.Log("pathRoot::" + tile.baseTile.idNumber + "::" + tile.baseTile.pathRoot);
		Vector2 p = new Vector2(float.Parse(s[0]), float.Parse(s[1]));
		if (p.y != 1)
		{
			//Debug.Log("tilefix>.Z::" + tile.baseTile.idNumber + "::" + (-.4330127f * (p.y - 1f)));
			tilefix.z = -.4330127f * (p.y - 1f);
		}
		if (p.x != 0)
		{
			//Debug.Log("tilefix>.X::" + tile.baseTile.idNumber + "::" + (p.x * .75f));
			tilefix.x = p.x * .75f;
		}
		//Debug.Log("tilefix>::" + tile.baseTile.idNumber + "::" + tilefix);
		return tilefix;
	}

	public void PruneInternalAnchors(TileGroup tileGroup2 = null)
    {
		List<Transform> connectors = GetOpenConnectorsTransforms().ToList();
		List<Transform> anchors = GetOpenAnchorsTransforms().ToList();

		if(tileGroup2 != null)
        {
			connectors.AddRange(tileGroup2.GetOpenConnectorsTransforms().ToList());
			anchors.AddRange(tileGroup2.GetOpenAnchorsTransforms().ToList());
        }

		//prune internal anchors
		for(int ic = connectors.Count - 1; ic >= 0; ic--)
        {
			for(int ia = anchors.Count - 1; ia >= 0; ia--)
            {
                if(Vector3.Distance(connectors[ic].position, anchors[ia].position) <= 0.2)
                {
					//Remove the anchor altogether
					Transform anchor = anchors[ia];
					anchors.RemoveAt(ia);
					Object.Destroy(anchor.gameObject);
                }
            }
        }

		//rename internal connectors as "connector-exclude"
		for (int c1 = 0; c1 < connectors.Count; c1++)
		{
			if(!connectors[c1].gameObject.name.Contains("exclude")) //don't look at connectors with "exclude" in their name
			{
				int nearby = 0;
				for (int c2 = 0; c2 < connectors.Count; c2++)
				{
					if (c1 != c2) //don't compare a connector to itself
					{
						if (Vector3.Distance(connectors[c1].position, connectors[c2].position) <= 1.0) //actual distances should be about 0.866
						{
							nearby++;
							if (nearby >= 6)
							{
								//add "exclude" to the name so it will be excluded from certain future tile attachment matches
								string name = connectors[c1].gameObject.name;
								connectors[c1].gameObject.name = connectors[c1].gameObject.name.Replace("connector", "connector-exclude");
								//Debug.Log("renamed " + name + " => " + connectors[c1].gameObject.name);
							}
						}
					}
				}
			}
		}
	}


	void AddRandomTokens(List<Transform> usedPositions )
	{
		if (chapter.randomInteractionGroup == "None") {return;}

		//Debug.Log("Add events to " + (chapter.isRandomTiles ? "random" : "fixed") + " tile group.");

		//usedPositions = wonky user placed token position
		InteractionManager im = GlowEngine.FindObjectOfType<InteractionManager>();
		//get array of interactions that are in the interaction group; don't include interactions that have already been placed (unless isReusable is set to indicate they can be used more than once)
		IInteraction[] interactionGroupArray = im.randomTokenInteractions
			.Where( x => x.dataName.EndsWith( chapter.randomInteractionGroup ) && (!x.isPlaced || x.isReusable)).ToArray();
		interactionGroupArray = GlowEngine.ShuffleArray(interactionGroupArray); //randomize the order so every time we play it's different
		//Debug.Log( "EVENTS IN GROUP [" + chapter.randomInteractionGroup + "]: " + interactionGroupArray.Length );

		//get all the possible token spawn locations that are NOT near FIXED tokens already placed
		List<Transform> attachTransforms = new List<Transform>();
		List<Transform> allOpenAttachTransforms = new List<Transform>();

		//Each tile
		foreach (Tile t in tileList)
		{
			var tileOpenAttachTransforms = new List<Transform>();
			attachTransforms.Clear();
			attachTransforms.AddRange( t.GetChildren( "token attach" ) );
			//Debug.Log("FIXED TOKENS on tile " + t.baseTile.idNumber + ": " + attachTransforms.Count);
			var usedInThisTile = from tu in usedPositions
								 where tu.GetComponent<MetaData>().tileID/*tile.hexTile.idNumber*/ == t.baseTile.idNumber
								 select tu;

			//Print attach point data
			/*
			foreach (Transform attachTransform in attachTransforms)
			{
				Debug.Log(t.baseTile.ToShortString() + " " + attachTransform.gameObject.name + " " + attachTransform.position);
			}
			*/

			//Each attach point
			foreach ( Transform attachTransform in attachTransforms)
			{
				float minDistance = 1000;

				//Each fixed token
				foreach ( Transform usedTransform in usedInThisTile )
				{
					float distance = Vector2.Distance(
						new Vector2(usedTransform.position.x, usedTransform.position.z), 
						new Vector2(attachTransform.position.x, attachTransform.position.z));
					/*
					Debug.Log("Compare " +
						"[" +
						attachTransform.gameObject.name + " " +
						attachTransform.position +
						"] to\r\n" +
						"[" +
						usedTransform.gameObject.name + " " +
						usedTransform.gameObject.GetComponent<MetaData>().interactionName +
						usedTransform.position +
						"]");
					Debug.Log(
						"=> distance " + distance);
					*/
					if (distance < minDistance)
					{
						minDistance = distance;
					}
				}

				Engine engine = Engine.FindEngine();
				float distanceTest = 1.5f;
				//if none of the tokens is near this attach point, add the attach point to the list
				if (minDistance > distanceTest)
				{
					//Debug.Log("Add " + attachTransform.gameObject.name + " to attach list based on minDistance.");
					tileOpenAttachTransforms.Add(attachTransform);
					if (engine.mapDebug)
					{
						GameObject flag = Object.Instantiate(engine.attachPointFlag, new Vector3(attachTransform.position.x, attachTransform.position.y, attachTransform.position.z), attachTransform.rotation, t.transform);
					}
				}
				else
				{
					//Debug.Log(attachTransform.gameObject.name + " too close to a token");
					if (engine.mapDebug)
					{
						GameObject flag = Object.Instantiate(engine.attachPointFlag, new Vector3(attachTransform.position.x, attachTransform.position.y, attachTransform.position.z), attachTransform.rotation, t.transform);
						flag.GetComponent<MeshRenderer>().material.color = new Color(255, 0, 0);
					}
				}
			}

			allOpenAttachTransforms.AddRange(tileOpenAttachTransforms);
		}

		//recreate allOpenAttachTransforms as a Set with UNIQUE items, no dupes
		var openTransformsSet = new HashSet<Transform>(allOpenAttachTransforms);
		allOpenAttachTransforms = GlowEngine.ShuffleArray(openTransformsSet.Select( x => x ).ToArray()).ToList(); //grab the array and then shuffle it so it's different every time
		//Debug.Log( "REQUESTED EVENTS: " + chapter.randomInteractionGroupCount );
		//Debug.Log( "USED POSITIONS: " + usedPositions.Count );
		//Debug.Log( "FOUND OPEN POSITIONS: " + allOpenAttachTransforms.Count() );

		//sanity check, max number of events based on how many requested and how many actually found in group how many actual open positions
		int max = Mathf.Min( interactionGroupArray.Length, Mathf.Min( chapter.randomInteractionGroupCount, allOpenAttachTransforms.Count() ) );
		//Debug.Log( $"GRABBING {max} EVENTS" );

		//generate random indexes to interactions within the group
		int[] rnds = GlowEngine.GenerateRandomNumbers( max );
		//randomly get randomInteractionGroupCount number of interactions
		IInteraction[] igs = new IInteraction[max];
		for ( int i = 0; i < max; i++ )
		{
			igs[i] = interactionGroupArray[rnds[i]];
			//Debug.Log( $"CHOSE EVENT: {igs[i].dataName} WITH TYPE {igs[i].tokenType}" );
		}

		//create the tokens on random tiles for the interactions we just got
		int[] rands = GlowEngine.GenerateRandomNumbers( max );
		for ( int i = 0; i < max; i++ )
		{
			//get tile this transform position belongs to
			Transform chosenAttachTransform = allOpenAttachTransforms[rands[i]];
			Tile tile = chosenAttachTransform.parent.GetComponent<Tile>();
			//Debug.Log( "TILE #:" + tile.baseTile.idNumber );
			//if the token points to a persistent event, swap the token type with the event it's delegating to

			//create new token prefab for this interaction
			GameObject go = null;
			if (igs[i].tokenType == TokenType.None)
			{
				go = Object.Instantiate(tileManager.noneTokenPrefab, tile.transform);
			}
			else if ( igs[i].tokenType == TokenType.Search )
			{
				go = Object.Instantiate( tileManager.searchTokenPrefab, tile.transform );
			}
			else if ( igs[i].tokenType == TokenType.Person )
			{
				if ( igs[i].personType == PersonType.Human )
					go = Object.Instantiate( tileManager.humanTokenPrefab, tile.transform );
				else if ( igs[i].personType == PersonType.Elf )
					go = Object.Instantiate( tileManager.elfTokenPrefab, tile.transform );
				else if ( igs[i].personType == PersonType.Halfpint )
					go = Object.Instantiate( tileManager.hobbitTokenPrefab, tile.transform );
				else if ( igs[i].personType == PersonType.Dwarf )
					go = Object.Instantiate( tileManager.dwarfTokenPrefab, tile.transform );
			}
			else if ( igs[i].tokenType == TokenType.Threat )
			{
				go = Object.Instantiate( tileManager.threatTokenPrefab, tile.transform );
			}
			else if ( igs[i].tokenType == TokenType.Darkness )
			{
				go = Object.Instantiate( tileManager.darkTokenPrefab, tile.transform );
			}
			else if (igs[i].tokenType == TokenType.DifficultGround)
			{
				go = Object.Instantiate(tileManager.difficultGroundTokenPrefab, tile.transform);
			}
			else if (igs[i].tokenType == TokenType.Fortified)
			{
				go = Object.Instantiate(tileManager.fortifiedTokenPrefab, tile.transform);
			}
			else if (igs[i].tokenType == TokenType.Terrain)
			{
				if (igs[i].terrainType == TerrainType.Barrels)
					go = Object.Instantiate(tileManager.barrelsTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Barricade)
					go = Object.Instantiate(tileManager.barricadeTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Boulder)
					go = Object.Instantiate(tileManager.boulderTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Bush)
					go = Object.Instantiate(tileManager.bushTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Chest)
					go = Object.Instantiate(tileManager.chestTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Elevation)
					go = Object.Instantiate(tileManager.elevationTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Fence)
					go = Object.Instantiate(tileManager.fenceTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.FirePit)
					go = Object.Instantiate(tileManager.firePitTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Fountain)
					go = Object.Instantiate(tileManager.fountainTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Log)
					go = Object.Instantiate(tileManager.logTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Mist)
					go = Object.Instantiate(tileManager.mistTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Pit)
					go = Object.Instantiate(tileManager.pitTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Pond)
					go = Object.Instantiate(tileManager.pondTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Rubble)
					go = Object.Instantiate(tileManager.rubbleTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Statue)
					go = Object.Instantiate(tileManager.statueTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Stream)
					go = Object.Instantiate(tileManager.streamTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Table)
					go = Object.Instantiate(tileManager.tableTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Trench)
					go = Object.Instantiate(tileManager.trenchTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Wall)
					go = Object.Instantiate(tileManager.wallTokenPrefab, tile.transform);
				else if (igs[i].terrainType == TerrainType.Web)
					go = Object.Instantiate(tileManager.webTokenPrefab, tile.transform);
			}
			else
			{
				Debug.Log( $"ERROR: TOKEN TYPE SET TO NONE FOR {igs[i].dataName}" );
			}

			//Scale tokens for hex map
			if (tile.baseTile.tileType == TileType.Hex)
			{
				go.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
			}
			go.transform.position = new Vector3(chosenAttachTransform.position.x, go.transform.position.y, chosenAttachTransform.position.z );
			go.GetComponent<MetaData>().tokenType = HandlePersistentTokenSwap( igs[i].dataName );//igs[i].tokenType;
			go.GetComponent<MetaData>().personType = igs[i].personType;
			go.GetComponent<MetaData>().terrainType = igs[i].terrainType;
			go.GetComponent<MetaData>().triggeredByName = "None";
			go.GetComponent<MetaData>().triggerName = "None";
			go.GetComponent<MetaData>().tokenInteractionText = igs[i].tokenInteractionText;
			go.GetComponent<MetaData>().tokenInteractionKey = igs[i].TranslationKey("tokenText");
			go.GetComponent<MetaData>().interactionName = igs[i].dataName;
			go.GetComponent<MetaData>().GUID = System.Guid.NewGuid();
			go.GetComponent<MetaData>().isRandom = true;
			//go.GetComponent<MetaData>().isCreatedFromReplaced = false;
			//go.GetComponent<MetaData>().hasBeenReplaced = false;

			//Mark the event from this group as having been placed on the map; that way if the same event group is used on another TileGroup, the same event won't come out more than once.
			igs[i].isPlaced = true;



			tile.tokenStates.Add( new TokenState()
			{
				isActive = false,
				parentTileGUID = tile.baseTile.GUID,
				localPosition = go.transform.localPosition,
				//localRotation = go.transform.localRotation,
				metaData = new MetaDataJSON( go.GetComponent<MetaData>() ),
			} );
		}
	}

	Transform[] AddFixedToken( Tile tile )
	{
		//Debug.Log("AddFixedTokens(" + tile.baseTile.idNumber + ")");
		List<Transform> usedPositions = new List<Transform>();
		//List<Vector3> openPositions = new List<Vector3>();
		//openPositions.AddRange( tile.GetChildren( "token attach" ).Select( x => x.position ) );

		foreach ( Token t in tile.baseTile.tokenList )
		{
			//if the token points to a persistent event, swap the token type with the event it's delegating to
			t.tokenType = HandlePersistentTokenSwap( t.triggerName );

			//Debug.Log( t.dataName );
			if ( /* t.tokenType == TokenType.Exploration || */ t.tokenType == TokenType.None )//sanity bail out
				continue;

			GameObject go = null;
			if (t.tokenType == TokenType.None)
			{
				go = Object.Instantiate(tileManager.noneTokenPrefab, tile.transform);
			}
			else if (t.tokenType == TokenType.Start)
            {
				go = Object.Instantiate(tileManager.startTokenPrefab, tile.transform);
            }
			else if ( t.tokenType == TokenType.Search )
			{
				go = Object.Instantiate( tileManager.searchTokenPrefab, tile.transform );
			}
			else if ( t.tokenType == TokenType.Person )
			{
				if ( t.personType == PersonType.Human )
					go = Object.Instantiate( tileManager.humanTokenPrefab, tile.transform );
				else if ( t.personType == PersonType.Elf )
					go = Object.Instantiate( tileManager.elfTokenPrefab, tile.transform );
				else if ( t.personType == PersonType.Halfpint )
					go = Object.Instantiate( tileManager.hobbitTokenPrefab, tile.transform );
				else if ( t.personType == PersonType.Dwarf )
					go = Object.Instantiate( tileManager.dwarfTokenPrefab, tile.transform );
			}
			else if ( t.tokenType == TokenType.Threat )
			{
				go = Object.Instantiate( tileManager.threatTokenPrefab, tile.transform );
			}
			else if ( t.tokenType == TokenType.Darkness )
			{
				go = Object.Instantiate( tileManager.darkTokenPrefab, tile.transform );
			}
			else if (t.tokenType == TokenType.DifficultGround)
			{
				go = Object.Instantiate(tileManager.difficultGroundTokenPrefab, tile.transform);
			}
			else if (t.tokenType == TokenType.Fortified)
			{
				go = Object.Instantiate(tileManager.fortifiedTokenPrefab, tile.transform);
			}
			else if (t.tokenType == TokenType.Terrain)
			{
				if (t.terrainType == TerrainType.Barrels)
					go = Object.Instantiate(tileManager.barrelsTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Barricade)
					go = Object.Instantiate(tileManager.barricadeTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Boulder)
					go = Object.Instantiate(tileManager.boulderTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Bush)
					go = Object.Instantiate(tileManager.bushTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Chest)
					go = Object.Instantiate(tileManager.chestTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Elevation)
					go = Object.Instantiate(tileManager.elevationTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Fence)
					go = Object.Instantiate(tileManager.fenceTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.FirePit)
					go = Object.Instantiate(tileManager.firePitTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Fountain)
					go = Object.Instantiate(tileManager.fountainTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Log)
					go = Object.Instantiate(tileManager.logTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Mist)
					go = Object.Instantiate(tileManager.mistTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Pit)
					go = Object.Instantiate(tileManager.pitTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Pond)
					go = Object.Instantiate(tileManager.pondTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Rubble)
					go = Object.Instantiate(tileManager.rubbleTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Statue)
					go = Object.Instantiate(tileManager.statueTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Stream)
					go = Object.Instantiate(tileManager.streamTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Table)
					go = Object.Instantiate(tileManager.tableTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Trench)
					go = Object.Instantiate(tileManager.trenchTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Wall)
					go = Object.Instantiate(tileManager.wallTokenPrefab, tile.transform);
				else if (t.terrainType == TerrainType.Web)
					go = Object.Instantiate(tileManager.webTokenPrefab, tile.transform);
			}

			//Scale tokens for hex map
			if(tile.baseTile.tileType == TileType.Hex)
            {
				go.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
			}

			go.GetComponent<MetaData>().tokenType = t.tokenType;
			go.GetComponent<MetaData>().personType = t.personType;
			go.GetComponent<MetaData>().terrainType = t.terrainType;
			go.GetComponent<MetaData>().triggeredByName = t.triggeredByName;
			go.GetComponent<MetaData>().interactionName = t.triggerName;
			go.GetComponent<MetaData>().GUID = t.GUID;
			//Get custom tokenInteractionText if there is any
			IInteraction inter = Engine.currentScenario.interactionObserver.Find(interact => t.triggerName == interact.dataName);
			go.GetComponent<MetaData>().tokenInteractionText = inter?.tokenInteractionText;
			go.GetComponent<MetaData>().tokenInteractionKey = inter?.TranslationKey("tokenText");


			//Offset to token in EDITOR coords. [256,256] is the center point since the editor board is 512x512.
			go.GetComponent<MetaData>().offset = t.vposition - new Vector3( 256, 0, 256) + new Vector3(25, 0, 25);
			if(Engine.currentScenario.fileVersion == "1.9" || Engine.currentScenario.fileVersion == "1.10")
            {
				go.GetComponent<MetaData>().offset = t.vposition - new Vector3(256, 0, 256);
			}
			else
            {
				//The tokens then need an additional offset of 25 because the editor used to offset the tokens by -25 but that functionality has been moved here instead.
				go.GetComponent<MetaData>().offset = t.vposition - new Vector3(256, 0, 256) + new Vector3(25, 0, 25);
			}

			var goMetaData = go.GetComponent<MetaData>();
			if (goMetaData.tokenType == TokenType.Terrain)
			{
				//The terrain tokens of different shapes each need different x and y offsets.
				//To be honest, I don't really understand the individual token offsets and just arrived at them by trial and error.
				//There are too many coordinate spaces in play - board in Editor, token size and offset in Editor, Game board, token size in Game, local token prefab coordinates and scale and position?
				if (new List<TerrainType>() { TerrainType.Barrels, TerrainType.Barricade, TerrainType.Chest, TerrainType.Elevation, TerrainType.Log, TerrainType.Table }.Contains(goMetaData.terrainType))
				{
					//31mm x 70mm rectangle
					go.GetComponent<MetaData>().offset = t.vposition - new Vector3(256, 0, 256) + new Vector3(10, 0, 8);
				}
				else if (new List<TerrainType>() { TerrainType.Fence, TerrainType.Stream, TerrainType.Trench, TerrainType.Wall }.Contains(goMetaData.terrainType))
				{
					//15mm x 94mm rectangle
					go.GetComponent<MetaData>().offset = t.vposition - new Vector3(256, 0, 256) + new Vector3(0, 0, 10);
				}
				else if (new List<TerrainType>() { TerrainType.Boulder, TerrainType.Bush, TerrainType.FirePit, TerrainType.Rubble, TerrainType.Statue, TerrainType.Web }.Contains(goMetaData.terrainType))
				{
					//37mm diameter ellipse
					go.GetComponent<MetaData>().offset = t.vposition - new Vector3(256, 0, 256) + new Vector3(5, 0, 5);
				}
				else if (new List<TerrainType>() { TerrainType.Fountain, TerrainType.Mist, TerrainType.Pit, TerrainType.Pond }.Contains(goMetaData.terrainType))
				{
					//75mm x 75mm rounded rectangle
					//Debug.Log("Large Round Terrain Type " + goMetaData.terrainType);
					go.GetComponent<MetaData>().offset = t.vposition - new Vector3(256, 0, 256) + new Vector3(15, 0, 15); // + new Vector3(135, 0, 100);
				}
			}
			go.GetComponent<MetaData>().isRandom = false;
			go.GetComponent<MetaData>().tileID = tile.baseTile.idNumber;
			//go.GetComponent<MetaData>().isCreatedFromReplaced = false;
			//go.GetComponent<MetaData>().hasBeenReplaced = false;
			//go.GetComponent<MetaData>().isActive = false;

			//calculate position of the Token
			Vector3 offset = go.GetComponent<MetaData>().offset;
			var center = tile.tilemesh.GetComponent<MeshRenderer>().bounds.center;
			var size = tile.tilemesh.GetComponent<MeshRenderer>().bounds.size;
			float scalar = 1;
			if (tile.baseTile.tileType == TileType.Hex)
			{
				scalar = Mathf.Max(size.x, size.z) / 512f; // 650f;
			}
			else if (tile.baseTile.tileType == TileType.Square)
			{
				scalar = Mathf.Max(size.x, size.z) / 512f;
			}
			offset *= scalar;
			offset = Vector3.Reflect( offset, new Vector3( 0, 0, 1 ) );
			var tokenPos = new Vector3( center.x + offset.x, 2, center.z + offset.z );
			go.transform.position = tokenPos.Y( 0 );

			//Rotate token around tile center here instead of Tile.RevealToken where it used to be done. Do this so AddRandomTokens will detect where fixed tokens are actually placed instead of where they were before they were properly rotated.
			go.transform.RotateAround(center, Vector3.up, tile.baseTile.angle);

			if (t.tokenType != TokenType.Start)
			{
				//Store the used position, but not for a Start token which isn't a normal token
				usedPositions.Add(go.transform);
			}

			//Rotate terrain tokens for square map
			Vector3 rotateCenter = Vector3.zero;
			if (goMetaData.tokenType == TokenType.Terrain)
			{
				var tokenSizeX = goMetaData.size.x;
				var tokenSizeZ = goMetaData.size.z;

				rotateCenter = tokenPos + new Vector3(tokenSizeX / 2, 0, -tokenSizeZ / 2);
				go.transform.RotateAround(rotateCenter, Vector3.up, (float)t.angle);
			}

			tile.tokenStates.Add( new TokenState()
			{
				isActive = false,
				parentTileGUID = tile.baseTile.GUID,
				localPosition = go.transform.localPosition,
				YRotation = (float)t.angle,
				metaData = new MetaDataJSON( go.GetComponent<MetaData>() ),
			} );
		}

		return usedPositions.ToArray();
	}

	/// <summary>
	/// Used by Replacement Event to replace existing token with new one and update owner tile's MetaDataJSON
	/// </summary>
	public MetaData ReplaceToken( IInteraction sourceEvent, MetaData oldMD, Tile tile )
	{
		GameObject go = null;

		if (sourceEvent.tokenType == TokenType.None)
		{
			go = Object.Instantiate(tileManager.noneTokenPrefab, tile.transform);
		}
		else if ( sourceEvent.tokenType == TokenType.Search )
		{
			go = Object.Instantiate( tileManager.searchTokenPrefab, tile.transform );
		}
		else if ( sourceEvent.tokenType == TokenType.Person )
		{
			if ( sourceEvent.personType == PersonType.Human )
				go = Object.Instantiate( tileManager.humanTokenPrefab, tile.transform );
			else if ( sourceEvent.personType == PersonType.Elf )
				go = Object.Instantiate( tileManager.elfTokenPrefab, tile.transform );
			else if ( sourceEvent.personType == PersonType.Halfpint )
				go = Object.Instantiate( tileManager.hobbitTokenPrefab, tile.transform );
			else if ( sourceEvent.personType == PersonType.Dwarf )
				go = Object.Instantiate( tileManager.dwarfTokenPrefab, tile.transform );
		}
		else if ( sourceEvent.tokenType == TokenType.Threat )
		{
			go = Object.Instantiate( tileManager.threatTokenPrefab, tile.transform );
		}
		else if ( sourceEvent.tokenType == TokenType.Darkness )
		{
			go = Object.Instantiate( tileManager.darkTokenPrefab, tile.transform );
		}
		else if (sourceEvent.tokenType == TokenType.DifficultGround)
		{
			go = Object.Instantiate(tileManager.difficultGroundTokenPrefab, tile.transform);
		}
		else if (sourceEvent.tokenType == TokenType.Fortified)
		{
			go = Object.Instantiate(tileManager.fortifiedTokenPrefab, tile.transform);
		}
		else if (sourceEvent.tokenType == TokenType.Terrain)
		{
			if (sourceEvent.terrainType == TerrainType.Barrels)
				go = Object.Instantiate(tileManager.barrelsTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Barricade)
				go = Object.Instantiate(tileManager.barricadeTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Boulder)
				go = Object.Instantiate(tileManager.boulderTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Bush)
				go = Object.Instantiate(tileManager.bushTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Chest)
				go = Object.Instantiate(tileManager.chestTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Elevation)
				go = Object.Instantiate(tileManager.elevationTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Fence)
				go = Object.Instantiate(tileManager.fenceTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.FirePit)
				go = Object.Instantiate(tileManager.firePitTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Fountain)
				go = Object.Instantiate(tileManager.fountainTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Log)
				go = Object.Instantiate(tileManager.logTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Mist)
				go = Object.Instantiate(tileManager.mistTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Pit)
				go = Object.Instantiate(tileManager.pitTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Pond)
				go = Object.Instantiate(tileManager.pondTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Rubble)
				go = Object.Instantiate(tileManager.rubbleTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Statue)
				go = Object.Instantiate(tileManager.statueTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Stream)
				go = Object.Instantiate(tileManager.streamTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Table)
				go = Object.Instantiate(tileManager.tableTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Trench)
				go = Object.Instantiate(tileManager.trenchTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Wall)
				go = Object.Instantiate(tileManager.wallTokenPrefab, tile.transform);
			else if (sourceEvent.terrainType == TerrainType.Web)
				go = Object.Instantiate(tileManager.webTokenPrefab, tile.transform);
		}

		//update old metadataJSON so token is not active
		TokenState oldtstate = tile.tokenStates.Where( x => x.metaData.GUID == oldMD.GUID ).FirstOr( null );
		//swap in relevant metadata from old target
		MetaData newMD = go.GetComponent<MetaData>();

		newMD.tokenType = sourceEvent.tokenType;
		newMD.personType = sourceEvent.personType;
		newMD.terrainType = sourceEvent.terrainType;
		newMD.triggeredByName = oldMD.triggeredByName;
		newMD.interactionName = sourceEvent.dataName;
		newMD.tokenInteractionText = sourceEvent.tokenInteractionText;
		newMD.tokenInteractionKey = sourceEvent.TranslationKey("tokenText");
		newMD.GUID = sourceEvent.GUID;//oldMD.GUID;
		newMD.offset = oldMD.offset;
		newMD.isRandom = false;
		//newMD.isCreatedFromReplaced = true;
		//newMD.hasBeenReplaced = false;
		newMD.tileID = tile.baseTile.idNumber;
		newMD.transform.position = oldMD.transform.position;
		newMD.transform.rotation = oldMD.transform.rotation;
		newMD.gameObject.SetActive( oldMD.gameObject.activeSelf );

		//add new token state for new token
		var ts = new TokenState()
		{
			isActive = oldtstate.isActive,
			parentTileGUID = tile.baseTile.GUID,
			localPosition = go.transform.localPosition,
			metaData = new MetaDataJSON( newMD ),
		};
		tile.tokenStates.Add( ts );

		oldtstate.isActive = false;//make old token inactive

		oldMD.gameObject.SetActive( false );//inactivate old token object
																				//oldMD.hasBeenReplaced = true;//mark it's been replaced

		return newMD;
	}

	/// <summary>
	/// swap token type to delegate event if it's a persistent event
	/// </summary>
	TokenType HandlePersistentTokenSwap( string eventName )
	{
		IInteraction persEvent = GlowEngine.FindObjectOfType<InteractionManager>().GetInteractionByName( eventName );
		if(persEvent == null) { return TokenType.None; }

		if ( persEvent is PersistentInteraction )
		{
			string delname = ( (PersistentInteraction)persEvent ).eventToActivate;
			IInteraction delEvent = GlowEngine.FindObjectOfType<InteractionManager>().GetInteractionByName( delname );
			return delEvent.tokenType;
		}

		return persEvent.tokenType;
	}

	void GenerateGroupCenter()
	{
		groupCenter = GlowEngine.AverageV3( tileList.Select( t => t.transform.position ).ToArray() );
	}

	/// <summary>
	/// animates tile up, reveals Tokens
	/// </summary>
	public void AnimateTileUp( Chapter chapter )
	{
		//Debug.Log( "AnimateTileUp::" + firstChapter );
		//animate upwards
		foreach ( Tile t in tileList )
		{
			Vector3 local = t.transform.position + new Vector3( 0, -.5f, 0 );
			t.transform.position = local;
		}
		float i = 0;
		Tweener tweener = null;
		foreach ( Tile t in tileList )
		{
			tweener = t.transform.DOMoveY( 0, 1.75f ).SetEase( Ease.OutCubic ).SetDelay( i );
			i += .5f;
		}

		//isPreExplored is false for all tiles
		//only Start can be toggled true
		if ( chapter.dataName == "Start" && chapter.isPreExplored )
			tweener?.OnComplete( () => { RevealInteractiveTokens(); } );
		else if(!Engine.currentScenario.scenarioTypeJourney)
			tweener?.OnComplete(() =>
			{
				Colorize();
				isExplored = true;
				RevealInteractiveTokens();
			});
		else if ( chapter.dataName == "Start" && !chapter.isPreExplored )
			tweener?.OnComplete( () =>
			{
				RevealExploreToken();
				RevealInteractiveTokens( true );
			} );
		else
			tweener?.OnComplete( () => { RevealExploreToken(); } );
	}

	/// <summary>
	/// Randomly attaches one group to another
	/// </summary>
	public bool AttachTo( TileGroup tgToAttachTo )
	{
		for (int i = 0; i < tgToAttachTo.tileList.Count; i++)
		{
			Debug.Log("attach to tile " + tgToAttachTo.tileList[i].baseTile.idNumber + tgToAttachTo.tileList[i].baseTile.tileSide);
			Debug.Log("attach to tile size: " + tgToAttachTo.tileList[i].meshRenderer.bounds.size);
			Debug.Log("attach to tile box min/max: " + tgToAttachTo.tileList[i].meshRenderer.bounds.min + " -> " + tgToAttachTo.tileList[i].meshRenderer.bounds.max);
		}

		//get all open connectors in THIS tilegroup (connectors are INside the bounds of the tile)
		//Debug.Log("GetOpenConnectors...");
		Vector3[] openConnectors = GlowEngine.RandomizeArray( GetOpenConnectors() );
		System.Tuple<Vector3, Vector3> openConnectorsMinMax = MinMax(openConnectors.ToList<Vector3>());
		//Debug.Log("Gotten. Connector Vector MinMax: " + openConnectorsMinMax.Item1 + " / " + openConnectorsMinMax.Item2);
		foreach(Transform t in GetOpenConnectorsTransforms())
        {
			if(Mathf.Approximately(t.position.x, openConnectorsMinMax.Item1.x) ||
				Mathf.Approximately(t.position.z, openConnectorsMinMax.Item1.z) ||
				Mathf.Approximately(t.position.x, openConnectorsMinMax.Item2.x) ||
				Mathf.Approximately(t.position.z, openConnectorsMinMax.Item2.z))
            {
				t.position.Y(3.0f);
				t.localPosition.Y(3.0f);
				//Debug.Log("Raise min/max gizmo at " + t.position);
            }
		}


		//get all open anchors on group we're connecting TO (anchors are OUTside the bounds of the tile)
		//Debug.Log("GetOpenAnchors");
		Vector3[] tgOpenConnectors = GlowEngine.RandomizeArray( tgToAttachTo.GetOpenAnchors() );
		System.Tuple<Vector3, Vector3> openAnchorsMinMax = MinMax(tgOpenConnectors.ToList<Vector3>());
		//Debug.Log("Gotten. Anchor Vector MinMax: " + openAnchorsMinMax.Item1 + " / " + openAnchorsMinMax.Item2);
		//dummy
		GameObject dummy = new GameObject();
		Vector3[] orTiles = new Vector3[tileList.Count];

		//record original CONTAINER position
		Vector3 or = containerObject.position;
		//record original TILE positions
		//Debug.Log("record tile positions...");
		for (int i = 0; i < tileList.Count; i++)
		{
			orTiles[i] = tileList[i].transform.position;
			//Debug.Log("tile " + tileList[i].baseTile.idNumber + tileList[i].baseTile.tileSide);
			//Debug.Log("tile size: " + tileList[i].meshRenderer.bounds.size);
			//Debug.Log("tile box min/max: " + tileList[i].meshRenderer.bounds.min + " -> " + tileList[i].meshRenderer.bounds.max);
		}
		//Debug.Log("recorded.");

		int vectorIndex = 1;
		bool safe = false;
		foreach ( Vector3 c in openConnectors )
		{
			//Debug.Log("foreach open connector " + vectorIndex + " of " + openConnectors.Count());
			vectorIndex++;

			safe = false;
			//parent each TILE to dummy
			//Debug.Log("parenting tiles...");
			foreach (Tile tile in tileList)
			{
				tile.transform.parent.transform.parent = dummy.transform;
			}
			//Debug.Log("parented.");
			//move containerObject to each connector in THIS group (connectors are INside the bounds of the tile)
			containerObject.position = c;
			//parent TILES back to containerObject
			//Debug.Log("parenting tiles back to containerObject...");
			foreach (Tile tile in tileList)
			{
				tile.transform.parent.transform.parent = containerObject.transform;
			}
			//Debug.Log("parented.");

			//move containerObject to each anchor trying to connect to (anchors are OUTside the bounds of the tile)
			int connectorIndex = 1;
			foreach ( Vector3 a in tgOpenConnectors )
			{
				connectorIndex++;

				containerObject.position = a;
				//rotate 360 for different orientations?

				//check collision
				if ( !CheckCollisionsWithinGroup( GetAllOpenConnectorsOnBoard() ) )
				{
					//Debug.Log("Succeeded in connecting.");
					safe = true;
					break;
				}
			}

			if ( safe )
			{
				break;
			}
			else
			{
				//Debug.Log( "RESETTING" );
				//reset tilegroup to original position
				containerObject.position = or;
				//Debug.Log("Reset tile original positions...");
				for (int i = 0; i < tileList.Count; i++)
				{
					tileList[i].transform.position = orTiles[i];
				}
				//Debug.Log("Reset.");
			}
		}

		Object.Destroy( dummy );
		if ( !safe )
		{
			Debug.Log( "AttachTo*********NOT FOUND" );
			return false;
		}

		//Debug.Log("GenerateGroupCenter...");
		GenerateGroupCenter();
		//Debug.Log("Generated: " + groupCenter);
		return true;
	}

	/// <summary>
	/// Randomly attaches one group to another - coroutine
	/// </summary>
	public System.Collections.IEnumerator AttachToCoroutine(TileGroup tgToAttachTo, int tileToAttachTo, TextPanel prepareTileTextPanel=null)
	{
		Debug.Log("AttachToCoroutine " + tgToAttachTo.GetChapter().dataName + ", tile: " + tileToAttachTo);
		WaitForSeconds wfs = new WaitForSeconds(0.5f);


		/*
		for (int i = 0; i < tgToAttachTo.tileList.Count; i++)
		{
			//Debug.Log("attach to tile " + tgToAttachTo.tileList[i].baseTile.idNumber + tgToAttachTo.tileList[i].baseTile.tileSide);
			//Debug.Log("attach to tile size: " + tgToAttachTo.tileList[i].meshRenderer.bounds.size);
			//Debug.Log("attach to tile box min/max: " + tgToAttachTo.tileList[i].meshRenderer.bounds.min + " -> " + tgToAttachTo.tileList[i].meshRenderer.bounds.max);
		}
		*/

		//get all open connectors in THIS tilegroup (connectors are INside the bounds of the tile)
		//Debug.Log("GetOpenConnectors...");
		//Vector3[] openConnectors = GlowEngine.RandomizeArray(GetOpenConnectors());
		//System.Tuple<Vector3, Vector3> openConnectorsMinMax = MinMax(openConnectorVectors.ToList<Vector3>());
		//Debug.Log("Gotten. Connector Vector MinMax: " + openConnectorsMinMax.Item1 + " / " + openConnectorsMinMax.Item2);

		Transform[] openConnectorTransforms = GlowEngine.RandomizeArray(GetOpenConnectorsTransforms(false));
		System.Tuple<Vector3, Vector3> openConnectorsMinMax = MinMax(openConnectorTransforms.Select(it => it.position).ToList<Vector3>());

		foreach (Transform t in GetOpenConnectorsTransforms())
		{
			if (Mathf.Approximately(t.position.x, openConnectorsMinMax.Item1.x) ||
				Mathf.Approximately(t.position.z, openConnectorsMinMax.Item1.z) ||
				Mathf.Approximately(t.position.x, openConnectorsMinMax.Item2.x) ||
				Mathf.Approximately(t.position.z, openConnectorsMinMax.Item2.z))
			{
				t.position.Y(3.0f);
				t.localPosition.Y(3.0f);
				//Debug.Log("Raise min/max gizmo at " + t.position);
			}
		}

		//Check if the tileToAttachTo exists in this tilegroup; if not, set it to 0
		if(!tgToAttachTo.tileList.Where(x => x.baseTile.idNumber == tileToAttachTo).Any())
        {
			tileToAttachTo = 0;
        }

		//Get the anchors of placed tile groups - one of these is what the connectors from above will attach to
		Vector3[] attachToOpenAnchors;
		if (tileToAttachTo != 0)
        {
			//Get the open anchors on the preferred attachment tile (if it exists)
			Vector3[] tileOpenAnchors = tgToAttachTo.GetOpenAnchorsFromTile(tileToAttachTo);
			List<Vector3> openAnchors = new List<Vector3>(GlowEngine.RandomizeArray(tileOpenAnchors));

			//Then get the open anchors on all the rest of the tiles in this tile group
			/*
			Vector3[] tgOpenAnchors = tgToAttachTo.GetOpenAnchorsExceptTile(tileToAttachTo);
			openAnchors.AddRange(GlowEngine.RandomizeArray(tgOpenAnchors));

			attachToOpenAnchors = openAnchors.ToArray();

			Debug.Log(chapter.dataName + " => " + chapter.attachHint + ", tileToAttachTo " + tileToAttachTo + " had " + tileOpenAnchors.Length + " anchors; other tiles had " + tgOpenAnchors.Length);
			*/

			attachToOpenAnchors = openAnchors.ToArray();
		}
		else
        {
			//get all open anchors on group we're connecting TO (anchors are OUTside the bounds of the tile)
			attachToOpenAnchors = GlowEngine.RandomizeArray(tgToAttachTo.GetOpenAnchors());
		}
		System.Tuple<Vector3, Vector3> openAnchorsMinMax = MinMax(attachToOpenAnchors.ToList<Vector3>());

		//dummy
		GameObject dummy = new GameObject();
		Vector3[] orTiles = new Vector3[tileList.Count];

		//record original CONTAINER position
		Vector3 or = containerObject.position;
		//record original TILE positions
		//Debug.Log("record tile positions...");
		for (int i = 0; i < tileList.Count; i++)
		{
			orTiles[i] = tileList[i].transform.position;
			//Debug.Log("tile " + tileList[i].baseTile.idNumber + tileList[i].baseTile.tileSide);
			//Debug.Log("tile size: " + tileList[i].meshRenderer.bounds.size);
			//Debug.Log("tile box min/max: " + tileList[i].meshRenderer.bounds.min + " -> " + tileList[i].meshRenderer.bounds.max);
		}
		//Debug.Log("recorded.");

		int vectorIndex = 1;
		bool safe = false;
		Engine engine = Engine.FindEngine();
		GameObject connectorSphere = null;

		foreach(Transform ct in openConnectorTransforms)
		{
			Vector3 c = ct.position;

			if (engine.mapDebug)
			{
				connectorSphere = RevealAnchorConnectorToken(ct, "connector");
			}

			if(prepareTileTextPanel != null)
            {
				//Display in the Prepare Tiles text panel
				prepareTileTextPanel.UpdateText("\n" + Translate("dialog.text.tile.ScoutingApproach", "\nScouting approach {0} / {1}...",
					new List<string> { vectorIndex.ToString(), openConnectorTransforms.Count().ToString()}));
			}
			else
            {
				//Display on the Loading screen
				engine.SetLoadingText2(Translate("dialog.text.Aria", ", Aria {0} / {1} }}",
					new List<string> { vectorIndex.ToString(), openConnectorTransforms.Count().ToString() }));
			}
			//Debug.Log("foreach open connector " + vectorIndex + " of " + openConnectorTransforms.Count());
			yield return null;
			vectorIndex++;

			safe = false;
			//parent each TILE to dummy
			//Debug.Log("parenting tiles...");
			foreach (Tile tile in tileList)
			{
				tile.transform.parent.transform.parent = dummy.transform;
			}
			//Debug.Log("parented.");
			//move containerObject to each connector in THIS group (connectors are INside the bounds of the tile)
			containerObject.position = c;
			//parent TILES back to containerObject
			//Debug.Log("parenting tiles back to containerObject...");
			foreach (Tile tile in tileList)
			{
				tile.transform.parent.transform.parent = containerObject.transform;
			}
			//Debug.Log("parented.");

			//move containerObject to each anchor trying to connect to (anchors are OUTside the bounds of the tile)
			int connectorIndex = 1;
			foreach (Vector3 a in attachToOpenAnchors)
			{
				connectorIndex++;

				containerObject.position = a;
				//rotate 360 for different orientations?
				yield return null;
				//yield return wfs;

				//check collision
				if (!CheckCollisionsWithinGroup(GetAllOpenConnectorsOnBoard()))
				{

					if (engine.mapDebug)
					{
						//Blink the tile group in its final position
						for (int i = 0; i < 5; i++)
						{
							Visible(false);
							yield return wfs;
							Visible(true);
							yield return wfs;
						}
					}

					//Debug.Log("Succeeded in connecting.");
					safe = true;
					Debug.Log("connected to index " + (connectorIndex-1));
					break;
				}
			}

			if (engine.mapDebug && connectorSphere != null)
			{
				connectorSphere.SetActive(false);
				Object.Destroy(connectorSphere);
			}

			if (safe)
			{
				break;
			}
			else
			{
				//Debug.Log( "RESETTING" );
				//reset tilegroup to original position
				containerObject.position = or;
				//Debug.Log("Reset tile original positions...");
				for (int i = 0; i < tileList.Count; i++)
				{
					tileList[i].transform.position = orTiles[i];
				}
				//Debug.Log("Reset.");
			}
		}

		Object.Destroy(dummy);
		if (!safe)
		{
			Debug.Log("AttachTo*********NOT FOUND");
			attachToCoroutineResult = false;
			yield break;
		}

		//Debug.Log("GenerateGroupCenter...");
		GenerateGroupCenter();
		//Debug.Log("Generated: " + groupCenter);
		attachToCoroutineResult = true;

		//This should prune anchors and exclude connectors at the boundary of the connected tiles
		//TODO: currently this won't handle if the tile also borders another tile besides this tile and tgToAttachTo
		PruneInternalAnchors(tgToAttachTo);
	}

	/// <summary>
	/// Randomly attaches one group to another - coroutine
	/// </summary>
	public System.Collections.IEnumerator AttachToWithDensityPreferenceCoroutine(TileGroup tgToAttachTo, int tileToAttachTo, DensityPreference densityPreference)
	{
		WaitForSeconds wfs = new WaitForSeconds(0.5f);

		/*
		for (int i = 0; i < tgToAttachTo.tileList.Count; i++)
		{
			Debug.Log("attach to tile " + tgToAttachTo.tileList[i].baseTile.idNumber + tgToAttachTo.tileList[i].baseTile.tileSide);
		}
		*/

		//get all open connectors in THIS tilegroup (connectors are INside the bounds of the tile)
		//Debug.Log("GetOpenConnectors...");

		Transform[] openConnectorTransforms = GlowEngine.RandomizeArray(GetOpenConnectorsTransforms(false));
		System.Tuple<Vector3, Vector3> openConnectorsMinMax = MinMax(openConnectorTransforms.Select(it => it.position).ToList<Vector3>());

		foreach (Transform t in GetOpenConnectorsTransforms())
		{
			if (Mathf.Approximately(t.position.x, openConnectorsMinMax.Item1.x) ||
				Mathf.Approximately(t.position.z, openConnectorsMinMax.Item1.z) ||
				Mathf.Approximately(t.position.x, openConnectorsMinMax.Item2.x) ||
				Mathf.Approximately(t.position.z, openConnectorsMinMax.Item2.z))
			{
				t.position.Y(3.0f);
				t.localPosition.Y(3.0f);
				//Debug.Log("Raise min/max gizmo at " + t.position);
			}
		}

		//Transform[] attachToOpenAnchorsTransforms = GlowEngine.RandomizeArray(tgToAttachTo.GetOpenAnchorsTransforms());
		Transform[] attachToOpenAnchorsTransforms;
		if (tileToAttachTo != 0)
		{
			//Get the open anchors on the preferred attachment tile (if it exists)
			Transform[] tileOpenAnchors = tgToAttachTo.GetOpenAnchorsFromTileTransforms(tileToAttachTo);
			List<Transform> openAnchors = new List<Transform>(GlowEngine.RandomizeArray(tileOpenAnchors));

			//Then get the open anchors on all the rest of the tiles in this tile group
			Transform[] tgOpenAnchors = tgToAttachTo.GetOpenAnchorsExceptTileTransforms(tileToAttachTo);
			openAnchors.AddRange(GlowEngine.RandomizeArray(tgOpenAnchors));

			attachToOpenAnchorsTransforms = openAnchors.ToArray();
		}
		else
		{
			//get all open anchors on group we're connecting TO (anchors are OUTside the bounds of the tile)
			attachToOpenAnchorsTransforms = GlowEngine.RandomizeArray(tgToAttachTo.GetOpenAnchorsTransforms());
		}
		Vector3[] attachToOpenAnchors = attachToOpenAnchorsTransforms.Select(it => it.position).ToArray<Vector3>();
		System.Tuple<Vector3, Vector3> openAnchorsMinMax = MinMax(attachToOpenAnchors.ToList<Vector3>());

		//dummy
		GameObject dummy = new GameObject();
		Vector3[] orTiles = new Vector3[tileList.Count];

		//record original CONTAINER position
		Vector3 or = containerObject.position;
		//record original TILE positions
		//Debug.Log("record tile positions...");
		for (int i = 0; i < tileList.Count; i++)
		{
			orTiles[i] = tileList[i].transform.position;
			//Debug.Log("tile " + tileList[i].baseTile.idNumber + tileList[i].baseTile.tileSide);
			//Debug.Log("tile size: " + tileList[i].meshRenderer.bounds.size);
			//Debug.Log("tile box min/max: " + tileList[i].meshRenderer.bounds.min + " -> " + tileList[i].meshRenderer.bounds.max);
		}
		//Debug.Log("recorded.");

		int vectorIndex = 1;
		bool foundAttachment = false;
		Engine engine = Engine.FindEngine();
		GameObject connectorSphere = null;
		Dictionary<System.Tuple<Transform, Transform>, int> densityMap = new Dictionary<System.Tuple<Transform, Transform>, int>();
		Transform[] allOpenConnectorsOnBoard = GetAllOpenConnectorsOnBoard();
		Transform[] allOpenAnchorsOnBoard = GetAllOpenAnchorsOnBoard();
		//foreach (Vector3 c in openConnectors)
		foreach (Transform connectorTransform in openConnectorTransforms)
		{
			Vector3 connectorTransformPosition = connectorTransform.position;

			if (engine.mapDebug)
			{
				connectorSphere = RevealAnchorConnectorToken(connectorTransform, "connector");
			}

			engine.SetLoadingText2(", Measure " + vectorIndex + " / " + openConnectorTransforms.Count() + " }");
			//Debug.Log("foreach open connector " + vectorIndex + " of " + openConnectorTransforms.Count());
			yield return null;
			vectorIndex++;

			//parent each TILE to dummy
			//Debug.Log("parenting tiles...");
			foreach (Tile tile in tileList)
			{
				tile.transform.parent.transform.parent = dummy.transform;
			}
			//Debug.Log("parented.");
			//move containerObject to each connector in THIS group (connectors are INside the bounds of the tile)
			containerObject.position = connectorTransformPosition;
			//parent TILES back to containerObject
			//Debug.Log("parenting tiles back to containerObject...");
			foreach (Tile tile in tileList)
			{
				tile.transform.parent.transform.parent = containerObject.transform;
			}
			//Debug.Log("parented.");

			//move containerObject to each anchor trying to connect to (anchors are OUTside the bounds of the tile)
			int connectorIndex = 1;
			foreach (Transform attachToAnchorTransform in attachToOpenAnchorsTransforms)
			{
				connectorIndex++;

				containerObject.position = attachToAnchorTransform.position;
				//rotate 360 for different orientations?
				yield return null;
				//yield return wfs;

				//check collision
				//if (!CheckCollisionsWithinGroup(GetAllOpenConnectorsOnBoard()))
				if (!CheckCollisionsWithinGroup(allOpenConnectorsOnBoard))
				{

					if (engine.mapDebug)
					{
						//Blink the tile group in its final position
						for (int i = 0; i < 1; i++)
						{
							Visible(false);
							yield return wfs;
							Visible(true);
							yield return wfs;
						}
					}

					//Debug.Log("Succeeded in connecting.");
					foundAttachment = true;
					//break;

					//Check for density - how many anchors the new tile attaches to
					//int attachmentCount = CountAttachmentsWithinGroup(GetAllOpenAnchorsOnBoard());
					int attachmentCount = CountAttachmentsWithinGroup(allOpenAnchorsOnBoard);
					densityMap[System.Tuple.Create(attachToAnchorTransform, connectorTransform)] = attachmentCount;
				}
			}

			if (engine.mapDebug && connectorSphere != null)
			{
				connectorSphere.SetActive(false);
				Object.Destroy(connectorSphere);
			}

		}

		if(foundAttachment)
		{
			//Debug.Log("DensityPreference: " + densityPreference);
			//Separate densityMap into 3 tranches
			Dictionary<int, int> densityValueMap = new Dictionary<int, int>();
			densityMap.Values.ToList().ForEach(v => densityValueMap[v] = v);
			List<int> densityValueList = densityValueMap.Keys.ToList();
			densityValueList.Sort();
			//Debug.Log("densityValueList: " + string.Join(", ", densityValueList));

			int minStartIndex = 0;
			int medStartIndex = densityValueList.Count / 3;
			int maxStartIndex = densityValueList.Count / 3 * 2;
			if (maxStartIndex >= densityValueList.Count) { maxStartIndex = densityValueList.Count - 1; }

			//Pick a set of attachment values in the prefered tranche based on the densityPreference
			List<int> densityList = new List<int>();
			switch (densityPreference)
			{
				case DensityPreference.FIRST: //FIRST shouldn't make it in here, but if it does, treat it as LOWEST
				case DensityPreference.LOWEST: densityList.Add(densityValueList[0]); break;
				case DensityPreference.LOW: densityList.AddRange(densityValueList.GetRange(minStartIndex, System.Math.Max(1, medStartIndex - minStartIndex))); break;
				case DensityPreference.LOW_MEDIUM: densityList.AddRange(densityValueList.GetRange(minStartIndex, System.Math.Max(1, maxStartIndex - medStartIndex))); break;
				case DensityPreference.MEDIAN: densityList.Add(densityValueList[densityValueList.Count / 2]); break;
				case DensityPreference.MEDIUM: densityList.AddRange(densityValueList.GetRange(medStartIndex, System.Math.Max(1, maxStartIndex - medStartIndex))); break;
				case DensityPreference.MEDIUM_HIGH: densityList.AddRange(densityValueList.GetRange(medStartIndex, System.Math.Max(1, densityValueList.Count - medStartIndex))); break;
				case DensityPreference.HIGH: densityList.AddRange(densityValueList.GetRange(maxStartIndex, System.Math.Max(1, densityValueList.Count - maxStartIndex))); break;
				case DensityPreference.HIGHEST: densityList.Add(densityValueList[System.Math.Max(0, densityValueList.Count - 1)]); break;
			}
			//Debug.Log("densityList: " + string.Join(", ", densityList));

			//Pick a random position from the densityMap that maches one of the chosen attachment values
			System.Tuple<Transform, Transform>[] availablePositions = densityMap.Where(x => densityList.Contains(x.Value)).Select(x => x.Key).ToArray();
			var randomizedArray = GlowEngine.RandomizeArray(availablePositions);
			Transform chosenAnchorTransform = randomizedArray[0].Item1;
			Transform chosenConnectorTransform = randomizedArray[0].Item2;
			//Debug.Log("density anchorPosition: " + chosenAnchorTransform.position);
			//Debug.Log("density connectorPosition: " + chosenConnectorTransform.position);


			//Reset tilegroup to original position
			containerObject.position = or;
			//Debug.Log("Reset tile original positions...");
			for (int i = 0; i < tileList.Count; i++)
			{
				tileList[i].transform.position = orTiles[i];
			}
			//Move the tilegroup's container to the chosen position
			foreach (Tile tile in tileList)
			{
				tile.transform.parent.transform.parent = dummy.transform;
			}
			//move containerObject to each connector in THIS group (connectors are INside the bounds of the tile)
			containerObject.position = chosenConnectorTransform.position;
			//parent TILES back to containerObject
			foreach (Tile tile in tileList)
			{
				tile.transform.parent.transform.parent = containerObject.transform;
			}
			containerObject.position = chosenAnchorTransform.position;
		}

		if(!foundAttachment)
		{
			//Debug.Log( "RESETTING" );
			//reset tilegroup to original position
			containerObject.position = or;
			//Debug.Log("Reset tile original positions...");
			for (int i = 0; i < tileList.Count; i++)
			{
				tileList[i].transform.position = orTiles[i];
			}
			//Debug.Log("Reset.");
		}

		Object.Destroy(dummy);
		if(!foundAttachment)
		{
			Debug.Log("AttachTo*********NOT FOUND");
			attachToCoroutineResult = false;
			yield break;
		}

		//Debug.Log("GenerateGroupCenter...");
		GenerateGroupCenter();
		//Debug.Log("Generated: " + groupCenter);
		attachToCoroutineResult = true;
	}


	/// <summary>
	/// reveal anchor/connector/special placeholder token for debug purposes
	/// </summary>
	public GameObject RevealAnchorConnectorToken(Transform t, string tokenName)
	{
		GameObject token = null;
		Engine engine = Engine.FindEngine();
		if (tokenName == "anchor")
		{
			token = Object.Instantiate(engine.anchorSphere, new Vector3(t.position.x, t.position.y, t.position.z), t.rotation);
		}
		else if (tokenName == "connector")
		{
			token = Object.Instantiate(engine.connectorSphere, new Vector3(t.position.x, t.position.y, t.position.z), t.rotation);
		}
		else if (tokenName == "special")
		{
			token = Object.Instantiate(engine.specialSphere, new Vector3(t.position.x, t.position.y, t.position.z), t.rotation);
		}
		if (token != null)
		{
			token.transform.parent = t;
			token.transform.position.Y(0.5f);
			token.SetActive(true);
			//var posVisAVisGP = token.transform.parent.parent.InverseTransformPoint(token.transform.position);
		}
		return token;
	}



	/// <summary>
	/// check collisions between THIS group's CONNECTORS and input test points (CONNECTORS)
	/// (connectors are INside the bounds of the tile)
	/// </summary>
	/// <returns>true if collision found</returns>
	public bool CheckCollisionsWithinGroup( Transform[] testPoints )
	{
		//List<Vector3> allConnectorsSet = new List<Vector3>();
		//List<Vector3> testVectors = new List<Vector3>();

		//create list of ALL connectors in ALL tiles in the group (connectors are INside the bounds of the tile)
		var allConnectorsSet = from tile in tileList from tf in tile.GetChildren( "connector" ) select tf.position;

		//create list of all test point connectors
		var testVectors = from tf in testPoints select tf.position;

		//create list of ALL connectors in ALL tiles in the group (connectors are INside the bounds of the tile)
		//foreach ( Tile tile in tileList )
		//{
		//foreach ( Transform t in tile.GetChildren( "connector" ) )
		//	allConnectorsSet.Add( t.position );

		//create list of all test point connectors
		//foreach ( Transform t in testPoints )
		//	testVectors.Add( t.position );

		bool collisionFound = false;

		//failure means that position is taken by a tile = COLLISION
		foreach ( Vector3 tp in testVectors )
		{
			foreach ( Vector3 a in allConnectorsSet )
			{
				float d = Vector3.Distance( a, tp );
				if (d <= .5)
				{
					collisionFound = true;
					break;
				}
			}
		}

		return collisionFound;
	}

	/// <summary>
	/// Count attachments between THIS group's CONNECTORS and input test points (ANCHORS)
	/// (connectors are INside the bounds of the tile, anchors are OUTside)
	/// </summary>
	/// <returns>true if collision found</returns>
	public int CountAttachmentsWithinGroup(Transform[] anchorPoints)
	{
		//create list of ALL connectors in ALL tiles in the group (connectors are INside the bounds of the tile)
		var allConnectorsSet = from tile in tileList from tf in tile.GetChildren("connector") select tf.position;

		//create list of all test point connectors
		var anchorVectors = from tf in anchorPoints select tf.position;

		int attachmentCount = 0;

		foreach (Vector3 tp in anchorVectors)
		{
			foreach (Vector3 a in allConnectorsSet)
			{
				float d = Vector3.Distance(a, tp);
				if (d <= .5)
				{
					attachmentCount++;
				}
			}
		}

		return attachmentCount;
	}

	/// <summary>
	/// Given a set of transforms, find the minimum x and minimum z, and the maximum x and maximum z values.
	/// Ignore the y value since we basically just care about 2D space
	/// </summary>
	/// <param name="transforms"></param>
	/// <returns></returns>
	public System.Tuple<Vector3, Vector3> MinMax(List<Vector3> vectors)
    {
		Vector3 min = new Vector3(float.MaxValue, 0, float.MaxValue);
		Vector3 max = new Vector3(float.MinValue, 0, float.MinValue);

		foreach(Vector3 vector in vectors)
        {
			if(vector.x < min.x) { min.x = vector.x; }
			if(vector.z < min.z) { min.z = vector.z; }

			if(vector.x > max.x) { max.x = vector.x; }
			if(vector.z > max.z) { max.z = vector.z; }
        }

		return new System.Tuple<Vector3, Vector3>(min, max);
    }

	/// <summary>
	/// Returns ALL connectors in ALL tiles on board across ALL tilegroups (except THIS one) that have isPlaced == true
	/// (connectors are INside the bounds of the tile)
	/// </summary>
	public Transform[] GetAllOpenConnectorsOnBoard()
	{
		//(connectors are INside the bounds of the tile)
		//get all connectors EXCEPT the ones in THIS tilegroup since we'll be testing THIS group's connectors against all OTHERS
		//otherwise it'll test against ITSELF
		var allConnectors = from tg in tileManager.GetAllTileGroups()
												where tg.GUID != GUID && tg.isPlaced
												from tile in tg.tileList
												from tf in tile.GetChildren( "connector" )
												select tf;
		return allConnectors.ToArray();
	}

	/// <summary>
	/// Returns ALL anchors in ALL tiles on board across ALL tilegroups (except THIS one) that have isPlaced == true
	/// (anchors are OUTside the bounds of the tile)
	/// </summary>
	public Transform[] GetAllOpenAnchorsOnBoard()
	{
		//(connectors are INside the bounds of the tile)
		//get all anchors EXCEPT the ones in THIS tilegroup since we'll be testing THIS group's anchors against all OTHERS
		//otherwise it'll test against ITSELF
		var allAnchors = from tg in tileManager.GetAllTileGroups()
							where tg.GUID != GUID && tg.isPlaced
							from tile in tg.tileList
							from tf in tile.GetChildren("anchor")
							select tf;
		return allAnchors.ToArray();
	}


	/// <summary>
	/// returns all connector positions in the tilegroup
	/// (connectors are INside the bounds of the tile)
	/// </summary>
	public Vector3[] GetOpenConnectors()
	{
		var bar = from tile in tileList from c in tile.GetChildren("connector") select c.name;
		//Debug.Log("openConnectors: " + string.Join(", ", bar.ToArray()));
		var foo = from tile in tileList from c in tile.GetChildren( "connector" ) select c.position;
		return foo.ToArray();
	}

	public Transform[] GetOpenConnectorsTransforms(bool allowExcluded=true)
	{
		var baz = from tile in tileList select tile.name;
		//Debug.Log("openConnectors tiles: " + string.Join(", ", baz.ToArray()));
		var bar = from tile in tileList from c in tile.GetChildren("connector") where c.name.Contains("exclude") == allowExcluded select c.name;
		//Debug.Log("openConnectors: " + string.Join(", ", bar.ToArray()));
		var foo = from tile in tileList from c in tile.GetChildren("connector") select c;
		return foo.ToArray();
	}

	//public Transform[] GetOpenAnchorsTransforms()
	//{
	//	List<Transform> allAnchorsSet = new List<Transform>();
	//	List<Transform> allConnectorsSet = new List<Transform>();
	//	List<Transform> safeAnchors = new List<Transform>();

	//	foreach ( Tile tile in tileList )
	//	{
	//		allAnchorsSet.AddRange( tile.GetChildren( "anchor" ) );
	//		allConnectorsSet.AddRange( tile.GetChildren( "connector" ) );
	//	}

	//	foreach ( Transform a in allAnchorsSet )
	//	{
	//		bool hit = false;
	//		foreach ( Transform c in allConnectorsSet )
	//		{
	//			float d = Vector3.Distance( c.position, a.position );
	//			if ( d <= .5f )
	//				hit = true;
	//		}
	//		if ( !hit && !safeAnchors.Contains( a ) )
	//			safeAnchors.Add( a );
	//	}

	//	return allAnchorsSet.ToArray();//safeAnchors.ToArray();
	//}
	/// <summary>
	/// returns all anchor positions (rounded up) in the group that are open to attach to
	/// </summary>
	///

	/// <summary>
	/// returns all anchor positions in the tilegroup
	/// (anchors are OUTside the bounds of the tile)
	/// </summary>
	public Vector3[] GetOpenAnchors()
	{
		var temp = from tile in tileList from c in tile.GetChildren("anchor") select c.name;
		Debug.Log("openAnchors: " + string.Join(", ", temp.ToArray()));
		var allAnchors = from tile in tileList from tf in tile.GetChildren( "anchor" ) select tf.position;
		return allAnchors.ToArray();
	}

	public Vector3[] GetOpenAnchorsFromTile(int tileToAttachTo)
	{
		var temp = from tile in tileList where tile.baseTile.idNumber == tileToAttachTo from tf in tile.GetChildren("anchor") select tf.name;
		Debug.Log("openAnchorsFromTile: " + tileToAttachTo + " => " + string.Join(", ", temp.ToArray()));
		var allAnchors = from tile in tileList where tile.baseTile.idNumber == tileToAttachTo from tf in tile.GetChildren("anchor") select tf.position;
		return allAnchors.ToArray();
	}

	public Vector3[] GetOpenAnchorsExceptTile(int tileToAttachTo)
	{
		var temp = from tile in tileList where tile.baseTile.idNumber != tileToAttachTo from tf in tile.GetChildren("anchor") select tf.name;
		Debug.Log("openAnchorsExceptTile: " + tileToAttachTo + " => " + string.Join(", ", temp.ToArray()));
		var allAnchors = from tile in tileList where tile.baseTile.idNumber != tileToAttachTo from tf in tile.GetChildren("anchor") select tf.position;
		return allAnchors.ToArray();
	}

	public Transform[] GetOpenAnchorsTransforms()
	{
		var bar = from tile in tileList from c in tile.GetChildren("anchor") select c.name;
		//Debug.Log("openConnectors: " + string.Join(", ", bar.ToArray()));
		var allAnchors = from tile in tileList from tf in tile.GetChildren("anchor") select tf;
		return allAnchors.ToArray();
	}

	public Transform[] GetOpenAnchorsFromTileTransforms(int tileToAttachTo)
	{
		var temp = from tile in tileList where tile.baseTile.idNumber == tileToAttachTo from tf in tile.GetChildren("anchor") select tf.name;
		Debug.Log("openAnchorsFromTile: " + tileToAttachTo + " => " + string.Join(", ", temp.ToArray()));
		var allAnchors = from tile in tileList where tile.baseTile.idNumber == tileToAttachTo from tf in tile.GetChildren("anchor") select tf;
		return allAnchors.ToArray();
	}

	public Transform[] GetOpenAnchorsExceptTileTransforms(int tileToAttachTo)
	{
		var temp = from tile in tileList where tile.baseTile.idNumber != tileToAttachTo from tf in tile.GetChildren("anchor") select tf.name;
		Debug.Log("openAnchorsExceptTile: " + tileToAttachTo + " => " + string.Join(", ", temp.ToArray()));
		var allAnchors = from tile in tileList where tile.baseTile.idNumber != tileToAttachTo from tf in tile.GetChildren("anchor") select tf;
		return allAnchors.ToArray();
	}

	public void RemoveGroup()
	{
		Object.Destroy( containerObject.gameObject );
	}

	/// <summary>
	/// drops in the Exploration token ONLY, skips player start tile
	/// </summary>
	public void RevealExploreToken()
	{
		foreach ( Tile t in tileList )
			if ( !t.baseTile.isStartTile )
				t.RevealExplorationToken();
	}

	/// <summary>
	/// only if FIRST tilegroup
	/// </summary>
	public void RevealInteractiveTokens( bool startTileOnly = false )
	{
		//Debug.Log( "RevealInteractiveTokens" );
		foreach (Tile t in tileList)
			if (!startTileOnly || (startTileOnly && t.baseTile.isStartTile))
			{
				t.RevealStartTokenAndThenInteractiveTokens();
			}
	}

	/// <summary>
	/// colorize whole group (START chapter ONLY), fire chapter exploreTrigger
	/// </summary>
	public void Colorize( bool onlyStart = false )
	{
		//Debug.Log( "EXPLORING GROUP isExplored?::" + isExplored );

		if ( isExplored )
			return;

		//isExplored = true;

		//if it's not the first chapter, set the "on explore" trigger
		//if ( chapter.dataName != "Start" )
		GlowEngine.FindObjectOfType<TriggerManager>().FireTrigger( chapter.exploreTrigger );

		foreach ( Tile t in tileList )
		{
			if ( onlyStart && t.baseTile.isStartTile )
				t.Colorize();
			else if ( !onlyStart )
				t.Colorize();
		}
	}

	public void Sepia(bool sepia)
    {
		foreach (Tile t in tileList)
		{
			t.meshRenderer.material.SetFloat("_sepiaValue", sepia ? 1 : 0);
		}
	}

	public void Visible(bool visible)
    {
		foreach (Tile t in tileList)
		{
			t.gameObject.SetActive(visible);
		}
	}

	public void ActivateTiles()
	{
		foreach (Tile tile in tileList)
		{
			tile.gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// returns all TokenStates[] that given token is in
	/// </summary>
	public TokenState[] GetTokenListByGUID( System.Guid guid )
	{
		return ( from tile in tileList
						 from tstate in tile.tokenStates
						 where tstate.metaData.GUID == guid
						 select tstate ).ToArray();
	}

	public void SetState( TileGroupState tileGroupState )
	{
		containerObject.position = tileGroupState.globalPosition;
		isExplored = tileGroupState.isExplored;

		foreach ( Tile tile in tileList )
		{
			SingleTileState sts = ( from t in tileGroupState.tileStates
															where t.tileGUID == tile.baseTile.GUID
															select t ).FirstOr( null );
			if ( sts != null )
				tile.SetState( sts );
		}

		GenerateGroupCenter();
	}

	override public string ToString()
    {
		return "[" + string.Join(", ", tileList) + "]";
    }
}
