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
		List<MonsterModifier> modifierList = (List<MonsterModifier>)value;
		/*
		List<int> idList = new List<int>();
		foreach (MonsterModifier modifier in modifierList)
		{
			idList.Add(modifier.id);
		}
		JToken t = JToken.FromObject(idList);
		*/
		JToken t = JToken.FromObject(modifierList);
		t.WriteTo(writer);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var jsonObject = JArray.Load(reader);
		MonsterModifier modifier = null;
		List<MonsterModifier> modifierList = new List<MonsterModifier>();

		foreach (var item in jsonObject)
		{
			Debug.Log("Try parsing MonsterModifier");
			Debug.Log(item);
			try
			{
				//Convert from a full MonsterModifier when loading a saved game
				//modifier = item.Value<MonsterModifier>();
				modifier = (MonsterModifier)item.ToObject(typeof(MonsterModifier));
				Debug.Log("Loaded MonsterModifier " + modifier.name);
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
				//Convert from an int when loading a Scenario. Custom modifiers will be hydrated later
				modifier = MonsterModifier.FromID(item.Value<int>());
				Debug.Log("Loaded MonsterModifier as int " + modifier.name);
			}

			if (modifier != null)
			{
				modifierList.Add(modifier);
			}
		}

		return modifierList;
	}
}