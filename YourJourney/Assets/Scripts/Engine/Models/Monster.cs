﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using static LanguageManager;

/// <summary>
/// Models one GROUP of enemies (up to 3 enemies in a group)
/// </summary>
public class Monster
{
	public static readonly int MAX_MODIFIERS = 7;
	public static readonly int REASONABLE_MODIFIERS = 3;

	public Guid GUID;
	public int id;
	public int index;
	public int activationsId;
	public string dataName;
	public string enumName;
	public bool isEmpty;
	public string triggerName;

	//public string bonuses;
	public int health;
	public int shieldValue;
	public int sorceryValue;
	public int moveA;
	public int moveB;
	public string[] moveSpecial;
	public bool isRanged;
	public int groupLimit;
	public int figureLimit;
	public int[] cost;
	public string[] tag;
	public int damage;
	public bool isFearsome;
	public string[] special;
	public bool isLarge;
	public bool isBloodThirsty;
	public bool isArmored;
	public bool isElite;
	[JsonConverter(typeof(MonsterModifierListConverter))]
	public List<MonsterModifier> modifierList = new List<MonsterModifier>();
	public int randomizedModifiersCount;
	public bool hasBanner = false;
	public Ability negatedBy { get; set; }
	public MonsterType monsterType { get; set; }
	public int count;
	public int movementValue;
	public int loreReward;
	public bool defaultStats;
	public bool immuneCleave { get; set; } = false;
	public bool immuneLethal { get; set; } = false;
	public bool immunePierce { get; set; } = false;
	public bool immuneSmite { get; set; } = false;
	public bool immuneStun { get; set; } = false;
	public bool immuneSunder { get; set; } = false;

