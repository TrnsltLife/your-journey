using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MonsterItem : MonoBehaviour
{
	public CanvasGroup cg;
	public RectTransform healthMeter, shieldMeter, sorcMeter;
	public Text healthText, shieldText, sorcText;
	public GameObject skullnbones;

	[HideInInspector]
	public Monster monster;
	[HideInInspector]
	public bool isDead = false;
	[HideInInspector]
	public bool activeMonster = false;//visible in combat screen

	int tempCurrentHealth, tempCurrentSunder, currentShield, currentSorc, currentMaxHealth, currentDead, idx, sunderModify, healthModify;
	float healthWidth = 324, shieldWidth = 324, sorcWidth = 324;
	bool startedDead;

	void Update()
	{
		healthMeter.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, isDead ? 0 : healthWidth );
		shieldMeter.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, isDead ? 0 : shieldWidth );
		sorcMeter.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, isDead ? 0 : sorcWidth );
		skullnbones.SetActive( isDead );
	}

	void UpdateUI()
	{
		healthText.text = tempCurrentHealth + "/" + monster.health;
		float width = 324 * tempCurrentHealth / monster.health;
		DOTween.To( () => healthWidth, x => healthWidth = x, width, .2f );

		shieldText.text = currentShield.ToString();
		width = 324 * currentShield / monster.health;
		DOTween.To( () => shieldWidth, x => shieldWidth = x, width, .2f );

		sorcText.text = currentSorc.ToString();
		width = 324 * currentSorc / monster.health;
		DOTween.To( () => sorcWidth, x => sorcWidth = x, width, .2f );
	}

	public int Damage( int damage, CombatModify modifier )
	{
		if ( startedDead )
		{
			isDead = true;
			return damage;
		}

		bool startedSunder = modifier.Sunder;

		//if ( sunderModify == 1 && !modifier.Sunder )//removing sunder
		//	sunderModify = 0;
		//else

		healthModify = modifier.Lethal ? currentMaxHealth / 2 : 0;
		int damageUsed = 0;
		tempCurrentHealth = currentMaxHealth;
		currentShield = Mathf.Max( 0, monster.shieldValue - tempCurrentSunder);

		//Sunder - permanently remove one point of armor
		sunderModify = modifier.Sunder ? 1 : 0;
		if ( modifier.Sunder )
		{
			if ( currentShield > 0 ) //If there's not shield on this one, Sunder may affect the next MonsterItem
            {
				currentShield = Mathf.Max(0, monster.shieldValue - tempCurrentSunder - sunderModify);
				if (!modifier.Cleave)
				{
					//Prevent the Sunder from applying to another MonsterItem unless Cleave is enabled (effects all enemies)
					modifier.Sunder = false;
				}
			}
		}

		if ( modifier.Pierce )
			currentShield = 0;
		currentSorc = monster.sorceryValue;
		if ( modifier.Smite )
			currentSorc = 0;
		currentDead = ( currentMaxHealth - healthModify ) + currentShield + currentSorc;
		isDead = false;

		for ( int i = damage; i > 0; i-- )
		{
			if ( currentShield > 0 )
			{
				currentShield--;
				damageUsed++;
				continue;
			}
			if ( currentSorc > 0 )
			{
				currentSorc--;
				damageUsed++;
				continue;
			}
			if ( damageUsed < currentDead )
			{
				tempCurrentHealth--;
				damageUsed++;
			}
			if ( damageUsed == currentDead )
			{
				isDead = true;
				currentShield = 0; //Set to 0 so remaining armor doesn't algorithmically resurrect dead enemies?
				//Restore Sunder for next enemy if necessary. When an enemy is defeated, Sunder applies to the next enemy as well.
				if (startedSunder) { modifier.Sunder = true; }
				break;
			}
		}

		UpdateUI();

		//Debug.Log( "LEFTOVER:" + Mathf.Max( 0, damage - damageUsed ) );

		if ( modifier.Cleave )
			return damage;
		else
			return Mathf.Max( 0, damage - damageUsed );
	}

	public bool Apply( bool stun )
	{
		Debug.Log( "STUN? " + stun );
		if ( stun && !monster.immuneStun )
		{
			monster.isStunned = true;
			FindObjectOfType<MonsterManager>().ExhaustMonster( monster, true );
		}

		monster.currentHealth[idx] = tempCurrentHealth;
		//monster.sunderValue = sunderModify;
		monster.currentSunder[idx] += sunderModify;
		return !isDead;
	}

	public void Show( Monster m, int index )
	{
		activeMonster = true;
		idx = index;
		monster = m;
		currentMaxHealth = monster.currentHealth[index];
		tempCurrentSunder = monster.currentSunder[index];
		startedDead = currentMaxHealth == 0;
		//sunderModify = monster.sunderValue;

		Damage( 0, new CombatModify() );

		cg.DOFade( 1, .5f );
	}

	public void Hide()
	{
		activeMonster = false;
		cg.alpha = 0;
	}
}
