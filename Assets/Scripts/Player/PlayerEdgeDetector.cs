using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script attached to colliders placed on the edges of the player, to notify the player GO if a side is touching the ground/wall.
public class PlayerEdgeDetector : MonoBehaviour {
	// ----------------------------------- Fields and Properties ----------------------------------- //
	// Is this edge collider touching anything?
	public bool isTouching { get; private set; }

	//  --------- Serialized Fields: Set in Inspector ---------  //

	// The direction that this edge collider lies on.
	public enum Direction { Top, Bottom, Left, Right }
	[SerializeField] Direction Side;

	// ------------------------------------------ Methods ------------------------------------------ //
	//  --------- OnCollisionStay ---------  //
	void OnCollisionStay2D(Collision2D collision) {
		if(collision.gameObject.tag == "Ground") {
			isTouching = true;

			// Special case for Platform Effectors: Don't let the player walljump off of platforms.
			if(collision.collider.GetComponent<PlatformEffector2D>() != null && (Side == Direction.Left || Side == Direction.Right) ) {
				isTouching = false;
			}

		}
	}

	//  --------- OnCollisionExit ---------  //
	void OnCollisionExit2D(Collision2D collision) {
		if(collision.gameObject.tag == "Ground") {
			isTouching = false;
		}
	}
}
