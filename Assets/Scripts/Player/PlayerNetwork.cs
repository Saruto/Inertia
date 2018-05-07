using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

[System.Serializable]
public class ToggleEvent : UnityEvent<bool> {

}

public class PlayerNetwork : NetworkBehaviour {
	// ----------------------------------- Fields and Properties ----------------------------------- //

	// The local player's gameobject.
	// This is a different value for each client machine.
	// Assumes that there's only 1 player per instance of the game running.
	public static GameObject LocalPlayer { get; private set; }

	[SerializeField] ToggleEvent onToggleShared;
	[SerializeField] ToggleEvent onToggleLocal;
	[SerializeField] ToggleEvent onToggleRemote;



	// ------------------------------------------ Methods ------------------------------------------ //
	//  --------- Start ---------  //
	void Start () {
		EnablePlayer();
	}
	
	//  --------- Update ---------  //
	void Update () {
		
	}

	void EnablePlayer() {
		onToggleShared.Invoke(true);
		if(isLocalPlayer) {
			onToggleLocal.Invoke(true);
			LocalPlayer = gameObject;
			Camera.main.GetComponent<CameraMovement>().SetTarget(LocalPlayer);
		} else {
			onToggleRemote.Invoke(true);
		}
	}

	void DisablePlayer() {
		onToggleShared.Invoke(false);
		if(isLocalPlayer) {
			onToggleLocal.Invoke(false);
			Camera.main.GetComponent<CameraMovement>().SetTarget(null);
		} else {
			onToggleRemote.Invoke(false);
		}
	}

	// Respawns the player.
	[Command]
	public void CmdRespawn() {
		RpcRespawn();
	}

	[ClientRpc]
	void RpcRespawn() {
		DisablePlayer();
		Invoke("Respawn", 1f);
	}

	void Respawn() {
		if(isLocalPlayer) {
			Transform spawn = NetworkManager.singleton.GetStartPosition();
			transform.position = spawn.position;
		}
		EnablePlayer();
	}
}
