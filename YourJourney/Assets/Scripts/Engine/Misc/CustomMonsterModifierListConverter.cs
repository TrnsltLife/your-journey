using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class CustomMonsterModifierListConverter: JsonConverter
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
		List<MonsterModifier> filteredList = new List<MonsterModifier>();
		modifierList.FindAll(it => it.id >= MonsterModifier.START_OF_CUSTOM_MODIFIERS).ForEach(it => filteredList.Add(it));
		JToken t = JToken.FromObject(filteredList);
		t.WriteTo(writer);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var jsonObject = JArray.Load(reader);
		MonsterModifier modifier = null;

		List<MonsterModifier> convertedList = new List<MonsterModifier>();

		foreach (var item in jsonObject)
		{
			modifier = item.ToObject<MonsterModifier>();
			convertedList.Add(modifier);
		}

		return convertedList;
	}
}