using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using static LanguageManager;

public class Tile : MonoBehaviour
{
	public Renderer meshRenderer;
	public Vector3 CurrentAnchor { get { return currentAnchor; } }
	[HideInInspector]
	public Vector3 centerPosition { get { return tilemesh.GetComponent<Renderer>().bounds.center; } }
	[HideInInspector]
	public int currentConnectorID;
	[HideInInspector]
	public BaseTile baseTile;
	[HideInInspector]
	public bool isExplored { get; set; } = false;
	//rootPosition is the position (connector) where all xforms take place from. Only used for building a loaded scenario (fixed, not random)
	public Transform rootPosition;//world coords
	public TileGroup tileGroup { get; set; }
	public Chapter chapter;

	//offset from connector to center of mesh
	[HideInInspector]
	public GameObject tilemesh;
	Vector3[] connectorOffset;
	Vector3 currentAnchor;
	int anchorCount, connectorCount;
	Transform exploreToken;
	InteractionManager interactionManager;
	float sepiaValue = 1;
	//queue of tokens to fire because tile wasn't explored yet
	[HideInInspector]
	public List<string> tokenTriggerList = new List<string>();
	[HideInInspector]
	public List<TokenState> tokenStates = new List<TokenState>();

	TriggerManager triggerManager;
	CamControl camControl;

	public GameObject anchorSphere;
	public GameObject connectorSphere;
	public GameObject specialSphere;

	public void Awake()
	{
		interactionManager = FindObjectOfType<InteractionManager>();

		tilemesh = GetChildren( "tile" )[0].gameObject;
		meshRenderer = tilemesh.GetComponent<Renderer>();
		anchorCount = GetCount( "anchor" );
		connectorCount = GetCount( "connector" );
		connectorOffset = new Vector3[connectorCount];

		int c = 0;
		for ( int i = 0; i < transform.childCount; i++ )
		{
			Transform child = transform.GetChild( i );

			if ( child.name.Contains( "connector" ) )
			{
				//calculate LOCAL offsets to each connector
				//connectorOffset[c++] = child.localPosition - Vector3.zero;
				connectorOffset[c++] = child.position - transform.position;
			}
		}

		Transform findResult = transform.Find("Exploration Token");
		if (findResult != null)
		{
			exploreToken = transform.Find("Exploration Token").transform;
			exploreToken.localPosition = new Vector3(exploreToken.localPosition.x, 2, exploreToken.localPosition.z);
			exploreToken.gameObject.SetActive(false);
		}

		meshRenderer.material.SetFloat( "_sepiaValue", 1 );

		triggerManager = FindObjectOfType<TriggerManager>();
		camControl = FindObjectOfType<CamControl>();
		//StartCoroutine( Wait1Frame() );
	}

	public void EnqueueTokenTrigger( string name )
	{
		Debug.Log( "Tile EnqueueTokenTrigger: " + name );
		if ( !tokenTriggerList.Contains( name ) )
			tokenTriggerList.Add( name );
	}

	//public Vector3 GetExploretokenPosition()
	//{
	//	Transform tf = transform.Find( "Exploration Token" );
	//	return new Vector3( tf.position.x, .26f, tf.position.z );
	//}

	//IEnumerator Wait1Frame()
	//{
	//	yield return null;
	//}

	public int GetCount( string name )
	{
		int c = 0;
		for ( int i = 0; i < transform.childCount; i++ )
			if ( transform.GetChild( i ).name.Contains( name ) )
				c++;
		//Debug.Log( "GetCount::" + name + "::" + c );
		return c;
	}

	/// <summary>
	/// gets all children whose name CONTAINS given string
	/// </summary>
	public Transform[] GetChildren( string name )
	{
		Transform[] t = new Transform[GetCount( name )];
		int c = 0;
		foreach ( Transform child in transform )
		{
			if ( child.name.Contains( name ) )
				t[c++] = child;
		}

		return t;
	}

	public bool IsDarknessTokenActive()
	{
		Transform[] dark = GetChildren( "Darkness Token" );

		foreach ( Transform child in dark )
		{
			if ( child.gameObject.activeInHierarchy )
				return true;
		}
		return false;
	}

	//public bool CheckCollision2()
	//{
	//	bool foundHit = false;
	//	Transform[] connectors = GetChildren( "connector" );

	//	for ( int i = 0; i < connectors.Length; i++ )
	//	{
	//		Vector3 connector = connectors[i].position + new Vector3( 0, 1, 0 );
	//		var hits = Physics.RaycastAll( connector, Vector3.down, 5 );
	//		foreach ( var hit in hits )
	//		{
	//			if ( hit.collider.name != "hit Plane"
	//				&& hit.collider.transform.parent.name != name )
	//			{
	//				//Debug.Log( "connector " + connectors[i].name + " HIT WITH::" + hit.collider?.gameObject.name );
	//				foundHit = true;
	//			}
	//		}
	//	}
	//	if ( foundHit )
	//		Debug.Log( name + " DETECTED COLLISION" );
	//	else
	//		Debug.Log( name + " DETECTED NO COLLISION" );

