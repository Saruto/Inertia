using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindEffectorScript : MonoBehaviour {

	// The player
	PlayerUmbrella umbrella;

	// The Area Effector
	AreaEffector2D windEffector;

	// Use this for initialization
	void Start () {
		windEffector = GetComponent<AreaEffector2D>();
	}
	
	// Update is called once per frame
	void Update () {
		if(umbrella == null) {
			if(PlayerNetwork.LocalPlayer == null) return;
			else umbrella = PlayerNetwork.LocalPlayer.GetComponent<PlayerUmbrella>();
		}
		// Activate/Deactivate the local version of the wind effector based on the local player.
		if(!umbrella.isUmbrellaOpen) {
			windEffector.enabled = false;
		} else {
			windEffector.enabled = true;
		}
	}
}
