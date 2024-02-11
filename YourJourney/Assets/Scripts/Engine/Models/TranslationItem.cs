using System;

public class TranslationItem
{
	public Guid GUID;
	public string key;
	public string text;

	public TranslationItem(string key, string text)
    {
		this.key = key;
		this.text = text;
    }
}