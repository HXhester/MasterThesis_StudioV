using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class NetworkedCameraClient : Photon.MonoBehaviour {

	public float stickDeadZone = 0.01f;
	public float movementSpeed = 1;
	public float rotationSpeed = 100;
	public float zSpeed = .5f;
	Camera camera;
	[SerializeField]List<GameObject> CameraPrefabs = new List<GameObject>();
	Renderer cameraMarker;


	void Start(){
		
		// make the cursor invisible and locked?
		GetAndApplySceneCamera();
		if (lockCursor)
		{
			Screen.lockCursor = true;
		}


	}
	void Update(){
		if (Input.GetAxis ("Horizontal") > 0.01f || Input.GetAxis ("Horizontal") < -0.01f || Input.GetAxis ("Vertical") > 0.01f || Input.GetAxis ("Vertical") < -0.01f) {
			transform.Translate (new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical")) * Time.deltaTime * movementSpeed);
		}if (Input.GetAxis ("Depth") < -stickDeadZone) {
			transform.Translate (new Vector3(0, -Time.deltaTime, 0) * movementSpeed);
		}if (Input.GetAxis ("Depth") > stickDeadZone) {
			transform.Translate (new Vector3(0, Time.deltaTime, 0) * movementSpeed);
		}if (Input.GetAxis ("UpAxis") > -1 || Input.GetAxis("DownAxis") > -1) {
			transform.Translate (new Vector3 (0, (-(Input.GetAxis ("UpAxis") + 1) + (Input.GetAxis("DownAxis") + 1)) * Time.deltaTime, 0) * zSpeed);
		}if (Input.GetAxis ("Pitch") > stickDeadZone || Input.GetAxis ("Pitch") < -stickDeadZone || Input.GetAxis("Jaw") > stickDeadZone || Input.GetAxis("Jaw") < -stickDeadZone) {
			transform.Rotate (new Vector3 (Input.GetAxis ("Pitch"),0, 0) * Time.deltaTime * rotationSpeed, Space.Self);
			transform.Rotate (new Vector3(0,Input.GetAxis("Jaw"),0) * Time.deltaTime * rotationSpeed,Space.World);
		}
	}

	[PunRPC]
	void RPC_AddToCameraManager(){
		
		GameObject cmo = GameObject.FindGameObjectWithTag ("CameraManager");
		if (cmo == null) {
			return;
		}
		CameraManager cm = cmo.GetComponent<CameraManager> ();
		cm.cameras.RemoveAll (item => item == null);
		cm.cameras.Add (camera); 
	}
		
	private void OnEnable(){
//		SceneManager.sceneLoaded += OnSceneLoaded;
		AvatarAndSceneManagerScript.OnSceneAsyncDone += OnSceneLoaded;
	}
	private void OnDisable(){
//		SceneManager.sceneLoaded -= OnSceneLoaded;
		AvatarAndSceneManagerScript.OnSceneAsyncDone -= OnSceneLoaded;

	}

	public virtual void OnSceneLoaded(){
		Debug.Log ("OnSceneLoaded");
		GetAndApplySceneCamera ();

	}

	void GetAndApplySceneCamera(){
		
		if (SceneManager.GetActiveScene ().name == "Launcher") {
			return;
		}
		if (camera != null) {
			camera.gameObject.SetActive(false);
			Destroy (camera.gameObject);
		}
		camera = GameObject.FindGameObjectWithTag("CameraClientPickup").GetComponent<Camera>();

		if (photonView.isMine) {
			Debug.Log ("ApplyCam " + camera.name);
			camera.enabled = true;
			camera.transform.parent = transform;
			camera.transform.localPosition = Vector3.zero;
			camera.transform.localRotation = Quaternion.Euler(Vector3.zero);
			camera.GetComponent<AudioListener> ().enabled = true;
			photonView.RPC ("RPC_AddToCameraManager", PhotonTargets.All, new object[]{ });
		}else{
			camera = Instantiate (camera, Vector3.zero, Quaternion.identity).GetComponent<Camera>();
			camera.transform.SetParent (transform);
			camera.transform.localPosition = Vector3.zero;
			camera.transform.localRotation = Quaternion.identity;
			camera.enabled = false;
			this.enabled = false;
		}
	}

	public void SetCameraMarkerTo(bool b){
		if (b) {
			
		} else {
		
		}
	}
		





	public bool lockCursor = false;

	public float sensitivity = 30;
	public int smoothing = 10;

	float ymove;
	float xmove;

	int iteration = 0;

	float xaggregate = 0;
	float yaggregate = 0;

	//int Ylimit = 0;
	public int Xlimit = 20;

	void FixedUpdate () {

		// ensure mouseclicks do not effect the screenlock

		if (lockCursor)
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			Screen.lockCursor = true;
		}

		float[] x = new float[smoothing];
		float[] y = new float[smoothing];

		// reset the aggregate move values

		xaggregate = 0;
		yaggregate = 0;

		// receive the mouse inputs

		ymove = Input.GetAxis("Mouse Y");
		xmove = Input.GetAxis("Mouse X");

		// cycle through the float arrays and lop off the oldest value, replacing with the latest

		y[iteration % smoothing] = ymove;
		x[iteration % smoothing] = xmove;

		iteration++;

		// determine the aggregates and implement sensitivity

		foreach (float xmov in x)
		{
			xaggregate += xmov;
		}

		xaggregate = xaggregate / smoothing * sensitivity;

		foreach (float ymov in y)
		{
			yaggregate += ymov;
		}

		yaggregate = yaggregate / smoothing * sensitivity;

		// turn the x start orientation to non-zero for clamp

		Vector3 newOrientation = transform.eulerAngles + new Vector3(-yaggregate, xaggregate, 0);
//		Vector3 newOrientation =  new Vector3(-ymove, xmove, 0);


		//xclamp = Mathf.Clamp(newOrientation.x, Xlimit, 360-Xlimit)%360;

		// rotate the object based on axis input (note the negative y axis)

		transform.eulerAngles = newOrientation;

	}


}