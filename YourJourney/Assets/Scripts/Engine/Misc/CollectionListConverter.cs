using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CollectionListConverter : JsonConverter
{
	public override bool CanWrite => true;
	public override bool CanRead => true;
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(string);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		List<Collection> collectionList = (List<Collection>)value;
		/*
		List<string> nameList = new List<string>();
		collectionList.ForEach(it => nameList.Add(it.Name));
		JToken t = JToken.FromObject(nameList);
		*/
		List<int> idList = new List<int>();
		collectionList.ForEach(it => idList.Add(it.ID));
		JToken t = JToken.FromObject(idList);
		t.WriteTo(writer);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var jsonObject = JArray.Load(reader);
		Collection collection = null;
		/*
		List<string> nameList = new List<string>();
		List<Collection> collectionList = new List<Collection>();

		foreach ( var item in jsonObject )
		{
			collection = Collection.FromName(item.Value<string>());
			collectionList.Add( collection );
		}
		*/
		List<int> idList = new List<int>();
		List<Collection> collectionList = new List<Collection>();

		foreach (var item in jsonObject)
		{
			//Debug.Log("CollectionConverter ReadJson <= " + item);
			collection = Collection.FromID(item.Value<int>());
			collectionList.Add(collection);
		}

		return collectionList;
	}
}

