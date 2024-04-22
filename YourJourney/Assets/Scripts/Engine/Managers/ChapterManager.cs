using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LanguageManager;

public class ChapterManager : MonoBehaviour
{
	public SpawnMarker startMarker;

	[HideInInspector]
	public List<Chapter> chapterList;

	TileGroup previousGroup;
	List<string> tokenTriggerQueue = new List<string>();
	List<string> darknessTiles = new List<string>();
	List<string> difficultTiles = new List<string>();
	List<string> fortifiedTiles = new List<string>();

	public void Init( Scenario s )
	{
		chapterList = new List<Chapter>( s.chapterObserver );
		previousGroup = null;
		darknessTiles.AddRange( new string[] { 
				//JiME Base Game
				"204B", "207B", "208B", "303B", "306B", "307B",
				//Shadowed Paths
				"212A", "215A", "216A", "219A", "221A", "310A",
				"102B", "210B", "211B", "212B", "213B", "214B", "215B", "216B", "217B", "218B", 
				"309B", "310B", "311B", "312B", "401B", "402B",
				//Spreading War
				"227A", "404A", 
				"224B", "225B", "227B", "314B", "316B", "318B", "319B", "320B", "404B", 

		});
		difficultTiles.AddRange(new string[] {
				//Shadowed Paths
				"210A", "214A", "220A", "311A", "313A", "401A", "402A",
				"214B", "220B", "313B", 
				//Spreading War
				"104A", "222A", "314A", "403A", "500A", 
				"224B", "226B", 
		});
		fortifiedTiles.AddRange(new string[]
		{
				//Spreading War
				"222A", "226A", "317A", "319A", "403A", 
				"104B", "222B", "223B", "225B", "227B", "316B", "318B", "320B", "403B", "404B", "500B", 
		});
		//Debug.Log( $"Chapter Manager: {chapterList.Count} Chapters Found" );
	}

	/// <summary>
	/// true if there are ACTIVE Darkness Tokens or tiles with Darkness on board
	/// </summary>
	public bool IsDarknessVisible()
	{
		var tm = FindObjectOfType<TileManager>().GetAllTileGroups();

		//any ACTIVE darkness TOKENS in explored tiles?
		var tokensfound = from tg in tm
											from tile in tg.tileList
											where tile.isExplored
											where tile.IsDarknessTokenActive()
											select tile;

		//any darkness TILES that are explored?
		var tilesfound =
			from tg in tm
			from tile in tg.tileList
			where tile.isExplored
			where darknessTiles.Contains( "" + tile.baseTile.idNumber + tile.baseTile.tileSide)
			select tile;

		//int tk = tokensfound.Count();
		//int tl = tilesfound.Count();
		//Debug.Log( "FOUND DARK:" );
		//Debug.Log( tk );
		//Debug.Log( tl );

		if ( tokensfound.Count() > 0 || tilesfound.Count() > 0 )
			return true;

		return false;
	}

	/// <summary>
	/// Enqueues a trigger name to activate a token later, after the chapter has been activated, but only if a token exists Triggered By the name
	/// </summary>
	public void EnqueueTokenTrigger( string name )
	{
		var foo = from chapter in chapterList
							from tile in chapter.tileObserver
							from token in ( (BaseTile)tile ).tokenList
							where token.triggeredByName == name
							select token;

		if ( foo.Count() > 0 && !tokenTriggerQueue.Contains( name ) )
		{
			Debug.Log( "Chapter EnqueueTokenTrigger: " + name );
			tokenTriggerQueue.Add( name );
		}
		//else
		//	Debug.Log( "*NOT Chapter EnqueueTokenTrigger: " + name );
	}

	public bool TriggerChapterByTrigger( string triggername )
	{
		if ( chapterList.Any( x => x.triggeredBy == triggername ) )
		{
			Chapter c = chapterList.Where( x => x.triggeredBy == triggername ).First();
			Debug.Log( "TriggerChapterByTrigger::Found Chapter: " + c.dataName );
			TryTriggerChapter( c.dataName, false );
			return true;
		}
		return false;
	}

