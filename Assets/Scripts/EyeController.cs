using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Research.Unity.CodeExamples;
using System;
using System.IO;

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

    public SkinnedMeshRenderer HmdFace;
    public SkinnedMeshRenderer OptitrackFace;

	private Vector3 _gazeDirection;
    private Vector3 new_gazeDirection;
    private SubscribingToHMDGazeData _subscribingGazeData;

    // Eye blink controller
    private float _leftBlinkTimeStamp = 0.0f;
    private float _leftPrevBlinkTimeStamp;
    private float _rightBlinkTimeStamp = 0.0f;
    private float _rightPrevBlinkTimeStamp;

    private float _leftEyeOpenness;
    private float _rightEyeOpenness;
    private float _leftBlinkBlendShape;
    private float _rightBlinkBlendShape;
    private float _newLeftBlinkBlendShape;
    private float _newRightBlinkBlendShape;

    private float t_EyeL;
    private float t_EyeR;
    private int _blinkLeftBlendShapeId = -1;
    private int _blinkRightBlendShapeId = -1;
    public float EyeMoveLerpSpeed = 20f;
    public float BlinkBlendSpeed = 4f;


    // Use this for initialization
    void Start() {

        if (photonView.isMine) {
            Camera eyeCam = GetComponent<PlayerManager>().Camera.GetComponent<Camera>();

            TobiiPro_Host.Instance.SetCameraUsedToRender(eyeCam);
            _subscribingGazeData = SubscribingToHMDGazeData.SubscribingInstance;
        }

        if (HmdFace != null) {
            var mesh = HmdFace.sharedMesh;


            for (int j = 0; j < mesh.blendShapeCount; j++) {
                var blendShapeName = mesh.GetBlendShapeName(j);

                if (blendShapeName.Contains("Blink_L"))
                    _blinkLeftBlendShapeId = j;
                if (blendShapeName.Contains("Blink_R"))
                    _blinkRightBlendShapeId = j;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		if (photonView.isMine) {

            // Sync data to another thread for recording
		    if (_subscribingGazeData != null)
		    {
                _gazeDirection = _subscribingGazeData.GazeDirection;
                _leftEyeOpenness = _subscribingGazeData.LeftEyeOpenness;
                _rightEyeOpenness = _subscribingGazeData.RightEyeOpenness;
            }
		   
            // Set eye gaze direction
            if (TobiiPro_Host.EyeTrackerInstance != null && !TobiiPro_Host.isEyeTrackerConnected)
            {
                eye_L.transform.localRotation = Quaternion.identity;
                eye_R.forward = eye_L.forward;

                r_eye_L.transform.localRotation = Quaternion.identity;
                r_eye_R.forward = r_eye_L.forward;

                _gazeDirection = eye_L.forward;               
            }
            else
            {
                // global rotation
                eye_L.forward = Vector3.Lerp(eye_L.forward, _gazeDirection, Time.deltaTime * EyeMoveLerpSpeed);
                eye_R.forward = eye_L.forward;

                r_eye_L.forward = Vector3.Lerp(r_eye_L.forward, _gazeDirection, Time.deltaTime * EyeMoveLerpSpeed);
                r_eye_R.forward = r_eye_L.forward;
            }
        } else {

            eye_L.forward = Vector3.Lerp(eye_L.forward, new_gazeDirection, Time.deltaTime * EyeMoveLerpSpeed);
            eye_R.forward = eye_L.forward;

            r_eye_L.forward = Vector3.Lerp(r_eye_L.forward, new_gazeDirection, Time.deltaTime * EyeMoveLerpSpeed);
            r_eye_R.forward = r_eye_L.forward;
        }

        //UpdateBlendShapes (transform);

        // Set eye blink
        if (_blinkLeftBlendShapeId != -1 && _blinkRightBlendShapeId != -1) {
            UpdateEyeBlink(HmdFace);
            UpdateEyeBlink(OptitrackFace);
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

	//void UpdateBlendShapes(Transform gameObject)
	//{
	//	//  Update this
	//	SkinnedMeshRenderer renderer = gameObject.GetComponent<SkinnedMeshRenderer>();
	//	if (renderer != null)
	//	{
	//		var mesh = renderer.sharedMesh;

	//		for (int j = 0; j < mesh.blendShapeCount; j++)
	//		{
	//			var blendShapeName = mesh.GetBlendShapeName(j);
	//			if (blendShapeName.Contains("Blink_L"))
	//			{
	//				renderer.SetBlendShapeWeight(j, Mathf.Lerp((1 - _prevLeftEyeOpenness) * 100f, (1 - _leftEyeOpenness) * 100.0f, t_EyeL));

	//				t_EyeL += _blinkBlendSpeed * Time.deltaTime;

	//				if (t_EyeL >= 1.0f) {
	//					_prevLeftEyeOpenness = _leftEyeOpenness;
	//					t_EyeL = 0.0f;
	//				}
	//			}
	//			if (blendShapeName.Contains ("Blink_R")) 
	//			{
	//				renderer.SetBlendShapeWeight(j, Mathf.Lerp((1 - _prevRightEyeOpenness) * 100f, (1-_rightEyeOpenness) * 100.0f, t_EyeR));

	//				t_EyeR += _blinkBlendSpeed * Time.deltaTime;

	//				if (t_EyeR >= 1.0f) {
	//					_prevRightEyeOpenness = _rightEyeOpenness;
	//					t_EyeR = 0.0f;
	//				}
	//			}
	//		}
	//	}
	//	foreach(Transform children in gameObject)
	//	{
	//		UpdateBlendShapes(children);
	//	}
	//}


    void UpdateEyeBlink(SkinnedMeshRenderer faceRenderer) {

        if (faceRenderer != null) {

            if (TobiiPro_Host.EyeTrackerInstance != null && !TobiiPro_Host.isEyeTrackerConnected) {
                faceRenderer.SetBlendShapeWeight(_blinkLeftBlendShapeId, 0f);
                faceRenderer.SetBlendShapeWeight(_blinkRightBlendShapeId, 0f);
                return;
            }

            _leftBlinkBlendShape = faceRenderer.GetBlendShapeWeight(_blinkLeftBlendShapeId);
            _rightBlinkBlendShape = faceRenderer.GetBlendShapeWeight(_blinkRightBlendShapeId);

            if (_leftBlinkTimeStamp - _leftPrevBlinkTimeStamp > 1.0f || _leftBlinkBlendShape != (1 - _leftEyeOpenness) * 100.0f) {
                faceRenderer.SetBlendShapeWeight(_blinkLeftBlendShapeId, Mathf.Lerp(_leftBlinkBlendShape, (1 - _leftEyeOpenness) * 100.0f, t_EyeL));
                t_EyeL += BlinkBlendSpeed * Time.deltaTime;


                if (t_EyeL >= 1.0f) {
                    faceRenderer.SetBlendShapeWeight(_blinkLeftBlendShapeId, (1 - _leftEyeOpenness) * 100.0f);
                    t_EyeL = 0.0f;

                    // Take the timestamp after lerp to blink
                    if (_leftEyeOpenness == 0) {
                        _leftPrevBlinkTimeStamp = Time.time;
                    }
                }
            }

            
            faceRenderer.SetBlendShapeWeight(_blinkRightBlendShapeId, _leftBlinkBlendShape);
            
        }
    }
}
