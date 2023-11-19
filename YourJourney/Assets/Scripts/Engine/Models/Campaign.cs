using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Campaign
{
	public Guid campaignGUID { get; set; }
	public string campaignName { get; set; }
	public string coverImage { get; set; }
	public string fileVersion { get; set; }
	public string storyText { get; set; }
	public string description { get; set; }
    [JsonIgnore]
	public List<int> collections { get; set; }

	public ObservableCollection<CampaignItem> scenarioCollection { get; set; }
	public ObservableCollection<Trigger> triggerCollection { get; set; }

	public bool startWithTrinkets { get; set; }
	public bool startWithMounts { get; set; }

	public Campaign()
	{
		scenarioCollection = new ObservableCollection<CampaignItem>();
		triggerCollection = new ObservableCollection<Trigger>();
	}
}
