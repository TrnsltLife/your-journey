using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TextTranslation : MonoBehaviour
{
    TMP_Text TMPTextObject;
    Text TextObject;
	public string TextKey;
    public string DefaultText;
    List<string> Values;
    bool translationEnabled = true;

	void Start()
    {
        TMPTextObject = GetComponent<TMP_Text>();
        TextObject = GetComponent<Text>();
        LanguageManager.AddSubscriber(OnUpdateTranslation);
        OnUpdateTranslation();
    }

    public void TranslationEnabled(bool enabled) { translationEnabled = enabled; }
    public bool IsTranslationEnabled() { return translationEnabled; }

    public void Change(string key)
    {
        TextKey = key;
        OnUpdateTranslation();
    }

    public void Change(string key, string defaultText)
    {
        TextKey = key;
        DefaultText = defaultText;
        OnUpdateTranslation();
    }

    public void Change(List<string> values)
    {
        Values = values;
        OnUpdateTranslation();
    }

    public void Change(string key, List<string> values)
    {
        TextKey = key;
        Values = values;
        OnUpdateTranslation();
    }

    public void Change(string key, string defaultText, List<string> values)
    {
        TextKey = key;
        DefaultText = defaultText;
        Values = values;
        OnUpdateTranslation();
    }

    public void OnUpdateTranslation()
    {
        if (!translationEnabled) { return; }

        if (!string.IsNullOrWhiteSpace(TextKey))
        {
            TextKey = TextKey.Trim();
        }

        string translation = LanguageManager.Translate(TextKey, DefaultText);
        if (Values != null && Values.Count > 0)
        {
            translation = string.Format(translation, Values.ToArray());
        }

        if (TextObject != null)
        {
            TextObject.text = translation;
        }

        if (TMPTextObject != null)
        {
            TMPTextObject.text = translation;
        }
    }

    void OnDestroy()
    {
        LanguageManager.RemoveSubscriber(OnUpdateTranslation);
    }
}
