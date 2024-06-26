﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class ThreatInteraction : InteractionBase
{
	public string triggerDefeatedName { get; set; }
	public bool[] includedEnemies { get; set; } //True/false items in this list will be in the same order as Shared MonsterType and Monster.monsterNames, etc.
	[DefaultValue( 0 )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public int basePoolPoints { get; set; }
	[DefaultValue( DifficultyBias.Medium )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public DifficultyBias difficultyBias { get; set; }

	public List<Monster> monsterCollection; //Contains scripted monsters

	public override InteractionType interactionType { get { return InteractionType.Threat; } set { } }

	public override string TranslationKey(string suffix) { return "event.enemy." + dataName + "." + suffix; }


	int lastEnemyIndex;
	int numSingleGroups;
	int modPoints;

	public void RemoveUnavailableScriptedMonsters()
    {
		foreach(Monster ms in monsterCollection)
        {
			ms.count = Math.Min(ms.count, MonsterManager.LeftInPool(ms.monsterType));
			if(ms.count <= 0)
            {
				Debug.Log("Scripted enemy group count for " + ms.dataName + " reduced to 0.");
			}
			MonsterManager.RemoveMonsterFromPool(ms.monsterType, ms.count);
		}
    }

	/// <summary>
	/// Generates enemy groups using Pool System
	/// </summary>
	public List<Monster> GenerateEncounter()
	{
		/*
				Large = +2 health, cost=1
				Bloodthirsty = 1 damage step up+wound bias, cost=2
				Armored = +1 armor, cost=1
		*/
		numSingleGroups = 0;
		lastEnemyIndex = -1;//avoid repeat enemy types if possible
		modPoints = 0;

		float poolCount = CalculateScaledPoints();
		float starting = poolCount;

		//if no enemies checked, returns 1000
		int lowestCost = LowestRequestedEnemyCost();
		if ( lowestCost == 1000 )
		{
			Debug.Log( "There are no Enemies included in the Pool." );
			return new List<Monster>();
		}

		if ( poolCount < lowestCost )
		{
			Debug.Log( "There aren't enough Pool Points to generate any Enemies given the current parameters." );
			return new List<Monster>();
		}

		//generate all the random monster groups possible
		List<Monster> mList = new List<Monster>();
		float lastPoolCount = -1000f;
		while ( poolCount >= lowestCost && poolCount != lastPoolCount )//lowest enemy cost
		{
			lastPoolCount = poolCount; //keep track of lastPoolCount to help avoid infinite loops when too many points are allocated that can't be used up by the available enemies

			Tuple<Monster, float> generated = GenerateMonster( poolCount );
			if ( generated.Item1.dataName != "modifier" )
			{
				poolCount = Math.Max( 0f, poolCount - generated.Item2 );
				mList.Add( generated.Item1 );
				Debug.Log( "Group cost: " + generated.Item2 );
			}
			else
			{
				//use dummy point
				poolCount = Math.Max( 0f, poolCount - 1f );
			}
		}

		//recalculate points left over
		poolCount = starting;
		foreach ( Monster sim in mList )
			poolCount -= sim.fCost;

		//if enough points left for more enemies, add them
		if ( poolCount > lowestCost )
		{
			foreach ( Monster sim in mList )
			{
				//% chance to add another
				if ( Bootstrap.random.Next( 100 ) < 50 && sim.count < 2 && sim.count < sim.groupLimit && MonsterManager.LeftInPool(sim.monsterType) > 0 && sim.singlecost <= poolCount)
				{
					poolCount = Math.Max( 0f, poolCount - sim.singlecost );
					sim.count++;
					MonsterManager.RemoveMonsterFromPool(sim.monsterType, 1);
					sim.fCost += sim.singlecost;
				}
			}
		}

		//add modifiers with any leftover points
		bool foundModifiers = true;
		while (foundModifiers && poolCount > 1)
		{
			foundModifiers = false;
			if (mList.Count > 0 && poolCount > 0)
			{
				foreach (Monster sim in mList)
				{
					if (sim.modifierList.Count < Monster.MAX_MODIFIERS && sim.modifierList.Count < Monster.REASONABLE_MODIFIERS)
					{
						List<MonsterModifier> modList = MonsterModifier.ListAvailableModifiersFor(sim.monsterType, sim.count, (int)poolCount);
						int modIndex = Bootstrap.random.Next(modList.Count + 1);
						if (modIndex != modList.Count)
						{
							MonsterModifier mod = modList[modIndex];
							if (sim.AddModifier(mod))
							{
								int modCost = mod.CalculateCost(sim.count);
								poolCount -= modCost;
								modPoints += modCost;
								sim.isElite = true;
								foundModifiers = true;
								Debug.Log("mod added: " + mod.name);
							}
						}
					}
				}
			}
		}

		//round it to 2 decimal places
		float leftOvers = (float)Math.Round( poolCount * 100f ) / 100f;

		Debug.Log( "leftover points: " + leftOvers );
		return mList;
	}

	Tuple<Monster, float> GenerateMonster( float points )
	{
		//monster type/cost
		List<Tuple<MonsterType, float>> mList = new List<Tuple<MonsterType, float>>();
		//create list of enemy candidates
		for ( int i = 0; i < includedEnemies.Length; i++ )
		{
			//skip using enemy if it was already used last iteration
			if ( includedEnemies.Count( x => x ) > 1 && lastEnemyIndex == i )
				continue;

			//skip enemies that have their MonsterManager.monsterPool exhausted
			if (MonsterManager.LeftInPool((MonsterType)i) <= 0)
				continue;

			//includedEnemies lines up with MonsterType enum and MonsterCost array
			if ( includedEnemies[i] && points >= Monsters.Cost(i) )
			{
				mList.Add( new Tuple<MonsterType, float>( (MonsterType)i, Monsters.Cost(i)) );
			}
		}

		//how many
		if ( mList.Count > 0 )//sanity check
		{
			//pick 1 at random
			int pick = Bootstrap.random.Next( 0, mList.Count );
			//Debug.Log( pick );

			Monster ms = Monster.MonsterFactory( mList[pick].Item1 );
			//.6 of cost per count above 1
			float groupcost = mList[pick].Item2;
			int upTo = groupcost <= points ? 1 : 0;
			int leftInPool = MonsterManager.LeftInPool(ms.monsterType);
			for ( int i = 0; i < 2 && i < leftInPool; i++ )
			{
				if ( ( groupcost + .6f * mList[pick].Item2 ) <= points )
				{
					upTo += 1;
					groupcost += .6f * mList[pick].Item2;
				}
			}

			int count = Bootstrap.random.Next( 1, upTo + 1 );
			//avoid a bunch of 1 enemy groups
			if ( count == 1 && numSingleGroups >= 1 )
			{
				if ( count + 1 <= upTo && count + 1 <= leftInPool)//if room to add 1 more...
				{
					//50% chance add another or use the points for modifiers
					if ( Bootstrap.random.Next( 100 ) > 50 || modPoints > 3 )
						count += 1;
					else
					{
						Monster skip = new Monster() { dataName = "modifier", fCost = 1 };
						return new Tuple<Monster, float>( skip, 0 );
					}
				}
				else//no more room, 30% to add a modifier point instead
				{
					Monster skip = new Monster() { dataName = "modifier", fCost = 0 };
					return new Tuple<Monster, float>( skip, Bootstrap.random.Next( 100 ) > 30 ? 1 : 0 );
				}
			}

			if(count > leftInPool) { count = leftInPool; } //make sure we don't exceed the available number of figures
			if(count > ms.groupLimit) { count = ms.groupLimit; } //make sure we don't exceed the allowed number of this kind of enemy in a group
			groupcost = mList[pick].Item2 + ( ( count - 1 ) * ( .6f * mList[pick].Item2 ) );
			lastEnemyIndex = (int)mList[pick].Item1;
			ms.count = count;
			MonsterManager.RemoveMonsterFromPool(ms.monsterType, count);
			ms.singlecost = mList[pick].Item2;
			ms.fCost = groupcost;
			if (count == 1)
			{
				numSingleGroups++;
			}
			else if (count == 0)
			{
				Monster skip = new Monster() { dataName = "modifier", fCost = 1 };
				return new Tuple<Monster, float>(skip, 1);
			}

			return new Tuple<Monster, float>( ms, mList[pick].Item2 * count );
		}
		else
		{
			Monster skip = new Monster() { dataName = "modifier", fCost = 1 };
			return new Tuple<Monster, float>( skip, 1 );
		}
	}

	private float CalculateScaledPoints()
	{
		float difficultyScale = 0;
		int bias = 0;

		if ( basePoolPoints == 0 )
			return 0;

		//set the base pool
		float poolCount = basePoolPoints;

		//set the difficulty bias
		if ( difficultyBias == DifficultyBias.Light )
			bias = 3;
		else if ( difficultyBias == DifficultyBias.Medium )
			bias = 5;
		else if ( difficultyBias == DifficultyBias.Heavy )
			bias = 7;

		//set the difficulty scale
		if ( Bootstrap.gameStarter.difficulty == Difficulty.Adventure )//easy
			difficultyScale = -.25f;
		else if ( Bootstrap.gameStarter.difficulty == Difficulty.Hard )//hard
			difficultyScale = .5f;

		//modify pool based on hero count above 1 and bias
		poolCount += ( Bootstrap.PlayerCount - 1 ) * bias;

		//modify pool based on difficulty scale
		poolCount += poolCount * difficultyScale;
		Debug.Log( "difficultyScale: " + difficultyScale );
		Debug.Log( "Scaled Pool Points: " + poolCount );
		return poolCount;
	}

	private int LowestRequestedEnemyCost()
	{
		int fCost = 1000;

		if ( includedEnemies == null )//backwards compatible
			return 1000;

		for ( int i = 0; i < includedEnemies.Length; i++ )
		{
			if ( includedEnemies[i] )
			{
				if (Monsters.Cost(i) < fCost )
				{
					fCost = Monsters.Cost(i);
				}
			}
		}

		return fCost;
	}
}