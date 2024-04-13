using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GridPosition
{
	public int x;
	public int z;
	public Transform t; //transform stored related to this GridPosition
}

public class ConnectorGrid
{
	public readonly float distanceX = 0.75f / 2;
	public readonly float distanceZ = 0.4330127f;
	public float minX = 100_000;
	public float minZ = 100_000;
	public float leftmostMinZ = 100_000;
	public float maxX = -100_000;
	public float maxZ = -100_000;
	public float offsetX = 0;
	public float offsetZ = 0;
	float wiggleRoom = 0.15f; //used to hopefully bump a coordinate over the point where its division and conversion to int will put it at the right location
	public int gridX;
	public int gridZ;
	public int[,] grid;
	public List<GridPosition> transformPositionList = new List<GridPosition>();

	public void AllocateTileGroupGridSize()
    {
		//prebuild a rectangular area of hopefully sufficient size to arrange a group of tiles in
		minX = 0;
		minZ = 0;
		leftmostMinZ = 0;
		maxX = 0;
		maxZ = 0;
		offsetX = 64 * distanceX;
		offsetZ = 64 * distanceZ;
		gridX = 128;
		gridZ = 128;
		grid = new int[gridX, gridZ];
	}

	public void CopyTileToTileGroup(ConnectorGrid tileGrid, int tileMarker)
    {
		GridPosition offsetPos = CalculateGroupOffsetFromTileOffset(tileGrid);
		foreach (GridPosition pos in tileGrid.transformPositionList)
        {
			//Debug.Log("tile [" + pos.x + "," + pos.z + "] => tileGroup [" + offsetPos.x + "," + offsetPos.z + "]");
			int tileValue = tileGrid.grid[pos.x, pos.z];
			//Debug.Log("tileValue: " + tileValue);
			int tileGroupValue = grid[offsetPos.x + pos.x, offsetPos.z + pos.z];
			//Debug.Log("tileGroupValue: " + tileGroupValue);

			if(tileGroupValue <= 0 && tileValue > 0)
            {
				grid[offsetPos.x + pos.x, offsetPos.z + pos.z] = tileMarker;
            }
		}
	}

	public void EstablishMinMaxFromTransformChildren(Transform transform)
    {
		//Loop through first to establish the min and max positions
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform t = transform.GetChild(i);
			if (t.name.Contains("anchor") || t.name.Contains("connector"))
			{
				if (t.position.x < minX) { minX = t.position.x; }
				if (t.position.x > maxX) { maxX = t.position.x; }
				if (t.position.z < minZ) { minZ = t.position.z; }
				if (t.position.z > maxZ) { maxZ = t.position.z; }
				if (t.position.x > minX - 0.1 && t.position.x < minX + 0.1)
                {
					if (t.position.z < leftmostMinZ) { leftmostMinZ = t.position.z; }
				}
			}
		}
	}

	public void EstablishOffsetToTranslateToZero()
    {
		//Offset will help to align the leftmost and topmost markers with the start of the array
		offsetX = 0 - minX;
		offsetZ = 0 - minZ;
	}

	public void AllocateGridSize()
    {
		gridX = (int)((maxX + offsetX + wiggleRoom) / distanceX) + 1;
		gridZ = (int)((maxZ + offsetZ + wiggleRoom) / distanceZ) + 1;
		grid = new int[gridX, gridZ];
	}

	public GridPosition CalculateGridPosition(float x, float z, Transform t = null)
    {
		int posX = (int)((x + offsetX + wiggleRoom) / distanceX);
		int posZ = (int)((z + offsetZ + wiggleRoom) / distanceZ);
		return new GridPosition() { x=posX, z=posZ, t= t };
	}

	public Vector3 ReverseGridPosition(float x, float z)
    {
		Vector3 v = new Vector3(x / distanceX - offsetX, 0, z / distanceZ - offsetZ);
		return v;
    }

	public GridPosition CalculateGroupOffsetFromTileOffset(ConnectorGrid tileGrid)
    {
		int newX = (int)((tileGrid.offsetX + offsetX + wiggleRoom) / distanceX);
		int newZ = (int)((tileGrid.offsetZ + offsetZ + wiggleRoom) / distanceZ);
		return new GridPosition() { x = newX, z = newZ };
    }

	/*
	Given an int[,] grid which is an array representation of a hex map, print the grid.
	Generate a grid array with GenerateConnectorGrid.

	Sample output:

	Grid Output 100A
	|    .    |
	|  .   .  |
	|.   O   .|
	|  O   O  |
	|.   O   .|
	|  .   .  |
	|    .    |


	Grid Output 221A
	|        .  |
	|      .   .|
	|    .   O  |
	|  .   O   .|
	|.   O   O  |
	|  O   O   .|
	|.   O   O  |
	|  .   .   .|
	|    .   .  |
	*/
	public override string ToString()
	{
		string gridDisplay = "";
		for (int z = 0; z < gridZ; z++)
		{
			string lineDisplay = "|";
			for (int x = 0; x < gridX; x++)
			{
				int value = grid[x, z];
				if (value == 2 || value == -1) //anchor OUTside tile
				{
					lineDisplay += ".";
				}
				else if (value == -2 || value == 1) //connector INside tile
				{
					lineDisplay += "O";
				}
				else if (value == 0)
				{
					lineDisplay += " ";
				}
				else
				{
					lineDisplay += "?";
				}
			}
			//gridDisplay += "|\r\n|";
			gridDisplay = lineDisplay + "|\r\n" + gridDisplay;
		}

		return gridDisplay;
	}

	public string TileGroupToString()
	{
		string gridDisplay = "";
		for (int z = 0; z < gridZ; z++)
		{
			string lineDisplay = "|";
			for (int x = 0; x < gridX; x++)
			{
				int value = grid[x, z];
				if (value < 0) //anchor OUTside tile
				{
					lineDisplay += ".";
				}
				else if (value == 0)
				{
					lineDisplay += " ";
				}
				else if (value >= 1 && value <= 9)
                {
					lineDisplay += value.ToString();
                }
				else
				{
					lineDisplay += "?";
				}
			}
			//gridDisplay += "|\r\n|";
			gridDisplay = lineDisplay + "|\r\n" + gridDisplay;
		}

		return gridDisplay;
	}
}
