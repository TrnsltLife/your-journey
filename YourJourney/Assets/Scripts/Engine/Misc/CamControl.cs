using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.EventSystems;

public class CamControl : MonoBehaviour
{
	public Transform uiRoot;
	public float focusDistMin = 1.35f, focusDistMax = 3f;
	public PostProcessProfile postProcProf;
	public PartyPanel partyPanel;
	public ChroniclePanel chroniclePanel;

	CombatPanel combatPanel;
	Camera cam;

	//old Z = -2.40108
	public float moveDragSpeed = .005f, rotateSpeed = 30, rotateDuration = .25f,
		doubleClickSpeed = .25f, zoomMin = 5f, zoomMax = 14f, smoothSpeed,
		tiltMin = 45f, tiltMax = 85f, zSlideMin = 0f, zSlideMax = 0.6f, targetLookAtY = 0.0f;

	//zoomMin = 3f, zoomMax = 20f

	float rotateAmount = 0, dClickTimer;
	Vector2 dragStart;
	Vector3 targetPos, targetZoom, DOF, targetDOF, targetLookAt;
	int dClickCount;
	bool dragging = false;
	Vector3 dragOrigin;

	void Awake()
	{
		cam = Camera.main;
		cam.transform.LookAt( transform );
		targetZoom = cam.transform.localPosition;
		DOF = Vector3.one;
		combatPanel = FindObjectOfType<CombatPanel>();
	}

	public void AdjustSettings(bool isJourneyMap)
    {
		//original values
		zoomMin = 3f;
		zoomMax = 6f;
		tiltMin = 50f;
		tiltMax = 55f;
		zSlideMin = 0f;
		zSlideMax = 0.6f;
		targetLookAtY = 0f;

		if (isJourneyMap)
        {
			zoomMin = 3f;
			zoomMax = 14f; // 20f
			tiltMin = 50f; // 45f
			tiltMax = 75f; // 85f
			zSlideMin = 0f;
			zSlideMax = 0.6f;
			targetLookAtY = 0f; //26.87f;
		}
		else //Battle Map
        {
			zoomMin = 5f;
			zoomMax = 14f;
			tiltMin = 55f;
			tiltMax = 75f;
			zSlideMin = 0f;
			zSlideMax = 0f;
			targetLookAtY = 0f;
		}

	}

	//void lookAt()
	//{
	//	cam.transform.DOLookAt( target.transform.position, 2, AxisConstraint.Y | AxisConstraint.X, Vector3.up ).SetEase( Ease.InOutQuad );
	//}

	void Update()
	{
		//position
		if ( !dragging )
			transform.position = Vector3.Lerp( transform.position, targetPos, smoothSpeed );
		//zoom
		cam.transform.localPosition = Vector3.Lerp( cam.transform.localPosition, targetZoom, .5f );
		//lookat
		cam.transform.localEulerAngles = Vector3.Lerp( cam.transform.localEulerAngles, targetLookAt, .5f );
		//dof
		//DOF = Vector3.Lerp( DOF, targetDOF, .2f );
		//postProcProf.TryGetSettings<DepthOfField>( out DepthOfField dof );
		//FloatParameter fp = new FloatParameter
		//{
		//	value = DOF.x
		//};
		//dof.focusDistance.value = fp;

		//disable camera control if panels are showing
		//if (uiRoot.childCount > 0) return;
		if (EventSystem.current.IsPointerOverGameObject()) return;
		else if (combatPanel.gameObject.activeInHierarchy) return;
		else if (partyPanel.gameObject.activeInHierarchy) return;
		else if (chroniclePanel.gameObject.activeInHierarchy) return;
		else if (FindObjectOfType<Engine>().settingsDialog.gameObject.activeInHierarchy) return;
		else if (FindObjectOfType<ProvokeMessage>().provokeMode) return;

		/*
		if ( uiRoot.childCount > 0
			|| combatPanel.gameObject.activeInHierarchy
			|| partyPanel.gameObject.activeInHierarchy
			|| chroniclePanel.gameObject.activeInHierarchy
			|| FindObjectOfType<Engine>().settingsDialog.gameObject.activeInHierarchy
			|| FindObjectOfType<ProvokeMessage>().provokeMode )
			return;
		*/

		HandleDragging();
		HandleRotation();
		HandleMove();
		HandleZoom();

		//Use to debug zoom and positioning
		if (Engine.FindEngine().mapDebug)
		{
			Engine.FindEngine().scenarioOverlay.SetActive(true);
			Engine.FindEngine().scenarioOverlay.GetComponent<Image>().color = new Color(1, 1, 1, 0); //set image transparent
			Engine.FindEngine().SetLoadingText("p" + targetPos + " z" + targetZoom + " F" + DOF + " *F" + targetDOF + " 8)" + targetLookAt);
		}
	}

	void HandleZoomOriginal() //original
    {
		float axis = Input.GetAxis("Mouse ScrollWheel");
		float y = cam.transform.localPosition.y;

		// scroll up
		if (axis > 0f)
		{
			if (y - .2f >= 3f)
				targetZoom = cam.transform.localPosition - new Vector3(0, .2f, 0);
		}
		// scroll down
		else if (axis < 0f)
		{
			if (y + .2f <= 6f)
				targetZoom = cam.transform.localPosition + new Vector3(0, .2f, 0);
		}

		float fdScalar = GlowEngine.RemapValue(y, 3, 6, focusDistMin, focusDistMax);
		targetDOF.Set(fdScalar, fdScalar, fdScalar);

		//Set the polar rotation - the map rotates around this axis
		if(Engine.currentScenario.scenarioTypeJourney)
		{
			targetLookAt.y = 26.87f; //offset angle for the hex map
		}
		else
        {
			targetLookAt.y = 0f; //this looks straight on to the battle map
		}
		//30.58
		targetLookAt.x = GlowEngine.RemapValue(targetZoom.y, .2f, 6f, 50f, 55f);
	}

