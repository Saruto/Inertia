using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
	// ----------------------------------- Fields and Properties ----------------------------------- //

	// The Player information
	GameObject PlayerTarget;
	Rigidbody2D PlayerRB;
	PlayerMovement PlayerMovement;

	// Camera information
	Camera Camera;

	// Variables required to determine the camera's target position
	// X
	const float SPEED_THRESHOLD = 10f, X_OFFSET_BASE = 4f;
	float xOffsetLastFrame = X_OFFSET_BASE;
	// Y
	const float Y_OFFSET_BASE = 2f;
	float yPositionLastFrame;

	// Used for SmoothDamp
	Vector3 velocity;
	const float smoothTime = 0.4f;

	//  --- Camera Control Variables ---  //
	// Is the Camera Fixed?
	bool cameraIsFixed = false;
	// The original zoom value of the camera on start.
	float originalZoomValue;

	//  --------- Serialized Fields: Set in Inspector ---------  //
	// The movement type to use
	enum MovementType { Instant, Smooth }
	[SerializeField] MovementType Type;


	// ------------------------------------------ Methods ------------------------------------------ //

	public void SetTarget(GameObject target) {
		PlayerTarget = target;
		if(target != null) {
			PlayerRB = PlayerTarget.GetComponent<Rigidbody2D>();
			PlayerMovement = PlayerTarget.GetComponent<PlayerMovement>();
			yPositionLastFrame = PlayerTarget.transform.position.y + Y_OFFSET_BASE;
		}
	}

	//  --------- Start ---------  //
	void Start () {
		Camera = GetComponent<Camera>();
		originalZoomValue = Camera.orthographicSize;
	}
	
	//  --------- LateUpdate ---------  //
	void LateUpdate () {
		if(PlayerTarget == null) return;
		if(cameraIsFixed) return;
		if(Type == MovementType.Instant) {
			transform.position = new Vector3(PlayerTarget.transform.position.x, PlayerTarget.transform.position.y + Y_OFFSET_BASE, transform.position.z);
		} else if(Type == MovementType.Smooth) {
			// Get the Target Position
			// X: The player pos + offset dependsing on where the player is moving towards.
			float x_val = PlayerTarget.transform.position.x + PlayerRB.velocity.x / 2f;
			/*
			if(PlayerRB.velocity.x > SPEED_THRESHOLD){
				x_val += X_OFFSET_BASE;
				xOffsetLastFrame = X_OFFSET_BASE;
			} else if(PlayerRB.velocity.x < -SPEED_THRESHOLD){
				x_val += -X_OFFSET_BASE;
				xOffsetLastFrame = -X_OFFSET_BASE;
			} else {
				x_val += xOffsetLastFrame;
			}*/
			// Y: The camera only moves it's y position when the player is on the ground.
			float y_val;
			y_val = PlayerTarget.transform.position.y + Y_OFFSET_BASE;
			/*if(PlayerMovement.OnGround() || PlayerMovement.OnWall()) {
				y_val = Player.transform.position.y + Y_OFFSET_BASE;
			} else {
				y_val = yPositionLastFrame;
			}
			yPositionLastFrame = y_val;*/
			// Finish
			Vector3 TargetPosition = new Vector3(x_val, y_val, transform.position.z);
		

			// Smooth movement on Player.
			transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref velocity, smoothTime);
		}
	}


	//  --------- Special Camera Controls ---------  //
	// These only do something if the player passed in is equal to PlayerTarget.

	// Sets the camera to a fixed position.
	const float FIXED_POS_DURATION = 1.5f;
	public void SetFixedPosition(Vector2 position, float zoomOffset, GameObject playerTarget) {
		if(playerTarget != PlayerTarget) return;
		cameraIsFixed = true;
		velocity = Vector3.zero;
		StartCoroutine(set_fixed_position_helper(position, zoomOffset));	
	}
	IEnumerator set_fixed_position_helper(Vector2 position, float zoomOffset) {
		float t = 0f;
		float x_start = transform.position.x, y_start = transform.position.y;
		float zoom_start = Camera.orthographicSize;
		while(t <= 1 && cameraIsFixed) {
			transform.position = new Vector3(
				Mathf.SmoothStep(x_start, position.x, t), 
				Mathf.SmoothStep(y_start, position.y, t), 
				transform.position.z
			);
			Camera.orthographicSize = Mathf.SmoothStep(zoom_start, originalZoomValue + zoomOffset, t);
			t += Time.deltaTime / FIXED_POS_DURATION;
			yield return null;
		}
	}

	// Unsets the camera's fixed position, if it is fixed.
	public void UnsetFixedPosition(GameObject playerTarget) {
		if(playerTarget != PlayerTarget) return;
		cameraIsFixed = false;
		StartCoroutine(unset_fixed_position_helper());
	}
	IEnumerator unset_fixed_position_helper() {
		float t = 0f;
		float zoom_start = Camera.orthographicSize;
		while(t <= 1 && !cameraIsFixed) {
			Camera.orthographicSize = Mathf.SmoothStep(zoom_start, originalZoomValue, t);
			t += Time.deltaTime / FIXED_POS_DURATION;
			yield return null;
		}
	}

}
