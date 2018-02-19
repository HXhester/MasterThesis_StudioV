using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHeadPos : Photon.MonoBehaviour {
	private Transform _cameraEye;
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
	void Start ()
	{
	    _cameraEye = transform.parent.GetComponent<PlayerManager>().Camera.transform;
        origHeadToEyeVector = - transform.position;
		headToEyeVector = origHeadToEyeVector;
		origRotation = transform.localEulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
        if(_cameraEye==null)
            return;
	    
		Vector3 newPos;
		Vector3 rot = _cameraEye.localEulerAngles;
		headToEyeVector = Quaternion.Euler (rot.x, rot.y, rot.z) * headToEyeVector;

        newPos = _cameraEye.position - headToEyeVector;

        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * LerpRate);

		transform.localEulerAngles = origRotation;
		transform.Rotate (_cameraEye.localEulerAngles);

	    headToEyeVector = origHeadToEyeVector;

	}
}