	/// <summary>
	/// creates Chapter tilegroup, attaches it to previous group, animates it up, optionally pre-explores it (colorize + reveal tokens), prompts starting location if applicable
	/// </summary>
	public void TryTriggerChapter( string chname, bool firstChapter )
	{
		Debug.Log( "TryTriggerChapter::" + chname );// + "::firstChapter=" + firstChapter );
		if ( chapterList.Any( x => x.dataName == chname ) )
		{
			//support multiple chapters?
			Chapter c = chapterList.Where( x => x.dataName == chname ).First();
			Debug.Log( "TryTriggerChapter::Found Chapter: " + c.dataName );
			//show flavor text
			if (!c.noFlavorText)
			{
				var im = FindObjectOfType<InteractionManager>().GetNewTextPanel();
				im.ShowOkContinue(c.flavorBookData.pages[0], ButtonIcon.Continue, () =>
				 {
					 StartCoroutine(FinishChapterTriggerCoroutine(c, firstChapter));
				 });
			}
			else
			{
				StartCoroutine(FinishChapterTriggerCoroutine(c, firstChapter));
			}
		}
	}

	void FinishChapterTrigger( Chapter c, bool firstChapter )
	{
		Debug.Log("FinishChapterTrigger chapter " + c.dataName + " firstChapter? " + firstChapter);
		string s = Translate("dialog.text.PrepareTiles", "Prepare the following tiles:") + "\r\n\r\n";
		foreach (BaseTile bt in c.tileObserver)
		{
			Debug.Log(bt.ToString());
			if (bt.tileType == TileType.Hex)
			{
				s += bt.idNumber + bt.tileSide
					+ "<font=\"Icon\">" + Collection.FromTileNumber(bt.idNumber).FontCharacter + "</font>" //Add the Collection symbol.
					+ ", ";
			}
			else if (bt.tileType == TileType.Square)
            {
				s += Translate(bt.tileSide == "A" ? "dialog.text.BattleTileGrass" : "BattleTileDirt",
						"Battle Map Tile " + (bt.tileSide == "A" ? " (Grass)" : " (Dirt)"))
					+ ", ";
			}
		}
		s = s.Substring( 0, s.Length - 2 ); //Remove trailing comma

		FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( s, ButtonIcon.Continue, () =>
		{
			//TileGroup tg = FindObjectOfType<TileManager>().CreateGroupFromChapter( c );
			TileGroup tg = c.tileGroup;

			if ( tg == null )
			{
				Debug.Log( "FinishChapterTrigger::WARNING::Chapter has no tiles: " + c.dataName );
				return;
			}

			tg.ActivateTiles();
			FindObjectOfType<Engine>().RemoveFog( tg.GetChapter().dataName );

			//attempt to attach this tg, but only if it IS dynamic
			//fall back to using random tg if it doesn't fit
			if ( c.isDynamic )
			{
				AttachDynamicTileCoroutine( tg, null );

				////get ALL explored tilegroups in play
				//var tilegroups = ( from ch in chapterList
				//									 where ch.tileGroup.isExplored
				//									 select ch.tileGroup ).ToList();

				//bool success = false;

				//if ( previousGroup != null )
				//{
				//	success = tg.AttachTo( previousGroup );
				//	//remove so not attempted again below
				//	tilegroups.Remove( previousGroup );
				//}
				//else
				//{
				//	int randIdx = GlowEngine.GenerateRandomNumbers( tilegroups.Count )[0];
				//	TileGroup randGroup = tilegroups[randIdx];
				//	success = tg.AttachTo( randGroup );
				//	//remove so not attempted again below
				//	tilegroups.RemoveAt( randIdx );
				//}

				//if ( !success )
				//{
				//	Debug.Log( "***SEARCHING for random tilegroup to attach to..." );
				//	foreach ( TileGroup _tg in tilegroups )
				//	{
				//		success = tg.AttachTo( _tg );
				//		if ( success )
				//			break;
				//	}
				//}
			}

			tg.AnimateTileUp( c );

			previousGroup = tg;

			if ( firstChapter && c.isPreExplored )
			{
				tg.Colorize();
				tg.isExplored = true;
			}
			else if (firstChapter)
            {
				//Starting tile group unexplored, but we still need to colorize the starting tile
				tg.Colorize(true);
            }

			FindObjectOfType<CamControl>().MoveTo( tg.groupCenter );

			//check triggered token queue
			var foo = from tname in tokenTriggerQueue from tile in tg.tileList.Where( x => x.HasTriggeredToken( tname ) ) select new { tile, tname };
			//if ( foo.Count() > 0 )
			//Debug.Log( "FinishChapterTrigger::" + foo.Count() );
			foreach ( var item in foo )
			{
				item.tile.EnqueueTokenTrigger( item.tname );
				//tokenTriggerQueue.Remove( item.tname );
			}

			if ( tg.startPosition.x != -1000 )
			{
				GlowTimer.SetTimer( 1, () =>
				{
					startMarker.Spawn( tg.startPosition );
					if ( firstChapter && !c.isPreExplored )
					{
						tg.Colorize( true );
						tg.isExplored = true;
					}
				} );
				FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue(Translate("dialog.text.PlaceHeroes", "Place your Heroes in the indicated position."), ButtonIcon.Continue, () =>
                {
					int scoutAmount = Engine.currentScenario.initialScout;
					if (scoutAmount > 0)
					{
						FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowScoutX(scoutAmount);
					}
				});
			}
		} );
	}


