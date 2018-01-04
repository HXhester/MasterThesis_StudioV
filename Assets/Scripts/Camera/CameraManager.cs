using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraManager : Photon.MonoBehaviour {

	public List<Camera> cameras;
	public Camera activeCamera;
	public Color selectedCameraColor;
	Color baseCameraColor;

	private KeyCode[] keyCodes = {
		KeyCode.Alpha0,
		KeyCode.Alpha1,
		KeyCode.Alpha2,
		KeyCode.Alpha3,
		KeyCode.Alpha4,
		KeyCode.Alpha5,
		KeyCode.Alpha6,
		KeyCode.Alpha7,
		KeyCode.Alpha8,
		KeyCode.Alpha9
	};
	// Use this for initialization
	void Awake () {
		if (!photonView.isMine) {
			this.enabled = false;

		} else {
			UpdateCameraList ();
			foreach (Camera c in cameras) {
				if (c != null) { 
					if (c.enabled) {
						activeCamera = c;
						baseCameraColor = activeCamera.GetComponentInChildren<Renderer> ().material.color;
					}
				}
			}
		}
	}
	void OnEnable(){
		if (photonView.isMine) {
			AvatarAndSceneManagerScript.OnSceneAsyncDone += UpdateCameraList;
		}
	}

	void OnDisable(){
		if (photonView.isMine) {
			AvatarAndSceneManagerScript.OnSceneAsyncDone -= UpdateCameraList;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown && photonView.isMine) {
			//Updates the camera list
			if (Input.GetKeyDown (KeyCode.U)) {
				UpdateCameraList ();
			}
			for (int i = 0; i < keyCodes.Length; i++) {
				if (Input.GetKeyDown (keyCodes [i])) {
					SetActiveCamera (i -1);
				}
			}
		}
	}
	void SetActiveCamera(int i){
		if (i < 0 || i >= cameras.Count) {
			Debug.LogWarning ("Cant switch to camera slot " + (i  + 1) + " because it was not found");
			return;
		}
		if (activeCamera == cameras [i]) {
			Debug.LogWarning ("Camera already selected");
			return;
		}
		if (cameras [i] == null) {
			Debug.LogWarning ("Selected camera dose not exist");
			UpdateCameraList ();
		}
		cameras [i].enabled = true;
		if(activeCamera != null){
			activeCamera.enabled = false;
			ResetCameraMarkerColor ();


			if(activeCamera.GetComponent<AudioListener>() != null){
				activeCamera.GetComponent<AudioListener> ().enabled = false;
			}
		}
		if(cameras [i].GetComponent<AudioListener> () != null){
			cameras [i].GetComponent<AudioListener> ().enabled = true;
		}

		Debug.Log(cameras [i].name);
		activeCamera = cameras [i];
//		photonView.RPC ("RPC_SetCameraMarkerColor", PhotonTargets.All, new object[]{activeCamera.GetComponent<CameraMarker>().viewId});
		SetCameraMarkerColor();
	}


	public void SetCameraMarkerColor(){
		Debug.Log ("activeCamera " + activeCamera);
		Debug.Log ("activeCamera aa" + activeCamera.GetComponent<CameraMarker>());


		try {
			activeCamera.transform.parent.GetComponent<CameraMarker> ().SetAsSelected(true);
		} catch {
			activeCamera.GetComponent<CameraMarker> ().SetAsSelected(true);
		}
	}

	public void ResetCameraMarkerColor(){
		try {
			activeCamera.transform.parent.GetComponent<CameraMarker> ().SetAsSelected(false);
		} catch  {
			activeCamera.GetComponent<CameraMarker> ().SetAsSelected(false);
		}
	}

	void AddAllConnectedCameraClients(){
		GameObject[] cameraObjs = GameObject.FindGameObjectsWithTag ("CameraClient");
		if (cameraObjs.Length > 0) {
			foreach (GameObject go in cameraObjs) {
				Camera c = go.GetComponentInChildren<Camera> ();
				if(!cameras.Contains(c)){
					cameras.Add(c);
				}
			}
		}
	}
	void AddAllStaticCameras(){
		Camera[] staticCameras = GetComponentsInChildren<Camera> ();
		foreach (Camera c in staticCameras) {
			if (!cameras.Contains (c) && c.gameObject.activeInHierarchy) {
				cameras.Add (c);
			}
		}
	}
	void AddAllVirtualCameras(){
		GameObject[] virtualCameras = GameObject.FindGameObjectsWithTag ("VirtualCamera");
		if (virtualCameras.Length > 0) {
			foreach (GameObject go in virtualCameras) {
				Camera c = go.GetComponent<Camera> ();
				if (!cameras.Contains (c) && c.gameObject.activeInHierarchy) {
					cameras.Add (c);
					c.targetDisplay = 0;
				}
			}
		}
	}
	void UpdateCameraList(){
		cameras.Clear ();
		AddAllConnectedCameraClients ();
		AddAllStaticCameras ();
		AddAllVirtualCameras ();
		if (activeCamera == null) {
			Debug.Log ("activeCam = null");
			SetActiveCamera (0);
		}
	}
}
