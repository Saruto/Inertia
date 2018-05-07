using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
	// ----------------------------------- Fields and Properties ----------------------------------- //
	// Player Rigidbody + Other Script Components
	Rigidbody2D rb;
	PlayerUmbrella umbrella;
	DistanceJoint2D HookJoint;

	// --------- Control Variables --------- //
	// Can the player stop the y velocity of their jump? This only becomes true a few frames after a jump.
	// The player can't stop their y velocity unless they actually jumped themselves first
	bool canStopJump = false;

	// Stops capping the H speed after going off the hook
	bool justJumpedOffHook = false;

	// --------- Wall Jumping/Clinging --------- //
	// Is the player currently clinging onto a wall?
	bool isOnWall = false;
	// Is/Was the player most recently on a left facing wall? or a right facing wall?
	bool wasOnLeftWall = false;
	bool wasOnRightWall = false;
	// Can the player wall jump? This stays true for a few frames after the player leaves a wall.
	bool canWallJump = false;

	// --------- Other Movement --------- //
	// The initial gravity scale of the player.
	float gravityScale;

	// The force applied for left and right movement.
	Vector2 ForceToApply = Vector2.zero;

	// True if the player's in a wind effector
	bool inWindEffector = false;
	
	//  --------- Serialized Fields: Set in Inspector ---------  //
	// The various edge detectors on the player.
	[SerializeField] PlayerEdgeDetector LeftDetector;
	[SerializeField] PlayerEdgeDetector RightDetector;
	[SerializeField] PlayerEdgeDetector FloorDetector;

	// Player movement constants
	[SerializeField] float RunningForce;
	[SerializeField] float JumpingForce;
	[SerializeField] float RunningTopSpeed;



	// ------------------------------------------ Methods ------------------------------------------ //
	//  --------- Start ---------  //
	void Start () {
		rb = GetComponent<Rigidbody2D>();
		umbrella = GetComponent<PlayerUmbrella>();
		HookJoint = GetComponent<DistanceJoint2D>();
		gravityScale = rb.gravityScale;
	}
	
	//  --------- Update ---------  //
	void Update () {
		if(umbrella.isUmbrellaHookOpen) {
			GrapplingHookMovement();
		} else if(umbrella.isUmbrellaOpen) {
			UmbrellaMovement();
		} else {
			NormalMovement();
		}
	}

	//  --------- Normal Movement State ---------  //
	void NormalMovement() {
		hooklength_change = Vector2.zero;
		// --- Jumping --- //
		JumpingHandler();
		JumpStifleHandler();
		// --- Left and Right Movement --- //
		GroundMovement();
		// --- Wall Clinging --- //
		WallClingingHandler();
		// --- Wall Jumping --- //
		WallJumpingHandler();
		if(wall_jump_stifle_special_active) WallJumpStifleHandler();
	}

	//  --------- Umbrella Movement State ---------  //
	void UmbrellaMovement() {
		canStopJump = false;
		hooklength_change = Vector2.zero;
		// --- Jumping --- //
		JumpingHandler();
		if(!inWindEffector) { JumpStifleHandler(); }
		// --- Left and Right Movement --- //
		GroundMovement();
		// Other
		rb.gravityScale = gravityScale;
		isOnWall = false;
	}

	//  --------- Grappling Hook Movement State ---------  //
	void GrapplingHookMovement() {
		canStopJump = false;	// make sure the player can't stop their jump after hooking
		// Swinging Motion
		if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
			Vector2 force;
			// direction vectors are perpendicular to the vector from the hook to the player. decrease the force as we get closer to the top
			if(Input.GetKey(KeyCode.A)) {
				Vector2 dir = umbrella.HookPlayerPerpendicularClockwise();
				force = dir * umbrella.HookSwingForce;
				// sinVal ranges from 0 to 1. we do this instead of using dir.x (also ranges from 0 to 1) to get a steeper rise
				float sinVal = Mathf.Sin(dir.x * Mathf.PI / 2);
				force *= Mathf.Abs(Mathf.Min(sinVal, 0f));
			} else {
				Vector2 dir = umbrella.HookPlayerPerpendicularCounterClockwise();
				force = dir * umbrella.HookSwingForce;
				float sinVal = Mathf.Sin(dir.x * Mathf.PI / 2);
				force *= Mathf.Max(sinVal, 0f);
			}
			// reduce the force further, or cap the velocity if it's too high.
			if(rb.velocity.sqrMagnitude > 30*30) {
				rb.velocity = rb.velocity.normalized * 30;
				ForceToApply = Vector2.zero;
			} else if(rb.velocity.sqrMagnitude > 20*20) {
				ForceToApply = force / (rb.velocity.magnitude - 19);
			} else {
				ForceToApply = force;
			}

		} else {
			ForceToApply = Vector2.zero;
		}
		// Changing Hook Length
		if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
			Vector2 dir;
			if(Input.GetKey(KeyCode.W)) {	// Up
				dir = (HookJoint.connectedAnchor - (Vector2)transform.position).normalized;
			} else {   // Down
				dir = ((Vector2)transform.position - HookJoint.connectedAnchor).normalized;
			}
			hooklength_change = dir * 6 * Time.deltaTime;
		} else {
			hooklength_change = Vector2.zero;
		}
	}

	Vector2 hooklength_change = Vector2.zero;

	//  --------- FixedUpdate ---------  //
	void FixedUpdate() {
		// Applies the Left and Right
		if(ForceToApply != Vector2.zero) {
			rb.AddForce(ForceToApply);
		}
		if(hooklength_change != Vector2.zero) {
			transform.Translate(hooklength_change);
		}
	}





	//  --------- State Functions ---------  //
	void JumpingHandler() {
		// Initial Jump
		if(Input.GetKeyDown(KeyCode.Space) && FloorDetector.isTouching) {
			rb.AddForce(Vector2.up * JumpingForce, ForceMode2D.Impulse);
			if(!umbrella.isUmbrellaOpen) canStopJump = true;
		}
	}

	void JumpStifleHandler() {
		// If the player is in the air, moving up, but is not holding space, immediately set the y velocity to zero.
		// This can only happen a few frames after the initial jump.
		if(!FloorDetector.isTouching && rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space) && !isOnWall && canStopJump) {
			rb.velocity = new Vector2(rb.velocity.x, 0f);
		}
	}

	void GroundMovement() {
		// Magnitude of the running force decreases as we approach top speed, going to 0 if we're at top speed.
		if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
			Vector2 force;
			if(Input.GetKey(KeyCode.A)) { force = Vector2.left * RunningForce; } 
			else { force = Vector2.right * RunningForce; }
			// Decrease the magnitude of the force based on current velocity (only if the input is along with the movement direction)
			bool inputInDirectionOfMotion = Mathf.Sign(force.x) == Mathf.Sign(rb.velocity.x);
			if(inputInDirectionOfMotion) {
				force /= Mathf.Max(1f, Mathf.Abs(rb.velocity.x));
			}
			// Only apply a force if we're not at top speed, or if the input direction is against the current movement direction.
			if(Mathf.Abs(rb.velocity.x) < RunningTopSpeed || !inputInDirectionOfMotion){
				ForceToApply = force;
			} else {
				// Just maintain top speed if we're already there
				if(!justJumpedOffHook) rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * RunningTopSpeed, rb.velocity.y);
				ForceToApply = Vector2.zero;
			}
		} else {
			if(!justJumpedOffHook) rb.velocity = new Vector2(0f, rb.velocity.y);
			ForceToApply = Vector2.zero;
		}
	}

	void WallClingingHandler() {
		if(!FloorDetector.isTouching) {
			// Clinging onto a left wall or on a right wall
			if((LeftDetector.isTouching && Input.GetKey(KeyCode.A)) || (RightDetector.isTouching && Input.GetKey(KeyCode.D))) {
				// If this is the first frame we got on the wall
				if(!isOnWall) {
					rb.velocity = new Vector2(rb.velocity.x, 0f);
					rb.gravityScale = gravityScale / 6f;
					canWallJump = true;
					isOnWall = true;
					wall_jump_stifle_special_active = false;
					if((LeftDetector.isTouching && Input.GetKey(KeyCode.A))) {
						wasOnLeftWall = true; wasOnRightWall = false;
					} else { 
						wasOnRightWall = true; wasOnLeftWall = false;
					}
				}
			}
			// In the air and not wall clinging.
			else {
				// If this was the 1st frame where we're not on the wall anymore.
				if(isOnWall) {
					StartCoroutine(WallJumpWindow());
				}
				rb.gravityScale = gravityScale;
				isOnWall = false;
			}
		} else {	// On the ground.
			isOnWall = false;
		}
	}

	void WallJumpingHandler() {
		if(Input.GetKeyDown(KeyCode.Space) && canWallJump) {
			rb.velocity = new Vector2(rb.velocity.x, 0f);
			canWallJump = false;
			if(wasOnLeftWall){
				rb.AddForce((Vector2.up + Vector2.right).normalized * JumpingForce * 1.5f, ForceMode2D.Impulse);
			} else{
				rb.AddForce((Vector2.up + Vector2.left).normalized * JumpingForce * 1.5f, ForceMode2D.Impulse);
			}
			// Handles special case where player stifles a walljump (glass cieling is vertical instead of horizontal in this case)
			if(wasOnLeftWall && Input.GetKey(KeyCode.A) || wasOnRightWall && Input.GetKey(KeyCode.D)) {
				canStopJump = false;
				wall_jump_stifle_special_active = true;
			} else if(!umbrella.isUmbrellaOpen) {
				wall_jump_stifle_special_active = false;
				canStopJump = true;
			}
		}
	}

	// Handles what happens when the player is holding towards the wall and stifles their jump (stops holding space) early.
	bool wall_jump_stifle_special_active = false;
	void WallJumpStifleHandler() {
		// turns itself off
		if(FloorDetector.isTouching) {
			wall_jump_stifle_special_active = false;
			canStopJump = true;
		} else if(Input.GetKeyUp(KeyCode.Space) && (wasOnLeftWall && Input.GetKey(KeyCode.A) || wasOnRightWall && Input.GetKey(KeyCode.D))) {
			rb.velocity = new Vector2(0f, rb.velocity.y);
			// this extra push simulates the JumpStifleHandler code, making sharp jumps over ledges possible
			if(rb.velocity.y > 5){	// don't want to add the force if we're already slow in the y direction
				if(wasOnLeftWall) rb.AddForce((Vector2.down + 1.5f*Vector2.left).normalized * JumpingForce * 0.5f, ForceMode2D.Impulse);
				else rb.AddForce((Vector2.down + 1.5f*Vector2.right).normalized * JumpingForce * 0.5f, ForceMode2D.Impulse);
			}
		}
	}




	//  --------- Public Functions ---------  //
	// Returns true if the player is on the ground
	public bool OnGround(){
		return FloorDetector.isTouching;
	}

	// Uncaps the player's speed until they touch the ground again.
	public void UncapHSpeedUntilOnGround() {
		StartCoroutine(uncap_speed());
	}
	IEnumerator uncap_speed() {
		justJumpedOffHook = true;
		while(!FloorDetector.isTouching) {
			yield return null;
		}
		justJumpedOffHook = false;
	}




	//  --------- Helper Functions ---------  //
	// Allows the player to wall jump for a few moments after leaving the wall.
	IEnumerator WallJumpWindow() {
		yield return new WaitForSeconds(0.15f);
		if(!isOnWall) canWallJump = false;	// if statement handles case where we quickly retouch a wall
	}

	//  --------- OnTriggerEnter2D ---------  //
	void OnTriggerStay2D(Collider2D collision) {
		if(collision.GetComponent<AreaEffector2D>() != null) {
			inWindEffector = true;
		}
	}
	void OnTriggerExit2D(Collider2D collision) {
		if(collision.GetComponent<AreaEffector2D>() != null){
			StartCoroutine(onTriggerExit_helper());
		}
	}
	IEnumerator onTriggerExit_helper() {
		// waits until the player touches the floor again before turning off the wind effector
		while(!FloorDetector.isTouching) {
			yield return null;
		}
		inWindEffector = false;
	}
}