	[DefaultValue( true )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool isEasy { get; set; } //adventure mode
	[DefaultValue( true )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool isNormal { get; set; }
	[DefaultValue( true )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool isHard { get; set; }
	[JsonIgnore]
	public ThreatInteraction interaction;//the interaction that spawned this

	public static string[] monsterNames = { "Ruffian", "Goblin Scout", "Orc Hunter", "Orc Marauder", "Hungry Varg", "Hill Troll", "Wight",
											"Atari", "Gargletarg", "Chartooth",
											"Giant Spider", "Pit Goblin", "Orc Taskmaster", "Shadowman", "Anonymous Thing", "Cave Troll", "Balerock", "Spawn of Uglygiant",
											"Supplicant of More-Goth", "Ursula", "Oliver",
											"Foul Beast", "Varg Rider", "Siege Engine", "War Elephant", "Soldier", "High-Orc Warrior",
											"Lord Javelin", "Lich-King", "Endris"
	};

	public int[] currentHealth { get; set; } = new int[3];
	public int[] currentSunder { get; set; } = new int[3];
	public bool isDead;
	public bool isExhausted;
	public bool isStunned;
	//public int sunderValue;
	public int deathTally;
	public int deadCount;
	public float fCost;
	public float singlecost;

	/// <summary>
	/// returns # of monsters that are alive
	/// </summary>
	public int ActiveMonsterCount
	{
		get
		{
			int c = 0;
			for ( int i = 0; i < count; i++ )
			{
				if ( currentHealth[i] > 0 )
					c++;
				//	c += currentHealth[i];
			}
			return c;
		}
	}

	public Monster()
	{

	}

	public void UpdateModifiersAndElite()
    {
		if (isArmored && !modifierList.Contains(MonsterModifier.ARMORED)) { modifierList.Add(MonsterModifier.ARMORED); isArmored = false; }
		if (isBloodThirsty && !modifierList.Contains(MonsterModifier.BLOODTHIRSTY)) { modifierList.Add(MonsterModifier.BLOODTHIRSTY); isBloodThirsty = false; }
		if (isLarge && !modifierList.Contains(MonsterModifier.LARGE)) { modifierList.Add(MonsterModifier.LARGE); isLarge = false; }
		if (modifierList.Count > 0) { isElite = true; }
	}

	public void LoadCustomModifiers(ObservableCollection<MonsterModifier> customModifiers)
	{
		//The default JSON converter for MonsterModifier can't look at the scenario's list of custom MonsterModifiers. So we need to hydrate it when we load the Monster in the MonsterEditorWindow.
		for (int i = 0; i < modifierList.Count; i++)
		{
			if (modifierList[i].id >= MonsterModifier.START_OF_CUSTOM_MODIFIERS)
			{
				MonsterModifier modData = customModifiers.First(it => it.id == modifierList[i].id);
				if (modData != null)
				{
					modifierList[i] = modData;
				}
			}
		}
	}

	public bool AddModifier(MonsterModifier mod)
    {
		if (modifierList.Count < MAX_MODIFIERS && !modifierList.Contains(mod))
        {
			modifierList.Add(mod);
			return true;
        }
		return false;
    }

	public void RandomizeModifiers()
    {
		//If randomizedModifiersCount is set to a number greater than 0 and less than modifierList.Count, pick thta number of modifiers at random.

		if(randomizedModifiersCount == 0 || randomizedModifiersCount == modifierList.Count) { return; } //use all the modifiers

		MonsterModifier[] modifierArray = GlowEngine.RandomizeArray(modifierList.ToArray());
		modifierList.Clear();
		for(int i = 0; i < randomizedModifiersCount && i < modifierArray.Length; i++)
        {
			modifierList.Add(modifierArray[i]);
        }
		randomizedModifiersCount = 0; //Set to 0 so any future calls to this function will not change the modifierList.
    }

	public int CalculateExtraDamage()
	{
		int extra = 0;
		foreach (var mod in modifierList)
		{
			extra += mod.damage;
		}
		return extra;
	}

	public int CalculateExtraFear()
    {
		int extra = 0;
		foreach(var mod in modifierList)
        {
			extra += mod.fear;
        }
		return extra;
    }

	public static List<MonsterType> Goblins()
	{
		return new List<MonsterType> { MonsterType.GoblinScout, MonsterType.GoblinScout, MonsterType.VargRider };
	}

	public static List<MonsterType> Orcs()
	{
		return new List<MonsterType> { MonsterType.OrcHunter, MonsterType.OrcMarauder, MonsterType.OrcTaskmaster, MonsterType.HighOrcWarrior, MonsterType.Gargletarg, MonsterType.SupplicantOfMoreGoth, MonsterType.LordJavelin };
	}

	public static List<MonsterType> Humans()
	{
		return new List<MonsterType> { MonsterType.Ruffian, MonsterType.Soldier, MonsterType.Atari, MonsterType.Endris };
	}

	public static List<MonsterType> Spirits()
	{
		return new List<MonsterType> { MonsterType.Wight, MonsterType.Shadowman, MonsterType.Ursula, MonsterType.LichKing };
	}

	public static List<MonsterType> Trolls()
	{
		return new List<MonsterType> { MonsterType.CaveTroll, MonsterType.HillTroll, MonsterType.Oliver };
	}

	public static List<MonsterType> Vargs()
	{
		return new List<MonsterType> { MonsterType.HungryVarg, MonsterType.VargRider, MonsterType.Chartooth };
	}

	public static List<MonsterType> Spiders()
	{
		return new List<MonsterType> { MonsterType.GiantSpider, MonsterType.SpawnOfUglygiant };
	}

	public static List<MonsterType> Flying()
    {
		return new List<MonsterType> { MonsterType.Balerock, MonsterType.FoulBeast, MonsterType.LichKing };
	}

	public static List<MonsterType> OtherBeasts()
	{
		return new List<MonsterType> { MonsterType.WarElephant, MonsterType.AnonymousThing };
	}

	public static List<MonsterType> AllBeasts()
	{
		List<MonsterType> monsterList = new List<MonsterType>();
		monsterList.AddRange(Trolls());
		monsterList.AddRange(Vargs());
		monsterList.AddRange(Spiders());
		monsterList.AddRange(Flying());
		monsterList.AddRange(OtherBeasts());
		return monsterList;
	}

	public static List<MonsterType> Humanoid()
	{
		List<MonsterType> monsterList = new List<MonsterType>();
		monsterList.AddRange(Goblins());
		monsterList.AddRange(Orcs());
		monsterList.AddRange(Humans());
		return monsterList;
	}

	//returns true if this monster can appear in current difficulty
	public bool IsValid()
	{
		if ( Bootstrap.gameStarter.difficulty == Difficulty.Adventure && isEasy )
			return true;
		else if ( Bootstrap.gameStarter.difficulty == Difficulty.Normal && isNormal )
			return true;
		else if ( Bootstrap.gameStarter.difficulty == Difficulty.Hard && isHard )
			return true;

		return false;
	}

	//returns Tuple<fear,damage>
	public Tuple<int, int> CalculateDamage()
	{
		//calculate total and split it between damage and fear
		//modifier adds damage if active enemies in group > 1
		//if it's a heavy hitter, limit the modifier to +1
		int modifier = ActiveMonsterCount == 1 ? 0 : ( damage == 4 ? 1 : ActiveMonsterCount - 1 );
		int total = damage + modifier + UnityEngine.Random.Range( -1, 2 );
		Debug.Log( "ActiveMonsterCount: " + ActiveMonsterCount );
		Debug.Log( "modified damage: " + modifier );
		Debug.Log( "total damage: " + total );
		int d = UnityEngine.Random.Range( 0, total + 1 );
		int f = total - d;
		if ( d == 0 && f == 0 )
			d = 1;

		//If isFearsome is true, the Fear should always be greater than the damage
		if ( isFearsome )
		{
			int temp = f;
			if (d > f)
			{
				f = d;
				d = temp;
			}
		}
		//Otherwise, make damage greater than fear
		else
		{
			int temp = d;
			if (f > d)
			{
				d = f;
				f = temp;
			}
		}

		return new Tuple<int, int>( f, d );
	}

	public static string MonsterName(Monster m, int count)
	{
		string monsterKey = m.enumName;
        if (String.IsNullOrWhiteSpace(monsterKey)) { monsterKey = m.dataName; }
		if (count == 1)
		{
			return Translate("monster.single.name." + m.enumName, m.dataName);
		}
		else
		{
			return Translate("monster.plural.name." + m.enumName, m.dataName + "(s)");
		}
	}

	public static string MonsterNameAttacker(Monster m, int count)
	{
		//Get the custom name from the in-scenario translation for scripted enemies
		string customName = Interpret("event.enemy." + m.interaction.dataName + ".monster." + m.index + ".name", "");
		if (!String.IsNullOrEmpty(customName)) { return customName; }

		//Otherwise get the translation for the standard monster name
		string monsterKey = m.enumName;
		if (String.IsNullOrWhiteSpace(monsterKey)) { monsterKey = m.dataName; }
		if (count == 1)
		{
			return Translate("monster.single.attacker." + monsterKey, "A(n) " + m.dataName);
		}
		else
		{
			return Translate("monster.plural.attacker." + monsterKey, "The " + m.dataName + "(s)");
		}
	}

	public static string MonsterNameObject(Monster m, int count)
	{
		//Get the custom name from the in-scenario translation for scripted enemies
		string customName = Interpret("event.enemy." + m.interaction.dataName + ".monster." + m.index + ".name", "");
		if (!String.IsNullOrEmpty(customName)) { return customName; }

		//Otherwise get the translation for the standard monster name
		string monsterKey = m.enumName;
		if (String.IsNullOrWhiteSpace(monsterKey)) { monsterKey = m.dataName; }
		if (count == 1)
		{
			return Translate("monster.single.object." + monsterKey, Translate("monster.single.name." + monsterKey, m.dataName));
		}
		else
		{
			return Translate("monster.plural.object." + monsterKey, Translate("monster.plural.name." + monsterKey, m.dataName + "(s)"));
		}
	}

	public static Monster MonsterFactory( MonsterType mType )
	{
		//light=2, medium=3, heavy=4
		int mId, mHealth = 0, mArmor = 0, mSorcery = 0, mMoveA = 0, mMoveB = 0, mGroupLimit = 0, mFigureLimit = 0, mDamage = 0, mSpeed = 0;
		int[] mCost = new int[] { 1000, 0, 0 };
		bool mRanged = false, mFearsome = false;
#pragma warning disable CS0219 // Variable is assigned but its value is never used
		string mEnumName = "";
		string mDataName = "";
#pragma warning restore CS0219 // Variable is assigned but its value is never used
		string[] mMoveSpecial, mTag, mSpecial = new string[0];

		switch (mType)
		{
			//Core Set
			case MonsterType.Ruffian:
				mId = 0;
				mEnumName = "Ruffian";
				mDataName = "Ruffian";
				mHealth = 5;
				mArmor = 0;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 7, 13, 19 };
				mTag = new string[] { "Humanoid", "Weak" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.GoblinScout:
				mId = 1;
				mEnumName = "GoblinScout";
				mDataName = "Goblin Scout";
				mHealth = 3;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 4, 7, 11 };
				mTag = new string[] { "Goblin", "Weak", "Small" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.OrcHunter:
				mId = 2;
				mEnumName = "OrcHunter";
				mDataName = "Orc Hunter";
				mHealth = 5;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = true;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 10, 19, 28 };
				mTag = new string[] { "Orc", "Ranged" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.OrcMarauder:
				mId = 3;
				mEnumName = "OrcMarauder";
				mDataName = "Orc Marauder";
				mHealth = 5;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 9, 17, 25 };
				mTag = new string[] { "Orc", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.HungryVarg:
				mId = 4;
				mEnumName = "HungryVarg";
				mDataName = "Hungry Varg";
				mHealth = 8;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 14, 27, 40 };
				mTag = new string[] { "Beast", "Powerful", "Fast" };
				mSpeed = 3;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.HillTroll:
				mId = 5;
				mEnumName = "HillTroll";
				mDataName = "Hill Troll";
				mHealth = 14;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 25, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large" };
				mSpeed = 2;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Wight:
				mId = 6;
				mEnumName = "Wight";
				mDataName = "Wight";
				mHealth = 6;
				mArmor = 0;
				mSorcery = 3;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 17, 32, 47 };
				mTag = new string[] { "Humanoid", "Powerful" };
				mSpeed = 1;
				mDamage = 4;
				mFearsome = true;
				mSpecial = new string[] { };
				break;

			//Villains of Eriador
			case MonsterType.Atari:
				mId = 7;
				mEnumName = "Atari";
				mDataName = "Atari";
				mHealth = 8;
				mArmor = 1;
				mSorcery = 1;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 100, 0, 0 };
				mTag = new string[] { "Humanoid", "Powerful", "Small" };
				mSpeed = 2;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Gargletarg:
				mId = 8;
				mEnumName = "Gargletarg";
				mDataName = "Gargletarg";
				mHealth = 8;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 3;
				mMoveSpecial = new string[] { "Encumbered" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 100, 0, 0 };
				mTag = new string[] { "Orc", "Powerful" };
				mSpeed = 1;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Chartooth:
				mId = 9;
				mEnumName = "Chartooth";
				mDataName = "Chartooth";
				mHealth = 8;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { "Predatory" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 100, 0, 0 };
				mTag = new string[] { "Beast", "Fast" };
				mSpeed = 3;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;

			//Shadowed Paths
			case MonsterType.GiantSpider:
				mId = 10;
				mEnumName = "GiantSpider";
				mDataName = "Giant Spider";
				mHealth = 4;
				mArmor = 0;
				mSorcery = 0;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 5, 8, 12 };
				mTag = new string[] { "Beast", "Weak", "Small", "Fast" };
				mSpeed = 3;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.PitGoblin:
				mId = 11;
				mEnumName = "PitGoblin";
				mDataName = "Pit Goblin";
				mHealth = 3;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = true;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 4, 7, 11 };
				mTag = new string[] { "Goblin", "Weak", "Small", "Ranged" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.OrcTaskmaster:
				mId = 12;
				mEnumName = "OrcTaskmaster";
				mDataName = "Orc Taskmaster";
				mHealth = 7;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 14, 27, 40 };
				mTag = new string[] { "Orc", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Shadowman:
				mId = 13;
				mEnumName = "Shadowman";
				mDataName = "Shadowman";
				mHealth = 5;
				mArmor = 1;
				mSorcery = 2;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { "Incorporeal" };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 17, 32, 47 };
				mTag = new string[] { "Humanoid", "Powerful", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.AnonymousThing:
				mId = 14;
				mEnumName = "AnonymousThing";
				mDataName = "Anonymous Thing";
				mHealth = 20;
				mArmor = 0;
				mSorcery = 0;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { "Tunneler" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 3;
				mCost = new int[] { 27, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large", "Fast" };
				mSpeed = 3;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.CaveTroll:
				mId = 15;
				mEnumName = "CaveTroll";
				mDataName = "Cave Troll";
				mHealth = 10;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 2;
				mFigureLimit = 2;
				mCost = new int[] { 20, 40, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Balerock:
				mId = 16;
				mEnumName = "Balerock";
				mDataName = "Balerock";
				mHealth = 18;
				mArmor = 2;
				mSorcery = 2;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 100, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large", "Slow" };
				mSpeed = 1;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { "Cleave" };
				break;
			case MonsterType.SpawnOfUglygiant:
				mId = 17;
				mEnumName = "SpawnOfUglygiant";
				mDataName = "Spawn of Uglygiant";
				mHealth = 18;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 100, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large" };
				mSpeed = 2;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;

			//Dwellers in Darkness
			case MonsterType.SupplicantOfMoreGoth:
				mId = 18;
				mEnumName = "SupplicantOfMoreGoth";
				mDataName = "Supplicant of More-Goth";
				mHealth = 9;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 100, 0, 0 };
				mTag = new string[] { "Orc", "Powerful", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = true;
				mSpecial = new string[] { };
				break;
			case MonsterType.Ursula:
				mId = 19;
				mEnumName = "Ursula";
				mDataName = "Ursula";
				mHealth = 16;
				mArmor = 2;
				mSorcery = 4;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 0, 0, 0 };
				mTag = new string[] { "Humanoid", "Powerful", "Fast" };
				mSpeed = 3;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Oliver:
				mId = 20;
				mEnumName = "Oliver";
				mDataName = "Oliver";
				mHealth = 11;
				mArmor = 0;
				mSorcery = 1;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 100, 0, 0 };
				mTag = new string[] { "Beast", "Large", "Slow" };
				mSpeed = 1;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;

			//Spreading War
			case MonsterType.FoulBeast:
				mId = 21;
				mEnumName = "FoulBeast";
				mDataName = "Foul Beast";
				mHealth = 8;
				mArmor = 0;
				mSorcery = 2;
				mMoveA = 2;
				mMoveB = 2;
				mMoveSpecial = new string[] { "Flying" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 3;
				mCost = new int[] { 24, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large", "Fast" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.VargRider:
				mId = 22;
				mEnumName = "VargRider";
				mDataName = "Varg Rider";
				mHealth = 10;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { };
				mRanged = true;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 14, 27, 40 };
				mTag = new string[] { "Goblin", "Beast", "Ranged" };
				mSpeed = 3;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.SiegeEngine:
				mId = 23;
				mEnumName = "SiegeEngine";
				mDataName = "Siege Engine";
				mHealth = 7;
				mArmor = 4;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 1;
				mMoveSpecial = new string[] { "Reinforcements" };
				mRanged = true;
				mGroupLimit = 1;
				mFigureLimit = 2;
				mCost = new int[] { 22, 0, 0 };
				mTag = new string[] { "Powerful", "Large", "Ranged", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.WarElephant:
				mId = 24;
				mEnumName = "WarElephant";
				mDataName = "War Elephant";
				mHealth = 10;
				mArmor = 4;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 3;
				mMoveSpecial = new string[] { "Stampede" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 30, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large" };
				mSpeed = 2;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Soldier:
				mId = 25;
				mEnumName = "Soldier";
				mDataName = "Soldier";
				mHealth = 6;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 8, 14, 20 };
				mTag = new string[] { "Humanoid", "Weak" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.HighOrcWarrior:
				mId = 26;
				mEnumName = "HighOrcWarrior";
				mDataName = "High-Orc Warrior";
				mHealth = 7;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 11, 19, 27 };
				mTag = new string[] { "Orc", "Fast" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;

			//Scourges of the Wastes
			case MonsterType.LordJavelin:
				mId = 27;
				mEnumName = "LordJavelin";
				mDataName = "Lord Javelin";
				mHealth = 12;
				mArmor = 2;
				mSorcery = 2;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 100, 0, 0 };
				mTag = new string[] { "Humanoid", "Powerful" };
				mSpeed = 2;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.LichKing:
				mId = 28;
				mEnumName = "LichKing";
				mDataName = "Lich-King";
				mHealth = 20;
				mArmor = 4;
				mSorcery = 4;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { "Flying" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 100, 0, 0 };
				mTag = new string[] { "Humanoid", "Beast", "Powerful", "Fast", "Flying" };
				mSpeed = 3;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Endris:
				mId = 29;
				mEnumName = "Endris";
				mDataName = "Endris";
				mHealth = 10;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 100, 0, 0 };
				mTag = new string[] { "Humanoid", "Small" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;

			default:
				mId = 0;
				mHealth = 5;
				mArmor = 0;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 7, 13, 19 };
				mTag = new string[] { "Humanoid", "Weak" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
		}

		return new Monster()
		{
			//dataName = monsterNames[(int)mType],
			id = mId,
			activationsId = mId,
			dataName = mDataName,
			enumName = mEnumName,
			monsterType = mType,
			GUID = Guid.NewGuid(),
			health = mHealth,
			shieldValue = mArmor,
			sorceryValue = mSorcery,
			moveA = mMoveA,
			moveB = mMoveB,
			moveSpecial = mSpecial,
			isRanged = mRanged,
			groupLimit = mGroupLimit,
			figureLimit = mFigureLimit,
			cost = mCost,
			movementValue = mSpeed,
			damage = mDamage,
			special = mSpecial,
			isFearsome = mFearsome,
			triggerName = "None",
			singlecost = mCost[0],
			isEasy = true,
			isNormal = true,
			isHard = true,
			negatedBy = Ability.Might
		};
	}
}
