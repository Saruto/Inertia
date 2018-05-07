using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedCameraTrigger : MonoBehaviour {
	// ----------------------------------- Fields and Properties ----------------------------------- //

	// The Player GO
	GameObject Player;


	//  --------- Serialized Fields: Set in Inspector ---------  //

	// The size increase or decrease applied to the camera's zoom.
	[SerializeField] float ZoomOffset;



	// ------------------------------------------ Methods ------------------------------------------ //
	//  --------- Start ---------  //
	void Start () {
		Player = GameObject.FindWithTag("Player");
	}

	//  --------- OnTriggerEnter2D ---------  //
	void OnTriggerEnter2D(Collider2D col) {
		if(col.gameObject == Player) {
			Camera.main.GetComponent<CameraMovement>().SetFixedPosition(transform.position, ZoomOffset);
		}
	}

	//  --------- OnTriggerExit2D ---------  //
	void OnTriggerExit2D(Collider2D col) {
		if(col.gameObject == Player) {
			Camera.main.GetComponent<CameraMovement>().UnsetFixedPosition();
		}
	}

}