	IEnumerator FinishChapterTriggerCoroutine(Chapter c, bool firstChapter)
	{
		Debug.Log("FinishChapterTrigger chapter " + c.dataName + " firstChapter? " + firstChapter);
		string s = Translate("dialog.text.PrepareTiles", "Prepare the following tiles:") + "\r\n\r\n";
		//Create the list of tiles
		foreach (BaseTile bt in c.tileObserver)
		{
			Debug.Log(bt.ToString());
			if (bt.tileType == TileType.Hex)
			{
				s += bt.idNumber + bt.tileSide
					+ "<font=\"Icon\">" + Collection.FromTileNumber(bt.idNumber).FontCharacter + "</font>" //Add the Collection symbol.
					+ ", ";
			}
			else if (bt.tileType == TileType.Square)
			{
				s += Translate(bt.tileSide == "A" ? "dialog.text.BattleTileGrass" : "BattleTileDirt",
						"Battle Map Tile " + (bt.tileSide == "A" ? " (Grass)" : " (Dirt)"))
					+ ", ";
			}
		}
		s = s.Substring(0, s.Length - 2); //Remove trailing comma

		TextPanel textPanel = FindObjectOfType<InteractionManager>().GetNewTextPanel();

		//TileGroup tg = FindObjectOfType<TileManager>().CreateGroupFromChapter( c );
		TileGroup tg = c.tileGroup;

		//Show the panel, and afterwards start creating the dynamic tile attachment if needed
		textPanel.ShowPrepareTiles(s, ButtonIcon.Continue, c.isDynamic, () =>
		{
			if (tg == null)
			{
				Debug.Log("FinishChapterTrigger::WARNING::Chapter has no tiles: " + c.dataName);
				return;
			}

			tg.ActivateTiles();
			FindObjectOfType<Engine>().RemoveFog(tg.GetChapter().dataName);

			tg.AnimateTileUp(c);

			previousGroup = tg;

			if (firstChapter && c.isPreExplored)
			{
				tg.Colorize();
				tg.isExplored = true;
			}
			else if (firstChapter)
			{
				//Starting tile group unexplored, but we still need to colorize the starting tile
				tg.Colorize(true);
			}

			FindObjectOfType<CamControl>().MoveTo(tg.groupCenter);

			//check triggered token queue
			var foo = from tname in tokenTriggerQueue from tile in tg.tileList.Where(x => x.HasTriggeredToken(tname)) select new { tile, tname };
			//if ( foo.Count() > 0 )
			//Debug.Log( "FinishChapterTrigger::" + foo.Count() );
			foreach (var item in foo)
			{
				item.tile.EnqueueTokenTrigger(item.tname);
				//tokenTriggerQueue.Remove( item.tname );
			}

			if (tg.startPosition.x != -1000)
			{
				GlowTimer.SetTimer(1, () =>
				{
					startMarker.Spawn(tg.startPosition);
					if (firstChapter && !c.isPreExplored)
					{
						tg.Colorize(true);
						tg.isExplored = true;
					}
				});
				FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue(Translate("dialog.text.PlaceHeroes", "Place your Heroes in the indicated position."), ButtonIcon.Continue, () =>
				{
					int scoutAmount = Engine.currentScenario.initialScout;
					if (scoutAmount > 0)
					{
						FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowScoutX(scoutAmount);
					}
				});
			}
		});

		//attempt to attach this tg, but only if it IS dynamic. The coroutine should also update the text panel's value with a progress bar type update.
		if (c.isDynamic)
		{
			yield return StartCoroutine(AttachDynamicTileCoroutine(tg, textPanel));
			textPanel.ShowButtonSingle(); //once we're done finding the attach point, show the button to close the text panel
			textPanel.UpdateText(""); //remove any Scouting Location text
		}

	}

