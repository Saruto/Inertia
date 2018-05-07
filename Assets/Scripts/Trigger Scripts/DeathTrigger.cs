using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathTrigger : MonoBehaviour {
	// ----------------------------------- Fields and Properties ----------------------------------- //


	//  --------- Serialized Fields: Set in Inspector ---------  //


	// ------------------------------------------ Methods ------------------------------------------ //
	//  --------- Start ---------  //
	void Start () {

	}
	
	//  --------- OnTriggerEnter2D ---------  //
	void OnTriggerEnter2D(Collider2D col) {
		if(col.gameObject.tag == "Player") {
			col.gameObject.GetComponent<PlayerNetwork>().CmdRespawn();
		}
	}
}
