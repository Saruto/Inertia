using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathTrigger : MonoBehaviour {
	// ----------------------------------- Fields and Properties ----------------------------------- //

	// The Player GO
	GameObject Player;


	//  --------- Serialized Fields: Set in Inspector ---------  //


	// ------------------------------------------ Methods ------------------------------------------ //
	//  --------- Start ---------  //
	void Start () {
		Player = GameObject.FindWithTag("Player");
	}
	
	//  --------- OnTriggerEnter2D ---------  //
	void OnTriggerEnter2D(Collider2D col) {
		if(col.gameObject == Player) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}