	//	return foundHit;
	//}

	/// <summary>
	/// returns TRUE if COLLISION
	/// </summary>
	public bool CheckCollision()
	{
		//new Vector3( -1.5f, 0, -0.4330127f )
		//int mask1 = 1 << 2;
		//int mask2 = 1 << 9;
		int mask = 1 << 10;
		//layerMask = ~layerMask;//inverse, NOT
		//~( mask1 | mask2 )

		tilemesh.layer = 2;

		bool foundHit = false;
		RaycastHit hit;

		Transform[] connectors = GetChildren( "connector" );

		for ( int i = 0; i < connectors.Length; i++ )
		{
			Vector3 connector = connectors[i].position + new Vector3( 0, 1, 0 );
			//Debug.Log( "CONNECTOR WORLD::" + connectors[i].name + "::" + connector );
			if ( Physics.Raycast( connector, Vector3.down, out hit, 3, mask ) )
			{
				foundHit = true;
				//Debug.Log( "connector " + connectors[i].name + " HIT WITH::" + hit.collider?.gameObject.name );
				//Debug.Log( "COLLIDED WITH::" + hit.collider?.gameObject.transform.position );
				break;
			}
		}

		if ( foundHit )
			Debug.Log( name + " DETECTED COLLISION" );
		else
			Debug.Log( name + " DETECTED NO COLLISION" );

		tilemesh.layer = 10;

		return foundHit;
	}

	/// <summary>
	/// Tile MOVES when setting Connector
	/// </summary>
	public void SetConnector( int idx )
	{
		//Debug.Log( "SetConnector::" + gameObject.name );
		//Debug.Log( "SetConnector()::idx=" + idx );
		//Debug.Log( "SetConnector()::connectorCount=" + connectorCount );
		if ( idx >= connectorCount )
		{
			Debug.Log( "SetConnector()::idx >= SetConnector" );
			return;
		}
		currentConnectorID = idx;
		transform.localPosition = Vector3.zero;
		transform.localPosition -= connectorOffset[idx];
	}

	//public void SetRandomConnector()
	//{
	//	int c = Random.Range( 0, connectorCount );
	//	currentConnectorID = c + 1;//1-based
	//	transform.localPosition = Vector3.zero;
	//	transform.localPosition -= connectorOffset[c];

	//	CheckCollision();
	//}

	public void SetAnchor( int idx )
	{
		//Debug.Log( "SetAnchor::" + gameObject.name );
		//Debug.Log( "SetAnchor()::idx=" + idx );
		//Debug.Log( "SetAnchor()::anchorCount=" + anchorCount );
		if ( idx >= anchorCount )
		{
			Debug.Log( "SetAnchor()::idx >= anchorCount" );
			return;
		}
		Transform[] anchors = GetChildren( "anchor" );
		currentAnchor = anchors[idx].position;
		//Debug.Log( "currentAnchor:" + anchors[idx].name );
		//Debug.Log( "currentAnchor=" + currentAnchor );
	}

	//public void SetRandomAnchor()
	//{
	//	Transform[] anchors = GetChildren( "anchor" );
	//	currentAnchor = anchors[Random.Range( 0, anchorCount )].position;
	//}

	//WORLD coords
	public void AttachTo( Vector3 anchor )
	{
		transform.parent.transform.position = anchor;
		transform.parent.transform.Translate( Vector3.zero );
	}

	/// <summary>
	/// Randomly attaches 2 tiles (random anchor/connector) within a given group, tile=previous tile already on board
	/// </summary>
	public void AttachTo( Tile tile, TileGroup tg )
	{
		//anchors = white outer transforms
		//connectors = red inner transforms
		Transform[] anchorPoints = tile.GetChildren( "anchor" );
		int[] ra = GlowEngine.GenerateRandomNumbers( anchorPoints.Length );
		int[] rc = GlowEngine.GenerateRandomNumbers( connectorCount );
		bool success = false;

		for ( int c = 0; c < connectorCount; c++ )
		{
			for ( int a = 0; a < anchorPoints.Length; a++ )//white anchors on board
			{
				tile.SetAnchor( ra[a] );
				SetConnector( rc[c] );
				AttachTo( tile.currentAnchor );
				Transform[] ap = GetChildren( "connector" );
				success = !tg.CheckCollisionsWithinGroup( ap );
				if ( success )
					break;
			}
			if ( success )
				break;
		}

		if ( !success )
		{
			Debug.Log( "FAILED TO FIND OPEN TILE LOCATION" );
			throw new System.Exception( "FAILED TO FIND OPEN TILE LOCATION" );
		}
	}

	public void SetPosition( Vector3 worldpos, float angle )
	{
		//rootPosition is the position where all xforms take place from
		//get vector from root hextile to center of model
		Vector3 v = transform.position - rootPosition.position;
		//move the parent into place
		transform.parent.transform.position = worldpos;
		//offset model from parent
		transform.localPosition = v;
		//rotate parent transform
		transform.parent.localRotation = Quaternion.Euler( 0, angle, 0 );
	}

