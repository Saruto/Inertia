using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerUmbrella : NetworkBehaviour {
	// ----------------------------------- Fields and Properties ----------------------------------- //
	
	// Player Rigidbody + Other Components
	Rigidbody2D rb;
	DistanceJoint2D HookJoint;
	LineRenderer HookGraphic;
	PlayerMovement playerMovement;

	// Is the Umbrella currently open?
	public bool isUmbrellaOpen { get; private set; }

	// Is the player currently hooking with the umbrella?
	public bool isUmbrellaHookOpen { get; private set; }


	//  --------- Serialized Fields: Set in Inspector ---------  //

	[Header("Umbrella")]

	// The top speed when falling while the umbrella is open (should be negative)
	[SerializeField] float UmbrellaFallingTopSpeed;

	// The umbrella sprite gameobject.
	[SerializeField] GameObject UmbrellaSprite;


	enum HookType { Anywhere, CollidersOnly }
	[Header("Hook")]
	// The kind of things we can hook onto
	[SerializeField] HookType HookTarget;

	// The swing force applied when swinging on the hook
	public float HookSwingForce;

	// The max length of the hook (doesn't apply to Anywhere hook type)
	[SerializeField] float HookMaxLength;



	// ------------------------------------------ Methods ------------------------------------------ //

	//  --------- Start ---------  //
	void Start () {
		rb = GetComponent<Rigidbody2D>();
		HookJoint = GetComponent<DistanceJoint2D>();
		HookGraphic = GetComponentInChildren<LineRenderer>();
		playerMovement = GetComponent<PlayerMovement>();
		UmbrellaSprite.SetActive(false);
		HookJoint.enabled = false;
		HookGraphic.enabled = false;
	}

	//  --------- Update ---------  //
	void Update() {
		if(isLocalPlayer) {
			// --- Umbrella Opening and Closing--- //
			if(Input.GetMouseButtonDown(0) && !isUmbrellaHookOpen) {
				CmdUmbrellaToggle();
			}
			if(isUmbrellaOpen) {
				if(rb.velocity.y < UmbrellaFallingTopSpeed) {
					rb.velocity = new Vector2(rb.velocity.x, UmbrellaFallingTopSpeed);

				}
			}


			// --- Umbrella Hook --- //
			// Hooking
			if(Input.GetMouseButtonDown(1) && !isUmbrellaHookOpen) {
				CmdTryHookshot(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
			}
			// Releasing
			else if((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space)) && isUmbrellaHookOpen) {
				CmdReleaseHookshot();
			}
			// Update the hook graphic if open
			if(isUmbrellaHookOpen) {
				HookGraphic.SetPositions(new Vector3[]{ transform.position, HookJoint.connectedAnchor });
				CmdUpdateHookshotGraphic();
			}
		}






	}

	//  --------- Helper Functions ---------  //
	/*
	// Returns true if we sucessfully grabbed something.
	bool MouseIsOverHookableTarget(){
		switch(HookTarget) {
		case HookType.Anywhere:
			return true;
		case HookType.CollidersOnly:
			RaycastHit2D hit = Physics2D.Raycast(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position, HookMaxLength);
			return hit.collider != null && !hit.collider.isTrigger;
		}
		throw new System.Exception();
	}

	// Returns the connected anchor point.
	Vector2 GetConnectedAnchorPoint() {
		switch(HookTarget) {
		case HookType.Anywhere:
			return Camera.main.ScreenToWorldPoint(Input.mousePosition);
		case HookType.CollidersOnly:
			RaycastHit2D hit = Physics2D.Raycast(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position, HookMaxLength);
			return hit.point;
		}
		throw new System.Exception();
	}
	*/

	//  --------- Public Functions ---------  //
	// Returns the swing direction vector when on a hook
	public Vector2 HookPlayerPerpendicularCounterClockwise() {		// moving right
		Vector2 hookToPlayer = (Vector2)transform.position - HookJoint.connectedAnchor;
		return new Vector2(-hookToPlayer.y, hookToPlayer.x).normalized;
	}
	public Vector2 HookPlayerPerpendicularClockwise() {		// moving left
		Vector2 hookToPlayer = (Vector2)transform.position - HookJoint.connectedAnchor;
		return new Vector2(hookToPlayer.y, -hookToPlayer.x).normalized;
	}




	//  --------- Network Functions ---------  //
	// Opens the umbrella for this player on all clients
	[Command]
	void CmdUmbrellaToggle() {
		RpcUmbrellaToggle();
	}
	[ClientRpc]
	void RpcUmbrellaToggle() {
		isUmbrellaOpen = !isUmbrellaOpen;
		if(isUmbrellaOpen) {
			UmbrellaSprite.SetActive(true);
		} else {
			UmbrellaSprite.SetActive(false);
		}
	}

	// Handle the hookshot visuals for this player on all clients.
	[Command]
	void CmdTryHookshot(Vector3 position, Vector3 screenToWorldPoint) {
		// Check to see if the client is hovering over a clickable target
		bool isOver = false;
		Vector2 anchorPoint = new Vector2();
		switch(HookTarget) {
		case HookType.Anywhere:
			isOver = true;
			anchorPoint = screenToWorldPoint;
			break;
		case HookType.CollidersOnly:
			RaycastHit2D hit = Physics2D.Raycast(position, screenToWorldPoint - position, HookMaxLength);
			isOver = hit.collider != null && !hit.collider.isTrigger;
			anchorPoint = hit.point;
			break;
		}
		if(isOver) {
			RpcThrowHookshot(anchorPoint);
		}
	}

	[ClientRpc]
	void RpcThrowHookshot(Vector2 point) {
		isUmbrellaHookOpen = true;
		HookJoint.connectedAnchor = point;
		HookJoint.enabled = true;
		HookGraphic.enabled = true;
		HookGraphic.SetPositions(new Vector3[]{ transform.position, point });
		// close the umbrella
		isUmbrellaOpen = false;
		UmbrellaSprite.SetActive(false);
	}

	[Command]
	void CmdReleaseHookshot() {
		RpcReleaseHookshot();
	}
	[ClientRpc]
	void RpcReleaseHookshot() {
		isUmbrellaHookOpen = false;
		playerMovement.UncapHSpeedUntilOnGround();
		HookJoint.enabled = false;
		HookGraphic.enabled = false;
	}

	[Command]
	void CmdUpdateHookshotGraphic() {
		RpcUpdateHookshotGraphic();
	}
	[ClientRpc]
	void RpcUpdateHookshotGraphic() {
		if(!isLocalPlayer)
			HookGraphic.SetPositions(new Vector3[]{ transform.position, HookJoint.connectedAnchor });
	}
}