	void HandleZoom() //new
	{
		float axis = Input.GetAxis( "Mouse ScrollWheel" );
		float y = cam.transform.localPosition.y;

		float zoomAmount = 2f;
		if (y <= 14f) { zoomAmount = 1f; }
		if (y <= 8f) { zoomAmount = 0.5f; }
		if (y <= 5f) { zoomAmount = 0.2f; }

		// scroll up
		if ( axis > 0f )
		{
			if (y - zoomAmount >= zoomMin)
				targetZoom = cam.transform.localPosition - new Vector3(0, zoomAmount, 0);
			else
				targetZoom = cam.transform.localPosition.Y(zoomMin);
		}
		// scroll down
		else if ( axis < 0f )
		{
			if (y + zoomAmount <= zoomMax)
				targetZoom = cam.transform.localPosition + new Vector3(0, zoomAmount, 0);
			else
				targetZoom = cam.transform.localPosition.Y(zoomMax);
		}

		//Change zSlide only when zooming
		if(axis != 0)
        {
			//targetPos.z = GlowEngine.RemapValue(targetZoom.y, zoomMin, zoomMax, zSlideMax, zSlideMin); //slide camera base forward zoomed out or backward zoomed in to keep focused object closer to the center of the screen
		}

		float fdScalar = GlowEngine.RemapValue( y, zoomMin, zoomMax, focusDistMin, focusDistMax );
		targetDOF.Set( fdScalar, fdScalar, fdScalar );
		//Set the polar rotation - the map rotates around this axis
		targetLookAt.y = targetLookAtY;

		//0f;
		//26.87f;
		//30.58
		//targetLookAt.x = GlowEngine.RemapValue( targetZoom.y, .2f, 6f, 50f, 55f );
		targetLookAt.x = GlowEngine.RemapValue(targetZoom.y, zoomMin, zoomMax, tiltMin, tiltMax); //change tilt of camera angle
	}

	public void MoveTo( Vector3 pos, bool evenBattleMap = false, float speed = .35f )
	{
		if ( pos == Vector3.zero )
			return;

		if (Engine.currentScenario.scenarioTypeJourney || evenBattleMap)
		{
			pos.y = 0;
			pos.z = pos.z + GlowEngine.RemapValue(targetZoom.y, zoomMin, zoomMax, zSlideMax, zSlideMin);
			targetPos = pos;
			smoothSpeed = speed;
		}
	}

	void HandleDragging()
	{
		if ( Input.GetMouseButtonDown( 0 ) )
		{
			dragOrigin = Input.mousePosition;
			return;
		}

		if ( !Input.GetMouseButton( 0 ) || dClickCount > 1 )
		{
			dragging = false;
			return;
		}

		dragging = true;
		float scalar = Vector3.Distance( dragOrigin, Input.mousePosition );
		scalar = GlowEngine.RemapValue( scalar, 0, 30, 0, .7f );

		//Vector3 pos = cam.ScreenToViewportPoint( Input.mousePosition - dragOrigin );
		Vector3 direction = Input.mousePosition - dragOrigin;
		direction = Vector3.Normalize( direction );

		Vector3 move = new Vector3( direction.x * scalar, 0, direction.y * scalar );
		transform.Translate( -move, Space.Self );
		targetPos = transform.position;
		dragOrigin = Input.mousePosition;
	}

	void HandleMove()
	{
		int mask1 = 1 << 2;
		int mask2 = 1 << 9;

		//mask = ~mask;

		//single click
		if ( Input.GetMouseButtonDown( 0 ) )
		{
			dClickCount++;
			dClickTimer = doubleClickSpeed;
			Ray ray = cam.ScreenPointToRay( Input.mousePosition );
			RaycastHit hit;

			//double click
			if ( dClickCount > 1 && Physics.Raycast( ray, out hit, Mathf.Infinity, ~( mask1 | mask2 ) ) )
			{
				//Debug.Log( "click::" + hit.collider.name );

				MoveTo( hit.collider.transform.parent.GetComponent<Tile>().centerPosition, true, .2f );
			}
		}

		dClickTimer -= Time.deltaTime;
		if ( dClickTimer <= 0 )
			dClickCount = 0;
	}

	void HandleRotation()
	{
		if ( Input.GetMouseButtonDown( 1 ) )
		{
			dragStart = Input.mousePosition;
		}

		if ( Input.GetMouseButton( 1 ) )
		{
			float d = Vector2.Distance( dragStart, Input.mousePosition );
			float delta = GlowEngine.RemapValue( d, 0, 50, 0, rotateSpeed );

			if ( Input.mousePosition.x < dragStart.x )
				rotateAmount = -delta;
			else if ( Input.mousePosition.x > dragStart.x )
				rotateAmount = delta;
			else
				rotateAmount = 0;

			transform.DORotate( new Vector3( 0, rotateAmount, 0 ), rotateDuration, RotateMode.WorldAxisAdd );
			dragStart = Input.mousePosition;
		}
	}

	public CamState GetState()
	{
		return new CamState()
		{
			position = transform.position,
			YRotation = transform.rotation.eulerAngles.y
		};
	}

	public void SetState( CamState camState )
	{
		transform.position = camState.position;
		MoveTo( camState.position, true );
		transform.rotation = Quaternion.Euler( 0, camState.YRotation, 0 );
	}


}