	public void RevealExplorationToken()
	{
		exploreToken.gameObject.SetActive( true );
		exploreToken.localScale = Vector3.one;
		exploreToken.position = exploreToken.position.Y( 2f );
		exploreToken.DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );
	}

	public void RevealStartTokenAndThenInteractiveTokens()
	{
		if (RevealToken(TokenType.Start))
		{
			FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue(Translate("dialog.text.PlaceHeroes", "Place your Heroes in the indicated position."), ButtonIcon.Continue, () => {
				//Things inside the lambda only happen once the user clicks Continue
				RevealInteractiveTokensAndShowInitialScoutDialog();
			});
		}
		else
        {
			RevealInteractiveTokens();
        }
	}

	public void RevealInteractiveTokensAndShowInitialScoutDialog()
    {
		RevealInteractiveTokens();

		StartCoroutine(DisplayInitialScoutDialog());
	}

	public void RevealInteractiveTokens()
	{
		RevealToken(TokenType.Search);
		RevealToken(TokenType.Person);
		RevealToken(TokenType.Threat);
		RevealToken(TokenType.Darkness);
		RevealToken(TokenType.DifficultGround);
		RevealToken(TokenType.Fortified);
		RevealToken(TokenType.Terrain);
	}


	IEnumerator DisplayInitialScoutDialog()
	{
		int scoutAmount = Engine.currentScenario.initialScout;
		if (scoutAmount > 0)
		{
			bool waiting = true;
			interactionManager.GetNewTextPanel().ShowScoutX(scoutAmount, () =>
			{
				waiting = false;
			});

			while (waiting)
				yield return null;
		}
	}

	/// <summary>
	/// Sets isExplored=true and does token removal animation
	/// </summary>
	public void RemoveExplorationToken()
	{
		isExplored = true;
		tileGroup.isExplored = true;
		Sequence sequence = DOTween.Sequence();
		sequence.Append( exploreToken.DOLocalMoveY( 1, 1 ).SetEase( Ease.InOutQuad ) );
		sequence.Join( exploreToken.DOScale( 0, 1 ) );
		sequence.Play().OnComplete( () => { exploreToken.gameObject.SetActive( false ); } );
	}

	public void RemoveInteractivetoken( Transform tf, MetaData metaData )
	{
		//mark token state as inactive
		TokenState ts;
		if ( !metaData.isRandom )
			ts = tokenStates.Where( x => x.metaData.GUID == metaData.GUID ).FirstOr( null );
		else
			ts = tokenStates.Where( x => x.metaData.interactionName == metaData.interactionName ).FirstOr( null );
		if ( ts != null )
			ts.isActive = false;

		Sequence sequence = DOTween.Sequence();
		sequence.Append( tf.DOLocalMoveY( 1, 1 ).SetEase( Ease.InOutQuad ) );
		sequence.Join( tf.DOScale( 0, 1 ) );
		sequence.Play().OnComplete( () => { tf.gameObject.SetActive( false ); } );
	}

	/// <summary>
	/// Explore tile - colorize only
	/// </summary>
	public void Colorize()
	{
		isExplored = true;

		DOTween.To( () => sepiaValue, x =>
		{
			sepiaValue = x;
			meshRenderer.material.SetFloat( "_sepiaValue", sepiaValue );
		}, 0, 2f );
	}

	/// <summary>
	/// reveal/drop token of specified type onto the tile ONLY if it's not a triggered Token (TriggeredBy)
	/// </summary>
	bool RevealToken( TokenType ttype )
	{
		//var size = tilemesh.GetComponent<MeshRenderer>().bounds.size;
		var center = tilemesh.GetComponent<MeshRenderer>().bounds.center;
		Transform[] tf = GetChildren( ttype.ToString() );

		Debug.Log("RevealToken " + ttype + " x " + tf.Count());

		bool placedToken = false;

		for ( int i = 0; i < tf.Length; i++ )
		{
			TokenState tState = null;
			MetaData metaData = tf[i].GetComponent<MetaData>();

			//if (this.baseTile.tileType == TileType.Square)
			//{
			//	tf[i].position.X(tf[i].position.x * 1.25f);
			//	tf[i].position.Z(tf[i].position.z * 1.25f);
			//}

			//only want FIXED tokens
			if ( !metaData.isRandom )//&& !metaData.hasBeenReplaced )
			{
				Debug.Log("RevealToken " + ttype + " !metaData.isRandmom");
				string tBy = metaData.triggeredByName;
				//skip if it's triggeredBy
				if ( tBy != "None")
				{
					//if it's not in the list, keep it hidden because it hasn't activated yet, move to next token in loop
					if ( !tokenTriggerList.Contains( tBy ) )//( tBy != "None" )
						continue;
				}

				//offset to token in EDITOR coords
				//Vector3 offset = tf[i].GetComponent<MetaData>().offset;
				////Debug.Log( "EDITOR offset:" + offset );
				//float scalar = Mathf.Max( size.x, size.z ) / 650f;
				//offset *= scalar;
				//offset = Vector3.Reflect( offset, new Vector3( 0, 0, 1 ) );

				//tf[i].position = new Vector3( center.x + offset.x, 2, center.z + offset.z );
				Debug.Log("RevealToken " + ttype + " DOLocalMoveY .3f, 1");

				tf[i].gameObject.SetActive( true );
				tf[i].position = tf[i].position.Y( 2 );
				tf[i].RotateAround( center, Vector3.up, baseTile.angle );
				tf[i].DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );

				//update token state to active
				tState = tokenStates.Where( x => x.metaData.GUID == metaData.GUID ).FirstOr( null );
			}
			else //if ( !metaData.hasBeenReplaced )
			{
				Debug.Log("RevealToken " + ttype + " else");
				Debug.Log("RevealToken " + ttype + " DOLocalMoveY .3f, 1");
				//random tokens are already placed during tile creation using preset transforms built into the mesh "token attach"
				tf[i].gameObject.SetActive( true );
				tf[i].position = tf[i].position.Y( 2 );
				tf[i].DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );

				//update token state to active
				tState = tokenStates.Where( x => x.metaData.interactionName == metaData.interactionName ).FirstOr( null );
			}

			Debug.Log("RevealToken " + ttype + " tState " + tState);

			if ( tState != null )
			{
				tState.isActive = true;
				tState.localPosition = tf[i].localPosition.Y( .3f );
				placedToken = true;
				Debug.Log("RevealToken " + ttype + " set active and localPosition: " + tState.localPosition);
			}
		}

		return placedToken;
	}

	/*
	Generate a rectangular array based on the hexagonally placed anchor and connector markers on a tile.
	*/
	public ConnectorGrid GenerateConnectorGrid(bool alreadyPlaced = false)
    {
		Debug.Log("GenerateConnectorGrid for tile " + this.ToString());
		ConnectorGrid grid = new ConnectorGrid();
		grid.EstablishMinMaxFromTransformChildren(transform);
		grid.EstablishOffsetToTranslateToZero();
		grid.AllocateGridSize();

		for (int i=0; i<transform.childCount; i++)
		{
			Transform t = transform.GetChild(i);
			int value = 0;
			if (t.name.Contains("anchor")) //(anchors are OUTside the bounds of the tile)
			{
				//value = alreadyPlaced ? 2 : -1;
				value = -1;
			}
			else if (t.name.Contains("connector")) //(connectors are INside the bounds of the tile)
			{
				//value = alreadyPlaced ? -2 : 1;
				value = 1;
			}

			//Convert the connector's tile position to its position in the array
			GridPosition gridPos = grid.CalculateGridPosition(t.position.x, t.position.z, t);
			grid.transformPositionList.Add(gridPos);

			if (value != 0)
			{
				if (gridPos.x >= grid.gridX || gridPos.z >= grid.gridZ || gridPos.x < 0 || gridPos.z < 0)
				{
					Debug.Log("Oops. Out of bounds. pos[" + gridPos.x + "," + gridPos.z + "] vs gridSize[" + grid.gridX + "," + grid.gridZ + "]");
				}
				grid.grid[gridPos.x, gridPos.z] = value;

			}
			else
            {
				Debug.Log("Skipping " + t.name + " at pos[" + gridPos.x + "," + gridPos.z + "]");
            }
		}

		Debug.Log("Grid Output " + this.ToString() + " " + this.transform.rotation + "*\r\n" + grid.ToString());

		return grid;
	}


	public void RevealAllAnchorConnectorTokens()
    {
		Debug.Log("RevealAllAnchorConnectorTokens for tile " + this.ToString());
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);

			if (child.name.Contains("anchor"))
			{
				RevealAnchorConnectorToken(child, "anchor");
			}
			else if (child.name.Contains("connector"))
			{
				RevealAnchorConnectorToken(child, "connector");
			}
		}
	}

	/// <summary>
	/// reveal anchor/connector/special placeholder token for debug purposes
	/// </summary>
	public void RevealAnchorConnectorToken(Transform t, string tokenName)
	{
		var center = tilemesh.GetComponent<MeshRenderer>().bounds.center;

		GameObject token = null;
		if(tokenName == "anchor")
        {
			//token = Instantiate(anchorSphere, new Vector3(t.localPosition.x + center.x, t.localPosition.y + center.y, t.localPosition.z + center.z), t.localRotation);
			token = Instantiate(anchorSphere, new Vector3(t.position.x, t.position.y, t.position.z), t.rotation);
			//token = Instantiate(anchorSphere, new Vector3(t.localPosition.x, t.localPosition.y, t.localPosition.z), t.localRotation);
			//token = Instantiate(anchorSphere, new Vector3(t.position.x + center.x, t.position.y + center.y, t.position.z + center.z), t.rotation);
			//Debug.Log("anchor at " + t.localPosition);
		}
		else if(tokenName == "connector")
        {
			//token = Instantiate(connectorSphere, new Vector3(t.localPosition.x + center.x, t.localPosition.y + center.y, t.localPosition.z + center.z), t.localRotation);
			token = Instantiate(connectorSphere, new Vector3(t.position.x, t.position.y, t.position.z), t.rotation);
			//token = Instantiate(connectorSphere, new Vector3(t.localPosition.x, t.localPosition.y, t.localPosition.z), t.localRotation);
			//token = Instantiate(connectorSphere, new Vector3(t.position.x + center.x, t.position.y + center.y, t.position.z + center.z), t.rotation);
			//Debug.Log("connector at " + t.localPosition);
		}
		else if (tokenName == "special")
		{
			//token = Instantiate(specialSphere, new Vector3(t.localPosition.x + center.x, t.localPosition.y + center.y, t.localPosition.z + center.z), t.localRotation);
			token = Instantiate(specialSphere, new Vector3(t.position.x, t.position.y, t.position.z), t.rotation);
			//token = Instantiate(specialSphere, new Vector3(t.localPosition.x, t.localPosition.y, t.localPosition.z), t.localRotation);
			//token = Instantiate(specialSphere, new Vector3(t.position.x + center.x, t.position.y + center.y, t.position.z + center.z), t.rotation);
			//Debug.Log("special at " + t.localPosition);
		}
		if (token != null)
		{
			token.transform.parent = transform;
			token.SetActive(true);
			Debug.Log(tokenName + " loc[" + token.transform.position.x + "," + token.transform.position.y + "," + token.transform.position.z + "]");
			var posVisAVisGP = token.transform.parent.parent.InverseTransformPoint(token.transform.position);
			Debug.Log(tokenName + " gp[" + posVisAVisGP.x + "," + posVisAVisGP.y + "," + posVisAVisGP.z + "]");
		}
	}

	public bool HasTriggeredToken( string name )
	{
		bool found = false;

		Transform[] tf = GetChildren( "Token" );
		for ( int i = 0; i < tf.Length; i++ )
		{
			string tBy = tf[i].GetComponent<MetaData>().triggeredByName;
			if ( tBy != name )
				continue;
			else
				found = true;
		}

		return found;
	}

	/// <summary>
	/// Reveals ALL triggered tokens on this tile from TryTriggerToken()
	/// </summary>
	public Tuple<Vector3[], string[]> RevealTriggeredTokens( string tname )
	{
		var size = tilemesh.GetComponent<MeshRenderer>().bounds.size;
		var center = tilemesh.GetComponent<MeshRenderer>().bounds.center;
		Transform[] tf = GetChildren( "Token(Clone)" );
		//Vector3 tpos = ( -12345f ).ToVector3();
		List<Vector3> tpos = new List<Vector3>();
		List<string> nameList = new List<string>();

		for ( int i = 0; i < tf.Length; i++ )
		{
			MetaData tfmetaData = tf[i].GetComponent<MetaData>();
			string tBy = tfmetaData.triggeredByName;
			if ( tBy != tname )//|| tfmetaData.hasBeenReplaced )
				continue;

			//offset to token in EDITOR coords
			Vector3 offset = tfmetaData.offset;
			float scalar = Mathf.Max( size.x, size.z ) / 650f;
			if(this.baseTile.tileType == TileType.Square)
            {
				scalar = Mathf.Max(size.x, size.z) / 512f;
			}
			offset *= scalar;
			offset = Vector3.Reflect( offset, new Vector3( 0, 0, 1 ) );

			tf[i].gameObject.SetActive( true );
			tf[i].position = new Vector3( center.x + offset.x, 2, center.z + offset.z );
			tf[i].RotateAround( center, Vector3.up, baseTile.angle );
			tf[i].DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );
			tpos.Add( tf[i].position );

			string tokName = tfmetaData.tokenType.ToString();
			if(tfmetaData.tokenType == TokenType.Person)
            {
				tokName = Translate("interaction." + tfmetaData.personType.ToString(), tfmetaData.personType.ToString());
            }
			else if(tfmetaData.tokenType == TokenType.Terrain)
            {
				tokName = Translate("interaction." + tfmetaData.terrainType.ToString(), tfmetaData.terrainType.ToString());
            }
			nameList.Add(tokName);

			//mark active in token state
			//MetaData metaData = tf[i].GetComponent<MetaData>();
			TokenState tState = null;
			if ( !tfmetaData.isRandom )
			{
				tState = tokenStates.Where( x => x.metaData.GUID == tf[i].GetComponent<MetaData>().GUID ).FirstOr( null );
			}
			else
			{
				tState = tokenStates.Where( x => x.metaData.interactionName == tf[i].GetComponent<MetaData>().interactionName ).FirstOr( null );
			}
			if ( tState != null )
			{
				tState.isActive = true;
				tState.localPosition = tf[i].localPosition.Y( .3f );
			}
		}

		return new Tuple<Vector3[], string[]>(tpos.ToArray(), nameList.ToArray());
	}

	/// <summary>
	/// Handle clicking on tokens
	/// </summary>
	public bool InputUpdate( Ray ray )
	{
		if ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase )
			return false;

		if ( Physics.Raycast( ray, out RaycastHit hit ) )
		{
			Transform objectHit = hit.transform;
			if ( objectHit.name == "Exploration Token" )
			{
				var objs = FindObjectsOfType<SpawnMarker>();
				foreach ( var ob in objs )
				{
					if ( ob.name.Contains( "SPAWNMARKER" ) )
						Destroy( ob.gameObject );
					if ( ob.name == "STARTMARKER" )
						ob.gameObject.SetActive( false );
					if (ob.name.StartsWith("Start Token"))
						ob.gameObject.SetActive(false);
				}

				Tile tile = objectHit.parent.GetComponent<Tile>();

				interactionManager.GetNewTextPanel().ShowQueryExploration( ( res ) =>
				{
					if ( res.btn1 )
					{
						ShowExplorationText( tile, () =>
						 {
							 tile.RemoveExplorationToken();
							 tile.Colorize();
							 tile.RevealInteractiveTokens();
							 //fire trigger on chapter exploration
							 triggerManager.FireTrigger( tile.chapter.exploreTrigger );
							 //fire trigger on tile exploration
							 triggerManager.FireTrigger( tile.baseTile.triggerName );
							 if(tile.tileGroup.ExploredAllTiles())
                             {
								 triggerManager.FireTrigger(tile.chapter.exploredAllTilesTrigger);
                             }
							 //objectHit.parent.GetComponent<Tile>().tileGroup.ExploreTile();
						 } );
					}
				} );

				camControl.MoveTo( tile.centerPosition, .2f );
				return true;
			}
			else if ( objectHit.name.Contains( "Token" ) )
			{
				Debug.Log("name " + objectHit.name);
				Debug.Log("parent " + objectHit.parent);
				Debug.Log("tile " + objectHit.parent.GetComponent<Tile>());
				Debug.Log("center " + objectHit.parent.GetComponent<Tile>().centerPosition);
				Debug.Log("camControl " + camControl);
				if (camControl != null)
				{
					camControl.MoveTo(objectHit.parent.GetComponent<Tile>().centerPosition, .2f);
				}
				QueryTokenInteraction( objectHit );
				return true;
			}
		}
		return false;
	}

	void ShowExplorationText( Tile tile, System.Action action )
	{
		string flavor = tile.baseTile.flavorBookData.pages.Count > 0 ? Interpret("tile." + tile.baseTile.idNumber + ".exploredText", tile.baseTile.flavorBookData.pages[0]) : "";
		string instructions = Translate("dialog.text.DiscardExploration", "Discard the exploration token.");
		if ( Bootstrap.gameStarter.difficulty != Difficulty.Hard )
			instructions += " " + Translate("dialog.text.GainInspiration", "Gain 1 inspiration.");
		if ( !string.IsNullOrEmpty( flavor ) )
			flavor = flavor + "\r\n\r\n" + instructions;
		else
			flavor = instructions;

		interactionManager.GetNewTextPanel().ShowOkContinue( flavor, ButtonIcon.Continue, action );
	}

	/// <summary>
	/// Show interaction flavor text, see if player wants to use an Action
	/// </summary>
	void QueryTokenInteraction( Transform objectHit )
	{
		MetaData metaData = objectHit.GetComponent<MetaData>();
		string objectEventName = metaData.interactionName;

		//Set the ->> Interaction button text.
		string objectEventToken = metaData.tokenType.ToString();
		if (metaData.tokenType == TokenType.Person)
		{
			//Set the ->> Interaction button to the type of Person (Human/Halfpint/Dwarf/Elf)
			objectEventToken = metaData.personType.ToString();
		}
		else if (metaData.tokenType == TokenType.Terrain)
        {
			//Set the ->> Interaction button to the type of Terrain(Boulder / Bush / FirePit / etc.)
			objectEventToken = metaData.terrainType.ToString();
        }
		if(!String.IsNullOrWhiteSpace(metaData.tokenInteractionText))
        {
			objectEventToken = Interpret(metaData.tokenInteractionKey, metaData.tokenInteractionText);
        }

		Tile tile = objectHit.parent.GetComponent<Tile>();

		IInteraction inter = interactionManager.GetInteractionByName( metaData.interactionName );

		//handle edge case PersistentInteraction
		if ( inter is PersistentInteraction )
		{
			//ONLY swap in delegate event if the pers event hasn't had its alt text triggered
			if ( !FindObjectOfType<TriggerManager>().IsTriggered( ( (PersistentInteraction)inter ).alternativeTextTrigger ) )
			{
				objectEventName = ( (PersistentInteraction)inter ).eventToActivate;
				IInteraction delegateInteraction = interactionManager.GetInteractionByName( objectEventName );

				//delegate action to this event
				//Set the ->> Interaction button text.
				objectEventToken = delegateInteraction.tokenType.ToString();
				if (metaData.tokenType == TokenType.Person)
				{
					//Set the ->> Interaction button to the type of Person (Human/Halfpint/Dwarf/Elf)
					objectEventToken = metaData.personType.ToString();
				}
				else if (metaData.tokenType == TokenType.Terrain)
				{
					//Set the ->> Interaction button to the type of Terrain(Boulder / Bush / FirePit / etc.)
					objectEventToken = metaData.terrainType.ToString();
				}
				if (!String.IsNullOrWhiteSpace(metaData.tokenInteractionText))
				{
					objectEventToken = Interpret(metaData.tokenInteractionKey, metaData.tokenInteractionText);
				}

				//make it persistent
				delegateInteraction.isPersistent = true;
			}
		}

		interactionManager.QueryTokenInteraction( objectEventName, objectEventToken, ( res ) =>
	{
		if ( res.btn2 )
		{
			Debug.Log( "INTERACT::" + res.interaction.dataName );
			if ( res.interaction.interactionType != InteractionType.Persistent )
			{
				interactionManager.ShowInteraction( res.interaction, objectHit, ( iresult ) =>
				{
					if ( !res.interaction.isPersistent && iresult.removeToken )
						tile.RemoveInteractivetoken( objectHit, metaData );
				} );
			}
			else
				Debug.Log( "Persistent Event, doing nothing" );
		}
		else
			Debug.Log( "NO BTN2" );
	} );
	}

	void CreateToken( TokenState tokenState, bool createStartToken = true )
	{
		GameObject go = null;
		TileManager tileManager = FindObjectOfType<TileManager>();

		if (tokenState.metaData.tokenType == TokenType.None)
		{
			go = GameObject.Instantiate(tileManager.noneTokenPrefab, gameObject.transform);
		}
		else if (tokenState.metaData.tokenType == TokenType.Start)
		{
            if (!createStartToken) { return; } //Special case when loading a saved game. We don't want to create the Start Token again.
			go = GameObject.Instantiate(tileManager.startTokenPrefab, gameObject.transform);
		}
		else if ( tokenState.metaData.tokenType == TokenType.Search )
		{
			go = GameObject.Instantiate( tileManager.searchTokenPrefab, gameObject.transform );
		}
		else if ( tokenState.metaData.tokenType == TokenType.Person )
		{
			if ( tokenState.metaData.personType == PersonType.Human )
				go = GameObject.Instantiate( tileManager.humanTokenPrefab, gameObject.transform );
			else if ( tokenState.metaData.personType == PersonType.Elf )
				go = GameObject.Instantiate( tileManager.elfTokenPrefab, gameObject.transform );
			else if ( tokenState.metaData.personType == PersonType.Halfpint )
				go = GameObject.Instantiate( tileManager.hobbitTokenPrefab, gameObject.transform );
			else if ( tokenState.metaData.personType == PersonType.Dwarf )
				go = GameObject.Instantiate( tileManager.dwarfTokenPrefab, gameObject.transform );
		}
		else if ( tokenState.metaData.tokenType == TokenType.Threat )
		{
			go = GameObject.Instantiate( tileManager.threatTokenPrefab, gameObject.transform );
		}
		else if ( tokenState.metaData.tokenType == TokenType.Darkness )
		{
			go = GameObject.Instantiate( tileManager.darkTokenPrefab, gameObject.transform );
		}
		else if (tokenState.metaData.tokenType == TokenType.DifficultGround)
		{
			go = GameObject.Instantiate(tileManager.difficultGroundTokenPrefab, gameObject.transform);
		}
		else if (tokenState.metaData.tokenType == TokenType.Fortified)
		{
			go = GameObject.Instantiate(tileManager.fortifiedTokenPrefab, gameObject.transform);
		}
		else if (tokenState.metaData.tokenType == TokenType.Terrain)
		{
			if (tokenState.metaData.terrainType == TerrainType.Barrels)
				go = GameObject.Instantiate(tileManager.barrelsTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Barricade)
				go = GameObject.Instantiate(tileManager.barricadeTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Boulder)
				go = GameObject.Instantiate(tileManager.boulderTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Bush)
				go = GameObject.Instantiate(tileManager.bushTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Chest)
				go = GameObject.Instantiate(tileManager.chestTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Elevation)
				go = GameObject.Instantiate(tileManager.elevationTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Fence)
				go = GameObject.Instantiate(tileManager.fenceTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.FirePit)
				go = GameObject.Instantiate(tileManager.firePitTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Fountain)
				go = GameObject.Instantiate(tileManager.fountainTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Log)
				go = GameObject.Instantiate(tileManager.logTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Mist)
				go = GameObject.Instantiate(tileManager.mistTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Pit)
				go = GameObject.Instantiate(tileManager.pitTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Pond)
				go = GameObject.Instantiate(tileManager.pondTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Rubble)
				go = GameObject.Instantiate(tileManager.rubbleTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Statue)
				go = GameObject.Instantiate(tileManager.statueTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Stream)
				go = GameObject.Instantiate(tileManager.streamTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Table)
				go = GameObject.Instantiate(tileManager.tableTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Trench)
				go = GameObject.Instantiate(tileManager.trenchTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Wall)
				go = GameObject.Instantiate(tileManager.wallTokenPrefab, gameObject.transform);
			else if (tokenState.metaData.terrainType == TerrainType.Web)
				go = GameObject.Instantiate(tileManager.webTokenPrefab, gameObject.transform);
		}


		MetaData newMD = go.GetComponent<MetaData>();

		newMD.triggerName = tokenState.metaData.triggerName;
		newMD.interactionName = tokenState.metaData.interactionName;
		newMD.triggeredByName = tokenState.metaData.triggeredByName;
		newMD.tokenInteractionText = tokenState.metaData.tokenInteractionText;
		newMD.tokenInteractionKey = tokenState.metaData.tokenInteractionKey;
		newMD.tokenType = tokenState.metaData.tokenType;
		newMD.personType = tokenState.metaData.personType;
		newMD.terrainType = tokenState.metaData.terrainType;
		newMD.offset = tokenState.metaData.offset;
		newMD.GUID = tokenState.metaData.GUID;
		newMD.isRandom = tokenState.metaData.isRandom;
		newMD.tileID = baseTile.idNumber;

		//newMD.transform.localScale = Vector3.one;
		newMD.transform.localPosition = tokenState.localPosition;
		//newMD.transform.localRotation = tokenState.localRotation;

		//Rotate terrain tokens
		if(tokenState.metaData.tokenType == TokenType.Terrain && tokenState.YRotation != 0)
        {
			//go.transform.RotateAround(tokenState.rotationCenter, Vector3.up, tokenState.YRotation);
			go.transform.Rotate(Vector3.up, tokenState.YRotation);
		}

		newMD.gameObject.SetActive( tokenState.isActive );
	}

	public void SetState( SingleTileState singleTileState )
	{
		isExplored = singleTileState.isExplored;
		if ( isExplored )
		{
			if (Engine.currentScenario.scenarioTypeJourney && singleTileState.isActive )
				exploreToken.gameObject.SetActive( false );
			DOTween.To( () => 0, x =>
			{
				sepiaValue = x;
				meshRenderer.material.SetFloat( "_sepiaValue", sepiaValue );
			}, 0, 2f );
		}
		else
		{
			if ( singleTileState.isActive )
				RevealExplorationToken();
			else
			{
				exploreToken.gameObject.SetActive( false );
				exploreToken.position = exploreToken.position.Y( 2f );
			}

			DOTween.To( () => 1, x =>
			{
				sepiaValue = x;
				meshRenderer.material.SetFloat( "_sepiaValue", sepiaValue );
			}, 1, 2f );
		}

		///RESTORE TILE STATE
		transform.parent.position = singleTileState.globalParentPosition;
		transform.parent.rotation = Quaternion.Euler( 0, singleTileState.globalParentYRotation, 0 );
		transform.position = singleTileState.globalPosition;
		tokenTriggerList = singleTileState.tokenTriggerList;
		tokenStates = singleTileState.tokenStates;
		gameObject.SetActive( singleTileState.isActive );

		///RESTORE TOKENS
		//clear tokenstates list and remove all tokens on tile
		tokenStates = singleTileState.tokenStates;
		//get all token on this tile
		Transform[] tf = GetChildren( "Token(Clone)" );
		var remove = tf.Select( x => x.gameObject );
		foreach ( var rmd in remove )
			Destroy( rmd.gameObject );
		//recreate tokens
		foreach ( TokenState ts in singleTileState.tokenStates )
		{
			CreateToken( ts, false );
		}

		//remove any tokens that are created from replaced event
		//only important for quick loading
		//var replacedMDs = tf.Where( x => x.GetComponent<MetaData>().isCreatedFromReplaced );
		//foreach ( var rmd in replacedMDs )
		//	Destroy( rmd.gameObject );


		//restore token states
		/*for ( int i = 0; i < tf.Length; i++ )
		{
			//token on tile
			MetaData md = tf[i].GetComponent<MetaData>();
			//find matching token state to token on tile
			TokenState token = ( from mjson in singleTileState.tokenStates
													 where mjson.metaData.GUID == md.GUID || mjson.metaData.interactionName == md.interactionName
													 select mjson ).FirstOr( null );
			if ( token != null )
			{
				md.gameObject.SetActive( token.isActive );
				md.transform.localScale = Vector3.one;
				md.transform.localPosition = token.localPosition;
				md.triggerName = token.metaData.triggerName;
				md.interactionName = token.metaData.interactionName;
				md.triggeredByName = token.metaData.triggeredByName;
				md.tokenType = token.metaData.tokenType;
				md.offset = token.metaData.offset;
				md.isRandom = token.metaData.isRandom;
				md.tileID = token.metaData.tileID;
				md.hasBeenReplaced = token.metaData.hasBeenReplaced;
				//md.isActive = token.metaData.isActive;
			}
		}*/
	}

	override public  string ToString()
    {
		return baseTile.ToShortString();
    }
}
