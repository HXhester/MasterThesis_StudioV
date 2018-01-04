using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHeadPos : Photon.MonoBehaviour {
	public Transform CameraEye;
//	public Transform remoteCameraEye;
	public float LerpRate = 42.0f;

    private Vector3 headToEyeVector;
	public Vector3 origHeadToEyeVector;
    private Vector3 origRotation;
    private Vector3 origHeadScale;

	// Use this for initialization
	void Awake()
	{

	    origHeadScale = transform.localScale;

	}
	void Start () {
		origHeadToEyeVector = - transform.position;
		headToEyeVector = origHeadToEyeVector;
		origRotation = transform.localEulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 newPos;
		Vector3 rot = CameraEye.localEulerAngles;
		headToEyeVector = Quaternion.Euler (rot.x, rot.y, rot.z) * headToEyeVector;

        newPos = CameraEye.position - headToEyeVector;

        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * LerpRate);

		transform.localEulerAngles = origRotation;
		transform.Rotate (CameraEye.localEulerAngles);

	    headToEyeVector = origHeadToEyeVector;

	}
}
