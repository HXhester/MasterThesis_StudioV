using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.VR;
using System;
using System.IO;

// TODO: play with validity

public class EyeController : Photon.MonoBehaviour
{
    public float lerpRate = 20f;
	public float _blinkBlendSpeed = 2f;
	[Header("Local")]
	public Transform eye_L;
	public Transform eye_R;
	[Header("Remote")]
	public Transform r_eye_L;
	public Transform r_eye_R;

	private Vector3 _gazeDirection;
	private float _leftEyeOpenness;
	private float _rightEyeOpenness;
    
    private float _prevLeftEyeOpenness;
	private float _prevRightEyeOpenness;
	static float t_EyeL = 0.0f;
	static float t_EyeR = 0.0f;

	private Vector3 new_gazeDirection;

	// Recording of eye gaze direction data
	private StreamWriter sw;
	private String filename;

    public DataRecording NewRecord;
    public bool isRecording = false;

	// Use this for initialization
	void Start () {
	    //TODO: uncomment all if(photonview.isMine)
        //if (photonView.isMine)
	    {

            NewRecord = new DataRecording();


            Camera eyeCam = transform.Find("[CameraRig]").Find("Camera (eye)").GetComponent<Camera>();
	        TobiiVR_Host.Instance.Init(eyeCam);
	        TobiiVR_Host.Instance.ValidTrackerData += OnValidData;

	        _prevLeftEyeOpenness = _leftEyeOpenness;
	        _prevRightEyeOpenness = _rightEyeOpenness;

	    }
	}
	
	// Update is called once per frame
	void Update () {
		if (photonView.isMine) {
			if (isRecording)
			{
			    NewRecord.Tobii_EyeDirection = _gazeDirection;
			}
			
			eye_L.forward = Vector3.Lerp (eye_L.forward, _gazeDirection, Time.deltaTime * lerpRate);
			eye_R.forward = eye_L.forward;

		    r_eye_L.forward = Vector3.Lerp(r_eye_L.forward, _gazeDirection, Time.deltaTime * lerpRate);
		    r_eye_R.forward = r_eye_L.forward;
        } else {
            
		    eye_L.forward = Vector3.Lerp(eye_L.forward, new_gazeDirection, Time.deltaTime * lerpRate);
		    eye_R.forward = eye_L.forward;

            r_eye_L.forward = Vector3.Lerp (r_eye_L.forward, new_gazeDirection, Time.deltaTime * lerpRate);
			r_eye_R.forward = r_eye_L.forward;
		}

		UpdateBlendShapes (transform);
	}

	private void OnValidData(object sender, EventArgs e)
	{
		if (photonView.isMine) {


			Quaternion rotation = Quaternion.LookRotation(TobiiVR_Host.Instance.LocalGazeDirection);
			Vector3 eyeDirection = rotation.eulerAngles;

			var x = Mathf.Clamp (eyeDirection.x, -20f, 20f);
			var y = Mathf.Clamp (eyeDirection.y, -10f, 10f);
			var z = eyeDirection.z;

			//_gazeDirection = new Vector3 (x, y, z);
	
			_gazeDirection = TobiiVR_Host.Instance.GazeDirection;
			_leftEyeOpenness = TobiiVR_Host.Instance.LatestData.Left.EyeOpenness;
			_rightEyeOpenness = TobiiVR_Host.Instance.LatestData.Right.EyeOpenness;
		}

	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting) {
			stream.SendNext (_gazeDirection);
			stream.SendNext (_leftEyeOpenness);
			stream.SendNext (_rightEyeOpenness);

		} else {
			new_gazeDirection = (Vector3)stream.ReceiveNext ();
			_leftEyeOpenness = (float)stream.ReceiveNext ();
			_rightEyeOpenness = (float)stream.ReceiveNext ();
		}
	}

	void UpdateBlendShapes(Transform gameObject)
	{
		//  Update this
		SkinnedMeshRenderer renderer = gameObject.GetComponent<SkinnedMeshRenderer>();
		if (renderer != null)
		{
			var mesh = renderer.sharedMesh;


			for (int j = 0; j < mesh.blendShapeCount; j++)
			{
				var blendShapeName = mesh.GetBlendShapeName(j);
				if (blendShapeName.Contains("Blink_L"))
				{
					renderer.SetBlendShapeWeight(j, Mathf.Lerp((1 - _prevLeftEyeOpenness) * 100f, (1 - _leftEyeOpenness) * 100.0f, t_EyeL));

					t_EyeL += _blinkBlendSpeed * Time.deltaTime;

					if (t_EyeL >= 1.0f) {
						_prevLeftEyeOpenness = _leftEyeOpenness;
						t_EyeL = 0.0f;
					}
				}
				if (blendShapeName.Contains ("Blink_R")) 
				{
					renderer.SetBlendShapeWeight(j, Mathf.Lerp((1 - _prevRightEyeOpenness) * 100f, (1-_rightEyeOpenness) * 100.0f, t_EyeR));

					t_EyeR += _blinkBlendSpeed * Time.deltaTime;

					if (t_EyeR >= 1.0f) {
						_prevRightEyeOpenness = _rightEyeOpenness;
						t_EyeR = 0.0f;
					}
				    

				}
			}

		}
		foreach(Transform children in gameObject)
		{
			UpdateBlendShapes(children);
		}
	}

	// Destroy eyetracking onValidData event
	void OnDisable(){
		TobiiVR_Host.Instance.ValidTrackerData -= OnValidData;
	}

	// Close the recording file when game quit
	void OnApplicationQuit(){
		if (sw != null) {
			sw.Close ();
		}

	}



}
