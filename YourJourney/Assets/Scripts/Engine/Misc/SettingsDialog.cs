using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class SettingsDialog : MonoBehaviour
{
	public CanvasGroup settingsCanvasGroup;
	public Toggle musicToggle, vignetteToggle, colorToggle, fullscreenToggle;
	public PostProcessVolume volume;
	public AudioSource musicSource;
	public Text buttonText;
	public TMP_Dropdown resolutionDropdown;
	public TMP_Dropdown skinpackDropdown;
    public TMP_Dropdown tileTexturePackDropdown;
    public TMP_Dropdown languageDropdown;

	RectTransform rect;
	Vector2 ap;
	Vector3 sp;
	Action quitAction;
	Action<string> skinUpdateAction;
	Action<string> tileTextureUpdateAction;
	Action<string> languageUpdateAction;
	Resolution[] resolutions;
	List<TMP_Dropdown.OptionData> resolutionList;

	List<string> skinpackList;
	List<TMP_Dropdown.OptionData> skinpackDropdownList;

	List<string> tileTexturePackList;
	List<TMP_Dropdown.OptionData> tileTexturePackDropdownList;


    List<LanguageManager.TranslationFileEntry> languageList;
	List<TMP_Dropdown.OptionData> languageDropdownList;

	public static string defaultSkinpack = "*Your Journey*";
	public static string defaultTileTexturePack = "*Your Journey*";
    public static string defaultSkinpack2 = "*Your Journey 2*";
	public static string defaultLanguage = "English";


	void Awake()
	{
		CalculateDialogPosition();
	}

	public void Show( string bTextKey, string bDefaultText, Action<string> languageUpdateAction, Action action = null, Action<string> skinUpdateAction = null, Action<string> tileTextureUpdateAction = null )
	{
		CalculateDialogPosition();

		quitAction = action;
		this.skinUpdateAction = skinUpdateAction;
		this.tileTextureUpdateAction = tileTextureUpdateAction;
        this.languageUpdateAction = languageUpdateAction;
		//buttonText.text = bText;
		buttonText.GetComponent<TextTranslation>()?.Change(bTextKey, bDefaultText);
		settingsCanvasGroup.alpha = 0;
		settingsCanvasGroup.gameObject.SetActive( true );
		settingsCanvasGroup.DOFade( 1, .5f );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		settingsCanvasGroup.gameObject.transform.DOMoveY( sp.y, .75f );

		//populate checkboxes
		var settings = Bootstrap.LoadSettings();
        musicToggle.isOn = settings.music == 1;
        vignetteToggle.isOn = settings.vignette == 1;
        colorToggle.isOn = settings.color == 1;
        fullscreenToggle.isOn = settings.fullscreen == 1;

        //populate resolutions dropdown
        Resolution savedResolution = new Resolution();
        savedResolution.width = settings.width;
        savedResolution.height = settings.height;

		resolutionList = new List<TMP_Dropdown.OptionData>();

#if UNITY_ANDROID && !UNITY_EDITOR
		//Android doesn't return a Screen.resolutions list properly, but it can get the default resolution from Display.main
        Resolution androidResolution = new Resolution();
        androidResolution.width = Display.main.systemWidth;
        androidResolution.height = Display.main.systemHeight;
		resolutions = new Resolution[1] { androidResolution };
        resolutionList.Add(new TMP_Dropdown.OptionData(androidResolution.width + "x" + androidResolution.height));
#else
		//This gets a list of resolutions on desktop OSes
        resolutions = Screen.resolutions;
#endif

        int selectedIndex = 0;
        foreach (var res in resolutions)
        {
            resolutionList.Add(new TMP_Dropdown.OptionData(res.width + "x" + res.height));
            if (res.width == savedResolution.width && res.height == savedResolution.height)
            {
                selectedIndex = resolutionList.Count - 1;
            }
        }


        resolutionDropdown.ClearOptions();
		resolutionDropdown.AddOptions(resolutionList);
		resolutionDropdown.SetValueWithoutNotify(selectedIndex);

        //populate skinpack dropdown
        string savedSkinpack = settings.skinpack;
        skinpackList = SkinsManager.LoadSkinpackDirectories();
		skinpackDropdownList = new List<TMP_Dropdown.OptionData>();
		int selectedSkinpackIndex = 0;

		skinpackDropdownList.Add(new TMP_Dropdown.OptionData(defaultSkinpack));
		if(savedSkinpack == defaultSkinpack) { selectedSkinpackIndex = 0; }

		skinpackDropdownList.Add(new TMP_Dropdown.OptionData(defaultSkinpack2));
		if(savedSkinpack == defaultSkinpack2) { selectedSkinpackIndex = 1; }

		foreach (var skinpack in skinpackList)
        {
			skinpackDropdownList.Add(new TMP_Dropdown.OptionData(skinpack));
			if(skinpack == savedSkinpack)
            {
				selectedSkinpackIndex = skinpackDropdownList.Count - 1;
            }
        }
		skinpackDropdown.ClearOptions();
		skinpackDropdown.AddOptions(skinpackDropdownList);
		skinpackDropdown.SetValueWithoutNotify(selectedSkinpackIndex);

        //populate tileTexturePack dropdown
        string savedtileTexturePack = settings.tileTexturePack;
        tileTexturePackList = TileTextureManager.LoadTileTexturePackDirectories();
        tileTexturePackDropdownList = new List<TMP_Dropdown.OptionData>();
        int selectedtileTexturePackIndex = 0;

        tileTexturePackDropdownList.Add(new TMP_Dropdown.OptionData(defaultTileTexturePack));
        if (savedtileTexturePack == defaultTileTexturePack) { selectedtileTexturePackIndex = 0; }

        foreach (var tileTexturePack in tileTexturePackList)
        {
            tileTexturePackDropdownList.Add(new TMP_Dropdown.OptionData(tileTexturePack));
            if (tileTexturePack == savedtileTexturePack)
            {
                selectedtileTexturePackIndex = tileTexturePackDropdownList.Count - 1;
            }
        }
        tileTexturePackDropdown.ClearOptions();
        tileTexturePackDropdown.AddOptions(tileTexturePackDropdownList);
        tileTexturePackDropdown.SetValueWithoutNotify(selectedtileTexturePackIndex);

        //populate language dropdown
        string savedLanguage = settings.language;
        //Debug.Log("Saved language is: " + savedLanguage);
        languageList = LanguageManager.DiscoverLanguageFiles();
		languageDropdownList = new List<TMP_Dropdown.OptionData>();
		int selectedLanguageIndex = 0;
		//languageDropdownList.Add(new TMP_Dropdown.OptionData(defaultLanguage));
		foreach (var language in languageList)
		{
			//string languageName = LanguageManager.LanguageNameFromFilename(language);
			string languageName = language.languageName;
			languageDropdownList.Add(new TMP_Dropdown.OptionData(languageName));
			//Debug.Log("Compare " + languageName + " to " + savedLanguage + " with List.Count " + languageDropdownList.Count);
			if (languageName == savedLanguage)
			{
				selectedLanguageIndex = languageDropdownList.Count - 1;
			}
		}
		languageDropdown.ClearOptions();
		languageDropdown.AddOptions(languageDropdownList);
		languageDropdown.SetValueWithoutNotify(selectedLanguageIndex);
	}

	public void OnClose()
	{
		//save settings
		Settings settings = new Settings
		{
            music = musicToggle.isOn ? 1 : 0,
            vignette = vignetteToggle.isOn ? 1 : 0,
            color = colorToggle.isOn ? 1 : 0,
            width = GetSelectedResolution().width,
            height = GetSelectedResolution().height,
            fullscreen = fullscreenToggle.isOn ? 1 : 0,
            skinpack = GetSelectedSkinpack(),
            language = GetSelectedLanguage(), 
			tileTexturePack = GetSelectedTileTexturePack()
		};
        Bootstrap.SaveSettings(settings);

        settingsCanvasGroup.DOFade( 0, .25f ).OnComplete( () =>
		{
			settingsCanvasGroup.gameObject.SetActive( false );
		} );
    }

    public void OnQuit()
	{
        //Debug.Log("OnQuit");
        //save settings
        Settings settings = new Settings
        {
            music = musicToggle.isOn ? 1 : 0,
            vignette = vignetteToggle.isOn ? 1 : 0,
            color = colorToggle.isOn ? 1 : 0,
            width = GetSelectedResolution().width,
            height = GetSelectedResolution().height,
            fullscreen = fullscreenToggle.isOn ? 1 : 0,
            skinpack = GetSelectedSkinpack(),
            language = GetSelectedLanguage(),
            tileTexturePack = GetSelectedTileTexturePack()
        };
        Bootstrap.SaveSettings(settings);

        if (quitAction != null)
		{
            //Debug.Log("Quit Action");
            settingsCanvasGroup.DOFade(0, .25f).OnComplete(() =>
			{
				settingsCanvasGroup.gameObject.SetActive(false);
				quitAction();
			});
		}
		else
		{
            //Debug.Log("Quit App");
            Application.Quit();
		}
	}

	public void OnMusic()
	{
		musicSource.enabled = musicToggle.isOn;
		//only start music if it wasn't already playing
		if ( musicSource.enabled )
			musicSource.Play();
	}

	public void OnVignette()
	{
		Vignette v;
		if ( volume.profile.TryGetSettings( out v ) )
			v.active = vignetteToggle.isOn;
	}

	public void OnColor()
	{
		ColorGrading cg;
		if ( volume.profile.TryGetSettings( out cg ) )
			cg.active = colorToggle.isOn;
	}

	public void OnFullscreen()
    {
		Screen.fullScreen = fullscreenToggle.isOn;
	}

	public void OnResolution()
    {
		Resolution res = GetSelectedResolution();
		if(res.width == 0 || res.height == 0)
        {
            //Invalid resolution selected, not changing resolution.
            return;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
		//Don't try to change the resolution on Android, it breaks things
		Screen.fullScreen = fullscreenToggle.isOn;
#else
        Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn);
#endif
		CalculateDialogPosition();
	}

	public void OnSkinpack()
    {
		//Debug.Log("SettingsDialog.OnSkinpack()");
		string skinpack = GetSelectedSkinpack();
		if(skinUpdateAction != null)
        {
			//Debug.Log("skinUpdateAction()");
			skinUpdateAction(skinpack);
        }
    }

    public void OnTileTexturePack()
    {
        //Debug.Log("SettingsDialog.OnTileTexturePack()");
        string tileTexturePack = GetSelectedTileTexturePack();
        if (tileTextureUpdateAction != null)
        {
            //Debug.Log("skinUpdateAction()");
            tileTextureUpdateAction(tileTexturePack);
        }
    }

    public void OnLanguage()
	{
		//Debug.Log("SettingsDialog.OnLanguage()");
		string language = GetSelectedLanguage();
		if (languageUpdateAction != null)
		{
			//Debug.Log("languageUpdateAction()");
			languageUpdateAction(language);
		}
	}

	private Resolution GetSelectedResolution()
    {
        int index = resolutionDropdown.GetComponent<TMP_Dropdown>().value;
        Resolution res = new Resolution();
		try
		{
			string[] resString = resolutionDropdown.options[index].text.Split('x');
			if (resString.Length == 2)
			{
				res.width = Int32.Parse(resString[0]);
				res.height = Int32.Parse(resString[1]);
			}
		}
		catch (Exception e) { }
        return res;
	}

	private string GetSelectedSkinpack()
    {
		int index = skinpackDropdown.GetComponent<TMP_Dropdown>().value;
		return skinpackDropdown.options[index].text;
	}

	private string GetSelectedLanguage()
	{
		int index = languageDropdown.GetComponent<TMP_Dropdown>().value;
		return languageDropdown.options[index].text;
	}

    private string GetSelectedTileTexturePack()
    {
        int index = tileTexturePackDropdown.GetComponent<TMP_Dropdown>().value;
        return tileTexturePackDropdown.options[index].text;
    }

    private void CalculateDialogPosition()
    {
		rect = settingsCanvasGroup.gameObject.GetComponent<RectTransform>();
		ap = rect.anchoredPosition;
		sp = settingsCanvasGroup.gameObject.transform.position;
	}
}
