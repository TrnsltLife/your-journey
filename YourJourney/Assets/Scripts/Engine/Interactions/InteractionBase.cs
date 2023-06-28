using System;

public abstract class InteractionBase : IInteraction
{
	public string dataName { get; set; }
	public Guid GUID { get; set; }
	public bool isEmpty { get; set; }
	public string triggerName { get; set; }
	public string triggerAfterName { get; set; }
	public bool isTokenInteraction { get; set; }
	public TokenType tokenType { get; set; }
	public PersonType personType { get; set; }
	public TerrainType terrainType { get; set; }
	public TextBookData textBookData { get; set; }
	public TextBookData eventBookData { get; set; }
	public string tokenInteractionText { get; set; }
	public int loreReward { get; set; }
	abstract public InteractionType interactionType { get; set; }
	public int xpReward { get; set; }
	public int threatReward { get; set; }
	public bool isPersistent { get; set; }
	public bool isPlaced { get; set; }
	public bool isReusable { get; set; }
}
