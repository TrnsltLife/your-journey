using System;
using System.Collections.Generic;
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
	public TMP_Dropdown languageDropdown;

	RectTransform rect;
	Vector2 ap;
	Vector3 sp;
	Action quitAction;
	Action<string> skinUpdateAction;
	Action<string> languageUpdateAction;
	Resolution[] resolutions;
	List<TMP_Dropdown.OptionData> resolutionList;

	List<string> skinpackList;
	List<TMP_Dropdown.OptionData> skinpackDropdownList;

	List<LanguageManager.TranslationFileEntry> languageList;
	List<TMP_Dropdown.OptionData> languageDropdownList;

	public static string defaultSkinpack = "*Your Journey*";
	public static string defaultSkinpack2 = "*Your Journey 2*";
	public static string defaultLanguage = "English";


	void Awake()
	{
		CalculateDialogPosition();
	}

	public void Show( string bTextKey, string bDefaultText, Action<string> languageUpdateAction, Action action = null, Action<string> skinUpdateAction = null )
	{
		CalculateDialogPosition();

		quitAction = action;
		this.skinUpdateAction = skinUpdateAction;
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
		musicToggle.isOn = settings.Item1 == 1;
		vignetteToggle.isOn = settings.Item2 == 1;
		colorToggle.isOn = settings.Item3 == 1;
		fullscreenToggle.isOn = settings.Item6 == 1;

		//populate resolutions dropdown
		Resolution savedResolution = new Resolution();
		savedResolution.width = settings.Item4;
		savedResolution.height = settings.Item5;
		resolutions = Screen.resolutions;
		resolutionList = new List<TMP_Dropdown.OptionData>();
		int selectedIndex = 0;
		foreach (var res in resolutions)
		{
			resolutionList.Add(new TMP_Dropdown.OptionData(res.width + "x" + res.height));
			if(res.width == savedResolution.width && res.height == savedResolution.height)
            {
				selectedIndex = resolutionList.Count - 1;
            }
		}
		resolutionDropdown.ClearOptions();
		resolutionDropdown.AddOptions(resolutionList);
		resolutionDropdown.SetValueWithoutNotify(selectedIndex);

		//populate skinpack dropdown
		string savedSkinpack = settings.Item7;
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

		//populate language dropdown
		string savedLanguage = settings.Rest.Item1;
		Debug.Log("Saved language is: " + savedLanguage);
		languageList = LanguageManager.DiscoverLanguageFiles();
		languageDropdownList = new List<TMP_Dropdown.OptionData>();
		int selectedLanguageIndex = 0;
		//languageDropdownList.Add(new TMP_Dropdown.OptionData(defaultLanguage));
		foreach (var language in languageList)
		{
			//string languageName = LanguageManager.LanguageNameFromFilename(language);
			string languageName = language.languageName;
			languageDropdownList.Add(new TMP_Dropdown.OptionData(languageName));
			Debug.Log("Compare " + languageName + " to " + savedLanguage + " with List.Count " + languageDropdownList.Count);
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
		Bootstrap.SaveSettings( new Tuple<int, int, int, int, int, int, string, Tuple<string>>(
			musicToggle.isOn ? 1 : 0,
			vignetteToggle.isOn ? 1 : 0,
			colorToggle.isOn ? 1 : 0,
			GetSelectedResolution().width,
			GetSelectedResolution().height,
			fullscreenToggle.isOn ? 1 : 0,
			GetSelectedSkinpack(),
			new Tuple<string>(GetSelectedLanguage())));

		settingsCanvasGroup.DOFade( 0, .25f ).OnComplete( () =>
		{
			settingsCanvasGroup.gameObject.SetActive( false );
		} );
	}

	public void OnQuit()
	{
		Debug.Log("OnQuit");
		//save settings
		Bootstrap.SaveSettings( new Tuple<int, int, int, int, int, int, string, Tuple<string>>(
			musicToggle.isOn ? 1 : 0,
			vignetteToggle.isOn ? 1 : 0,
			colorToggle.isOn ? 1 : 0,
			GetSelectedResolution().width,
			GetSelectedResolution().height,
			fullscreenToggle.isOn ? 1 : 0,
			GetSelectedSkinpack(),
			new Tuple<string>(GetSelectedLanguage())
			));

		if (quitAction != null)
		{
			Debug.Log("Quit Action");
			settingsCanvasGroup.DOFade(0, .25f).OnComplete(() =>
			{
				settingsCanvasGroup.gameObject.SetActive(false);
				quitAction();
			});
		}
		else
		{
			Debug.Log("Quit App");
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
		Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn);
		CalculateDialogPosition();
	}

	public void OnSkinpack()
    {
		Debug.Log("SettingsDialog.OnSkinpack()");
		string skinpack = GetSelectedSkinpack();
		if(skinUpdateAction != null)
        {
			Debug.Log("skinUpdateAction()");
			skinUpdateAction(skinpack);
        }
    }

	public void OnLanguage()
	{
		Debug.Log("SettingsDialog.OnLanguage()");
		string language = GetSelectedLanguage();
		if (languageUpdateAction != null)
		{
			Debug.Log("languageUpdateAction()");
			languageUpdateAction(language);
		}
	}

	private Resolution GetSelectedResolution()
    {
		int index = resolutionDropdown.GetComponent<TMP_Dropdown>().value;
		string[] resString = resolutionDropdown.options[index].text.Split('x');
		Resolution res = new Resolution();
		res.width = Int32.Parse(resString[0]);
		res.height = Int32.Parse (resString[1]);
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

	private void CalculateDialogPosition()
    {
		rect = settingsCanvasGroup.gameObject.GetComponent<RectTransform>();
		ap = rect.anchoredPosition;
		sp = settingsCanvasGroup.gameObject.transform.position;
	}
}
