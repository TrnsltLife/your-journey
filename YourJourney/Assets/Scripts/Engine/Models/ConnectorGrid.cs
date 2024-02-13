using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GridPosition
{
	public int x;
	public int z;
}

public class ConnectorGrid
{
	public readonly float distanceX = 0.75f / 2;
	public readonly float distanceZ = 0.4330127f;
	public float minX = 100_000;
	public float minZ = 100_000;
	public float maxX = -100_000;
	public float maxZ = -100_000;
	public float offsetX = 0;
	public float offsetZ = 0;
	float wiggleRoom = 0.15f; //used to hopefully bump a coordinate over the point where its division and conversion to int will put it at the right location
	public int gridX;
	public int gridZ;
	public int[,] grid;

	public void AllocateGridSize()
    {
		gridX = (int)((maxX + offsetX + wiggleRoom) / distanceX) + 1;
		gridZ = (int)((maxZ + offsetZ + wiggleRoom) / distanceZ) + 1;
		grid = new int[gridX, gridZ];
	}

	public GridPosition CalculateGridPosition(float x, float z)
    {
		int posX = (int)((x + offsetX + wiggleRoom) / distanceX);
		int posZ = (int)((z + offsetZ + wiggleRoom) / distanceZ);
		return new GridPosition() { x=posX, z=posZ };
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
		string gridDisplay = "|";
		for (int z = 0; z < gridZ; z++)
		{
			for (int x = 0; x < gridX; x++)
			{
				int value = grid[x, z];
				if (value == 2 || value == -1) //anchor OUTside tile
				{
					gridDisplay += ".";
				}
				else if (value == -2 || value == 1) //connector INside tile
				{
					gridDisplay += "O";
				}
				else if (value == 0)
				{
					gridDisplay += " ";
				}
				else
				{
					gridDisplay += "?";
				}
			}
			gridDisplay += "|\r\n|";
		}

		return gridDisplay;
	}
}
