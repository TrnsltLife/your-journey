using UnityEngine;

public class ProvokeButton : MonoBehaviour
{
	public Transform spinner;

	public void OnClick()
	{
		var objs = FindObjectsOfType<SpawnMarker>();
		foreach ( var ob in objs )
		{
			if ( ob.name.Contains( "SPAWNMARKER" ) )
				Destroy( ob.gameObject );
			if ( ob.name == "STARTMARKER" )
				ob.gameObject.SetActive( false );
			if (ob.name.StartsWith("Start Token"))
				ob.gameObject.SetActive(false);
		}

		if ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase
			|| FindObjectOfType<ProvokeMessage>().provokeMode
			|| FindObjectOfType<MonsterManager>().monsterList.Count == 0 )
			return;

		spinner.gameObject.SetActive( !spinner.gameObject.activeSelf );
		FindObjectOfType<ProvokeMessage>().Show();
	}

	private void Update()
	{
		spinner.localEulerAngles = new Vector3( 0, 0, ( Time.time * 10f ) % 360 );
		spinner.localScale = GlowEngine.SineAnimation( .64f, .68f, 15f ).ToVector3();
	}

	public void DisableSpinner()
	{
		spinner.gameObject.SetActive( false );
	}
}
