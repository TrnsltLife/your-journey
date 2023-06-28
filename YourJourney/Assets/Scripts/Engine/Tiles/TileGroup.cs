using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class TileGroup
{
	public TileManager tileManager;
	public Vector3 groupCenter;
	//0th Tile is the ROOT
	public List<Tile> tileList;
	public bool isExplored { get; set; }
	public Vector3 startPosition { get; set; }

	//each Tile is a child of containerObject
	public Transform containerObject;
	//List<Vector3> childOffsets;//normalized vectors pointint to child
	Chapter chapter { get; set; }
	//int[] randomTileIndices;//randomly index into chapter tileObserver
	[HideInInspector]
	public System.Guid GUID { get; set; }//same as chapter GUID

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
		//Debug.Log( "BuildRandomFromChapter" );
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
//tile.gameObject.SetActive(true);
//tile.RevealAllAnchorConnectorTokens();
			//rotate go object
			tile.transform.parent.localRotation = Quaternion.Euler( 0, tileroot.angle, 0 );
			//set go's parent
			tile.transform.parent.transform.parent = containerObject;
			containerObject.name += " " + tileroot.idNumber.ToString();
			if ( previous != null )
			{
				tile.AttachTo( previous, this );
			}
			tileList.Add( tile );
			previous = tile;

			//add fixed tokens
			if ( tile.baseTile.tokenList.Count > 0 )
				usedPositions.AddRange( AddFixedToken( tile ) );

			if (tileroot.isStartTile )
			{
				startPosition = tile.GetChildren( "token attach" )[0].position.Y( SpawnMarker.SPAWN_HEIGHT );
				tile.isExplored = true;
			}
		}

		//add random tokens
		if ( c.usesRandomGroups )
			AddRandomTokens( usedPositions.ToArray() );

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

		for ( int i = 0; i < c.tileObserver.Count; i++ )
		{
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
			//add a token, if there is one
			//if ( !c.usesRandomGroups )
			if ( tile.baseTile.tokenList.Count > 0 )
				usedPositions.AddRange( AddFixedToken( tile ) );

			//find starting position if applicable and add the player spawn marker
			if ( bt.isStartTile )
			{
				tile.isExplored = true;
				startPosition = tile.GetChildren( "token attach" )[0].position.Y( SpawnMarker.SPAWN_HEIGHT );
			}
		}

		//add random tokens
		//if ( c.usesRandomGroups && usedPositions.Count > 0 ) // <-- This code was blocking random events from occurring *unless* there was at least one fixed token
		if ( c.usesRandomGroups )
			AddRandomTokens( usedPositions.ToArray() );

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

	void AddRandomTokens( Transform[] usedPositions )
	{
		if ( chapter.randomInteractionGroup == "None" )
			return;
		//Debug.Log("Add events to " + (chapter.isRandomTiles ? "random" : "fixed") + " tile group.");
		//usedPositions = wonky user placed token position
		InteractionManager im = GlowEngine.FindObjectOfType<InteractionManager>();
		//get array of interactions that are in the interaction group; don't include interactions that have already been placed (unless isReusable is set to indicate they can be used more than once)
		IInteraction[] interactionGroupArray = im.randomTokenInteractions
			.Where( x => x.dataName.EndsWith( chapter.randomInteractionGroup ) && (!x.isPlaced || x.isReusable)).ToArray();
		interactionGroupArray = GlowEngine.ShuffleArray(interactionGroupArray); //randomize the order so every time we play it's different
		//Debug.Log( "EVENTS IN GROUP [" + chapter.randomInteractionGroup + "]: " + interactionGroupArray.Length );

		//get all the possible token spawn locations that are NOT near FIXED tokens already placed
		List<Transform> attachtfs = new List<Transform>();
		List<Transform> finalOpenTFS = new List<Transform>();
		foreach ( Tile t in tileList )
		{
			attachtfs.Clear();
			attachtfs.AddRange( t.GetChildren( "token attach" ) );
			var usedInThisTile = from tu in usedPositions
													 where tu.GetComponent<MetaData>().tileID/*tile.hexTile.idNumber*/ == t.baseTile.idNumber
													 select tu;

			var opentfs = new List<Transform>();
			foreach ( Transform tf in attachtfs )
			{
				float minD = 1000;
				foreach ( Transform utf in usedInThisTile )
				{
					float d = Vector3.Distance( utf.position, tf.position );
					if ( d < minD )
						minD = d;
				}
				if ( minD > 1.1f )
					opentfs.Add( tf );
			}

			finalOpenTFS.AddRange( opentfs );
		}

		//recreate opentfs as hash with UNIQUE items, no dupes
		var openhash = new HashSet<Transform>( finalOpenTFS );
		finalOpenTFS = GlowEngine.ShuffleArray(openhash.Select( x => x ).ToArray()).ToList(); //grab the array and then shuffle it so it's different every time
		//Debug.Log( "REQUESTED EVENTS: " + chapter.randomInteractionGroupCount );
		//Debug.Log( "USED POSITIONS: " + usedPositions.Length );
		//Debug.Log( "FOUND POSSIBLE POSITIONS: " + attachtfs.Count );
		//Debug.Log( "FOUND OPEN POSITIONS: " + finalOpenTFS.Count() );

		//sanity check, max number of events based on how many requested and how many actually found in group how many actual open positions
		int max = Mathf.Min( interactionGroupArray.Length, Mathf.Min( chapter.randomInteractionGroupCount, finalOpenTFS.Count() ) );
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
		int[] rands = GlowEngine.GenerateRandomNumbers( max/*opentfs.Count()*/ );
		for ( int i = 0; i < max/*igs.Length*/; i++ )
		{
			//get tile this transform position belongs to
			Tile tile = finalOpenTFS[rands[i]].parent.GetComponent<Tile>();
			//Debug.Log( "TILE #:" + tile.hexTile.idNumber );
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

			go.transform.position = new Vector3( finalOpenTFS[rands[i]].position.x, go.transform.position.y, finalOpenTFS[rands[i]].position.z );
			go.GetComponent<MetaData>().tokenType = HandlePersistentTokenSwap( igs[i].dataName );//igs[i].tokenType;
			go.GetComponent<MetaData>().personType = igs[i].personType;
			go.GetComponent<MetaData>().terrainType = igs[i].terrainType;
			go.GetComponent<MetaData>().triggeredByName = "None";
			go.GetComponent<MetaData>().triggerName = "None";
			go.GetComponent<MetaData>().tokenInteractionText = igs[i].tokenInteractionText;
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
			go.GetComponent<MetaData>().tokenInteractionText = Engine.currentScenario.interactionObserver.Find(interact => t.triggerName == interact.dataName)?.tokenInteractionText;


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

			usedPositions.Add( go.transform );

			//Rotate terrain tokens for square map
			Vector3 rotateCenter = Vector3.zero;
			if (goMetaData.tokenType == TokenType.Terrain)
			{
				var tokenSizeX = goMetaData.size.x;
				var tokenSizeZ = goMetaData.size.z;

				rotateCenter = tokenPos + new Vector3(tokenSizeX / 2, 0, -tokenSizeZ / 2);
				go.transform.RotateAround(rotateCenter, Vector3.up, (float)t.angle);
				//Debug.Log("rotate token with size [" + tokenSizeX + ", " + tokenSizeZ + "] " + t.angle + " degrees around " + rotateCenter + " vs tokenPos " + tokenPos);
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
		//get all open connectors in THIS tilegroup
		Vector3[] openConnectors = GlowEngine.RandomizeArray( GetOpenConnectors() );
		//get all open anchors on group we're connecting TO
		Vector3[] tgOpenConnectors = GlowEngine.RandomizeArray( tgToAttachTo.GetOpenAnchors() );
		//dummy
		GameObject dummy = new GameObject();
		Vector3[] orTiles = new Vector3[tileList.Count];

		//record original CONTAINER position
		Vector3 or = containerObject.position;
		//record original TILE positions
		for ( int i = 0; i < tileList.Count; i++ )
			orTiles[i] = tileList[i].transform.position;

		bool safe = false;
		foreach ( Vector3 c in openConnectors )
		{
			safe = false;
			//parent each TILE to dummy
			foreach ( Tile tile in tileList )
				tile.transform.parent.transform.parent = dummy.transform;
			//move containerObject to each connector in THIS group
			containerObject.position = c;
			//parent TILES back to containerObject
			foreach ( Tile tile in tileList )
				tile.transform.parent.transform.parent = containerObject.transform;

			//move containerObject to each anchor trying to connect to
			foreach ( Vector3 a in tgOpenConnectors )
			{
				containerObject.position = a;
				//rotate 360 for different orientations?

				//check collision
				if ( !CheckCollisionsWithinGroup( GetAllOpenConnectorsOnBoard() ) )
				{
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
				for ( int i = 0; i < tileList.Count; i++ )
					tileList[i].transform.position = orTiles[i];
			}
		}

		Object.Destroy( dummy );
		if ( !safe )
		{
			Debug.Log( "AttachTo*********NOT FOUND" );
			return false;
		}

		GenerateGroupCenter();
		return true;
	}

	/// <summary>
	/// check collisions between THIS group's CONNECTORS and input test points (CONNECTORS)
	/// </summary>
	/// <returns>true if collision found</returns>
	public bool CheckCollisionsWithinGroup( Transform[] testPoints )
	{
		//List<Vector3> allConnectorsSet = new List<Vector3>();
		//List<Vector3> testVectors = new List<Vector3>();

		//create list of ALL connectors in ALL tiles in the group
		var allConnectorsSet = from tile in tileList from tf in tile.GetChildren( "connector" ) select tf.position;

		//create list of all test point connectors
		var testVectors = from tf in testPoints select tf.position;

		//create list of ALL connectors in ALL tiles in the group
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
				if ( d <= .5 )
					collisionFound = true;
			}
		}

		return collisionFound;
	}

	/// <summary>
	/// Returns ALL connectors in ALL tiles on board across ALL tilegroups (except THIS one)
	/// </summary>
	public Transform[] GetAllOpenConnectorsOnBoard()
	{
		//get all connectors EXCEPT the ones in THIS tilegroup since we'll be testing THIS group's connectors against all OTHERS
		//otherwise it'll test against ITSELF
		var allConnectors = from tg in tileManager.GetAllTileGroups()
												where tg.GUID != GUID
												from tile in tg.tileList
												from tf in tile.GetChildren( "connector" )
												select tf;
		return allConnectors.ToArray();
	}

	/// <summary>
	/// returns all connector positions in the tilegroup
	/// </summary>
	public Vector3[] GetOpenConnectors()
	{
		var bar = from tile in tileList from c in tile.GetChildren("connector") select c.name;
		//Debug.Log("openConnectors: " + string.Join(", ", bar.ToArray()));
		var foo = from tile in tileList from c in tile.GetChildren( "connector" ) select c.position;
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
	public Vector3[] GetOpenAnchors()
	{
		var bar = from tile in tileList from c in tile.GetChildren("anchor") select c.name;
		//Debug.Log("openConnectors: " + string.Join(", ", bar.ToArray()));
		var allAnchors = from tile in tileList from tf in tile.GetChildren( "anchor" ) select tf.position;
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
		foreach ( Tile t in tileList )
			if ( startTileOnly && t.baseTile.isStartTile )
				t.RevealInteractiveTokens();
			else if ( !startTileOnly )
				t.RevealInteractiveTokens();
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
}
