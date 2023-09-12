using System;
using System.Collections.ObjectModel;

public class Translation
{
	public string dataName;
	public string langName;
	public Guid GUID;
	public ObservableCollection<TranslationItem> translationItems = new ObservableCollection<TranslationItem>();
}