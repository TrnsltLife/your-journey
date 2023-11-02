using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

internal class MonsterModifierListConverter : JsonConverter
{
	public override bool CanWrite => true;
	public override bool CanRead => true;
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(MonsterModifier);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		//Write this out as a list of MonsterModifier because we write (and read) the full MonsterModifier objects when we save a game with GameState.
		List<MonsterModifier> modifierList = (List<MonsterModifier>)value;
		JToken t = JToken.FromObject(modifierList);
		t.WriteTo(writer);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//We read in either:
		// 1. an integer that will be converted to a MonsterModifier (from the Scenario file)
		// 2. a full MonsterModifier object (from a saved GameState)
		var jsonObject = JArray.Load(reader);
		MonsterModifier modifier = null;
		List<MonsterModifier> modifierList = new List<MonsterModifier>();

		foreach (var item in jsonObject)
		{
			try
			{
				//Convert from a full MonsterModifier when loading a saved game from GameState
				modifier = (MonsterModifier)item.ToObject(typeof(MonsterModifier));
			}
			catch (Exception e)
			{
				//Convert from an int when loading a new Scenario. Default modifiers will be hydrated from MonsterModifier.FromID. Custom modifiers will be hydrated later in Engine.
				modifier = MonsterModifier.FromID(item.Value<int>());
			}

			if (modifier != null)
			{
				modifierList.Add(modifier);
			}
		}

		return modifierList;
	}
}