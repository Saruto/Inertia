using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedCameraTrigger : MonoBehaviour {
	// ----------------------------------- Fields and Properties ----------------------------------- //


	//  --------- Serialized Fields: Set in Inspector ---------  //

	// The size increase or decrease applied to the camera's zoom.
	[SerializeField] float ZoomOffset;



	// ------------------------------------------ Methods ------------------------------------------ //
	//  --------- Start ---------  //
	void Start () {
		
	}

	//  --------- OnTriggerEnter2D ---------  //
	void OnTriggerEnter2D(Collider2D col) {
		if(col.gameObject.tag == "Player") {
			Camera.main.GetComponent<CameraMovement>().SetFixedPosition(transform.position, ZoomOffset, col.gameObject);
		}
	}

	//  --------- OnTriggerExit2D ---------  //
	void OnTriggerExit2D(Collider2D col) {
		if(col.gameObject.tag == "Player") {
			Camera.main.GetComponent<CameraMovement>().UnsetFixedPosition(col.gameObject);
		}
	}

}
