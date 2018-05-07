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
		umbrella = GameObject.FindWithTag("Player").GetComponent<PlayerUmbrella>();
		windEffector = GetComponent<AreaEffector2D>();
	}
	
	// Update is called once per frame
	void Update () {
		if(!umbrella.isUmbrellaOpen) {
			windEffector.enabled = false;
		} else {
			windEffector.enabled = true;
		}
	}
}
