using System;
using System.Collections.ObjectModel;
using System.Globalization;
using UnityEngine;

public class BaseTile : ITile
{
	public float angle { get; set; }
	public int idNumber { get; set; }
	public int tokenCount { get; set; }
	public Guid GUID { get; set; }
	public string position
	{
		get { return vposition.ToString(); }
		set
		{
			string[] s = value.Split( ',' );
			vposition = new Vector3(
				float.Parse( s[0], CultureInfo.InvariantCulture ),
				0,
				float.Parse( s[1], CultureInfo.InvariantCulture ) );
		}
	}
	public TileType tileType { get; set; }
	public string tileSide { get; set; }
	public TextBookData flavorBookData { get; set; }
	public ObservableCollection<Token> tokenList { get; set; }

	public string hexRoot { get; set; }

	private string _pathRoot;
	public string pathRoot 
	{ 
		get => _pathRoot;
		set
		{
			_pathRoot = value;
			hexRoot = _pathRoot;
		}
	}

	public Vector3 vposition { get; set; }
	public bool isStartTile { get; set; }
	public string triggerName { get; set; }

	public BaseTile()
	{
	}

	public BaseTile( int n, Vector position, float angle )
	{
		idNumber = n;
		tokenCount = ( n / 100 ) % 10;
		GUID = Guid.NewGuid();
		tileSide = "A";
		vposition = position.ToVector3();
		this.angle = angle;
		tokenList = new ObservableCollection<Token>();
		flavorBookData = new TextBookData() { pages = new System.Collections.Generic.List<string>() { "" } };
		isStartTile = false;
		triggerName = "None";
	}

	virtual public string ToString()
    {
		return idNumber + tileSide + " [" + position + "] " + angle + "*, " + tokenCount + " tokens" + (isStartTile ? "[start]" : "");
    }

	public string ToShortString()
    {
		return idNumber + tileSide;
    }
}