	IEnumerator AttachTile( TileGroup tg )
	{
		//get ALL explored tilegroups in play
		var tilegroups = ( from ch in chapterList
											 where ch.tileGroup.isExplored
											 select ch.tileGroup ).ToList();

		bool success = false;

		if ( previousGroup != null )
		{
			success = tg.AttachTo( previousGroup );
			//remove so not attempted again below
			tilegroups.Remove( previousGroup );
		}
		else
		{
			int randIdx = GlowEngine.GenerateRandomNumbers( tilegroups.Count )[0];
			TileGroup randGroup = tilegroups[randIdx];
			success = tg.AttachTo( randGroup );
			//remove so not attempted again below
			tilegroups.RemoveAt( randIdx );
		}

		if ( !success )
		{
			Debug.Log( "***SEARCHING for random tilegroup to attach to..." );
			foreach ( TileGroup _tg in tilegroups )
			{
				success = tg.AttachTo( _tg );
				if ( success )
					break;
			}
		}
		yield return null;
	}

	IEnumerator AttachDynamicTileCoroutine(TileGroup tg, TextPanel textPanel)
	{
		//get ALL explored tilegroups in play
		var tilegroups = (from ch in chapterList
						  where ch.tileGroup.isExplored && ch.tileGroup.isPlaced
						  select ch.tileGroup).ToList();

		bool success = false;

		if (previousGroup != null)
		{
			yield return StartCoroutine(tg.AttachToCoroutine(previousGroup, 0, textPanel));
			success = tg.attachToCoroutineResult;

			//remove so not attempted again below
			tilegroups.Remove(previousGroup);
		}
		else
		{
			int randIdx = GlowEngine.GenerateRandomNumbers(tilegroups.Count)[0];
			TileGroup randGroup = tilegroups[randIdx];

			yield return StartCoroutine(tg.AttachToCoroutine(randGroup, 0, textPanel));
			success = tg.attachToCoroutineResult;

			//remove so not attempted again below
			tilegroups.RemoveAt(randIdx);
		}
		tg.isPlaced = success;

		if (!success)
		{
			Debug.Log("***SEARCHING for random tilegroup to attach to...");
			foreach (TileGroup _tg in tilegroups)
			{
				yield return StartCoroutine(tg.AttachToCoroutine(_tg, tg.GetChapter().attachTileHint, textPanel));
				success = tg.attachToCoroutineResult;
				if (success)
				{
					tg.isPlaced = true;
					break;
				}
			}
		}
		yield return null;
	}

	public ChapterState GetState()
	{
		return new ChapterState()
		{
			tokenTriggerQueue = tokenTriggerQueue,
			previousGroupGUID = previousGroup.GUID
		};
	}

	public void SetState( ChapterState chapterState )
	{
		tokenTriggerQueue = chapterState.tokenTriggerQueue;

		var groups = FindObjectOfType<TileManager>().GetAllTileGroups();
		previousGroup = ( from tg in groups
											where tg.GUID == chapterState.previousGroupGUID
											select tg ).First();
	}
}
