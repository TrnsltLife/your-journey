using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Monsters
{
	/*
	 * A set of variables and accessor methods to initialize and get the default Monster values
	 */
	private static List<Monster> MonstersList = Enum.GetValues(typeof(MonsterType)).OfType<MonsterType>().Select(mt => Monster.MonsterFactory(mt)).ToList();

	public static Monster Get(int i)
	{
		return MonstersList[i];
	}

	public static Monster Get(MonsterType mt)
    {
		return Get((int)mt);
    }

	public static List<Monster> List()
	{
		return MonstersList;
	}

	public static int Cost(int i) 
	{ 
		return Get(i).cost[0];
	}

	public static int Cost(MonsterType mt)
	{
		return Cost((int)mt);
	}
}
