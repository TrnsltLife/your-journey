using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/*
https://forum.unity.com/threads/replacing-text-with-textmesh-pro.515594/#:~:text=Setup%3A%20Project%20Settings%20%3E%20TextMeshPro%20%3E%20Settings%20%3E,%3E%20Text%20To%20TextMeshPro%20Code%20%28CSharp%29%3A%20using%20UnityEngine%3B
Setup: Project Settings > TextMeshPro > Settings > Default Font Asset (Set to the TMP font you want to use)

Usage: 1) Select Text(s) in Hierarchy 2) Select: Tools > Text To TextMeshPro

- Full UNDO support
- The converter can now process game objects without a Text component without errors. (it just ignores those objects)
- An option to recursively process all the children of the selected objects. (even disabled ones if wanted)
- An option to remove obsolete Text UI effects components. (TextMeshPro uses materials instead for doing these effects)

So now, with the new options, you can just select all the root objects of a scene and the utility will convert all Text UI components in it to TextMeshPro without breaking a sweat.
Ideal for upgrading whole scenes quickly.
*/

/*
public class TextToTextMeshPro : Editor
{
    public class TextMeshProSettings
    {
        public bool Enabled;
        public FontStyles FontStyle;
        public float FontSize;
        public float FontSizeMin;
        public float FontSizeMax;
        public float LineSpacing;
        public bool EnableRichText;
        public bool EnableAutoSizing;
        public TextAlignmentOptions TextAlignmentOptions;
        public bool WrappingEnabled;
        public TextOverflowModes TextOverflowModes;
        public string Text;
        public Color Color;
        public bool RayCastTarget;
    }

    static private bool recursive = false;
    static private bool applyOnDisabled = false;
    static private bool removeEffectComponents = false;

    [MenuItem("Tools/Text To TextMeshPro", false, 4000)]
    static void DoIt()
    {
        if (TMPro.TMP_Settings.defaultFontAsset == null)
        {
            EditorUtility.DisplayDialog("ERROR!", "Assign a default font asset in project settings!", "OK", "");
            return;
        }

        recursive = EditorUtility.DisplayDialog("Text To TextMeshPro", "Process all the children of the selected objects recursively?", "Yes", "No");
        if (recursive) applyOnDisabled = EditorUtility.DisplayDialog("Text To TextMeshPro", "Process disabled objects too?", "Yes", "No");

        removeEffectComponents = EditorUtility.DisplayDialog("Text To TextMeshPro", "Remove obsolete component effects Shadow and Outline?\n\nThis components have no effect on TextMeshPro and can be safely removed.\nTo apply shadows and outlines on TextMeshPro components, use different materials instead.", "Yes", "No");

        foreach (GameObject gameObject in Selection.gameObjects)
        {
            if (recursive)
            {
                var childs = gameObject.GetComponentsInChildren<Transform>(applyOnDisabled);
                foreach (var ch in childs)
                    ConvertTextToTextMeshPro(ch.gameObject);
            }
            else
                ConvertTextToTextMeshPro(gameObject);
        }
    }

    static void ConvertTextToTextMeshPro(GameObject target)
    {
        if (removeEffectComponents)
        {
            var sha = target.GetComponents<UnityEngine.UI.Shadow>();
            foreach (var cmp in sha) Undo.DestroyObjectImmediate(cmp);
            var outl = target.GetComponents<UnityEngine.UI.Outline>();
            foreach (var cmp in outl) Undo.DestroyObjectImmediate(cmp);
        }

        Text uiText = target.GetComponent<Text>();
        if (uiText == null) return;


        TextMeshProSettings settings = GetTextMeshProSettings(uiText);

        Undo.RecordObject(target, "Text To TextMeshPro");
        Undo.DestroyObjectImmediate(uiText);

        TextMeshProUGUI tmp = target.AddComponent<TextMeshProUGUI>();
        tmp.enabled = settings.Enabled;
        tmp.fontStyle = settings.FontStyle;
        tmp.fontSize = settings.FontSize;
        tmp.fontSizeMin = settings.FontSizeMin;
        tmp.fontSizeMax = settings.FontSizeMax;
        tmp.lineSpacing = settings.LineSpacing;
        tmp.richText = settings.EnableRichText;
        tmp.enableAutoSizing = settings.EnableAutoSizing;
        tmp.alignment = settings.TextAlignmentOptions;
        tmp.enableWordWrapping = settings.WrappingEnabled;
        tmp.overflowMode = settings.TextOverflowModes;
        tmp.text = settings.Text;
        tmp.color = settings.Color;
        tmp.raycastTarget = settings.RayCastTarget;

        Debug.Log(target.name + " converted to TextMeshProUGUI");
    }

    static TextMeshProSettings GetTextMeshProSettings(Text uiText)
    {
        if (uiText == null)
        {
            EditorUtility.DisplayDialog("ERROR!", "You must select a Unity UI Text Object to convert.", "OK", "");
            return null;
        }

        return new TextMeshProSettings
        {
            Enabled = uiText.enabled,
            FontStyle = FontStyleToFontStyles(uiText.fontStyle),
            FontSize = uiText.fontSize,
            FontSizeMin = uiText.resizeTextMinSize,
            FontSizeMax = uiText.resizeTextMaxSize,
            LineSpacing = uiText.lineSpacing,
            EnableRichText = uiText.supportRichText,
            EnableAutoSizing = uiText.resizeTextForBestFit,
            TextAlignmentOptions = TextAnchorToTextAlignmentOptions(uiText.alignment),
            WrappingEnabled = HorizontalWrapModeToBool(uiText.horizontalOverflow),
            TextOverflowModes = VerticalWrapModeToTextOverflowModes(uiText.verticalOverflow),
            Text = uiText.text,
            Color = uiText.color,
            RayCastTarget = uiText.raycastTarget
        };
    }

    static bool HorizontalWrapModeToBool(HorizontalWrapMode overflow)
    {
        return overflow == HorizontalWrapMode.Wrap;
    }

    static TextOverflowModes VerticalWrapModeToTextOverflowModes(VerticalWrapMode verticalOverflow)
    {
        return verticalOverflow == VerticalWrapMode.Truncate ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
    }

    static FontStyles FontStyleToFontStyles(FontStyle fontStyle)
    {
        switch (fontStyle)
        {
            case FontStyle.Normal:
                return FontStyles.Normal;

            case FontStyle.Bold:
                return FontStyles.Bold;

            case FontStyle.Italic:
                return FontStyles.Italic;

            case FontStyle.BoldAndItalic:
                return FontStyles.Bold | FontStyles.Italic;
        }

        Debug.LogWarning("Unhandled font style " + fontStyle);
        return FontStyles.Normal;
    }

    static TextAlignmentOptions TextAnchorToTextAlignmentOptions(TextAnchor textAnchor)
    {
        switch (textAnchor)
        {
            case TextAnchor.UpperLeft:
                return TextAlignmentOptions.TopLeft;

            case TextAnchor.UpperCenter:
                return TextAlignmentOptions.Top;

            case TextAnchor.UpperRight:
                return TextAlignmentOptions.TopRight;

            case TextAnchor.MiddleLeft:
                return TextAlignmentOptions.Left;

            case TextAnchor.MiddleCenter:
                return TextAlignmentOptions.Center;

            case TextAnchor.MiddleRight:
                return TextAlignmentOptions.Right;

            case TextAnchor.LowerLeft:
                return TextAlignmentOptions.BottomLeft;

            case TextAnchor.LowerCenter:
                return TextAlignmentOptions.Bottom;

            case TextAnchor.LowerRight:
                return TextAlignmentOptions.BottomRight;
        }

        Debug.LogWarning("Unhandled text anchor " + textAnchor);
        return TextAlignmentOptions.TopLeft;
    }
}
*/